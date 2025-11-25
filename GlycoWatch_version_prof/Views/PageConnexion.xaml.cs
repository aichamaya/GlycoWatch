using System;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page de connexion.
    /// Permet à l'utilisateur de saisir son pseudo et son mot de passe
    /// pour accéder à l'application.
    /// </summary>
    public partial class PageConnexion : Page
    {
        /// <summary>
        /// Constructeur. Initialise la page.
        /// </summary>
        public PageConnexion()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Événement appelé lorsqu'on clique sur le bouton "Se connecter".
        /// Valide les champs, tente la connexion et redirige vers l'accueil.
        /// </summary>
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Effacer le message d'erreur précédent
            LblError.Text = string.Empty;

            string pseudo = (TxtPseudo.Text ?? string.Empty).Trim();
            string motDePasse = TxtPassword.Password;

            // Validation de base
            if (string.IsNullOrWhiteSpace(pseudo) || string.IsNullOrWhiteSpace(motDePasse))
            {
                LblError.Text = "Veuillez entrer votre pseudo et votre mot de passe.";
                return;
            }

            try
            {
                // App.Auth.Login doit renvoyer un utilisateur (ou null si échec)
                var utilisateur = App.Auth.Login(pseudo, motDePasse);
                if (utilisateur == null)
                {
                    LblError.Text = "Identifiants invalides. Vérifiez votre pseudo ou votre mot de passe.";
                    return;
                }

                // Connexion réussie : on mémorise l'utilisateur courant
                App.CurrentUser = utilisateur;

                // Redirection vers la page d'accueil
                if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
                {
                    fenetrePrincipale.AllerAccueil();
                }
            }
            catch (Exception)
            {
                // Message non technique pour l'utilisateur
                LblError.Text = "Une erreur est survenue lors de la connexion. Veuillez réessayer.";
            }
        }

        /// <summary>
        /// Lien "Créer un compte" : redirige vers la page d'inscription.
        /// </summary>
        private void GoInscription_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerInscription();
            }
        }
    }
}
