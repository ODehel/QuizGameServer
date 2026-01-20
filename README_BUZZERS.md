# ?? QuizGame Server - Gestionnaire de Buzzers

Application WPF .NET 10 pour découvrir et gérer les buzzers connectés à un réseau Freebox.

## ?? Fonctionnalités

? **Scan réseau WiFi** - Détecte automatiquement tous les buzzers ("Quiz*")  
? **Gestion des connexions** - Connecte/déconnecte les buzzers via TCP  
? **Interface WPF moderne** - UI intuitive avec statut en temps réel  
? **Journal d'événements** - Historique complet de toutes les actions  
? **Détails des périphériques** - Affiche IP, MAC, port, statut  

## ?? Architecture

```
QuizGame.Domain/
??? Entities/
?   ??? Buzzer.cs                 # Modèle de buzzer
??? Events/
    ??? BuzzerEvent.cs            # Événements buzzer

QuizGame.Infrastructure/
??? Services/
?   ??? IBuzzerDiscoveryService   # Interface de scan
?   ??? BuzzerDiscoveryService    # Implémentation du scan
?   ??? IBuzzerCommunicationService  # Interface TCP
?   ??? BuzzerCommunicationService   # Implémentation TCP
?   ??? BuzzerManager             # Orchestrateur

QuizGame.Application/
??? (Logique métier)

QuizGame.Presentation.Wpf/
??? MainWindow.xaml(.cs)          # Interface principale
??? App.xaml(.cs)                 # Initialisation
??? ViewModels/
?   ??? BuzzerViewModel.cs        # Logique de présentation
??? Converters/
    ??? ValueConverters.cs        # Converteurs de données
```

## ?? Démarrage rapide

### 1. Compilation

```bash
dotnet build
```

### 2. Exécution

```bash
cd src/QuizGame.Presentation.Wpf
dotnet run
```

### 3. Utilisation

1. Cliquez sur **"Scan réseau"** pour découvrir les buzzers
2. Sélectionnez un buzzer dans la liste
3. Cliquez sur **"Connecter"** pour établir une connexion
4. Consultez le journal d'événements pour les détails

## ?? Détail des couches

### Domain
Contient les modèles métier sans dépendances externes.

**Classes principales :**
- `Buzzer` : Représente un périphérique buzzer
- `BuzzerEvent` : Événement déclenché par un buzzer
- `BuzzerStatus` : État de connexion (Disconnected, Connecting, Connected, Error)

### Infrastructure
Implémente la découverte réseau et la communication TCP.

**Services :**
- `BuzzerDiscoveryService` : Scanne le réseau via ping/ARP
- `BuzzerCommunicationService` : Gère les connexions TCP
- `BuzzerManager` : Orchestrateur central

### Application
Contient la logique métier de haut niveau.

### Presentation.Wpf
Interface utilisateur WPF.

**Composants :**
- `BuzzerViewModel` : Logique de présentation (MVVM)
- `MainWindow` : Fenêtre principale
- Converteurs de données pour la liaison WPF

## ?? Configuration

### Passerelle réseau

Par défaut, l'application scanne `192.168.1.x`. Pour modifier :

**Fichier :** `src/QuizGame.Presentation.Wpf/App.xaml.cs`

```csharp
app.InitializeBuzzerServices("192.168.50");  // Scanne 192.168.50.x
```

### Port de communication

Chaque buzzer utilise le port **5000** par défaut. Pour modifier :

**Fichier :** `src/QuizGame.Domain/Entities/Buzzer.cs`

```csharp
public int Port { get; set; } = 8000;  // Changez le port par défaut
```

## ?? Protocole de communication

Les buzzers communiquent via TCP avec des messages texte :

**Format :** `{event}\n`

**Événements Buzzer ? Serveur :**
- `press` : Bouton appuyé
- `release` : Bouton relâché
- `ping` : Signal de vie

**Événements Serveur ? Buzzer :**
- `play_sound` : Jouer un son
- `led_on` : Allumer la LED
- `led_off` : Éteindre la LED

## ?? Fichiers de documentation

- **`BUZZER_INTEGRATION_GUIDE.md`** : Guide d'intégration technique complet
- **`UI_USER_GUIDE.md`** : Guide d'utilisation de l'interface
- **`README.md`** : Ce fichier

## ?? Dépannage

### Aucun buzzer détecté
- Vérifiez que les buzzers sont alimentés
- Vérifiez que le pare-feu autorise les pings ICMP
- Vérifiez la passerelle configurée

### Connexion impossible
- Vérifiez que le port 5000 est accessible
- Essayez de redémarrer le buzzer
- Vérifiez la connectivité avec `ping <ip_buzzer>`

### L'interface ne démarre pas
- Vérifiez que .NET 10 est installé
- Consultez les logs dans la console pour les erreurs détaillées

## ?? Dépendances

- **.NET 10**
- **WPF** (inclus dans .NET)
- **Aucun package NuGet externe** pour le moment

## ?? Points d'extension

L'architecture modulaire permet d'ajouter facilement :

1. **Autres types de découverte** : Implémenter une autre `IBuzzerDiscoveryService`
2. **Autres protocoles de communication** : Implémenter une autre `IBuzzerCommunicationService`
3. **Persistance** : Ajouter une couche de base de données
4. **API REST** : Ajouter un service web pour l'accès distant
5. **Authentification** : Ajouter une couche d'authentification

## ?? Licence

Ce projet est parte du QuizGameServer.

## ?? Contributeurs

- Olivier Dehel (ODehel)

## ?? Support

Pour toute question ou problème :
1. Consultez les guides de documentation
2. Vérifiez les logs du journal d'événements
3. Vérifiez les paramètres de configuration

---

**Dernière mise à jour** : 2024  
**Version** : 1.0.0-beta  
**Status** : ? Fonctionnel
