using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class ProfilePage : Page
    {
        public ProfilePage() { InitializeComponent(); Loaded += OnLoaded; }

        private void OnLoaded(object? s, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;
            TxtMin.Text = (u.TargetLow ?? 4.0).ToString(CultureInfo.InvariantCulture);
            TxtMax.Text = (u.TargetHigh ?? 10.0).ToString(CultureInfo.InvariantCulture);
        }

        private void Save_Click(object s, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;
            if (double.TryParse(TxtMin.Text, out var lo) &&
                double.TryParse(TxtMax.Text, out var hi) && lo > 0 && hi > lo)
            {
                App.Profiles.UpdateTargets(u.Id, lo, hi);
                u.TargetLow = lo; u.TargetHigh = hi;  // maj en mémoire
                LblInfo.Text = "Profil mis à jour.";
            }
            else LblInfo.Text = "Valeurs invalides.";
        }

        private void Back_Click(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.GoHome();
        }
    }
}
