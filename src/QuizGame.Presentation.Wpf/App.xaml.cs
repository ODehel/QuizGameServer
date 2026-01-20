using System.Configuration;
using System.Data;
using System.Windows;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Presentation.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private BuzzerDiscoveryService? _discoveryService;
    private BuzzerCommunicationService? _communicationService;
    public BuzzerManager? BuzzerManager { get; private set; }

    public void InitializeBuzzerServices(string? freeboxGateway = null)
    {
        try
        {
            // Initialiser les services d'infrastructure
            _discoveryService = new BuzzerDiscoveryService(freeboxGateway ?? "192.168.1");
            _communicationService = new BuzzerCommunicationService();

            // Créer le gestionnaire de buzzers
            BuzzerManager = new BuzzerManager(_discoveryService, _communicationService);

            // S'abonner aux événements de buzzer
            BuzzerManager.BuzzerEventOccurred += OnBuzzerEvent;

            MessageBox.Show("Services de buzzer initialisés avec succès", "Succès");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de l'initialisation des services: {ex.Message}", "Erreur");
        }
    }

    private void OnBuzzerEvent(object? sender, QuizGame.Domain.Events.BuzzerEvent buzzerEvent)
    {
        Dispatcher.Invoke(() =>
        {
            // Logs pour le debugging
            System.Diagnostics.Debug.WriteLine(
                $"[{buzzerEvent.EventType}] {buzzerEvent.BuzzerName} @ {buzzerEvent.Timestamp:HH:mm:ss.fff}"
            );

            // Ajouter la logique de mise à jour UI ici
            // Ex: MainWindow.UpdateBuzzerDisplay(buzzerEvent);
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        BuzzerManager?.Dispose();
        base.OnExit(e);
    }
}

