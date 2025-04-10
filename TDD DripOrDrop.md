# Technical Design Document (TDD) \- **Drip or Drop (DoD)**

## **Sommaire :**

[**1\. Présentation Générale**](#1.-présentation-générale)

&nbsp;&nbsp;&nbsp;&nbsp;[1.1 Objectif du Document](#1.1-objectif-du-document)

&nbsp;&nbsp;&nbsp;&nbsp;[1.2 Plateformes Ciblées](#1.2-plateformes-ciblées)

&nbsp;&nbsp;&nbsp;&nbsp;[1.3 Architecture Générale](#1.3-architecture-générale)

[**2\. Architecture Logicielle**](#2.-architecture-logicielle)

&nbsp;&nbsp;&nbsp;&nbsp;[2.1 Structure du Projet](#2.1-structure-du-projet)

[**3\. Interface Utilisateur et Système d’Inputs (GUI, HUD, Responsive Design)**](#3.-interface-utilisateur-et-système-d’inputs-\(gui,-hud,-responsive-design\))

&nbsp;&nbsp;&nbsp;&nbsp;[3.1 Inputs](#3.1-inputs)

&nbsp;&nbsp;&nbsp;&nbsp;[3.2 GUI et HUD](#3.2-gui-et-hud)

&nbsp;&nbsp;&nbsp;&nbsp;[3.3 Responsive Design et Adaptation mobile/tablette](#3.3-responsive-design-et-adaptation-mobile/tablette)

[**4\. Gestion des Sessions et Matchmaking**](#4.-gestion-des-sessions-et-matchmaking)

&nbsp;&nbsp;&nbsp;&nbsp;[4.1 Fonctionnement du Matchmaking](#4.1-fonctionnement-du-matchmaking)

&nbsp;&nbsp;&nbsp;&nbsp;[4.2 Gestion des Sessions de Jeu](#4.2-gestion-des-sessions-de-jeu)

[**5\. Optimisation et Performances**](#5.-optimisation-et-performances)

&nbsp;&nbsp;&nbsp;&nbsp;[5.1 Gestion du CPU/GPU sur Mobile](#5.1-gestion-du-cpu/gpu-sur-mobile)

&nbsp;&nbsp;&nbsp;&nbsp;[5.2 Optimisation Réseau](#5.2-optimisation-réseau)

&nbsp;&nbsp;&nbsp;&nbsp;[5.3 Gestion des Fonctionnalités Modulables (Scope Reduction)](#5.3-gestion-des-fonctionnalités-modulables-\(scope-reduction\))

[**6\. Monétisation et Gestion des Achats**](#6.-monétisation-et-gestion-des-achats)

&nbsp;&nbsp;&nbsp;&nbsp;[6.1 Système de Monétisation](#6.1-système-de-monétisation)

&nbsp;&nbsp;&nbsp;&nbsp;[6.2 Sécurité des Transactions](#6.2-sécurité-des-transactions)

&nbsp;&nbsp;&nbsp;&nbsp;[6.3 Pourquoi Unity IAP pour les Achats In-App ?](#6.3-pourquoi-unity-iap-pour-les-achats-in-app-?)

[**7\. Gestion des Bugs et Support**](#7.-gestion-des-bugs-et-support)

[**8\. Documentation et Évolutivité**](#8.-documentation-et-évolutivité)

[**9\. Préparation pour une Extension Future du Jeu**](#9.-préparation-pour-une-extension-future-du-jeu)

[**10\. Conclusion**](#10.-conclusion)

---

# **1\. Présentation Générale**

## **1.1 Objectif du Document**

Ce document détaille l’architecture technique et les spécifications du développement du jeu **Drip or Drop**, un party game multijoueur sur **mobile (iOS et Android)** développé sous **Unity 6**. Il vise à garantir une **expérience fluide, optimisée et immersive**, en intégrant des mécaniques de gameplay avancées et une gestion efficace des ressources mobiles.

## **1.2 Plateformes Ciblées**

- **Mobile** : iOS (iPhone, iPad) & Android (Smartphones et Tablettes)  
- **Moteur** : Unity 6  
- **Multijoueur** : Netcode for GameObjects  
- **Langages** : C\# pour le développement, JSON pour la gestion des données

## **1.3 Architecture Générale**

Le jeu repose sur une architecture **client-serveur** avec une **synchronisation en temps réel** des éléments de gameplay (tenues, votes, interactions). Il intègre une **gestion optimisée des ressources** pour garantir une consommation minimale de mémoire et de batterie. La communication réseau est assurée par **Netcode for GameObjects**, qui offre une latence optimisée et une scalabilité adaptée aux jeux mobiles multijoueurs.

---

# **2\. Architecture Logicielle**

### **2.1 Structure du Projet**

Le projet est structuré pour séparer les **composants principaux** et assurer une maintenance aisée.
![Diagramme de Classe](https://i.imgur.com/G47ed32.png)

- **Core** : Contient la logique principale du jeu, notamment la gestion des scènes et l'interface utilisateur.  
  - `GameManager`  
  - `SceneManager`  
  - `UIManager`

- **Networking** : Gestion des connexions et synchronisation des données multijoueurs.  
  - `NetcodeManager`  
  - `Matchmaking`  
  - `PlayerSync`  
  - `LagCompensation`  
  - `ReconnectionHandler`

- **Gameplay** : Mécaniques principales du jeu, comme l'édition de tenues et les votes.  
  - `ClothingSystem`  
  - `VoteSystem`  
  - `SabotageManager`  
  - `GameModeManager`  
  - `ReportSystem`

- **UI** : Interface et commandes tactiles.  
  - `MobileInputHandler`  
  - `TouchGestures`  
  - `MenuNavigation`  
  - `HUDManager`  
  - `FeedbackAnimations`

- **Données et Sauvegarde** : Gestion du stockage et de la persistance des données.  
  - `SaveSystem`  
  - `PlayerStats`  
  - `FirebaseManager`  
  - `LogStorage`  
  - `LeaderboardStorage`  
  - `ClothingSave`  
  - `CloudSyncHandler`

---

# 

# **3\. Interface Utilisateur et Système d’Inputs (GUI, HUD, Responsive Design)**

## **3.1 Inputs**

**Touch Controls** : Tap, swipe, drag & drop pour navigation et interaction.

**Compatibilité avec les manettes** : Support Xbox, PlayStation via Bluetooth (optionnel).

**Retour haptique** pour améliorer le ressenti des actions.

## **3.2 GUI et HUD**

* **Affichage dynamique du HUD** en fonction des modes de jeu.  
* **Animations de feedback visuel** pour informer le joueur (Tweening, Shader effects).  
* **Barres d’état et icônes interactives** pour actions principales (temps de vote, score, tenue en cours).

## **3.3 Responsive Design et Adaptation mobile/tablette**

* **Interface dynamique** qui s’adapte à la résolution et à l’orientation de l’écran.  
* **Gestion des UI Scalers** pour éviter les éléments trop petits ou mal placés.  
* **Optimisation des polices et tailles d’icônes** pour assurer lisibilité et accessibilité.

---

# **4\. Gestion des Sessions et Matchmaking**

## **4.1 Fonctionnement du Matchmaking**

- **Mode de connexion** : *Netcode for GameObjects* en mode Host/Client, où un joueur agit comme hôte et les autres rejoignent en tant que clients.  
- **Regroupement des joueurs** :  
  - **ELO/Glicko-2** pour équilibrer les niveaux.  
  - Matchmaking basé sur **latence réseau et localisation**.  
  - Algorithme de gestion des files d’attente pour éviter les déséquilibres de matchmaking.  
- **Gestion des joueurs AFK** :  
  - Expulsion après un certain temps d’inactivité.  
  - Remplacement automatique du joueur AFK par un bot ou attente d’un remplaçant.

## **4.2 Gestion des Sessions de Jeu**

**L'hôte est responsable de la synchronisation et de la persistance des données de session** tant que la partie est active.

- **Création de lobby privé/public** :  
  - Parties privées avec code d’accès.  
  - Système de file d’attente pour rejoindre un groupe aléatoire.  
- **Validation des joueurs avant le début de la partie** :  
  - Interface de confirmation avant lancement.  
  - Vérification des connexions avant le début du match.

---

# **5\. Optimisation et Performances**

## **5.1 Gestion du CPU/GPU sur Mobile**

- **Object pooling** pour limiter l’instanciation de nouveaux objets et réduire la charge CPU.  
- **Optimisation des draw calls** avec **GPU instancing et Dynamic Batching**.  
- **LOD (Level of Detail)** sur les avatars et accessoires pour améliorer le framerate.

## **5.2 Optimisation Réseau**

- **Compression des paquets Netcode** pour réduire la bande passante utilisée.  
- **Compensation de lag** via interpolation/extrapolation pour maintenir une fluidité de jeu.  
- **Fréquence d’envoi des mises à jour** ajustable selon le type d’événement (ex: animation en haute fréquence, chat en basse fréquence).  
- **Clients envoient leurs mises à jour au host, qui les redistribue**, plutôt qu’un serveur centralisé qui fait la synchronisation.  
- **Netcode en mode Host/Client réduit les coûts serveurs**, mais dépend de la stabilité de l’hôte.

**Risques de désynchronisation en cas de perte du host**, et éventuellement, une **solution de migration d’hôte** (Host Migration) pour éviter que la partie ne s’arrête si le host quitte.

## **5.3 Gestion des Fonctionnalités Modulables (Scope Reduction)**

* **Définition des fonctionnalités essentielles** :  
  * Déterminer quelles mécaniques sont indispensables au fonctionnement de base (multijoueur, gestion des tenues, votes).  
* **Liste des fonctionnalités désactivables sans impacter le jeu** :  
  * **Effets visuels avancés** (shaders complexes, animations secondaires).  
  * **Modes de jeu optionnels** (Sabotage Stylé, Imposteur du Style).  
  * **Personnalisation avancée des avatars** (certains accessoires ou skins premium).  
  * **Sons et effets audio secondaires**.  
* **Mécanisme de désactivation** :  
  * Activation/Désactivation via **paramètres de compilation** ou configuration JSON.  
  * Réduction dynamique des ressources utilisées sur mobile bas de gamme.

---

# **6\. Monétisation et Gestion des Achats**

## **6.1 Système de Monétisation**

- **Achats in-app** : Skins, accessoires et items cosmétiques.  
- **Battle Pass et contenu saisonnier** avec progression débloquant des récompenses exclusives.

## **6.2 Sécurité des Transactions**

- **Validation des achats via Unity IAP Server Validation,** qui s’intègre directement avec les stores ***Apple*** et ***Google***.  
- **Chiffrement des données sensibles** avec les protocoles natifs d’Apple et Google.  
- **Les transactions sont validées côté serveur par Unity**, ce qui empêche la falsification des reçus d’achat.  
- **Google Play et l’App Store encryptent déjà les données de paiement** avant transmission.

## **6.3 Pourquoi Unity IAP pour les Achats In-App ?**
* **Gestion simplifiée des transactions** sans serveur customisé.  
* **Validation automatique des paiements** et réduction du risque de fraude.  
* **Prise en charge des abonnements et promotions** (ex : Battle Pass).  
* **Suivi des revenus directement via Unity Dashboard**.

---

# **7\. Gestion des Bugs et Support**

- **Système de reporting des bugs en jeu** : Interface pour signaler un bug instantanément.  
- **Envoi automatique des logs de crash à Firebase** pour analyse des erreurs.  
- **Gestion des mises à jour et correctifs via patching** pour éviter le re-téléchargement intégral du jeu.


---

# **8\. Documentation et Évolutivité**

- **Création d’un Wiki de Documentation** via Notion pour centraliser toutes les infos techniques.  
- **Checklist de Debugging** pour les développeurs et testeurs.  
- **Plan de migration et scalabilité** :  
  - Choix de Firebase pour son coût modéré et son **scaling automatique**.  
  - Comparaison avec AWS Lambda et PlayFab pour une potentielle évolution future.

---

# **9\. Préparation pour une Extension Future du Jeu**

- **Support des contrôleurs mobiles** : Xbox, PlayStation, et manettes Bluetooth.  
- **Exploration d’un mode en Réalité Augmentée (AR)** pour personnaliser les tenues en 3D.  
- **Possibilité d’un portage WebGL/PC** avec des contrôles adaptés.

---

# **10\. Conclusion**

*Drip or Drop* repose sur une **architecture optimisée et évolutive**, avec un accent mis sur **le multijoueur performant, l’optimisation mobile et la sécurité des données**. Grâce à **Unity 6, Netcode for GameObjects, les API Unity et Firebase**, nous assurons une **expérience fluide et compétitive**. L’évolutivité du backend garantit que le jeu pourra s’adapter à une base de joueurs grandissante. 
