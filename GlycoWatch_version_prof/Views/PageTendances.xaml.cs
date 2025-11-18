using GlycoWatch_version_prof.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
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
    /// - conseils
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
            ConstruireTendances();
        }

        private void Periode_Changee(object sender, SelectionChangedEventArgs e)
        {
            ConstruireTendances();
        }

        private void Actualiser_Click(object sender, RoutedEventArgs e)
        {
            ConstruireTendances();
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
            {
                shell.AllerAccueil();
            }
        }

        /// <summary>
        /// Petit utilitaire pour mettre le texte dans les labels du haut.
        /// </summary>
        private void SetTexte(string name, string text)
        {
            if (FindName(name) is TextBlock tb)
            {
                tb.Text = text;
            }
        }

        private void AfficherConseil(List<MeasureDb> mesures, double cibleMin, double cibleMax)
        {
            if (LblConseils == null)
                return;

            if (mesures == null || mesures.Count == 0)
            {
                LblConseils.Text = "Aucune mesure disponible. Pensez à enregistrer vos glycémies régulièrement.";
                return;
            }

            double moyenne = mesures.Average(m => m.Glycemie);
            int total = mesures.Count;
            int enCible = mesures.Count(m => m.Glycemie >= cibleMin && m.Glycemie <= cibleMax);
            double pourcentage = total > 0 ? (double)enCible / total * 100.0 : 0.0;

            if (pourcentage >= 80)
            {
                LblConseils.Text = $"Bravo ! {pourcentage:0}% de vos mesures sont dans la cible. Continuez ainsi 👍";
            }
            else if (pourcentage >= 50)
            {
                LblConseils.Text = $"Vous avez {pourcentage:0}% de mesures dans la cible. C’est encourageant, mais vous pouvez faire encore mieux 💪";
            }
            else
            {
                LblConseils.Text = $"Seulement {pourcentage:0}% de vos mesures sont dans la cible. Discutez avec votre professionnel de la santé pour ajuster votre routine 🧠";
            }
        }

        /// <summary>
        /// Construit :
        /// - les stats (moyenne, min/max, % en cible, nb mesures)
        /// - la courbe des glycémies
        /// - les conseils
        /// </summary>
        private void ConstruireTendances()
        {
            var u = App.CurrentUser;
            if (u == null)
            {
                MessageBox.Show("Veuillez vous connecter.", "Information");
                return;
            }

            // Période : 7 ou 30 jours
            int nbJours = (CmbPeriode?.SelectedIndex ?? 0) == 1 ? 30 : 7;
            DateTime debut = DateTime.Today.AddDays(-nbJours + 1);
            DateTime finInclus = DateTime.Today.AddDays(1).AddSeconds(-1);

            // Récupérer les mesures
            var mesures = App.Measures
                             .Filter(u.Id, debut, finInclus, string.Empty, string.Empty)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            // Statistiques + conseils même si le graphique est vide
            double cibleMin = u.TargetLow ?? 4.0;
            double cibleMax = u.TargetHigh ?? 10.0;

            if (mesures.Count == 0)
            {
                SetTexte("TxtMoyenne", "-");
                SetTexte("TxtMinMax", "-");
                SetTexte("TxtPourcZone", "-");
                SetTexte("TxtNbMesures", "0");

                AfficherConseil(mesures, cibleMin, cibleMax);

                if (PlotCourbes != null)
                {
                    PlotCourbes.Model = new PlotModel { Title = $"Glycémie sur {nbJours} jours" };
                    PlotCourbes.InvalidatePlot(true);
                }

                return;
            }

            // Calcul stats
            double moyenne = mesures.Average(m => m.Glycemie);
            double min = mesures.Min(m => m.Glycemie);
            double max = mesures.Max(m => m.Glycemie);
            int nbMesures = mesures.Count;
            int enZone = mesures.Count(m => m.Glycemie >= cibleMin && m.Glycemie <= cibleMax);
            double pourcentageZone = (double)enZone / nbMesures * 100.0;

            SetTexte("TxtMoyenne", $"{moyenne:0.0}");
            SetTexte("TxtMinMax", $"{min:0.0} / {max:0.0}");
            SetTexte("TxtPourcZone", $"{pourcentageZone:0}%");
            SetTexte("TxtNbMesures", nbMesures.ToString(CultureInfo.InvariantCulture));

            // Conseils
            AfficherConseil(mesures, cibleMin, cibleMax);

            // Construction du graphique
            var model = new PlotModel
            {
                Title = $"Glycémie sur {nbJours} jours",
                PlotMargins = new OxyThickness(50, 20, 20, 40),
                Padding = new OxyThickness(10)
            };

            // Axe X : dates
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

            // Axe Y : glycémie
            var axeY = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Glycémie (mmol/L)",
                Minimum = 3,
                Maximum = 15,
                MajorStep = 1
            };
            model.Axes.Add(axeY);

            // Courbe des glycémies
            var courbe = new LineSeries
            {
                Title = "Glycémie",
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerStroke = OxyColors.DarkBlue,
                MarkerFill = OxyColors.White,
                StrokeThickness = 2.5,
                Color = OxyColors.SteelBlue,
                //Smooth = true
            };

            foreach (var m in mesures)
            {
                courbe.Points.Add(DateTimeAxis.CreateDataPoint(m.DateMesure, m.Glycemie));
            }

            model.Series.Add(courbe);

            // Lignes de cible min / max
            double minX = DateTimeAxis.ToDouble(debut);
            double maxX = DateTimeAxis.ToDouble(finInclus);

            var ligneLow = new LineSeries
            {
                Title = "Cible min",
                Color = OxyColors.Firebrick,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneLow.Points.Add(new DataPoint(minX, cibleMin));
            ligneLow.Points.Add(new DataPoint(maxX, cibleMin));
            model.Series.Add(ligneLow);

            var ligneHigh = new LineSeries
            {
                Title = "Cible max",
                Color = OxyColors.SteelBlue,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneHigh.Points.Add(new DataPoint(minX, cibleMax));
            ligneHigh.Points.Add(new DataPoint(maxX, cibleMax));
            model.Series.Add(ligneHigh);

            // Appliquer au PlotView
            if (PlotCourbes != null)
            {
                PlotCourbes.Model = model;
                PlotCourbes.InvalidatePlot(true);
            }
        }
    }
}
