using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof
{
    /// <summary>
    /// Fenêtre principale très simple : elle affiche ou cache la barre du haut
    /// selon l'état de connexion, et navigue entre les pages.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Au chargement de la fenêtre : si déjà connecté -> Accueil, sinon -> Connexion
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
                AllerAccueil();
            else
                AllerConnexion();

            RafraichirBienvenue();
        }

        /// <summary>Affiche un message de bienvenue ou le nom de l'appli.</summary>
        private void RafraichirBienvenue()
        {
            if (LblBienvenue == null) return;
            LblBienvenue.Text = (App.CurrentUser != null)
                ? $"Bonjour, {App.CurrentUser.Nom}"
                : "GlycoWatch";
        }

        /// <summary>Affiche/masque la barre du haut.</summary>
        private void AfficherBarre(bool visible)
        {
            if (BarreHaut == null) return;
            BarreHaut.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }


        
        public void AllerAccueil()
        {
            AfficherBarre(true);
            Cadre.Navigate(new Views.PageAccueil());
            RafraichirBienvenue();
        }

        public void AllerProfil()
        {
            AfficherBarre(true);
            Cadre.Navigate(new Views.PageProfil());
        }

        public void AllerMesures()
        {
            AfficherBarre(true);
            Cadre.Navigate(new Views.PageMesures());
        }

        public void AllerTendances()
        {
            AfficherBarre(true);
            Cadre.Navigate(new Views.PageTendances());
        }

        public void AllerHistorique()
        {
            AfficherBarre(true);
            Cadre.Navigate(new Views.PageHistorique());
        }

        public void AllerConnexion()
        {
            AfficherBarre(false);
            Cadre.Navigate(new Views.PageConnexion());
        }

        public void AllerInscription()
        {
            AfficherBarre(false); 
            Cadre.Navigate(new Views.PageInscription());
        }

        // ------------------- Boutons de la barre -------------------

        private void BtnAccueil_Click(object sender, RoutedEventArgs e) => AllerAccueil();
        private void BtnProfil_Click(object sender, RoutedEventArgs e) => AllerProfil();
        private void BtnMesures_Click(object sender, RoutedEventArgs e) => AllerMesures();
        private void BtnTendances_Click(object sender, RoutedEventArgs e) => AllerTendances();
        private void BtnHistorique_Click(object sender, RoutedEventArgs e) => AllerHistorique();

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Déconnexion simple
            App.CurrentUser = null;
            AllerConnexion();
        }
    }
}
