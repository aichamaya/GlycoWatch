using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class PageInscription : Page
    {
        public PageInscription()
        {
            InitializeComponent();
        }

        private void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            LblInfo.Text = "";

            var nom = (TxtNom.Text ?? "").Trim();
            var pseudo = (TxtPseudo.Text ?? "").Trim();
            var mdp = TxtPassword.Password;
            DateTime? naissance = DpNaissance.SelectedDate;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(pseudo) || string.IsNullOrWhiteSpace(mdp))
            {
                LblInfo.Text = "Tous les champs sont obligatoires (nom, pseudo, mot de passe).";
                return;
            }

            // Register(pseudo, mdp, nom, naissance) — signature à 4 paramètres
            var nouvelUtilisateur = App.Auth.Register(pseudo, mdp, nom, naissance);
            if (nouvelUtilisateur == null)
            {
                LblInfo.Text = "Création impossible (pseudo déjà pris ?).";
                return;
            }

            MessageBox.Show("Compte créé ! Vous pouvez vous connecter.");
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerConnexion();
        }

        private void GoConnexion_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerConnexion();
        }
    }
}
