using QuizGame.Domain.Entities;
using QuizGame.Infrastructure.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace QuizGame.Presentation.Wpf.ViewModels;

public class TestBuzzersViewModel : INotifyPropertyChanged
{
    private BuzzerManager? _buzzerManager;
    private string _statusMessage = "Prêt à tester";
    private bool _isTesting;
    private readonly Dispatcher _dispatcher;

    public ObservableCollection<Buzzer> ConnectedBuzzers { get; }
    public ObservableCollection<TestLog> TestLogs { get; }

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

    public bool IsTesting
    {
        get => _isTesting;
        set
        {
            if (_isTesting != value)
            {
                _isTesting = value;
                OnPropertyChanged(nameof(IsTesting));
                OnPropertyChanged(nameof(IsTestButtonEnabled));
            }
        }
    }

    public bool IsTestButtonEnabled => !IsTesting;

    public ICommand TestCommunicationCommand { get; }
    public ICommand StartQuizCommand { get; }
    public ICommand ClearLogsCommand { get; }
    public ICommand RenameBuzzerCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public TestBuzzersViewModel()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
        ConnectedBuzzers = new ObservableCollection<Buzzer>();
        TestLogs = new ObservableCollection<TestLog>();

        TestCommunicationCommand = new RelayCommand(_ => TestCommunicationAsync(), _ => IsTestButtonEnabled);
        StartQuizCommand = new RelayCommand(_ => StartQuizAsync(), _ => IsTestButtonEnabled);
        ClearLogsCommand = new RelayCommand(_ => TestLogs.Clear());
        RenameBuzzerCommand = new RelayCommand(async (parameter) => await RenameBuzzerAsync(parameter as Buzzer), _ => true);
    }

    public void Initialize(BuzzerManager buzzerManager)
    {
        _buzzerManager = buzzerManager;
        RefreshConnectedBuzzers();
    }

    public void RefreshConnectedBuzzers()
    {
        if (_buzzerManager == null)
            return;

        _dispatcher.Invoke(() =>
        {
            var allBuzzers = _buzzerManager.GetAllBuzzers();
            var connected = allBuzzers.Where(b => b.Status == BuzzerStatus.Connected).ToList();

            ConnectedBuzzers.Clear();
            foreach (var buzzer in connected)
            {
                ConnectedBuzzers.Add(buzzer);
            }

            StatusMessage = $"{connected.Count} buzzer(s) connecté(s)";
        });
    }

    private async void TestCommunicationAsync()
    {
        if (_buzzerManager == null || ConnectedBuzzers.Count == 0)
        {
            StatusMessage = "Aucun buzzer connecté";
            return;
        }

        IsTesting = true;
        StatusMessage = "Test de communication en cours...";
        AddLog($"Envoi simultané de TestCommunicationBluetooth à {ConnectedBuzzers.Count} buzzer(s)", TestLogType.Info);

        try
        {
            // Créer les tâches d'envoi pour tous les buzzers
            var sendTasks = ConnectedBuzzers.Select(async buzzer =>
            {
                try
                {
                    var result = await _buzzerManager.SendMessageAsync(buzzer.Id, "TestCommunicationBluetooth");
                    return new { buzzer, result };
                }
                catch (Exception ex)
                {
                    AddLog($"? {buzzer.Name} ({buzzer.IpAddress}) - Erreur: {ex.Message}", TestLogType.Error);
                    return new { buzzer, result = false };
                }
            }).ToList();

            // Envoyer tous les messages simultanément
            var results = await Task.WhenAll(sendTasks);

            int successCount = 0;
            int failureCount = 0;

            foreach (var item in results)
            {
                if (item.result)
                {
                    AddLog($"? {item.buzzer.Name} ({item.buzzer.IpAddress}) - Réponse reçue", TestLogType.Success);
                    successCount++;
                }
                else
                {
                    AddLog($"? {item.buzzer.Name} ({item.buzzer.IpAddress}) - Pas de réponse", TestLogType.Error);
                    failureCount++;
                }
            }

            StatusMessage = $"Test terminé: {successCount} réussi(s), {failureCount} échoué(s)";
            AddLog($"Test de communication - Résultat: {successCount}/{ConnectedBuzzers.Count} réussi(s)", 
                   failureCount == 0 ? TestLogType.Success : TestLogType.Error);
        }
        finally
        {
            IsTesting = false;
        }
    }

    private async void StartQuizAsync()
    {
        if (_buzzerManager == null || ConnectedBuzzers.Count == 0)
        {
            StatusMessage = "Aucun buzzer connecté";
            return;
        }

        IsTesting = true;
        StatusMessage = "Démarrage du quiz en cours...";
        AddLog($"Envoi simultané de DebutQuiz à {ConnectedBuzzers.Count} buzzer(s)", TestLogType.Info);

        try
        {
            // Créer les tâches d'envoi pour tous les buzzers
            var sendTasks = ConnectedBuzzers.Select(async buzzer =>
            {
                try
                {
                    var result = await _buzzerManager.SendMessageAsync(buzzer.Id, "DebutQuiz");
                    return new { buzzer, result };
                }
                catch (Exception ex)
                {
                    AddLog($"? {buzzer.Name} ({buzzer.IpAddress}) - Erreur: {ex.Message}", TestLogType.Error);
                    return new { buzzer, result = false };
                }
            }).ToList();

            // Envoyer tous les messages simultanément
            var results = await Task.WhenAll(sendTasks);

            int successCount = 0;
            int failureCount = 0;

            foreach (var item in results)
            {
                if (item.result)
                {
                    AddLog($"? {item.buzzer.Name} ({item.buzzer.IpAddress}) - Réponse reçue", TestLogType.Success);
                    successCount++;
                }
                else
                {
                    AddLog($"? {item.buzzer.Name} ({item.buzzer.IpAddress}) - Pas de réponse", TestLogType.Error);
                    failureCount++;
                }
            }

            StatusMessage = $"Quiz démarré: {successCount} réussi(s), {failureCount} échoué(s)";
            AddLog($"Démarrage du quiz - Résultat: {successCount}/{ConnectedBuzzers.Count} réussi(s)", 
                   failureCount == 0 ? TestLogType.Success : TestLogType.Error);
        }
        finally
        {
            IsTesting = false;
        }
    }

    private void AddLog(string message, TestLogType type)
    {
        _dispatcher.Invoke(() =>
        {
            var log = new TestLog
            {
                Timestamp = DateTime.Now,
                Message = message,
                Type = type
            };
            TestLogs.Insert(0, log);

            // Garder seulement les 100 derniers logs
            while (TestLogs.Count > 100)
                TestLogs.RemoveAt(TestLogs.Count - 1);
        });
    }

    private async Task RenameBuzzerAsync(Buzzer? buzzer)
    {
        if (buzzer == null || _buzzerManager == null)
            return;

        // Vérifier si le nom a changé
        var newName = buzzer.Name;
        if (string.IsNullOrWhiteSpace(newName))
            return;

        try
        {
            AddLog($"Renommage de {buzzer.MacAddress} en '{newName}'...", TestLogType.Info);
            var result = await _buzzerManager.SetBuzzerNameAsync(buzzer.Id, newName);

            if (result)
            {
                AddLog($"? {buzzer.IpAddress} renommé en '{newName}'", TestLogType.Success);
            }
            else
            {
                AddLog($"? Erreur: impossible de renommer {buzzer.IpAddress}", TestLogType.Error);
            }
        }
        catch (Exception ex)
        {
            AddLog($"? Erreur lors du renommage de {buzzer.IpAddress}: {ex.Message}", TestLogType.Error);
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TestLog
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
    public TestLogType Type { get; set; }
    public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Message}";
}

public enum TestLogType
{
    Info,
    Success,
    Error
}
