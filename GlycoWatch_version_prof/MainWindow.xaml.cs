using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Views;

namespace GlycoWatch_version_prof
{
    /// <summary>
    /// Fenêtre principale : contient la barre de navigation
    /// et un Frame qui affiche les pages (connexion, accueil, mesures, etc.).
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Au chargement, on affiche soit la connexion (si pas connecté),
        /// soit la page d'accueil.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null)
            {
                AllerConnexion();
            }
            else
            {
                AllerAccueil();
            }
        }

        // ========== NAVIGATION ==========

        public void AllerAccueil()
        {
            FramePrincipal.Navigate(new PageAccueil());
        }

        public void AllerProfil()
        {
            FramePrincipal.Navigate(new PageProfil());
        }

        public void AllerMesures()
        {
            FramePrincipal.Navigate(new PageMesures());
        }

        public void AllerTendances()
        {
            FramePrincipal.Navigate(new PageTendances());
        }

        public void AllerHistorique()
        {
            FramePrincipal.Navigate(new PageHistorique());
        }

        public void AllerConnexion()
        {
            FramePrincipal.Navigate(new PageConnexion());
        }

        public void AllerInscription()
        {
            FramePrincipal.Navigate(new PageInscription());
        }

        /// <summary>
        /// Méthode utilisée par XAML (GoHome).
        /// On la laisse publique pour éviter les erreurs.
        /// </summary>
        public void GoHome(object sender, RoutedEventArgs e)
        {
            AllerAccueil();
        }


        private void BtnAccueil_Click(object sender, RoutedEventArgs e)
        {
            AllerAccueil();
        }

        private void BtnProfil_Click(object sender, RoutedEventArgs e)
        {
            AllerProfil();
        }

        private void BtnMesures_Click(object sender, RoutedEventArgs e)
        {
            AllerMesures();
        }

        private void BtnTendances_Click(object sender, RoutedEventArgs e)
        {
            AllerTendances();
        }

        private void BtnHistorique_Click(object sender, RoutedEventArgs e)
        {
            AllerHistorique();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            AllerConnexion();
        }
        public void GoHome()
        {
            GoHome(this, new RoutedEventArgs());
        }
    }
}