using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page d'accueil avec raccourcis et petits indicateurs (sur 7 jours).
    /// Affiche :
    /// - un message de bienvenue personnalisé;
    /// - le nombre de mesures sur 7 jours;
    /// - la moyenne de glycémie sur 7 jours;
    /// - le pourcentage de mesures dans la zone cible.
    /// </summary>
    public partial class PageAccueil : Page
    {
        /// <summary>
        /// Constructeur : initialise la page et abonne l'événement Loaded.
        /// </summary>
        public PageAccueil()
        {
            InitializeComponent();
            Loaded += PageAccueil_Loaded;
        }

        /// <summary>
        /// Événement appelé lorsque la page est chargée.
        /// Met à jour le message de bienvenue et les indicateurs sur 7 jours.
        /// </summary>
        private void PageAccueil_Loaded(object sender, RoutedEventArgs e)
        {
            // Texte de bienvenue
            if (App.CurrentUser != null)
            {
                LblBienvenueAccueil.Text = $"— Bonjour et Bienvenu, {App.CurrentUser.Nom}";
            }
            else
            {
                LblBienvenueAccueil.Text = "— Bonjour";
            }

            // Indicateurs sur 7 jours
            try
            {
                var utilisateur = App.CurrentUser;
                if (utilisateur == null)
                {
                    LblNbMesures.Text = "-";
                    LblMoyenne7.Text = "-";
                    LblPctZone7.Text = "-";
                    return;
                }

                // Période des 7 derniers jours : de J-6 à aujourd'hui inclus
                DateTime debut = DateTime.Today.AddDays(-6);
                DateTime fin = DateTime.Today.AddDays(1).AddSeconds(-1);

                // Récupération des mesures de l'utilisateur sur cette période
                var mesures = App.Measures
                                 .Filter(utilisateur.Id, debut, fin, null, null)
                                 .OrderBy(m => m.DateMesure)
                                 .ToList();

                if (mesures.Count == 0)
                {
                    LblNbMesures.Text = "0";
                    LblMoyenne7.Text = "-";
                    LblPctZone7.Text = "-";
                    return;
                }

                // Nombre total de mesures
                LblNbMesures.Text = mesures.Count.ToString();

                // Moyenne de glycémie
                var valeurs = mesures.Select(m => m.Glycemie).ToList();
                double moyenne = valeurs.Average();
                LblMoyenne7.Text = moyenne.ToString("0.0", CultureInfo.InvariantCulture);

                // Pourcentage de mesures dans la cible
                double cibleMin = utilisateur.TargetLow ?? 4.0;
                double cibleMax = utilisateur.TargetHigh ?? 10.0;

                int nbDansCible = valeurs.Count(v => v >= cibleMin && v <= cibleMax);
                double pourcentage = (nbDansCible * 100.0) / valeurs.Count;

                LblPctZone7.Text = $"{pourcentage:0}%";
            }
            catch (Exception)
            {

                LblNbMesures.Text = "-";
                LblMoyenne7.Text = "-";
                LblPctZone7.Text = "-";
            }
        }


        /// <summary>
        /// Bouton "Aller aux mesures".
        /// </summary>
        private void AllerMesures_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerMesures();
            }
        }

        /// <summary>
        /// Bouton "Voir mon profil".
        /// </summary>
        private void AllerProfil_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerProfil();
            }
        }

        /// <summary>
        /// Bouton "Voir les tendances".
        /// </summary>
        private void AllerTendances_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerTendances();
            }
        }
    }
}
