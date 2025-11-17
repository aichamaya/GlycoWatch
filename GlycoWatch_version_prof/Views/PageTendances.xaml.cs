using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page des tendances glycémiques (7 ou 30 jours).
    /// Code débutant, simple et commenté.
    /// </summary>
    public partial class PageTendances : Page
    {
        public PageTendances()
        {
            InitializeComponent();
            Loaded += PageTendances_Loaded;
        }

        private void PageTendances_Loaded(object sender, RoutedEventArgs e)
        {
            ConstruireGraphiqueEtStats();
        }

        private void Periode_Changee(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;   // évite d'appeler trop tôt
            ConstruireGraphiqueEtStats();
        }

        private void Actualiser_Click(object sender, RoutedEventArgs e)
        {
            ConstruireGraphiqueEtStats();
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.AllerAccueil();
        }

        /// <summary>
        /// Applique le PlotModel au contrôle PlotView de la page,
        /// en gérant le cas où la page n'est pas encore complètement chargée.
        /// </summary>
        private void AppliquerPlot(PlotModel model)
        {
            // Si la page n’est pas encore chargée, on attend l’événement Loaded
            if (!IsLoaded)
            {
                Loaded += (_, __) =>
                {
                    if (Plot != null)
                    {
                        Plot.Model = model;
                        Plot.InvalidatePlot(true);
                    }
                };
                return;
            }

            
            if (Plot == null)
            {
              
                return;
            }

            Plot.Model = model;
            Plot.InvalidatePlot(true);
        }

        /// <summary>
        /// Récupère les mesures, calcule les statistiques et construit le graphique.
        /// </summary>
        private void ConstruireGraphiqueEtStats()
        {
            var u = App.CurrentUser;
            if (u == null)
            {
                MessageBox.Show("Veuillez vous connecter.", "Information");
                return;
            }

            // 1) Période choisie : 7 ou 30 jours
            int nbJours = (CmbPeriode?.SelectedIndex == 1) ? 30 : 7;
            DateTime debut = DateTime.Today.AddDays(-nbJours + 1);
            DateTime fin = DateTime.Today.AddDays(1).AddSeconds(-1);

            // 2) Récupérer les mesures
            var mesures = App.Measures
                             .Filter(u.Id, debut, fin, null, null)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            // Variables pour les conseils
            double moy = 0, min = 0, max = 0, pct = 0;
            double cibleMin = u.TargetLow ?? 4.0;
            double cibleMax = u.TargetHigh ?? 10.0;

            // 3) Statistiques
            if (mesures.Count == 0)
            {
                LblMoyenne.Text = "-";
                LblMinMax.Text = "-";
                LblDansCible.Text = "-";
                LblNb.Text = "0";

                LblConseils.Text = "Aucune mesure sur la période sélectionnée. " +
                                   "Ajoutez quelques mesures pour voir vos tendances.";
            }
            else
            {
                var valeurs = mesures.Select(m => m.Glycemie).ToList();
                moy = valeurs.Average();
                min = valeurs.Min();
                max = valeurs.Max();

                int dansLaCible = valeurs.Count(v => v >= cibleMin && v <= cibleMax);
                pct = (dansLaCible * 100.0) / valeurs.Count;

                LblMoyenne.Text = moy.ToString("0.0", CultureInfo.InvariantCulture);
                LblMinMax.Text = $"{min:0.0} / {max:0.0}";
                LblDansCible.Text = $"{pct:0}% (cible {cibleMin:0.0}-{cibleMax:0.0})";
                LblNb.Text = mesures.Count.ToString();

                var conseils = new System.Text.StringBuilder();

                if (pct >= 80)
                    conseils.AppendLine("✅ Très bon contrôle : vous êtes souvent dans la zone cible.");
                else if (pct >= 50)
                    conseils.AppendLine("👍 Globalement correct, mais il reste une marge pour augmenter le % de mesures dans la zone.");
                else
                    conseils.AppendLine("⚠ De nombreuses valeurs sont hors de la zone cible. " +
                                        "Parlez-en avec un professionnel de la santé.");

                if (max > cibleMax + 2)
                    conseils.AppendLine($"• Certaines glycémies sont nettement au-dessus de {cibleMax:0.0} mmol/L.");

                if (min < cibleMin - 1)
                    conseils.AppendLine($"• Des valeurs proches ou en dessous de {cibleMin:0.0} mmol/L apparaissent : attention aux hypoglycémies.");

                conseils.AppendLine("Astuce : notez le contexte repas/activité pour mieux comprendre vos variations.");

                LblConseils.Text = conseils.ToString();
            }

           

            if (Plot == null)
            {
                MessageBox.Show("Erreur : contrôle 'Plot' introuvable dans la page.", "Tendances");
                return;
            }

            var model = new PlotModel
            {
                Title = $"{nbJours} derniers jours",
                IsLegendVisible = true
            };

            var axeX = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "Date",
                IntervalType = DateTimeIntervalType.Days,
                MinorIntervalType = DateTimeIntervalType.Days,
                IsZoomEnabled = true,
                IsPanEnabled = true
            };
            model.Axes.Add(axeX);

            var axeY = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Glycémie (mmol/L)",
                IsZoomEnabled = true,
                IsPanEnabled = true
            };
            model.Axes.Add(axeY);

            // Courbe glycémie
            var serie = new LineSeries
            {
                Title = "Glycémie",
                MarkerType = MarkerType.Circle,
                StrokeThickness = 2
            };
            foreach (var m in mesures)
                serie.Points.Add(DateTimeAxis.CreateDataPoint(m.DateMesure, m.Glycemie));

            model.Series.Add(serie);

            // Moyenne mobile simple (fenêtre 3)
            if (mesures.Count >= 3)
            {
                var mm = new LineSeries
                {
                    Title = "Moy. mobile (3)",
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 2
                };

                for (int i = 0; i < mesures.Count; i++)
                {
                    int d = Math.Max(0, i - 1);
                    int f = Math.Min(mesures.Count - 1, i + 1);
                    var avg = mesures.Skip(d).Take(f - d + 1).Select(x => x.Glycemie).Average();
                    mm.Points.Add(DateTimeAxis.CreateDataPoint(mesures[i].DateMesure, avg));
                }

                model.Series.Add(mm);
            }

           
            double xStart = DateTimeAxis.ToDouble(debut);
            double xEnd = DateTimeAxis.ToDouble(fin);

            var ligneLow = new LineSeries
            {
                Title = "Cible min",
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneLow.Points.Add(new DataPoint(xStart, cibleMin));
            ligneLow.Points.Add(new DataPoint(xEnd, cibleMin));
            model.Series.Add(ligneLow);

            var ligneHigh = new LineSeries
            {
                Title = "Cible max",
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneHigh.Points.Add(new DataPoint(xStart, cibleMax));
            ligneHigh.Points.Add(new DataPoint(xEnd, cibleMax));
            model.Series.Add(ligneHigh);

           
            Plot.Model = model;
            Plot.InvalidatePlot(true);
        }

    }
}
