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
    /// <summary>Page simple pour afficher les tendances (7/30 jours) + stats.</summary>
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

        private void Periode_Changee(object sender, SelectionChangedEventArgs e) => ConstruireGraphiqueEtStats();
        private void Actualiser_Click(object sender, RoutedEventArgs e) => ConstruireGraphiqueEtStats();
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerAccueil();
        }

        private void SetTexte(string name, string text)
        {
            if (FindName(name) is TextBlock tb) tb.Text = text;
        }

        private void AppliquerPlot(PlotModel model)
        {
            // Sécurise l’accès au PlotView même si le visuel n’est pas encore prêt
            if (!IsLoaded)
            {
                Loaded += (_, __) => AppliquerPlot(model);
                return;
            }

            var pv = Plot ?? (FindName("Plot") as OxyPlot.Wpf.PlotView);
            if (pv == null) return;                 
            pv.Model = model;
            pv.InvalidatePlot(true);
        }

        private void ConstruireGraphiqueEtStats()
        {
            var u = App.CurrentUser;
            if (u == null)
            {
                MessageBox.Show("Veuillez vous connecter.", "Information");
                return;
            }

            // Période
            int nbJours = (CmbPeriode?.SelectedIndex == 1) ? 30 : 7;
            DateTime debut = DateTime.Today.AddDays(-nbJours + 1);
            DateTime fin = DateTime.Today.AddDays(1).AddSeconds(-1);

            // Données
            var mesures = App.Measures
                             .Filter(u.Id, debut, fin, null, null)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            // Stats
            if (mesures.Count == 0)
            {
                SetTexte("LblMoyenne", "-");
                SetTexte("LblMinMax", "-");
                SetTexte("LblDansCible", "-");
                SetTexte("LblNb", "0");
            }
            else
            {
                var valeurs = mesures.Select(m => m.Glycemie).ToList();
                double moy = valeurs.Average();
                double min = valeurs.Min();
                double max = valeurs.Max();

                double cibleMin = u.TargetLow ?? 4.0;
                double cibleMax = u.TargetHigh ?? 10.0;

                int dansLaCible = valeurs.Count(v => v >= cibleMin && v <= cibleMax);
                double pct = (dansLaCible * 100.0) / valeurs.Count;

                SetTexte("LblMoyenne", moy.ToString("0.0", CultureInfo.InvariantCulture));
                SetTexte("LblMinMax", $"{min:0.0} / {max:0.0}");
                SetTexte("LblDansCible", $"{pct:0}% (cible {cibleMin:0.0}-{cibleMax:0.0})");
                SetTexte("LblNb", mesures.Count.ToString());
            }

            // Modèle OxyPlot
            var model = new PlotModel { Title = $"{nbJours} derniers jours" };

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

            var serie = new LineSeries { Title = "Glycémie", MarkerType = MarkerType.Circle, StrokeThickness = 2 };
            foreach (var m in mesures)
                serie.Points.Add(DateTimeAxis.CreateDataPoint(m.DateMesure, m.Glycemie));
            model.Series.Add(serie);

            if (mesures.Count >= 3)
            {
                var mm = new LineSeries { Title = "Moy. mobile (3)", LineStyle = LineStyle.Solid, StrokeThickness = 2 };
                for (int i = 0; i < mesures.Count; i++)
                {
                    int d = Math.Max(0, i - 1);
                    int f = Math.Min(mesures.Count - 1, i + 1);
                    var avg = mesures.Skip(d).Take(f - d + 1).Select(x => x.Glycemie).Average();
                    mm.Points.Add(DateTimeAxis.CreateDataPoint(mesures[i].DateMesure, avg));
                }
                model.Series.Add(mm);
            }

            double yLow = u.TargetLow ?? 4.0;
            double yHigh = u.TargetHigh ?? 10.0;
            double xStart = DateTimeAxis.ToDouble(debut);
            double xEnd = DateTimeAxis.ToDouble(fin);

            var ligneLow = new LineSeries { Title = "Cible min", LineStyle = LineStyle.Dash, StrokeThickness = 1 };
            ligneLow.Points.Add(new DataPoint(xStart, yLow));
            ligneLow.Points.Add(new DataPoint(xEnd, yLow));

            var ligneHigh = new LineSeries { Title = "Cible max", LineStyle = LineStyle.Dash, StrokeThickness = 1 };
            ligneHigh.Points.Add(new DataPoint(xStart, yHigh));
            ligneHigh.Points.Add(new DataPoint(xEnd, yHigh));

            model.Series.Add(ligneLow);
            model.Series.Add(ligneHigh);

            
            AppliquerPlot(model);
        }
    }
}
