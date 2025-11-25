using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page de gestion du profil de l'utilisateur.
    /// Permet de consulter et modifier :
    /// - le nom;
    /// - le pseudo;
    /// - la date de naissance;
    /// - la plage de glycémie cible (min et max);
    /// - l'unité de mesure (affichage seulement ici).
    /// </summary>
    public partial class ProfilePage : Page
    {
        /// <summary>
        /// Constructeur. Initialise la page et abonne l'événement de chargement.
        /// </summary>
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Événement appelé lorsque la page est chargée.
        /// On remplit les champs avec les informations de l'utilisateur courant.
        /// </summary>
        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            var utilisateur = App.CurrentUser;
            if (utilisateur == null)
            {
                MessageBox.Show("Aucun utilisateur connecté. Veuillez vous connecter d'abord.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            // Remplir les champs avec les informations actuelles
            TxtNom.Text = utilisateur.Nom;
            TxtPseudo.Text = utilisateur.Pseudo;
            DpDob.SelectedDate = utilisateur.DateNaissance;

            TxtMin.Text = (utilisateur.TargetLow ?? 4.0).ToString(CultureInfo.InvariantCulture);
            TxtMax.Text = (utilisateur.TargetHigh ?? 10.0).ToString(CultureInfo.InvariantCulture);

            // Unité de mesure (affichage seulement)
            TxtUnite.Text = string.IsNullOrWhiteSpace(utilisateur.Unite)
                ? "mmol/L"
                : utilisateur.Unite;

            LblInfo.Text = string.Empty;
        }

        /// <summary>
        /// Événement appelé lorsqu'on clique sur le bouton "Enregistrer" (ou "Sauvegarder").
        /// Valide les champs min/max puis met à jour la base de données et l'utilisateur en mémoire.
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var utilisateur = App.CurrentUser;
            if (utilisateur == null)
            {
                MessageBox.Show("Aucun utilisateur connecté. Veuillez vous connecter d'abord.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            LblInfo.Text = string.Empty;

            // On permet à l'utilisateur de taper soit un point, soit une virgule.
            string texteMin = TxtMin.Text.Replace(',', '.');
            string texteMax = TxtMax.Text.Replace(',', '.');

            // Validation de la valeur minimale
            if (!double.TryParse(texteMin, NumberStyles.Float, CultureInfo.InvariantCulture, out var limiteBasse)
                || limiteBasse <= 0)
            {
                LblInfo.Text = "Valeur minimale de la cible invalide (doit être un nombre positif).";
                return;
            }

            // Validation de la valeur maximale
            if (!double.TryParse(texteMax, NumberStyles.Float, CultureInfo.InvariantCulture, out var limiteHaute)
                || limiteHaute <= limiteBasse)
            {
                LblInfo.Text = "Valeur maximale de la cible invalide (elle doit être plus grande que la valeur minimale).";
                return;
            }

            try
            {
                // Mise à jour dans la base de données
                App.Profiles.UpdateTargets(utilisateur.Id, limiteBasse, limiteHaute);

                // Mise à jour en mémoire
                utilisateur.TargetLow = limiteBasse;
                utilisateur.TargetHigh = limiteHaute;

                LblInfo.Text = "Profil mis à jour avec succès.";
            }
            catch (Exception)
            {
                // On ne montre pas les détails techniques à l'utilisateur.
                LblInfo.Text = "Une erreur est survenue lors de la mise à jour du profil. Veuillez réessayer.";
            }
        }

        /// <summary>
        /// Événement appelé lorsqu'on clique sur le bouton "Retour".
        /// Retourne à la page d'accueil de l'application.
        /// </summary>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.GoHome();
            }
        }
    }
}
