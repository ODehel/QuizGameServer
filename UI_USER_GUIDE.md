# Guide d'utilisation de l'interface Buzzers

## ?? Présentation générale

L'interface QuizGame Gestionnaire de Buzzers est divisée en 4 sections principales :

### 1. **En-tête** (Haut)
- Titre : "Gestionnaire de Buzzers QuizGame"
- Message de statut en temps réel
- Affiche l'état actuel de l'application

### 2. **Panneau gauche** 
- **Section Scan réseau** : Bouton pour démarrer/arrêter le scan
- **Liste des périphériques** : Affiche tous les buzzers découverts
  - Nom du buzzer
  - Adresse IP
  - Adresse MAC
  - Indicateur de statut (? Connecté, ? Déconnecté, ? Connexion, ? Erreur)

### 3. **Panneau droit**
- **Détails du buzzer sélectionné** :
  - Nom
  - Adresse IP
  - Adresse MAC
  - Port
  - Statut
  - Boutons d'action (Connecter/Déconnecter)
- **Journal d'événements** : Historique de toutes les actions
  - Codes couleur : Info (gris), Succès (vert), Erreur (rouge)

### 4. **Pied de page** (Bas)
- Information de copyright

---

## ?? Guide d'utilisation

### **Étape 1 : Scanner le réseau**

1. Cliquez sur le bouton **"Scan réseau"** dans le panneau gauche
2. L'application affichera le message "Scan en cours..."
3. Attendez le résultat (~2-3 secondes)
4. Les buzzers découverts apparaîtront dans la liste

**Remarque** : Le scan cherche tous les périphériques dont le nom commence par "Quiz"

### **Étape 2 : Sélectionner un buzzer**

1. Cliquez sur un buzzer dans la liste du panneau gauche
2. Les détails du buzzer s'affichent dans le panneau droit
3. Le buzzer est maintenant sélectionné (surligné)

### **Étape 3 : Connecter un buzzer**

1. Assurez-vous qu'un buzzer est sélectionné
2. Cliquez sur le bouton **"Connecter"** (vert)
3. L'état du buzzer change à "? Connexion..." puis à "? Connecté"
4. Un message de succès apparaît dans le journal d'événements

### **Étape 4 : Déconnecter un buzzer**

1. Assurez-vous qu'un buzzer connecté est sélectionné
2. Cliquez sur le bouton **"Déconnecter"** (rouge)
3. L'état du buzzer revient à "? Déconnecté"

### **Étape 5 : Consulter les événements**

1. Tous les événements s'affichent dans le journal (panneau droit bas)
2. Les événements les plus récents apparaissent en haut
3. Utilisez le bouton **"Effacer"** pour vider l'historique

---

## ?? Interprétation des indicateurs

### **Statuts des buzzers**

| Indicateur | Signification | Couleur |
|-----------|---------------|---------|
| ? Connecté | Le buzzer est connecté et actif | Vert (#27AE60) |
| ? Déconnecté | Le buzzer n'est pas connecté | Rouge (#E74C3C) |
| ? Connexion... | Tentative de connexion en cours | Orange (#F39C12) |
| ? Erreur | Une erreur s'est produite | Rouge foncé (#C0392B) |

### **Couleurs des logs**

| Couleur | Type d'événement |
|---------|-----------------|
| Gris | Informations générales |
| Vert | Succès/Connexion établie |
| Rouge | Erreur/Problème |

---

## ?? Types d'événements dans le journal

- **[HH:MM:SS] Scan réseau - X appareil(s) détecté(s)**
  - Scan terminé avec résultats
  
- **[HH:MM:SS] Connecté à NomBuzzer**
  - Connexion établie avec succès
  
- **[HH:MM:SS] Déconnecté de NomBuzzer**
  - Déconnexion effectuée

- **[HH:MM:SS] NomBuzzer: Pressed**
  - Bouton du buzzer appuyé

- **[HH:MM:SS] NomBuzzer: Released**
  - Bouton du buzzer relâché

- **[HH:MM:SS] Erreur : Message d'erreur**
  - Erreur rencontrée lors d'une opération

---

## ?? Fonctionnalités avancées

### **Reconnaissance des buzzers**

L'application scanne automatiquement le réseau sur la plage IP configurée (par défaut : 192.168.1.x).

Pour modifier la plage IP, éditez le fichier `App.xaml.cs` :

```csharp
app.InitializeBuzzerServices("192.168.50");  // Utilise 192.168.50.x
```

### **Limite du journal d'événements**

Le journal conserve les 100 derniers événements. Les plus anciens sont supprimés automatiquement.

### **Sélection automatique**

Après un scan, le premier buzzer découvert est sélectionné automatiquement.

---

## ?? Dépannage

### **Problème : Aucun buzzer n'est détecté**

**Causes possibles :**
- Les buzzers ne sont pas allumés
- Les buzzers ne sont pas sur le même réseau WiFi
- Le pare-feu bloque les pings ICMP
- La plage IP configurée est incorrecte

**Solutions :**
1. Vérifiez que les buzzers sont allumés
2. Vérifiez la connectivité réseau avec `ping`
3. Vérifiez les paramètres du pare-feu
4. Vérifiez la passerelle configurée dans `App.xaml.cs`

### **Problème : Connexion impossible à un buzzer**

**Causes possibles :**
- Le port 5000 n'est pas ouvert sur le buzzer
- Le buzzer ne répond pas aux requêtes TCP
- Problème de connectivité réseau

**Solutions :**
1. Vérifiez que le port 5000 est accessible
2. Essayez de redémarrer le buzzer
3. Vérifiez la connectivité réseau avec `ping <ip_buzzer>`
4. Vérifiez les logs pour des messages d'erreur détaillés

### **Problème : Le bouton "Connecter" est désactivé**

**Cause :** Le buzzer est déjà connecté ou aucun buzzer n'est sélectionné

**Solution :**
1. Sélectionnez un buzzer "Déconnecté"
2. Ou déconnectez d'abord le buzzer avant de reconnecter

---

## ?? Informations affichées par buzzer

Pour chaque buzzer découvert, l'interface affiche :

- **Nom** : Identifiant du buzzer (e.g., "Quiz-01")
- **Adresse IP** : Adresse IP sur le réseau (e.g., "192.168.1.100")
- **Adresse MAC** : Adresse MAC du buzzer (e.g., "00:1A:2B:3C:4D:5E")
- **Port** : Port de communication TCP (par défaut : 5000)
- **Statut** : État de connexion actuel

---

## ?? Conseils d'utilisation

1. **Effectuez un scan avant de commencer** pour découvrir tous les buzzers disponibles
2. **Connectez les buzzers un par un** pour vérifier leur bon fonctionnement
3. **Consultez le journal d'événements** pour diagnostiquer les problèmes
4. **Gardez l'interface ouverte** pendant les jeux pour monitorer les connexions
5. **Vérifiez la passerelle réseau** si aucun buzzer n'est détecté

---

## ?? Support

Pour plus d'informations sur l'intégration des buzzers, consultez `BUZZER_INTEGRATION_GUIDE.md`
