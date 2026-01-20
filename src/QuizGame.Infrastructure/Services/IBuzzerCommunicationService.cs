using QuizGame.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizGame.Infrastructure.Services;

/// <summary>
/// Service de communication avec les buzzers via TCP
/// </summary>
public interface IBuzzerCommunicationService : IDisposable
{
    /// <summary>
    /// Établit une connexion avec un buzzer
    /// </summary>
    Task<bool> ConnectAsync(Buzzer buzzer);

    /// <summary>
    /// Déconnecte un buzzer
    /// </summary>
    Task DisconnectAsync(string buzzerId);

    /// <summary>
    /// Envoie une commande à un buzzer
    /// </summary>
    Task<bool> SendMessageAsync(Buzzer buzzer, string action);

    /// <summary>
    /// Définit le nom d'un buzzer
    /// </summary>
    Task<bool> SetBuzzerNameAsync(Buzzer buzzer, string newName);

    /// <summary>
    /// Événement déclenché quand un message est reçu d'un buzzer
    /// </summary>
    event EventHandler<BuzzerMessageReceivedEventArgs>? MessageReceived;

    /// <summary>
    /// Événement déclenché quand le statut d'un buzzer change
    /// </summary>
    event EventHandler<BuzzerStatusChangedEventArgs>? StatusChanged;
}

public class BuzzerMessageReceivedEventArgs : EventArgs
{
    public string BuzzerId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class BuzzerStatusChangedEventArgs : EventArgs
{
    public string BuzzerId { get; set; } = string.Empty;
    public BuzzerStatus OldStatus { get; set; }
    public BuzzerStatus NewStatus { get; set; }
}
