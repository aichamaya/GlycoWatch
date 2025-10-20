using System;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage() { InitializeComponent(); }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var nom = (TxtNom.Text ?? "").Trim();
            var pseudo = (TxtPseudo.Text ?? "").Trim();
            var pass = TxtPass.Password ?? "";
            var dob = DpDob.SelectedDate;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(pseudo) || string.IsNullOrWhiteSpace(pass))
            { LblError.Text = "Nom, Pseudo et Mot de passe sont requis."; return; }

            try
            {
                var u = App.Auth.Register(nom, pseudo, pass, dob);
                MessageBox.Show($"Compte créé pour {u.Nom} ({u.Pseudo}).", "Succès");
                if (Window.GetWindow(this) is MainWindow shell) shell.GoLogin();
            }
            catch (Exception ex) { LblError.Text = ex.Message; }
        }

        private void GoLogin(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.GoLogin();
        }
    }
}
