using QuizGame.Domain.Entities;
using QuizGame.Domain.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizGame.Infrastructure.Services;

/// <summary>
/// Service orchestrateur pour gérer les buzzers
/// </summary>
public class BuzzerManager : IDisposable
{
    private readonly IBuzzerDiscoveryService _discoveryService;
    private readonly IBuzzerCommunicationService _communicationService;
    private readonly Dictionary<string, Buzzer> _buzzers;
    private readonly object _buzzersLock = new();

    public event EventHandler<BuzzerEvent>? BuzzerEventOccurred;

    public BuzzerManager(IBuzzerDiscoveryService discoveryService, IBuzzerCommunicationService communicationService)
    {
        _discoveryService = discoveryService;
        _communicationService = communicationService;
        _buzzers = new Dictionary<string, Buzzer>();

        // S'abonner aux événements de communication
        _communicationService.MessageReceived += OnBuzzerMessageReceived;
        _communicationService.StatusChanged += OnBuzzerStatusChanged;
    }

    /// <summary>
    /// Scanne le réseau et découvre les buzzers dont le nom commence par "Quiz"
    /// </summary>
    public async Task<List<Buzzer>> DiscoverBuzzersAsync(string namePrefix = "Quiz")
    {
        var discoveredBuzzers = await _discoveryService.DiscoverBuzzersAsync(namePrefix);

        lock (_buzzersLock)
        {
            foreach (var buzzer in discoveredBuzzers)
            {
                if (!_buzzers.ContainsKey(buzzer.Id))
                {
                    _buzzers[buzzer.Id] = buzzer;
                }
            }
        }

        return discoveredBuzzers;
    }

    /// <summary>
    /// Connecte un buzzer
    /// </summary>
    public async Task<bool> ConnectBuzzerAsync(string buzzerId)
    {
        lock (_buzzersLock)
        {
            if (!_buzzers.TryGetValue(buzzerId, out var buzzer))
                return false;

            buzzer.Status = BuzzerStatus.Connecting;
        }

        var buzzer2 = _buzzers[buzzerId];
        var result = await _communicationService.ConnectAsync(buzzer2);

        return result;
    }

    /// <summary>
    /// Déconnecte un buzzer
    /// </summary>
    public async Task DisconnectBuzzerAsync(string buzzerId)
    {
        lock (_buzzersLock)
        {
            if (_buzzers.TryGetValue(buzzerId, out var buzzer))
            {
                buzzer.Status = BuzzerStatus.Disconnected;
            }
        }

        await _communicationService.DisconnectAsync(buzzerId);
    }

    /// <summary>
    /// Envoie un message à un buzzer
    /// </summary>
    public async Task<bool> SendMessageAsync(string buzzerId, string message)
    {
        return await _communicationService.SendMessageAsync(buzzerId, message);
    }

    /// <summary>
    /// Obtient tous les buzzers gérés
    /// </summary>
    public List<Buzzer> GetAllBuzzers()
    {
        lock (_buzzersLock)
        {
            return new List<Buzzer>(_buzzers.Values);
        }
    }

    private void OnBuzzerMessageReceived(object? sender, BuzzerMessageReceivedEventArgs e)
    {
        var buzzerEvent = new BuzzerEvent
        {
            BuzzerId = e.BuzzerId,
            BuzzerName = _buzzers.TryGetValue(e.BuzzerId, out var buzzer) ? buzzer.Name : "Unknown",
            EventType = DetermineBuzzerEventType(e.Message),
            Timestamp = DateTime.UtcNow,
            Data = new() { { "message", e.Message } }
        };

        BuzzerEventOccurred?.Invoke(this, buzzerEvent);
    }

    private void OnBuzzerStatusChanged(object? sender, BuzzerStatusChangedEventArgs e)
    {
        lock (_buzzersLock)
        {
            if (_buzzers.TryGetValue(e.BuzzerId, out var buzzer))
            {
                buzzer.Status = e.NewStatus;
            }
        }

        var eventType = e.NewStatus == BuzzerStatus.Connected ? BuzzerEventType.Connected : BuzzerEventType.Disconnected;
        var buzzerName = _buzzers.TryGetValue(e.BuzzerId, out var buzzer2) ? buzzer2.Name : "Unknown";

        var buzzerEvent = new BuzzerEvent
        {
            BuzzerId = e.BuzzerId,
            BuzzerName = buzzerName,
            EventType = eventType,
            Timestamp = DateTime.UtcNow
        };

        BuzzerEventOccurred?.Invoke(this, buzzerEvent);
    }

    private BuzzerEventType DetermineBuzzerEventType(string message)
    {
        return message.Contains("press", StringComparison.OrdinalIgnoreCase) ? BuzzerEventType.Pressed : BuzzerEventType.Released;
    }

    public void Dispose()
    {
        _communicationService?.Dispose();
    }
}
