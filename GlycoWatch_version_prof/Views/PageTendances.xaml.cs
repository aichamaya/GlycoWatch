using GlycoWatch_version_prof.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page des tendances :
    /// - stats sur 7 / 30 jours
    /// - courbe des glycémies
    /// </summary>
    public partial class PageTendances : Page
    {
        public PageTendances()
        {
            InitializeComponent();
            Loaded += PageTendances_Loaded;
        }

        private void PageTendances_Loaded(object? sender, RoutedEventArgs e)
        {
            ConstruireCourbe();
        }

        private void Periode_Changee(object sender, SelectionChangedEventArgs e)
        {
            ConstruireCourbe();
        }

        private void Actualiser_Click(object sender, RoutedEventArgs e)
        {
            ConstruireCourbe();
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
            {
                shell.AllerAccueil();
            }
        }

        private void SetTexte(string name, string text)
        {
            if (FindName(name) is TextBlock tb)
            {
                tb.Text = text;
            }
        }
        private void AfficherConseil(List<MeasureDb> mesures, double cibleMin, double cibleMax)
        {
            if (mesures == null || mesures.Count == 0)
            {
                LblConseils.Text = "Aucune mesure disponible. Pensez à enregistrer vos glycémies régulièrement.";
                return;
            }

            double moyenne = mesures.Average(m => m.Glycemie);
            int total = mesures.Count;
            int enCible = mesures.Count(m => m.Glycemie >= cibleMin && m.Glycemie <= cibleMax);
            double pourcentage = (double)enCible / total * 100;

            if (pourcentage >= 80)
            {
                LblConseils.Text = $"Bravo ! {pourcentage:0}% de vos mesures sont dans la cible. Continuez ainsi 👍";
            }
            else if (pourcentage >= 50)
            {
                LblConseils.Text = $"Vous avez {pourcentage:0}% de mesures dans la cible. C’est encourageant, mais vous pouvez faire mieux 💪";
            }
            else
            {
                LblConseils.Text = $"Seulement {pourcentage:0}% de vos mesures sont dans la cible. Pensez à revoir votre alimentation ou votre activité 🧠";
            }
        }

        /// <summary>
        /// Construit :
        /// - les stats (moyenne, min/max, % en cible, nb mesures)
        /// - la courbe des glycémies
        /// </summary>
        private void ConstruireCourbe()
        {
            var u = App.CurrentUser;
            if (u == null)
            {
                MessageBox.Show("Veuillez vous connecter.", "Information");
                return;
            }

            int nbJours = (CmbPeriode?.SelectedIndex ?? 0) == 1 ? 30 : 7;
            DateTime debut = DateTime.Today.AddDays(-nbJours + 1);
            DateTime finInclus = DateTime.Today.AddDays(1).AddSeconds(-1);

            var mesures = App.Measures
                             .Filter(u.Id, debut, finInclus, string.Empty, string.Empty)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            if (!mesures.Any())
            {
                MessageBox.Show("Aucune mesure disponible pour cette période.", "Information");
                return;
            }

            var model = new PlotModel { Title = $"Glycémie sur {nbJours} jours" };

            var axeX = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "dd/MM",
                Title = "Date",
                IntervalType = DateTimeIntervalType.Days,
                MinorIntervalType = DateTimeIntervalType.Days,
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            model.Axes.Add(axeX);

            var axeY = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Glycémie (mmol/L)"
            };
            model.Axes.Add(axeY);

            var courbe = new LineSeries
            {
                Title = "Glycémie",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.DarkBlue,
                Color = OxyColors.SkyBlue
            };

            foreach (var m in mesures)
            {
                courbe.Points.Add(DateTimeAxis.CreateDataPoint(m.DateMesure, m.Glycemie));
            }

            model.Series.Add(courbe);

            double yLow = u.TargetLow ?? 4.0;
            double yHigh = u.TargetHigh ?? 10.0;
            double minX = DateTimeAxis.ToDouble(debut);
            double maxX = DateTimeAxis.ToDouble(finInclus);

            var ligneLow = new LineSeries
            {
                Title = "Cible min",
                Color = OxyColors.Firebrick,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneLow.Points.Add(new DataPoint(minX, yLow));
            ligneLow.Points.Add(new DataPoint(maxX, yLow));
            model.Series.Add(ligneLow);

            var ligneHigh = new LineSeries
            {
                Title = "Cible max",
                Color = OxyColors.SteelBlue,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneHigh.Points.Add(new DataPoint(minX, yHigh));
            ligneHigh.Points.Add(new DataPoint(maxX, yHigh));
            model.Series.Add(ligneHigh);

            if (PlotCourbes != null)
            {
                PlotCourbes.Model = model;
                PlotCourbes.InvalidatePlot(true);
            }

        }
    }
}