using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            TxtNom.Text = u.Nom;
            TxtPseudo.Text = u.Pseudo;
            DpDob.SelectedDate = u.DateNaissance;
            TxtMin.Text = (u.TargetLow ?? 4.0).ToString(CultureInfo.InvariantCulture);
            TxtMax.Text = (u.TargetHigh ?? 10.0).ToString(CultureInfo.InvariantCulture);
            TxtUnite.Text = string.IsNullOrWhiteSpace(u.Unite) ? "mmol/L" : u.Unite;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            if (!double.TryParse(TxtMin.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lo) || lo <= 0)
            { LblInfo.Text = "Cible min invalide."; return; }

            if (!double.TryParse(TxtMax.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var hi) || hi <= lo)
            { LblInfo.Text = "Cible max invalide (doit être > min)."; return; }

            try
            {
                App.Profiles.UpdateTargets(u.Id, lo, hi);   // maj BD
                // maj en mémoire
                u.TargetLow = lo;
                u.TargetHigh = hi;

                LblInfo.Text = "Profil mis à jour.";
            }
            catch (Exception ex)
            {
                LblInfo.Text = "Erreur: " + ex.Message;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoHome();
        }
    }
}
