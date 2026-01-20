# Intégration des Buzzers QuizGame

## ?? Objectif
Intégrer les buzzers connectés au réseau Freebox dans l'application QuizGameServer pour permettre une communication bidirectionnelle entre les buzzers et l'application.

## ?? Architecture

### Couches

#### 1. **Domain** (`QuizGame.Domain`)
- Modèles métier : `Buzzer`, `BuzzerEvent`
- Enumerations : `BuzzerStatus`, `BuzzerEventType`
- Pas de dépendances externes

#### 2. **Infrastructure** (`QuizGame.Infrastructure`)
- **Services de découverte** : `IBuzzerDiscoveryService`
  - Scanne le réseau pour découvrir les périphériques
  - Filtre les buzzers par nom (ex: "Quiz")
  
- **Services de communication** : `IBuzzerCommunicationService`
  - Gère les connexions TCP avec les buzzers
  - Envoie et reçoit des messages
  
- **Gestionnaire orchestrateur** : `BuzzerManager`
  - Orchestre la découverte et la communication
  - Génère des événements pour chaque action buzzer

#### 3. **Application** (`QuizGame.Application`)
- Logique métier d'application
- Injection de dépendances via `BuzzerServiceExtensions`

#### 4. **Présentation** (`QuizGame.Presentation.Wpf`)
- Interface utilisateur WPF
- Affichage des buzzers et événements

## ?? Utilisation

### Initialisation (dans `App.xaml.cs`)

```csharp
using Microsoft.Extensions.DependencyInjection;
using QuizGame.Application.DependencyInjection;
using QuizGame.Infrastructure.Services;

namespace QuizGame.Presentation.Wpf;

public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    private BuzzerManager _buzzerManager;

    public App()
    {
        var services = new ServiceCollection();
        
        // Enregistrer les services de buzzer
        // Optionnellement, spécifier la passerelle Freebox (par défaut: 192.168.1)
        services.AddBuzzerServices("192.168.1");
        
        _serviceProvider = services.BuildServiceProvider();
        _buzzerManager = _serviceProvider.GetRequiredService<BuzzerManager>();

        // S'abonner aux événements de buzzer
        _buzzerManager.BuzzerEventOccurred += OnBuzzerEvent;
    }

    private void OnBuzzerEvent(object sender, BuzzerEvent buzzerEvent)
    {
        MainWindow.Dispatcher.Invoke(() =>
        {
            // Mettre à jour l'UI avec l'événement
            // Ex: buzzerEvent.EventType, buzzerEvent.BuzzerName, etc.
        });
    }
}
```

### Découverte de buzzers

```csharp
// Découvrir tous les buzzers commençant par "Quiz"
var buzzers = await _buzzerManager.DiscoverBuzzersAsync("Quiz");

foreach (var buzzer in buzzers)
{
    Console.WriteLine($"Buzzer: {buzzer.Name} ({buzzer.IpAddress})");
}
```

### Connexion à un buzzer

```csharp
var buzzerId = buzzers[0].Id;
var connected = await _buzzerManager.ConnectBuzzerAsync(buzzerId);

if (connected)
{
    Console.WriteLine("Connecté !");
}
```

### Envoi de messages

```csharp
await _buzzerManager.SendMessageAsync(buzzerId, "play_sound");
```

### Écoute des événements

```csharp
_buzzerManager.BuzzerEventOccurred += (sender, buzzerEvent) =>
{
    switch (buzzerEvent.EventType)
    {
        case BuzzerEventType.Connected:
            Console.WriteLine($"{buzzerEvent.BuzzerName} connecté");
            break;
        
        case BuzzerEventType.Pressed:
            Console.WriteLine($"{buzzerEvent.BuzzerName} appuyé");
            break;
            
        case BuzzerEventType.Released:
            Console.WriteLine($"{buzzerEvent.BuzzerName} relâché");
            break;
            
        case BuzzerEventType.Disconnected:
            Console.WriteLine($"{buzzerEvent.BuzzerName} déconnecté");
            break;
    }
};
```

### Obtenir l'état actuel

```csharp
var allBuzzers = _buzzerManager.GetAllBuzzers();

var connectedBuzzers = allBuzzers
    .Where(b => b.Status == BuzzerStatus.Connected)
    .ToList();

Console.WriteLine($"{connectedBuzzers.Count} buzzers connectés");
```

## ?? Protocole de communication buzzer

Les buzzers doivent implémenter un protocole TCP simple :

### Format des messages
- Chaque message se termine par `\n`
- Format: `{event_type}:{data}`

### Événements supportés

**Buzzer ? Serveur:**
- `press` : Bouton appuyé
- `release` : Bouton relâché
- `ping` : Signal de vie

**Serveur ? Buzzer:**
- `play_sound` : Jouer un son
- `led_on` : Allumer la LED
- `led_off` : Éteindre la LED

## ?? Configuration

### Passerelle Freebox personnalisée

Si votre réseau Freebox n'utilise pas `192.168.1.x`:

```csharp
services.AddBuzzerServices("192.168.50");  // Utilise 192.168.50.x
```

### Port de communication

Par défaut, les buzzers utilisent le port `5000`. Pour personnaliser:

```csharp
var buzzer = new Buzzer 
{ 
    Name = "Quiz-01",
    IpAddress = "192.168.1.100",
    Port = 8000  // Port personnalisé
};
```

## ?? Limitations

- Le scan réseau prend ~2-3 secondes par classe C
- La découverte fonctionne sur le même sous-réseau
- Les connexions TCP utilisent un timeout de 5 secondes
- Les messages sont limités à 1024 octets

## ?? Troubleshooting

### Aucun buzzer détecté
1. Vérifier que les buzzers sont alimentés et sur le même réseau
2. Vérifier que le pare-feu n'bloque pas les pings
3. Vérifier la passerelle Freebox configurée

### Connexion impossible
1. Vérifier que le port 5000 est ouvert sur les buzzers
2. Vérifier la connectivité réseau avec `ping`
3. Vérifier les logs de la console

### Messages non reçus
1. Vérifier le format des messages (doit se terminer par `\n`)
2. Vérifier que le buzzer répond aux pings
3. Vérifier que la connexion est établie

## ?? Références

- Domaine: `src/QuizGame.Domain/Entities/Buzzer.cs`
- Infrastructure: `src/QuizGame.Infrastructure/Services/`
- Application: `src/QuizGame.Application/DependencyInjection/`
- Présentation: `src/QuizGame.Presentation.Wpf/`
