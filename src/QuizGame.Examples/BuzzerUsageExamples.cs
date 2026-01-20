using QuizGame.Domain.Entities;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Examples;

/// <summary>
/// Exemples d'utilisation des services de buzzer
/// </summary>
public class BuzzerUsageExamples
{
    /// <summary>
    /// Exemple 1 : Scan réseau simple
    /// </summary>
    public static async Task Example1_SimpleNetworkScan()
    {
        var discoveryService = new BuzzerDiscoveryService("192.168.1");
        
        // Découvrir les buzzers commençant par "Quiz"
        var buzzers = await discoveryService.DiscoverBuzzersAsync("Quiz");
        
        Console.WriteLine($"Buzzers découverts : {buzzers.Count}");
        foreach (var buzzer in buzzers)
        {
            Console.WriteLine($"  - {buzzer.Name} ({buzzer.IpAddress})");
        }
    }

    /// <summary>
    /// Exemple 2 : Connexion et communication
    /// </summary>
    public static async Task Example2_ConnectAndCommunicate()
    {
        var discoveryService = new BuzzerDiscoveryService("192.168.1");
        var communicationService = new BuzzerCommunicationService();
        
        // Découvrir les buzzers
        var buzzers = await discoveryService.DiscoverBuzzersAsync("Quiz");
        if (buzzers.Count == 0)
        {
            Console.WriteLine("Aucun buzzer trouvé");
            return;
        }
        
        var buzzer = buzzers.First();
        
        // S'abonner aux événements
        communicationService.MessageReceived += (s, args) =>
        {
            Console.WriteLine($"Message reçu du buzzer {args.BuzzerId}: {args.Message}");
        };
        
        communicationService.StatusChanged += (s, args) =>
        {
            Console.WriteLine($"Statut du buzzer {args.BuzzerId}: {args.OldStatus} -> {args.NewStatus}");
        };
        
        // Se connecter
        var connected = await communicationService.ConnectAsync(buzzer);
        if (connected)
        {
            Console.WriteLine("Connecté au buzzer");
            
            // Envoyer un message
            await communicationService.SendMessageAsync(buzzer.Id, "play_sound");
            
            // Attendre un peu
            await Task.Delay(2000);
            
            // Déconnecter
            await communicationService.DisconnectAsync(buzzer.Id);
        }
        else
        {
            Console.WriteLine("Impossible de se connecter");
        }
        
        communicationService.Dispose();
    }

    /// <summary>
    /// Exemple 3 : Utiliser le BuzzerManager
    /// </summary>
    public static async Task Example3_UseBuzzerManager()
    {
        var discoveryService = new BuzzerDiscoveryService("192.168.1");
        var communicationService = new BuzzerCommunicationService();
        var buzzerManager = new BuzzerManager(discoveryService, communicationService);
        
        // S'abonner aux événements
        buzzerManager.BuzzerEventOccurred += (s, buzzerEvent) =>
        {
            Console.WriteLine($"[{buzzerEvent.EventType}] {buzzerEvent.BuzzerName} @ {buzzerEvent.Timestamp:HH:mm:ss}");
        };
        
        // Découvrir les buzzers
        Console.WriteLine("Scan en cours...");
        var buzzers = await buzzerManager.DiscoverBuzzersAsync("Quiz");
        Console.WriteLine($"Trouvé {buzzers.Count} buzzer(s)");
        
        // Connecter chaque buzzer
        foreach (var buzzer in buzzers)
        {
            var result = await buzzerManager.ConnectBuzzerAsync(buzzer.Id);
            Console.WriteLine($"{buzzer.Name}: {'Connecté' if result else 'Erreur'}");
        }
        
        // Lister les buzzers gérés
        var allBuzzers = buzzerManager.GetAllBuzzers();
        Console.WriteLine($"\nBuzzers gérés: {allBuzzers.Count}");
        foreach (var buzzer in allBuzzers)
        {
            Console.WriteLine($"  - {buzzer.Name}: {buzzer.Status}");
        }
        
        buzzerManager.Dispose();
    }

    /// <summary>
    /// Exemple 4 : Gestion des erreurs et timeouts
    /// </summary>
    public static async Task Example4_ErrorHandling()
    {
        var discoveryService = new BuzzerDiscoveryService("192.168.1");
        var communicationService = new BuzzerCommunicationService();
        
        try
        {
            var buzzers = await discoveryService.DiscoverBuzzersAsync("Quiz");
            
            if (buzzers.Count == 0)
            {
                Console.WriteLine("Aucun buzzer découvert - vérifiez la passerelle réseau");
                return;
            }
            
            var buzzer = buzzers.First();
            
            // Essayer de se connecter avec gestion d'erreur
            var connected = await communicationService.ConnectAsync(buzzer);
            if (!connected)
            {
                Console.WriteLine($"Impossible de se connecter à {buzzer.Name}");
                Console.WriteLine($"IP: {buzzer.IpAddress}:{buzzer.Port}");
                return;
            }
            
            // Vérifier la connexion avant d'envoyer
            if (communicationService is BuzzerCommunicationService svc)
            {
                var messageSent = await communicationService.SendMessageAsync(buzzer.Id, "test");
                if (!messageSent)
                {
                    Console.WriteLine("Impossible d'envoyer le message - connexion perdue");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}");
        }
        finally
        {
            communicationService.Dispose();
        }
    }

    /// <summary>
    /// Exemple 5 : Scan avec timeout personnalisé
    /// </summary>
    public static async Task Example5_ScanWithCustomSettings()
    {
        // Pour une autre passerelle réseau
        var discoveryService = new BuzzerDiscoveryService("10.0.0");
        
        // Scan complet
        var allDevices = await discoveryService.ScanNetworkAsync();
        Console.WriteLine($"Appareils trouvés: {allDevices.Count}");
        
        // Scan filtrés
        var quizBuzzers = await discoveryService.DiscoverBuzzersAsync("Quiz");
        var testDevices = await discoveryService.DiscoverBuzzersAsync("Test");
        
        Console.WriteLine($"Buzzers 'Quiz': {quizBuzzers.Count}");
        Console.WriteLine($"Appareils 'Test': {testDevices.Count}");
    }

    /// <summary>
    /// Exemple 6 : Monitoring avec polling
    /// </summary>
    public static async Task Example6_MonitoringWithPolling()
    {
        var discoveryService = new BuzzerDiscoveryService("192.168.1");
        var communicationService = new BuzzerCommunicationService();
        var buzzerManager = new BuzzerManager(discoveryService, communicationService);
        
        // Scan initial
        var buzzers = await buzzerManager.DiscoverBuzzersAsync("Quiz");
        Console.WriteLine($"Découverte initiale: {buzzers.Count} buzzer(s)");
        
        // Boucle de monitoring
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(5000);
            
            Console.WriteLine($"\n--- Vérification {i + 1} ---");
            var currentBuzzers = buzzerManager.GetAllBuzzers();
            var connectedCount = currentBuzzers.Count(b => b.Status == BuzzerStatus.Connected);
            var disconnectedCount = currentBuzzers.Count(b => b.Status == BuzzerStatus.Disconnected);
            
            Console.WriteLine($"Connectés: {connectedCount}, Déconnectés: {disconnectedCount}");
        }
        
        buzzerManager.Dispose();
    }
}

/// <summary>
/// Programme de test interactif
/// </summary>
public class InteractiveTester
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== QuizGame Buzzer Manager Test ===\n");
        
        Console.WriteLine("Sélectionnez un exemple à exécuter:");
        Console.WriteLine("1. Scan réseau simple");
        Console.WriteLine("2. Connexion et communication");
        Console.WriteLine("3. Utiliser BuzzerManager");
        Console.WriteLine("4. Gestion des erreurs");
        Console.WriteLine("5. Scan avec paramètres personnalisés");
        Console.WriteLine("6. Monitoring avec polling");
        
        var choice = Console.ReadLine();
        
        try
        {
            switch (choice)
            {
                case "1":
                    await BuzzerUsageExamples.Example1_SimpleNetworkScan();
                    break;
                case "2":
                    await BuzzerUsageExamples.Example2_ConnectAndCommunicate();
                    break;
                case "3":
                    await BuzzerUsageExamples.Example3_UseBuzzerManager();
                    break;
                case "4":
                    await BuzzerUsageExamples.Example4_ErrorHandling();
                    break;
                case "5":
                    await BuzzerUsageExamples.Example5_ScanWithCustomSettings();
                    break;
                case "6":
                    await BuzzerUsageExamples.Example6_MonitoringWithPolling();
                    break;
                default:
                    Console.WriteLine("Choix invalide");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        
        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
