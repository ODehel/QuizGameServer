using QuizGame.Domain.Entities;
using QuizGame.Domain.Events;
using QuizGame.Infrastructure.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuizGame.Presentation.Wpf.ViewModels;

public class BuzzerViewModel : INotifyPropertyChanged
{
    private BuzzerManager? _buzzerManager;
    private bool _isScanning;
    private string _statusMessage = "Prêt";
    private Buzzer? _selectedBuzzer;
    private readonly Dispatcher _dispatcher;

    public ObservableCollection<Buzzer> DiscoveredBuzzers { get; }
    public ObservableCollection<BuzzerEventLog> EventLogs { get; }

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (_isScanning != value)
            {
                _isScanning = value;
                OnPropertyChanged(nameof(IsScanning));
                OnPropertyChanged(nameof(ScanButtonText));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    public Buzzer? SelectedBuzzer
    {
        get => _selectedBuzzer;
        set
        {
            if (_selectedBuzzer != value)
            {
                _selectedBuzzer = value;
                OnPropertyChanged(nameof(SelectedBuzzer));
                OnPropertyChanged(nameof(IsConnectButtonEnabled));
            }
        }
    }

    public string ScanButtonText => IsScanning ? "Arrêter le scan..." : "Scan réseau";
    public bool IsConnectButtonEnabled => SelectedBuzzer != null && SelectedBuzzer.Status == BuzzerStatus.Disconnected;

    public ICommand ScanNetworkCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand ClearLogsCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public BuzzerViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        DiscoveredBuzzers = new ObservableCollection<Buzzer>();
        EventLogs = new ObservableCollection<BuzzerEventLog>();

        ScanNetworkCommand = new RelayCommand(_ => ScanNetworkAsync(), _ => !IsScanning);
        ConnectCommand = new RelayCommand(_ => ConnectAsync(), _ => IsConnectButtonEnabled);
        DisconnectCommand = new RelayCommand(_ => DisconnectAsync(), _ => SelectedBuzzer?.Status == BuzzerStatus.Connected);
        ClearLogsCommand = new RelayCommand(_ => EventLogs.Clear());
    }

    public void Initialize(BuzzerManager buzzerManager)
    {
        _buzzerManager = buzzerManager;
        _buzzerManager.BuzzerEventOccurred += OnBuzzerEvent;
    }

    private async void ScanNetworkAsync()
    {
        if (IsScanning || _buzzerManager == null)
            return;

        IsScanning = true;
        StatusMessage = "Scan en cours...";

        try
        {
            var buzzers = await _buzzerManager.DiscoverBuzzersAsync("Quiz");
            
            _dispatcher.Invoke(() =>
            {
                DiscoveredBuzzers.Clear();
                foreach (var buzzer in buzzers)
                {
                    DiscoveredBuzzers.Add(buzzer);
                }

                StatusMessage = $"Scan terminé : {buzzers.Count} buzzer(s) trouvé(s)";
                AddLog($"Scan réseau - {buzzers.Count} appareil(s) détecté(s)", LogType.Info);
            });
        }
        catch (Exception ex)
        {
            _dispatcher.Invoke(() =>
            {
                StatusMessage = $"Erreur lors du scan : {ex.Message}";
                AddLog($"Erreur : {ex.Message}", LogType.Error);
            });
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async void ConnectAsync()
    {
        if (_buzzerManager == null || SelectedBuzzer == null)
            return;

        try
        {
            StatusMessage = $"Connexion à {SelectedBuzzer.Name}...";
            var result = await _buzzerManager.ConnectBuzzerAsync(SelectedBuzzer.Id);

            if (result)
            {
                StatusMessage = $"? Connecté à {SelectedBuzzer.Name}";
                AddLog($"Connecté à {SelectedBuzzer.Name}", LogType.Success);
            }
            else
            {
                StatusMessage = $"? Impossible de se connecter à {SelectedBuzzer.Name}";
                AddLog($"Erreur de connexion à {SelectedBuzzer.Name}", LogType.Error);
            }

            OnPropertyChanged(nameof(IsConnectButtonEnabled));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
            AddLog($"Erreur de connexion : {ex.Message}", LogType.Error);
        }
    }

    private async void DisconnectAsync()
    {
        if (_buzzerManager == null || SelectedBuzzer == null)
            return;

        try
        {
            await _buzzerManager.DisconnectBuzzerAsync(SelectedBuzzer.Id);
            StatusMessage = $"Déconnecté de {SelectedBuzzer.Name}";
            AddLog($"Déconnecté de {SelectedBuzzer.Name}", LogType.Info);
            OnPropertyChanged(nameof(IsConnectButtonEnabled));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur de déconnexion : {ex.Message}";
            AddLog($"Erreur de déconnexion : {ex.Message}", LogType.Error);
        }
    }

    private void OnBuzzerEvent(object? sender, BuzzerEvent buzzerEvent)
    {
        _dispatcher.Invoke(() =>
        {
            AddLog($"{buzzerEvent.BuzzerName}: {buzzerEvent.EventType}", LogType.Info);

            // Rafraîchir les commandes sans modifier la collection
            // Cela évite de perdre la sélection SelectedBuzzer
            OnPropertyChanged(nameof(IsConnectButtonEnabled));
            CommandManager.InvalidateRequerySuggested();
        });
    }

    private void AddLog(string message, LogType type)
    {
        var log = new BuzzerEventLog
        {
            Timestamp = DateTime.Now,
            Message = message,
            Type = type
        };
        EventLogs.Insert(0, log);

        // Garder seulement les 100 derniers logs
        while (EventLogs.Count > 100)
            EventLogs.RemoveAt(EventLogs.Count - 1);
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class BuzzerEventLog
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public LogType Type { get; set; }
    public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Message}";
}

public enum LogType
{
    Info,
    Success,
    Error
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);
}
