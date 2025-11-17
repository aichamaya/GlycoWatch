using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Profil utilisateur (nom, pseudo, naissance, cibles min/max).
    /// Code débutant + commentaires.
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

            // Chargement des champs depuis l'utilisateur connecté
            ChampNom.Text = u.Nom;
            ChampPseudo.Text = u.Pseudo;       // Lecture seule ici
            ChampNaissance.SelectedDate = u.DateNaissance;
            ChampCibleMin.Text = (u.TargetLow ?? 4.0).ToString(CultureInfo.InvariantCulture);
            ChampCibleMax.Text = (u.TargetHigh ?? 10.0).ToString(CultureInfo.InvariantCulture);
            ChampUnite.Text = string.IsNullOrWhiteSpace(u.Unite) ? "mmol/L" : u.Unite;
        }

        /// <summary>Enregistre les cibles dans la base (validation simple).</summary>
        private void Enregistrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;

            if (!double.TryParse(ChampCibleMin.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var min) || min <= 0)
            { LblInfo.Text = "Cible min invalide."; return; }

            if (!double.TryParse(ChampCibleMax.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var max) || max <= min)
            { LblInfo.Text = "Cible max invalide (doit être > min)."; return; }

            // Sauvegarde des cibles 
            App.Profiles.UpdateTargets(u.Id, min, max);

            // Mettre à jour l'objet utilisateur en mémoire
            u.TargetLow = min;
            u.TargetHigh = max;
            u.Unite = string.IsNullOrWhiteSpace(ChampUnite.Text) ? "mmol/L" : ChampUnite.Text.Trim();

            LblInfo.Text = "Profil mis à jour.";
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerAccueil();
        }
    }
}
