using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage() { InitializeComponent(); }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var pseudo = (TxtPseudo.Text ?? "").Trim();
            var pass = TxtPassword.Password ?? "";

            if (string.IsNullOrWhiteSpace(pseudo)) { LblError.Text = "Pseudo requis."; return; }
            if (string.IsNullOrWhiteSpace(pass)) { LblError.Text = "Mot de passe requis."; return; }

            var user = App.Auth.Login(pseudo, pass);
            if (user == null) { LblError.Text = "Identifiants invalides."; return; }

            App.CurrentUser = user;
            if (Window.GetWindow(this) is MainWindow shell) shell.GoHome();
        }

        private void GoRegister(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.GoRegister();
        }
    }
}
