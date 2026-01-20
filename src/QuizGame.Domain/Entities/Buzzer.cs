using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuizGame.Domain.Entities;

/// <summary>
/// Représente un buzzer connecté au réseau
/// </summary>
public class Buzzer : INotifyPropertyChanged
{
    private string _id = Guid.NewGuid().ToString();
    private string _name = string.Empty;
    private string _ipAddress = string.Empty;
    private string _macAddress = string.Empty;
    private BuzzerStatus _status = BuzzerStatus.Disconnected;
    private DateTime _lastSeen;
    private int _port = 80;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string IpAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    public string MacAddress
    {
        get => _macAddress;
        set => SetProperty(ref _macAddress, value);
    }

    public BuzzerStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public DateTime LastSeen
    {
        get => _lastSeen;
        set => SetProperty(ref _lastSeen, value);
    }

    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

public enum BuzzerStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}
