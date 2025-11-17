using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views.Controls
{
    /// <summary>
    /// Petit panneau autonome : calcule le % en zone sur les 30 derniers jours
    /// et affiche 3 à 5 conseils basés sur ce pourcentage.
    /// </summary>
    public partial class HealthAdvicePanel : UserControl
    {
        public HealthAdvicePanel()
        {
            InitializeComponent();
            Loaded += (_, __) => Refresh();
        }

        /// <summary>Recalcule les conseils selon les données actuelles.</summary>
        public void Refresh()
        {
            var u = App.CurrentUser;
            if (u == null)
            {
                ListTips.ItemsSource = new[] { "Connectez-vous pour afficher des conseils personnalisés." };
                LblContext.Text = "";
                return;
            }

            // Fenêtre d’analyse : 30 jours glissants
            DateTime fin = DateTime.Today.AddDays(1).AddSeconds(-1);
            DateTime debut = DateTime.Today.AddDays(-29);

            // On récupère les mesures (triées) ; null-safe (string.Empty)
            var mesures = App.Measures
                             .Filter(u.Id, debut, fin, string.Empty, string.Empty)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            if (mesures.Count == 0)
            {
                ListTips.ItemsSource = new[] { "Aucune donnée récente. Enregistrez quelques mesures pour démarrer le suivi." };
                LblContext.Text = "0 mesure sur les 30 derniers jours";
                return;
            }

            // Bornes cibles (avec valeurs par défaut)
            double min = u.TargetLow ?? 4.0;
            double max = u.TargetHigh ?? 10.0;

            int ok = mesures.Count(m => m.Glycemie >= min && m.Glycemie <= max);
            double pct = ok * 100.0 / mesures.Count;

            LblContext.Text = $"{mesures.Count} mesures • {pct:0}% en zone ({min:0.0}-{max:0.0} mmol/L)";

            // Conseils simples selon 3 paliers
            List<string> tips;
            if (pct < 40)
            {
                tips = new List<string>
                {
                    "Essayez de mesurer à des moments réguliers (ex. avant repas + 2h après).",
                    "Ajoutez une marche de 15–20 min si possible après les repas.",
                    "Hydratez-vous suffisamment dans la journée.",
                    "Révisez, si nécessaire, vos objectifs mini/maxi avec un professionnel.",
                    "Notez le contexte des mesures (repas, activité) pour mieux comprendre."
                };
            }
            else if (pct < 70)
            {
                tips = new List<string>
                {
                    "Vous progressez 👍. Gardez le rythme de saisie des mesures.",
                    "Repérez les repas qui sortent de la zone et ajustez les portions.",
                    "Préférez des collations à index glycémique bas (noix, yaourt nature…).",
                    "Pensez à bouger 10–15 min les jours plus sédentaires."
                };
            }
            else
            {
                tips = new List<string>
                {
                    "Super! Votre % en zone est élevé 👏",
                    "Stabilisez la routine qui fonctionne (horaires, repas, activité).",
                    "Continuez d’enregistrer votre contexte pour confirmer les bons choix.",
                    "Planifiez un rappel hebdo pour jeter un œil aux tendances."
                };
            }

            ListTips.ItemsSource = tips;
        }
    }
}
