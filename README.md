# GlycoWatch
Est une application WPF permettant à une personne diabétique de suivre ses glycémies au quotidien, de visualiser les tendances et d’obtenir quelques conseils simples à partir de ses données.

## 1. Nature et objectif du projet

- **Type de projet** : Application de bureau WPF (.NET 8, C#)
- **Clientèle cible** : Personnes vivant avec le diabète de type 2 (ou proches) qui souhaitent :
  - enregistrer leurs valeurs de glycémie,
  - consulter des courbes et statistiques simples,
  - exporter leurs données,
  - mieux comprendre la part de mesures dans la zone cible.

L’objectif n’est pas de remplacer un suivi médical, mais de **fournir un outil de suivi personnel** facile à utiliser.

## 2. Technologies utilisées

- **Langage** : C# (.NET 8)
- **Framework UI** : WPF (Windows Presentation Foundation)
- **Base de données** : fichier SQL local (via `Db.cs` / `UserDb.cs`)
- **Bibliothèques principales** :
  - `OxyPlot` : affichage des courbes de glycémie
- **Outils** :
  - Visual Studio 2022
  - GitHub pour le suivi de projet (Issues / Project / Releases)
  
  ## 3. Fonctionnalités servies par le projet

	### 3.1 Gestion du compte

- Création de compte (nom complet, pseudo, mot de passe, date de naissance).
- Connexion / déconnexion.
- Page Profil avec :
  - affichage des informations de base,
  - définition de la **zone cible de glycémie** (min / max),
  - sauvegarde en base de données.
  
  ### 3.2 Mesures de glycémie

- Ajout d’une mesure :
  - glycémie,
  - date / heure,
  - contexte repas (texte libre),
  - activité (texte libre).
- Modification d’une mesure existante.
- Suppression avec **message de confirmation**.
- Filtres sur l’historique :
  - plage de dates,
  - texte dans “repas”,
  - texte dans “activité”.
- Messages de statut non intrusifs en bas de la page (succès / erreur).
### 3.3 Tendances et statistiques

- Page **Tendances** :
  - courbe de glycémie sur 7 ou 30 jours,
  - lignes de zone cible (min / max),
  - indicateurs :
    - moyenne,
    - min / max,
    - % en zone,
    - nombre de mesures.
- Page **Accueil** :
  - message de bienvenue personnalisé,
  - résumé sur 7 jours :
    - nombre de mesures,
    - moyenne,
    - % en zone.

### 3.4 Historique et export

- Tableau complet des mesures (triées par date).
- Filtres (date / repas / activité).
- **Export CSV** (pour Excel / LibreOffice).
- **Export JSON** (pour usage technique ou développeur).

### 3.5 Conseils et tutoriel vidéo

- Panneau de **conseils santé** qui se met à jour à partir du % de mesures dans la zone cible sur 30 jours :
  - conseils différents si % faible / moyen / élevé.
- Page **Tutoriel** :
  - lecture d’une vidéo locale `Videos/tutoriel_glycowatch.mp4`,
  - contrôle Lecture / Pause / Recommencer,
  - intégration dans l’application (pas de YouTube).
  
## 4. Degré de complétion

- **Fonctionnalités de base** (gestion des mesures, profil, tendances, historique, export) : **implémentées**.
- **Conseils santé** : implémentés de manière simple (pas d’algorithme médical).
- **Tutoriel vidéo** : intégré et fonctionnel.

## 5. Pistes d’amélioration

- Ajouter un système de **profils multiples** sur le même ordinateur.
- Ajouter la **gestion de rappels** (ex. notifications pour mesurer la glycémie).
- Améliorer l’analyse :
  - temps passé en hypo / hyper,
  - moyennes par plage horaire (matin / midi / soir).
- Permettre l’export au format **PDF** avec un rapport synthétique.
- Support d’autres unités (mg/dL) avec conversion automatique.

## 6. Procédure d’installation – Utilisateur

### Prérequis

- Windows 10 ou 11
- .NET Desktop Runtime 8 (si non déjà installé)

### Installation

1. Télécharger la **Release** `GlycoWatch_version_prof` depuis GitHub.
2. Décompresser le fichier dans un dossier (par ex. `C:\Program Files\GlycoWatch` ou sur le Bureau).
3. Lancer `GlycoWatch_version_prof.exe`.
4. Optionnel : créer un raccourci vers le `.exe` sur le Bureau.

Aucun installeur n’est nécessaire, l’application est portable (base de données locale dans le dossier de l’application ou dans un sous-dossier).
## 7. Procédure d’installation – Développeur

### 7.1 Prérequis

- Visual Studio 2022 
  - Workload **“.NET Desktop Development”** installé.
- .NET SDK 8
- Git


### 7.2 Cloner le dépôt

`bash
git clone https://github.com/aichamaya/GlycoWatch.git
cd GlycoWatch_version_prof

### 7.3 Construction et exécution
- - Ouvrir `GlycoWatch_version_prof.sln` dans Visual Studio.
    
- Vérifier que le projet `GlycoWatch_version_prof` est **Projet de démarrage**.
    
- Lancer en mode Débogage ou Release.
### 8. Compte utilisateur de test
- utilisateur
  - pseudo : oua2
  - mot de passe :1234
### 9.Gestion de projet et dépôt GitHub
- Le projet est suivi dans GitHub via :
	- les **commits** sur la branche principale,    
	- un **Project** de type Kanban (Tâches à faire, En cours, Terminées, Bogues).
	
- La Release utilisée pour la remise contient :
	- le code source complet, 
	- le binaire `.exe` prêt à l’emploi,   
	- ce fichier `README.md`.


