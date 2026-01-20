# ?? Index de la documentation

**Navigation rapide vers tous les documents du projet**

---

## ?? Démarrage rapide

1. **Première utilisation ?** ? Commencez par [README_BUZZERS.md](README_BUZZERS.md)
2. **Questions sur l'architecture ?** ? Consultez [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md)
3. **Besoin d'aide pour l'UI ?** ? Lisez [UI_USER_GUIDE.md](UI_USER_GUIDE.md)
4. **Vous êtes perdu ?** ? Ce fichier vous aide !

---

## ?? Documentation par catégorie

### ??? Architecture & Configuration

| Document | Contenu | Lire si... |
|----------|---------|-----------|
| [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md) | Architecture technique complète, services, protocole | Vous êtes développeur ou intégrateur |
| [README_BUZZERS.md](README_BUZZERS.md) | Vue d'ensemble du projet, démarrage, structure | Vous commencez avec le projet |
| [SUMMARY.md](SUMMARY.md) | Résumé de l'implémentation, technos utilisées | Vous voulez un aperçu global |

### ?? Utilisation de l'interface

| Document | Contenu | Lire si... |
|----------|---------|-----------|
| [UI_USER_GUIDE.md](UI_USER_GUIDE.md) | Guide complet de l'interface WPF | Vous utilisez l'application |
| [UI_PREVIEW.md](UI_PREVIEW.md) | Preview visuelle, codes couleur, layout | Vous visualisez l'interface |

### ?? Développement

| Document | Contenu | Lire si... |
|----------|---------|-----------|
| [src/QuizGame.Examples/BuzzerUsageExamples.cs](src/QuizGame.Examples/BuzzerUsageExamples.cs) | 6 exemples de code fonctionnels | Vous écrivez du code |
| [Fichiers de code source](src/) | Code source complet | Vous explorez l'implémentation |

### ?? Dépannage

| Document | Contenu | Lire si... |
|----------|---------|-----------|
| [TROUBLESHOOTING.md](TROUBLESHOOTING.md) | Guide de dépannage avec solutions | Vous avez un problème |
| [CHECKLIST.md](CHECKLIST.md) | Checklist de validation | Vous validez l'implémentation |

---

## ??? Structure des fichiers

```
QuizGameServer/
?
??? src/
?   ??? QuizGame.Domain/
?   ?   ??? Entities/Buzzer.cs          ? Modèle de buzzer
?   ?   ??? Events/BuzzerEvent.cs       ? Événements
?   ?
?   ??? QuizGame.Infrastructure/
?   ?   ??? Services/
?   ?       ??? BuzzerDiscoveryService.cs       ? Scan réseau
?   ?       ??? BuzzerCommunicationService.cs   ? TCP
?   ?       ??? BuzzerManager.cs                ? Orchestration
?   ?
?   ??? QuizGame.Presentation.Wpf/
?   ?   ??? MainWindow.xaml(.cs)                ? UI principale
?   ?   ??? App.xaml(.cs)                       ? Initialisation
?   ?   ??? ViewModels/BuzzerViewModel.cs       ? Logique présentation
?   ?   ??? Converters/ValueConverters.cs       ? Converteurs WPF
?   ?
?   ??? QuizGame.Examples/
?       ??? BuzzerUsageExamples.cs              ? Exemples de code
?
??? Documentation/
?   ??? README_BUZZERS.md                ? Vue d'ensemble
?   ??? BUZZER_INTEGRATION_GUIDE.md      ? Guide technique
?   ??? UI_USER_GUIDE.md                 ? Guide utilisateur
?   ??? UI_PREVIEW.md                    ? Preview UI
?   ??? SUMMARY.md                       ? Résumé
?   ??? TROUBLESHOOTING.md               ? Dépannage
?   ??? CHECKLIST.md                     ? Validation
?   ??? INDEX.md                         ? Ce fichier
?
??? README.md (racine)                   ? Projet principal
```

---

## ?? Trouver des informations

### "Je veux..."

#### **...comprendre l'architecture**
?? [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md) - Section "Architecture"

#### **...utiliser l'application**
?? [UI_USER_GUIDE.md](UI_USER_GUIDE.md) - Section "Guide d'utilisation"

#### **...écrire du code qui utilise les buzzers**
?? [src/QuizGame.Examples/BuzzerUsageExamples.cs](src/QuizGame.Examples/BuzzerUsageExamples.cs)

#### **...configurer l'application pour mon réseau**
?? [README_BUZZERS.md](README_BUZZERS.md) - Section "Configuration"

#### **...dépanner un problème**
?? [TROUBLESHOOTING.md](TROUBLESHOOTING.md)

#### **...voir comment l'interface looks**
?? [UI_PREVIEW.md](UI_PREVIEW.md)

#### **...connaître les détails techniques**
?? [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md) - Section "Protocole de communication"

#### **...démarrer une nouvelle partie du code**
?? [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md) - Section "Points d'extension"

#### **...comprendre le workflow global**
?? [SUMMARY.md](SUMMARY.md)

---

## ?? Documentation par étape

### Étape 1 : Installation & Configuration
1. Lire [README_BUZZERS.md](README_BUZZERS.md) - "Démarrage rapide"
2. Compiler le projet
3. Vérifier [TROUBLESHOOTING.md](TROUBLESHOOTING.md) si erreur

### Étape 2 : Première utilisation
1. Lancer l'application
2. Consulter [UI_USER_GUIDE.md](UI_USER_GUIDE.md)
3. Cliquer "Scan réseau"

### Étape 3 : Intégration personnalisée
1. Lire [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md)
2. Consulter les [exemples](src/QuizGame.Examples/BuzzerUsageExamples.cs)
3. Adapter le code à vos besoins

### Étape 4 : Dépannage
1. Consulter [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. Vérifier la [CHECKLIST.md](CHECKLIST.md)
3. Vérifier les logs de l'application

---

## ?? Cas d'utilisation courants

### Je suis **utilisateur final**
1. Lisez [UI_USER_GUIDE.md](UI_USER_GUIDE.md)
2. Consultez [TROUBLESHOOTING.md](TROUBLESHOOTING.md) si besoin

### Je suis **développeur** qui intègre ce module
1. Lisez [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md)
2. Consultez [src/QuizGame.Examples/BuzzerUsageExamples.cs](src/QuizGame.Examples/BuzzerUsageExamples.cs)
3. Adaptez le code

### Je suis **administrateur réseau**
1. Lisez [README_BUZZERS.md](README_BUZZERS.md) - "Configuration"
2. Configurez la passerelle dans [App.xaml.cs](src/QuizGame.Presentation.Wpf/App.xaml.cs)
3. Vérifiez les pare-feu ([TROUBLESHOOTING.md](TROUBLESHOOTING.md))

### Je suis **testeur QA**
1. Lisez [CHECKLIST.md](CHECKLIST.md)
2. Consultez [UI_PREVIEW.md](UI_PREVIEW.md)
3. Testez avec [TROUBLESHOOTING.md](TROUBLESHOOTING.md) comme guide

---

## ?? Points clés

### Architecture
- 4 couches : Domain ? Infrastructure ? Application ? Presentation
- Pattern MVVM pour WPF
- Services découplés

### Fonctionnalités
- Scan réseau via Ping + DNS + ARP
- Communication TCP bidirectionnelle
- Interface WPF moderne
- Journal d'événements en temps réel

### Configuration
- Passerelle réseau : `App.xaml.cs`
- Port : `Buzzer.cs`
- Filtre : `DiscoverBuzzersAsync("Quiz")`

### Dépannage
- Pas de buzzer ? ? Vérifier ping et pare-feu
- Connexion échoue ? ? Vérifier port 5000
- Interface figée ? ? C'est normal pendant le scan

---

## ?? Liens rapides

- [Vue d'ensemble](README_BUZZERS.md)
- [Guide technique](BUZZER_INTEGRATION_GUIDE.md)
- [Guide utilisateur](UI_USER_GUIDE.md)
- [Dépannage](TROUBLESHOOTING.md)
- [Exemples de code](src/QuizGame.Examples/BuzzerUsageExamples.cs)
- [Source Domain](src/QuizGame.Domain/)
- [Source Infrastructure](src/QuizGame.Infrastructure/)
- [Source Présentation](src/QuizGame.Presentation.Wpf/)

---

## ? FAQ

**Q: Par où je commence ?**  
A: Si vous êtes utilisateur, lisez [UI_USER_GUIDE.md](UI_USER_GUIDE.md). Si vous êtes développeur, lisez [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md).

**Q: Comment configurer pour mon réseau ?**  
A: Éditez `App.xaml.cs` ligne 26 avec votre passerelle réseau.

**Q: Aucun buzzer n'est détecté**  
A: Consultez [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - "Aucun buzzer détecté".

**Q: Comment ajouter une nouvelle fonctionnalité ?**  
A: Lisez [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md) - "Points d'extension".

**Q: Où est le code source ?**  
A: Consultez [README_BUZZERS.md](README_BUZZERS.md) - "Architecture".

---

## ?? Support

1. **Problème** ? ? Consultez [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. **Question technique** ? ? Lisez [BUZZER_INTEGRATION_GUIDE.md](BUZZER_INTEGRATION_GUIDE.md)
3. **Question d'utilisation** ? ? Consultez [UI_USER_GUIDE.md](UI_USER_GUIDE.md)
4. **Code example** ? ? Consultez [BuzzerUsageExamples.cs](src/QuizGame.Examples/BuzzerUsageExamples.cs)

---

## ?? Version

- **Version** : 1.0.0-beta
- **Date** : 2024
- **Statut** : ? Complet et fonctionnel
- **Target** : .NET 10

---

**Bienvenue dans QuizGame Buzzer Manager ! ??**

*Commencez par le document approprié pour votre rôle.*
