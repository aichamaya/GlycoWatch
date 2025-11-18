using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Views;

namespace GlycoWatch_version_prof
{
    /// <summary>
    /// Fenêtre principale : contient un Frame (Cadre) et gère la navigation
    /// entre Connexion, Accueil, Mesures, Profil, Tendances.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Au démarrage, on affiche la page de connexion
            AllerConnexion();
        }

        // -------------------------
        // Méthodes de navigation
        // -------------------------

        /// <summary>Affiche la page de connexion.</summary>
        public void AllerConnexion()
        {
            Cadre.Navigate(new PageConnexion());
        }

        /// <summary>Affiche la page d'inscription.</summary>
        public void AllerInscription()
        {
            Cadre.Navigate(new PageInscription());
        }

        /// <summary>Affiche la page d'accueil.</summary>
        public void AllerAccueil()
        {
            Cadre.Navigate(new PageAccueil());
        }

        /// <summary>Affiche la page des mesures.</summary>
        public void AllerMesures()
        {
            Cadre.Navigate(new PageMesures());
        }

        /// <summary>Affiche la page de profil.</summary>
        public void AllerProfil()
        {
            Cadre.Navigate(new PageProfil());
        }

        /// <summary>Affiche la page des tendances.</summary>
        public void AllerTendances()
        {
            Cadre.Navigate(new PageTendances());
        }

        // ------------------------------------
        // Gestion des clics sur les boutons
        // (si utilisés dans MainWindow.xaml)
        // ------------------------------------

        private void BtnAccueil_Click(object sender, RoutedEventArgs e)
        {
            AllerAccueil();
        }

        private void BtnMesures_Click(object sender, RoutedEventArgs e)
        {
            AllerMesures();
        }

        private void BtnProfil_Click(object sender, RoutedEventArgs e)
        {
            AllerProfil();
        }

        private void BtnTendances_Click(object sender, RoutedEventArgs e)
        {
            AllerTendances();
        }

        private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
        {
            // On efface l'utilisateur courant puis on revient à Connexion
            App.CurrentUser = null;
            AllerConnexion();
        }

        /// <summary>
        /// Méthode appelée par Click="GoHome" dans le XAML.
        /// C'est juste un alias pour retourner à l'accueil.
        /// </summary>
        private void GoHome(object sender, RoutedEventArgs e)
        {
            AllerAccueil();
        }
    }
}
