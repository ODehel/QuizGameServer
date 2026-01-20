namespace QuizGame.Domain.Events;

/// <summary>
/// Événement déclenché par un buzzer
/// </summary>
public class BuzzerEvent
{
    public string BuzzerId { get; set; } = string.Empty;
    public string BuzzerName { get; set; } = string.Empty;
    public BuzzerEventType EventType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum BuzzerEventType
{
    Connected,
    Disconnected,
    Pressed,
    Released,
    Error
}
