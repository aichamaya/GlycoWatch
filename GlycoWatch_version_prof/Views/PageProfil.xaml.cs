using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page de profil utilisateur : affiche et met à jour les cibles glycémiques.
    /// </summary>
    public partial class PageProfil : Page
    {
        public PageProfil()
        {
            InitializeComponent();
            Loaded += PageProfil_Loaded;
        }

        private void PageProfil_Loaded(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            ChampNom.Text = u.Nom;
            ChampPseudo.Text = u.Pseudo;
            ChampNaissance.SelectedDate = u.DateNaissance;
            ChampCibleMin.Text = (u.TargetLow ?? 4.0).ToString(CultureInfo.InvariantCulture);
            ChampCibleMax.Text = (u.TargetHigh ?? 10.0).ToString(CultureInfo.InvariantCulture);
            ChampUnite.Text = string.IsNullOrWhiteSpace(u.Unite) ? "mmol/L" : u.Unite;
        }

        /// <summary>Enregistre les cibles dans la base de données.</summary>
        private void Enregistrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;

            if (!double.TryParse(ChampCibleMin.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var min) || min <= 0)
            { LblInfo.Text = "Cible min invalide."; return; }

            if (!double.TryParse(ChampCibleMax.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var max) || max <= min)
            { LblInfo.Text = "Cible max invalide (doit être > min)."; return; }

            App.Profiles.UpdateTargets(u.Id, min, max);
            u.TargetLow = min; u.TargetHigh = max;
            LblInfo.Text = "Profil mis à jour.";
        }

        /// <summary>Retour à l'accueil.</summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerAccueil();
        }
    }
}
