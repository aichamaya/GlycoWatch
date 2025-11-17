using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Views;

namespace GlycoWatch_version_prof
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AllerConnexion(); // démarrer sur la page de connexion
        }

        // --- Helpers ---
        private void AfficherBarre(bool visible)
        {
            BarreLiens.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            LblUser.Text = App.CurrentUser != null ? App.CurrentUser.Nom : "";
        }

        private void Navigue(Page p, bool afficherBarre)
        {
            Cadre.Navigate(p);
            AfficherBarre(afficherBarre);
        }

        // --- Routes publiques (utilisées par les pages) ---
        public void AllerConnexion() => Navigue(new PageConnexion(), false);
        public void AllerInscription() => Navigue(new PageInscription(), false);

        public void AllerAccueil() => Navigue(new PageAccueil(), true);
        public void AllerProfil() => Navigue(new PageProfil(), true);
        public void AllerMesures() => Navigue(new PageMesures(), true);
        public void AllerTendances() => Navigue(new PageTendances(), true);
        public void AllerHistorique() => Navigue(new PageHistorique(), true);

        // --- Boutons barre ---
        private void BtnAccueil_Click(object s, RoutedEventArgs e) => AllerAccueil();
        private void BtnProfil_Click(object s, RoutedEventArgs e) => AllerProfil();
        private void BtnMesures_Click(object s, RoutedEventArgs e) => AllerMesures();
        private void BtnTendances_Click(object s, RoutedEventArgs e) => AllerTendances();
        private void BtnHistorique_Click(object s, RoutedEventArgs e) => AllerHistorique();

        private void BtnDeconnexion_Click(object s, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            AllerConnexion();
        }
    }
}
