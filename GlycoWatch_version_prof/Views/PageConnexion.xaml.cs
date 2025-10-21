using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class PageConnexion : Page
    {
        public PageConnexion()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LblError.Text = "";

            var pseudo = TxtPseudo.Text.Trim();
            var mdp = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(pseudo) || string.IsNullOrWhiteSpace(mdp))
            {
                LblError.Text = "Veuillez entrer votre pseudo et votre mot de passe.";
                return;
            }

            // App.Auth.Login doit renvoyer un Utilisateur ou null
            var usr = App.Auth.Login(pseudo, mdp);
            if (usr == null)
            {
                LblError.Text = "Identifiants invalides.";
                return;
            }

            App.CurrentUser = usr;
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerAccueil();
        }

        private void GoInscription_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerInscription();
        }
    }
}
