# ?? Guide de dépannage complet

## ?? Problèmes courants et solutions

---

## 1?? **Problème : L'application ne démarre pas**

### Symptômes
- Message d'erreur au lancement
- Fenêtre ne s'ouvre pas
- Application plante

### Causes possibles et solutions

#### ? .NET 10 non installé
```
Erreur: It was not possible to find any installed .NET Core SDKs
```
**Solution:**
```bash
# Vérifier la version installée
dotnet --version

# Installer .NET 10
# Télécharger depuis https://dotnet.microsoft.com/download
```

#### ? Erreur de compilation
```
CS0103: The name 'xxx' does not exist in the current context
```
**Solutions:**
1. Nettoyer et reconstruire
```bash
dotnet clean
dotnet build
```

2. Restaurer les packages
```bash
dotnet restore
```

#### ? Erreur de référence de projet
```
CS1061: 'XXX' does not contain a definition for 'YYY'
```
**Solution:**
Vérifier les références dans les `.csproj`:
```xml
<ItemGroup>
  <ProjectReference Include="..\QuizGame.Infrastructure\QuizGame.Infrastructure.csproj" />
</ItemGroup>
```

---

## 2?? **Problème : Aucun buzzer n'est détecté**

### Symptômes
- Clic sur "Scan réseau" ? Message "0 buzzer trouvé(s)"
- Liste vide
- Journal affiche pas d'erreur

### Causes possibles et solutions

#### ? Buzzers pas allumés
**Vérification:**
```bash
# Ping les buzzers manuellement
ping 192.168.1.100
ping 192.168.1.101
```
**Solution:** Allumer les buzzers

#### ? Buzzers sur un réseau différent
**Vérification:**
```bash
# Afficher les adresses IP disponibles
ipconfig
```
**Solution:** Mettre à jour la passerelle dans `App.xaml.cs`:
```csharp
app.InitializeBuzzerServices("10.0.0");  // Votre réseau
```

#### ? Pare-feu bloque les pings
**Vérification:**
```bash
# Essayer avec un autre outil
nmap 192.168.1.0/24
```
**Solution:** Autoriser ICMP dans le pare-feu Windows
```bash
# PowerShell (Admin)
New-NetFirewallRule -DisplayName "Allow ICMP IPv4" -Protocol icmpv4 -Enabled True -RemoteAddress Any
```

#### ? Plage IP incorrecte
**Solution:** Vérifier la plage dans les logs et ajuster:
```csharp
// Avant
services.AddBuzzerServices("192.168.1");
// Après
services.AddBuzzerServices("192.168.50");
```

#### ? DNS ne résout pas les noms
**Solution:** Ignorer cet avertissement - l'IP est suffisante

---

## 3?? **Problème : Connexion impossible à un buzzer**

### Symptômes
- Clic sur "Connecter" ? Message "Impossible de se connecter"
- Statut reste "? Connexion..."
- Journal affiche erreur de connexion

### Causes possibles et solutions

#### ? Port 5000 non accessible
**Vérification:**
```bash
# Tester la connexion TCP
Test-NetConnection -ComputerName 192.168.1.100 -Port 5000
# ou
telnet 192.168.1.100 5000
```
**Solutions:**
1. Vérifier que le buzzer écoute le port 5000
2. Modifier le port dans `Buzzer.cs`:
```csharp
public int Port { get; set; } = 8000;  // Port personnalisé
```

#### ? Buzzer n'est pas accessible
**Vérification:**
```bash
# Ping le buzzer
ping 192.168.1.100
# Essayer SSH/Telnet
ssh root@192.168.1.100
```
**Solution:** Redémarrer le buzzer

#### ? Timeout de connexion
**Symptôme:** Attendre 5 secondes avant l'erreur
**Solution:** Augmenter le timeout dans `BuzzerCommunicationService.cs`:
```csharp
// Avant
var timeoutTask = Task.Delay(5000, cts.Token);
// Après
var timeoutTask = Task.Delay(10000, cts.Token);  // 10 secondes
```

#### ? Firewall local bloque la connexion
**Solution:** Autoriser l'application
```bash
# PowerShell (Admin)
New-NetFirewallRule -DisplayName "QuizGame" -Program "C:\path\to\QuizGame.Presentation.Wpf.exe" -Action Allow
```

---

## 4?? **Problème : L'interface se fige pendant le scan**

### Symptômes
- Application devient non-réactive pendant 2-3 secondes
- Boutons non cliquables
- Impossible d'arrêter le scan

### Cause
Le scan réseau s'exécute sur le thread principal

### Solutions

#### Solution 1 : Patienter
C'est normal - attendez la fin du scan (2-3 secondes)

#### Solution 2 : Réduire la plage IP
Scannez uniquement les IPs où vous savez qu'il y a des buzzers:
```csharp
// Avant
for (int i = 1; i <= 254; i++)

// Après
for (int i = 100; i <= 105; i++)  // Seulement 5 adresses
```

#### Solution 3 : Augmenter le timeout des pings
```csharp
var reply = await ping.SendPingAsync(ipAddress, 100);  // 100ms au lieu de 500ms
```

---

## 5?? **Problème : Messages ne sont pas reçus**

### Symptômes
- Buzzer connecté mais pas d'événements
- Journal vide après connexion
- Pas de "Pressed" ou "Released"

### Causes possibles

#### ? Format des messages incorrect
**Format attendu:** `{message}\n`
**Exemple:** `press\n`

**Solution:** Vérifier le format des messages envoyés par le buzzer

#### ? Port d'écoute incorrect
**Solution:** Vérifier que le buzzer envoie sur le bon port

#### ? Encodage différent
**Solution:** Vérifier l'encodage UTF-8 dans le buzzer

#### ? Connecté mais pas connecté réellement
**Solution:** Ajouter un ping de vérification:
```csharp
var sent = await buzzerManager.SendMessageAsync(buzzerId, "ping");
if (!sent)
{
    Console.WriteLine("Connexion perdue");
}
```

---

## 6?? **Problème : Adresse MAC affiche "00:00:00:00:00:00"**

### Symptôme
Tous les buzzers affichent une MAC par défaut

### Cause
La récupération de MAC via ARP échoue

### Solutions

#### ? C'est normal
L'IP est suffisante pour se connecter

#### Cause possible : ARP ne fonctionne pas
```bash
# Tester ARP manuellement
arp -a 192.168.1.100
```

#### Solution
Ignorer la MAC - elle n'est pas critique pour la connexion

---

## 7?? **Problème : Journal d'événements plein / ralentissement**

### Symptômes
- Interface ralentit après plusieurs heures
- Scrolling lent du journal
- Utilisation mémoire augmente

### Cause
Trop d'événements stockés

### Solution
Le système nettoie automatiquement (max 100 événements), mais vous pouvez :

1. **Effacer manuellement** : Bouton "Effacer" dans l'UI
2. **Réduire la limite** : Éditer `BuzzerViewModel.cs`
```csharp
// Avant
while (EventLogs.Count > 100)

// Après
while (EventLogs.Count > 50)  // Limite réduite
```

---

## 8?? **Problème : Exception en temps d'exécution**

### Exemple d'erreur
```
ObjectDisposedException: Cannot access a disposed object
```

### Cause
Accès à un objet après Dispose()

### Solution
S'assurer que les services sont correctement initialisés avant utilisation

---

## 9?? **Problème : L'application consomme beaucoup de CPU**

### Symptômes
- Utilisation CPU élevée (>50%)
- Ventilateur bruyant
- Batterie se vide rapidement

### Causes possibles

#### ? Boucle de scan infinie
**Solution:** Cliquer sur "Arrêter le scan..."

#### ? Trop de tentatives de reconnexion
**Solution:** Réduire la fréquence des tentatives

#### ? Événements reçus trop rapidement
**Solution:** Ajouter un délai entre les traitements

---

## ?? **Problème : Erreurs de sécurité / Access Denied**

### Symptôme
```
System.UnauthorizedAccessException
System.InvalidOperationException: Access Denied
```

### Causes possibles

#### ? Pas de droits administrateur
**Solution:** Lancer l'application en mode administrateur

#### ? Pare-feu bloque l'application
**Solution:** Autoriser dans Windows Defender
```bash
# PowerShell (Admin)
netsh advfirewall firewall add rule name="QuizGame" dir=in action=allow program="C:\path\app.exe"
```

#### ? Port < 1024 (réservé)
**Solution:** Utiliser un port > 1024 dans `Buzzer.cs`
```csharp
public int Port { get; set; } = 5000;  // OK (> 1024)
```

---

## ?? Checklist de dépannage

Avant de chercher ailleurs, vérifier :

- [ ] .NET 10 est installé
- [ ] L'application compile sans erreur
- [ ] Les buzzers sont allumés
- [ ] Les buzzers sont sur le même réseau
- [ ] La passerelle réseau est correcte
- [ ] Le pare-feu autorise les pings ICMP
- [ ] Le pare-feu autorise les connexions TCP port 5000
- [ ] Au moins un buzzer répond aux pings
- [ ] Les buzzers ont les noms commençant par "Quiz"
- [ ] L'application a les droits administrateur (si nécessaire)

---

## ??? Commandes de diagnostic

### Vérifier la configuration réseau
```bash
ipconfig /all
```

### Tester la connectivité
```bash
ping 192.168.1.100
ping 192.168.1.101
```

### Vérifier l'accessibilité des ports
```bash
Test-NetConnection -ComputerName 192.168.1.100 -Port 5000
```

### Scanner le réseau
```bash
nmap 192.168.1.0/24
arp -a
```

### Consulter les logs du pare-feu Windows
```bash
Get-NetFirewallLog -LogFileName "C:\Windows\System32\LogFiles\Firewall\pfirewall.log"
```

---

## ?? Si le problème persiste

1. **Consultez les logs** dans le journal d'événements
2. **Vérifiez la documentation** : BUZZER_INTEGRATION_GUIDE.md
3. **Testez avec les exemples** : `BuzzerUsageExamples.cs`
4. **Vérifiez les configurations** : 
   - `App.xaml.cs`
   - `Buzzer.cs`
   - `BuzzerDiscoveryService.cs`

---

## ?? Ressources utiles

- **Test connectivité** : https://learn.microsoft.com/en-us/windows/win32/winsock/testing-winsock-software
- **Pare-feu Windows** : https://learn.microsoft.com/en-us/windows/security/threat-protection/windows-firewall
- **.NET Documentation** : https://learn.microsoft.com/en-us/dotnet/
- **WPF Documentation** : https://learn.microsoft.com/en-us/dotnet/desktop/wpf/

---

**Dernière mise à jour** : 2024  
**Pour support** : Consultez les guides de documentation
