using QuizGame.Domain.Entities;
using System.Collections.Concurrent;
using System.Text.Json;

namespace QuizGame.Infrastructure.Services;

/// <summary>
/// Implémentation du service de communication HTTP avec les buzzers
/// </summary>
public class BuzzerCommunicationService : IBuzzerCommunicationService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, BuzzerStatus> _buzzerStatuses;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _monitoringTokens;

    public event EventHandler<BuzzerMessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<BuzzerStatusChangedEventArgs>? StatusChanged;

    public BuzzerCommunicationService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        _buzzerStatuses = new ConcurrentDictionary<string, BuzzerStatus>();
        _monitoringTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
    }

    public async Task<bool> ConnectAsync(Buzzer buzzer)
    {
        try
        {
            // Vérifier si le buzzer est joignable en appelant le endpoint /status
            var statusUrl = $"http://{buzzer.IpAddress}/status";
            var response = await _httpClient.GetAsync(statusUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var statusResponse = JsonSerializer.Deserialize<BuzzerStatusResponse>(content);

                if (statusResponse?.Status == "ok")
                {
                    _buzzerStatuses[buzzer.Id] = BuzzerStatus.Connected;
                    OnStatusChanged(buzzer.Id, BuzzerStatus.Disconnected, BuzzerStatus.Connected);

                    // Démarrer le monitoring du buzzer
                    _ = MonitorBuzzerAsync(buzzer);

                    return true;
                }
            }

            _buzzerStatuses[buzzer.Id] = BuzzerStatus.Error;
            OnStatusChanged(buzzer.Id, BuzzerStatus.Connecting, BuzzerStatus.Error);
            return false;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Erreur HTTP vers {buzzer.IpAddress}: {ex.Message}");
            _buzzerStatuses[buzzer.Id] = BuzzerStatus.Error;
            OnStatusChanged(buzzer.Id, BuzzerStatus.Connecting, BuzzerStatus.Error);
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Erreur inattendue: {ex.GetType().Name} - {ex.Message}");
            _buzzerStatuses[buzzer.Id] = BuzzerStatus.Error;
            OnStatusChanged(buzzer.Id, BuzzerStatus.Connecting, BuzzerStatus.Error);
            return false;
        }
    }

    public async Task DisconnectAsync(string buzzerId)
    {
        if (_monitoringTokens.TryRemove(buzzerId, out var cts))
        {
            cts?.Cancel();
            cts?.Dispose();
        }

        _buzzerStatuses.TryRemove(buzzerId, out _);
        OnStatusChanged(buzzerId, BuzzerStatus.Connected, BuzzerStatus.Disconnected);
    }

    public async Task<bool> SendMessageAsync(Buzzer buzzer, string action)
    {
        try
        {
            if (!_buzzerStatuses.TryGetValue(buzzer.Id, out var status) || status != BuzzerStatus.Connected)
            {
                return false;
            }

            var messageUrl = $"http://{buzzer.IpAddress}/message";
            var payload = new BuzzerCommandRequest
            {
                Type = "command",
                Action = action
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(messageUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var commandResponse = JsonSerializer.Deserialize<BuzzerCommandResponse>(responseContent);

                if (commandResponse?.Status == "received")
                {
                    System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Commande confirmée: {action}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Réponse invalide: {responseContent}");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Échec de l'envoi de la commande. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Erreur HTTP lors de l'envoi de commande: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Erreur lors de l'envoi du message: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SetBuzzerNameAsync(Buzzer buzzer, string newName)
    {
        try
        {
            if (!_buzzerStatuses.TryGetValue(buzzer.Id, out var status) || status != BuzzerStatus.Connected)
            {
                return false;
            }

            var setNameUrl = $"http://{buzzer.IpAddress}/set-name";
            var payload = new SetNameRequest
            {
                Name = newName
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(setNameUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var setNameResponse = JsonSerializer.Deserialize<SetNameResponse>(responseContent);

                if (setNameResponse?.Status == "ok")
                {
                    // Mettre à jour le nom du buzzer localement
                    buzzer.Name = newName;
                    System.Diagnostics.Debug.WriteLine($"[Buzzer] Nom mis à jour: {buzzer.MacAddress} -> {newName}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Réponse invalide: {responseContent}");
                    return false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Échec de la définition du nom. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Erreur HTTP lors de la définition du nom: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Buzzer {buzzer.Name}] Erreur lors de la définition du nom: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private async Task MonitorBuzzerAsync(Buzzer buzzer)
    {
        var cts = new CancellationTokenSource();
        if (!_monitoringTokens.TryAdd(buzzer.Id, cts))
        {
            cts.Dispose();
            return;
        }

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await Task.Delay(2000, cts.Token);

                try
                {
                    var statusUrl = $"http://{buzzer.IpAddress}/status";
                    var response = await _httpClient.GetAsync(statusUrl, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var statusResponse = JsonSerializer.Deserialize<BuzzerStatusResponse>(content);

                        if (statusResponse?.Status == "ok")
                        {
                            _buzzerStatuses[buzzer.Id] = BuzzerStatus.Connected;
                        }
                        else
                        {
                            _buzzerStatuses[buzzer.Id] = BuzzerStatus.Error;
                        }
                    }
                    else
                    {
                        _buzzerStatuses[buzzer.Id] = BuzzerStatus.Error;
                        OnStatusChanged(buzzer.Id, BuzzerStatus.Connected, BuzzerStatus.Disconnected);
                    }
                }
                catch
                {
                    _buzzerStatuses[buzzer.Id] = BuzzerStatus.Error;
                    OnStatusChanged(buzzer.Id, BuzzerStatus.Connected, BuzzerStatus.Disconnected);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Monitoring arrêté normalement
        }
    }

    private void OnMessageReceived(string buzzerId, string message)
    {
        MessageReceived?.Invoke(this, new BuzzerMessageReceivedEventArgs
        {
            BuzzerId = buzzerId,
            Message = message
        });
    }

    private void OnStatusChanged(string buzzerId, BuzzerStatus oldStatus, BuzzerStatus newStatus)
    {
        StatusChanged?.Invoke(this, new BuzzerStatusChangedEventArgs
        {
            BuzzerId = buzzerId,
            OldStatus = oldStatus,
            NewStatus = newStatus
        });
    }

    public void Dispose()
    {
        _httpClient?.Dispose();

        foreach (var kvp in _monitoringTokens)
        {
            kvp.Value?.Cancel();
            kvp.Value?.Dispose();
        }
        _monitoringTokens.Clear();

        _buzzerStatuses.Clear();
    }

    private class BuzzerStatusResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("device")]
        public string? Device { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("ip")]
        public string? Ip { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("mac")]
        public string? Mac { get; set; }
    }

    private class BuzzerCommandRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("action")]
        public string Action { get; set; } = null!;
    }

    private class BuzzerCommandResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    private class SetNameRequest
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    private class SetNameResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
