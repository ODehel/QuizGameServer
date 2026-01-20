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

    public async Task<bool> SendMessageAsync(string buzzerId, string message)
    {
        // À implémenter selon votre protocole HTTP
        // Exemple : POST /message avec le contenu du message
        return false;
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
}
