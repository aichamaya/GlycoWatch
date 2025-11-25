using System;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page permettant la création d’un nouveau compte utilisateur.
    /// L’utilisateur doit fournir : nom, pseudo, mot de passe, date de naissance (optionnelle).
    /// </summary>
    public partial class PageInscription : Page
    {
        public PageInscription()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Événement déclenché lorsqu’on clique sur le bouton “Créer le compte”.
        /// Vérifie les champs, tente la création puis redirige vers la page de connexion.
        /// </summary>
        private void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            LblInfo.Text = string.Empty;

            string nom = (TxtNom.Text ?? string.Empty).Trim();
            string pseudo = (TxtPseudo.Text ?? string.Empty).Trim();
            string motDePasse = TxtPassword.Password;
            DateTime? dateNaissance = DpNaissance.SelectedDate;

            // Validation simple
            if (string.IsNullOrWhiteSpace(nom) ||
                string.IsNullOrWhiteSpace(pseudo) ||
                string.IsNullOrWhiteSpace(motDePasse))
            {
                LblInfo.Text = "Tous les champs obligatoires doivent être remplis (nom, pseudo, mot de passe).";
                return;
            }

            try
            {
                // Tentative d'enregistrement
                var nouvelUtilisateur = App.Auth.Register(pseudo, motDePasse, nom, dateNaissance);

                if (nouvelUtilisateur == null)
                {
                    LblInfo.Text = "Impossible de créer le compte. Le pseudo est peut-être déjà utilisé.";
                    return;
                }

                MessageBox.Show("Compte créé avec succès ! Vous pouvez maintenant vous connecter.",
                                "Succès",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                // Redirection vers la connexion
                if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
                {
                    fenetrePrincipale.AllerConnexion();
                }
            }
            catch (Exception)
            {
                // Message non technique pour l'utilisateur final
                LblInfo.Text = "Une erreur est survenue lors de la création du compte. Veuillez réessayer.";
            }
        }

        /// <summary>
        /// Lien “Déjà un compte ? Se connecter” → retourne à la page de connexion.
        /// </summary>
        private void GoConnexion_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerConnexion();
            }
        }
    }
}
