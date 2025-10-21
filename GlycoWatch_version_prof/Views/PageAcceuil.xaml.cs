using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page d'accueil avec raccourcis et petits indicateurs (sur 7 jours).
    /// Code débutant et commenté.
    /// </summary>
    public partial class PageAccueil : Page
    {
        public PageAccueil()
        {
            InitializeComponent();
            Loaded += PageAccueil_Loaded;
        }

        private void PageAccueil_Loaded(object sender, RoutedEventArgs e)
        {
            // Texte de bienvenue
            if (App.CurrentUser != null)
                LblBienvenueAccueil.Text = $"— Bonjour, {App.CurrentUser.Nom}";
            else
                LblBienvenueAccueil.Text = "— Bonjour";

            // Indicateurs sur 7 jours (si dépôt disponible)
            try
            {
                var u = App.CurrentUser;
                if (u == null)
                {
                    LblNbMesures.Text = "-";
                    LblMoyenne7.Text = "-";
                    LblPctZone7.Text = "-";
                    return;
                }

                DateTime debut = DateTime.Today.AddDays(-6); // 7 jours incluant aujourd'hui
                DateTime fin = DateTime.Today.AddDays(1).AddSeconds(-1);

                // Récupère les mesures (adapte si ton dépôt a un autre nom)
                var mesures = App.Measures
                                 .Filter(u.Id, debut, fin, null, null)
                                 .OrderBy(m => m.DateMesure)
                                 .ToList();

                if (mesures.Count == 0)
                {
                    LblNbMesures.Text = "0";
                    LblMoyenne7.Text = "-";
                    LblPctZone7.Text = "-";
                    return;
                }

                LblNbMesures.Text = mesures.Count.ToString();

                var valeurs = mesures.Select(m => m.Glycemie).ToList();
                double moy = valeurs.Average();
                LblMoyenne7.Text = moy.ToString("0.0", CultureInfo.InvariantCulture);

                double cibleMin = u.TargetLow ?? 4.0;
                double cibleMax = u.TargetHigh ?? 10.0;
                int ok = valeurs.Count(v => v >= cibleMin && v <= cibleMax);
                double pct = (ok * 100.0) / valeurs.Count;

                LblPctZone7.Text = $"{pct:0}%";
            }
            catch
            {
                // En cas d'absence de dépôt ou d'erreur, on évite de crasher
                LblNbMesures.Text = "-";
                LblMoyenne7.Text = "-";
                LblPctZone7.Text = "-";
            }
        }

        // Raccourcis navigation (via MainWindow)
        private void AllerMesures_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerMesures();
        }

        private void AllerProfil_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerProfil();
        }

        private void AllerTendances_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerTendances();
        }
    }
}
