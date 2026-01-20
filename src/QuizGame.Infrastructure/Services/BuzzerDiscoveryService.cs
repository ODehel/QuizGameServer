using QuizGame.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuizGame.Infrastructure.Services;

/// <summary>
/// Implémentation du service de découverte des buzzers par HTTP
/// </summary>
public class BuzzerDiscoveryService : IBuzzerDiscoveryService
{
    private readonly string _networkPrefix;
    private readonly HttpClient _httpClient;
    private const int StartIp = 1;
    private const int EndIp = 254;

    public BuzzerDiscoveryService(string? networkPrefix = null)
    {
        // Préfixe réseau par défaut (ex: "192.168.1")
        _networkPrefix = networkPrefix ?? "192.168.1";
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    }

    public async Task<List<Buzzer>> DiscoverBuzzersAsync(string namePrefix = "Quiz")
    {
        return await ScanNetworkAsync();
    }

    public async Task<List<Buzzer>> ScanNetworkAsync()
    {
        var buzzers = new List<Buzzer>();

        try
        {
            var tasks = new List<Task<Buzzer?>>();

            // Scanner les adresses IP du réseau en parallèle
            for (int i = StartIp; i <= EndIp; i++)
            {
                var ipAddress = $"{_networkPrefix}.{i}";
                tasks.Add(ProbeBuzzerAsync(ipAddress));
            }

            var results = await Task.WhenAll(tasks);

            foreach (var buzzer in results.Where(b => b != null))
            {
                buzzers.Add(buzzer!);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du scan réseau: {ex.Message}");
        }

        return buzzers;
    }

    private async Task<Buzzer?> ProbeBuzzerAsync(string ipAddress)
    {
        try
        {
            var statusUrl = $"http://{ipAddress}/status";
            var response = await _httpClient.GetAsync(statusUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var statusResponse = JsonSerializer.Deserialize<BuzzerStatusResponse>(content);

                if (statusResponse?.Status == "ok" && !string.IsNullOrEmpty(statusResponse.Device) && statusResponse.Device.StartsWith("Quiz", StringComparison.OrdinalIgnoreCase))
                {
                    return new Buzzer
                    {
                        IpAddress = ipAddress,
                        MacAddress = NormalizeMacAddress(statusResponse.Mac),
                        Name = statusResponse.Device ?? $"Buzzer {ipAddress}",
                        Status = BuzzerStatus.Disconnected,
                        LastSeen = DateTime.UtcNow,
                        Port = 80
                    };
                }
            }
        }
        catch
        {
            // Buzzer non trouvé ou inaccessible à cette adresse
        }

        return null;
    }

    private string NormalizeMacAddress(string? macAddress)
    {
        if (string.IsNullOrEmpty(macAddress))
            return string.Empty;

        return macAddress.ToLower().Replace(":", "-");
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
