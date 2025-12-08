using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Views;

namespace GlycoWatch_version_prof
{
    /// <summary>
    /// Fenêtre principale de l'application GlycoWatch.
    /// Elle contient :
    /// - une barre de navigation (boutons en haut ou sur le côté);
    /// - un Frame (FramePrincipal) qui affiche les différentes pages :
    ///   connexion, accueil, profil, mesures, tendances, historique, etc.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructeur. Initialise la fenêtre et abonne l'événement Loaded.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Lors du chargement de la fenêtre principale :
        /// - si aucun utilisateur n'est connecté, on affiche la page de connexion;
        /// - sinon, on affiche directement la page d'accueil.
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

        // ======================================================
        //  MÉTHODES DE NAVIGATION CENTRALES
        // ======================================================

        /// <summary>
        /// Affiche la page d'accueil dans le Frame principal.
        /// </summary>
        public void AllerAccueil()
        {
            FramePrincipal.Navigate(new PageAccueil());
        }

        /// <summary>
        /// Affiche la page de profil.
        /// </summary>

        public void AllerProfil()
        {
            FramePrincipal.Navigate(new ProfilePage());
        }


        /// <summary>
        /// Affiche la page de gestion des mesures.
        /// </summary>
        public void AllerMesures()
        {
            FramePrincipal.Navigate(new PageMesures());
        }

        /// <summary>
        /// Affiche la page des tendances (graphique + statistiques).
        /// </summary>
        public void AllerTendances()
        {
            FramePrincipal.Navigate(new PageTendances());
        }

        /// <summary>
        /// Affiche la page d'historique des mesures.
        /// </summary>
        public void AllerHistorique()
        {
            FramePrincipal.Navigate(new PageHistorique());
        }

        /// <summary>
        /// Affiche la page de connexion.
        /// </summary>
        public void AllerConnexion()
        {
            FramePrincipal.Navigate(new PageConnexion());
        }

        /// <summary>
        /// Affiche la page d'inscription (création de compte).
        /// </summary>
        public void AllerInscription()
        {
            FramePrincipal.Navigate(new PageInscription());
        }

        /// <summary>
        /// Méthode utilisée dans le XAML comme gestionnaire d'événement (GoHome).
        /// Signature imposée par les boutons (object sender, RoutedEventArgs e).
        /// </summary>
        public void GoHome(object sender, RoutedEventArgs e)
        {
            AllerAccueil();
        }

        /// <summary>
        /// Méthode utilitaire sans paramètre, pour les pages qui veulent
        /// simplement retourner à l'accueil en appelant fenetre.GoHome().
        /// </summary>
        public void GoHome()
        {
            AllerAccueil();
        }

        // ======================================================
        //  GESTION DES CLICS SUR LES BOUTONS DE NAVIGATION
        // ======================================================

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

        /// <summary>
        /// Déconnecte l'utilisateur courant et retourne à la page de connexion.
        /// </summary>
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            AllerConnexion();
        }
        public void AllerTutoriel()
        {
            FramePrincipal.Navigate(new PageTutoriel());
        }

        private void BtnTutoriel_Click(object sender, RoutedEventArgs e)
        {
            AllerTutoriel();
        }

    }
}
