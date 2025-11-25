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
    /// Page d'affichage des tendances de glycémie.
    /// Cette page présente :
    /// - des statistiques (moyenne, min, max, % en zone cible, nombre de mesures);
    /// - une courbe des glycémies;
    /// - un court texte de conseils personnalisés.
    /// </summary>
    public partial class PageTendances : Page
    {
        /// <summary>
        /// Constructeur. Initialise la page et abonne l'événement de chargement.
        /// </summary>
        public PageTendances()
        {
            InitializeComponent();
            Loaded += PageTendances_Loaded;
        }

        /// <summary>
        /// Événement appelé lorsque la page est complètement chargée.
        /// </summary>
        private void PageTendances_Loaded(object? sender, RoutedEventArgs e)
        {
            ConstruireTendances();
        }

        /// <summary>
        /// Événement appelé lorsque l'utilisateur change la période (7 jours / 30 jours).
        /// </summary>
        private void Periode_Changee(object sender, SelectionChangedEventArgs e)
        {
            ConstruireTendances();
        }

        /// <summary>
        /// Événement appelé lorsque l'utilisateur clique sur le bouton "Actualiser".
        /// </summary>
        private void Actualiser_Click(object sender, RoutedEventArgs e)
        {
            ConstruireTendances();
        }

        /// <summary>
        /// Événement appelé lorsque l'utilisateur clique sur le bouton "Retour".
        /// </summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerAccueil();
            }
        }

        /// <summary>
        /// Utilitaire pour modifier le texte d'un TextBlock à partir de son nom.
        /// </summary>
        /// <param name="nomControle">Nom du contrôle (TextBlock) dans le XAML.</param>
        /// <param name="texte">Texte à afficher.</param>
        private void DefinirTexte(string nomControle, string texte)
        {
            if (FindName(nomControle) is TextBlock blocTexte)
            {
                blocTexte.Text = texte;
            }
        }

        /// <summary>
        /// Affiche un conseil simple en fonction du pourcentage de mesures dans la zone cible.
        /// </summary>
        /// <param name="mesures">Liste des mesures de glycémie.</param>
        /// <param name="cibleMin">Valeur minimale de la zone cible.</param>
        /// <param name="cibleMax">Valeur maximale de la zone cible.</param>
        private void AfficherConseil(List<MeasureDb> mesures, double cibleMin, double cibleMax)
        {
            if (LblConseils == null)
            {
                return;
            }

            if (mesures == null || mesures.Count == 0)
            {
                LblConseils.Text = "Aucune mesure disponible. Pensez à enregistrer vos glycémies régulièrement.";
                return;
            }

            int nombreTotal = mesures.Count;
            int nombreEnCible = mesures.Count(m => m.Glycemie >= cibleMin && m.Glycemie <= cibleMax);
            double pourcentageEnCible = (double)nombreEnCible / nombreTotal * 100.0;

            if (pourcentageEnCible >= 80)
            {
                LblConseils.Text = $"Bravo ! {pourcentageEnCible:0}% de vos mesures sont dans la cible. Continuez ainsi 👍";
            }
            else if (pourcentageEnCible >= 50)
            {
                LblConseils.Text = $"Vous avez {pourcentageEnCible:0}% de mesures dans la cible. C’est encourageant, mais vous pouvez faire encore mieux 💪";
            }
            else
            {
                LblConseils.Text = $"Seulement {pourcentageEnCible:0}% de vos mesures sont dans la cible. Discutez avec votre professionnel de la santé pour ajuster votre routine 🧠";
            }
        }

        /// <summary>
        /// Construit les tendances :
        /// - statistiques (moyenne, min, max, % en cible, nombre de mesures);
        /// - courbe des glycémies;
        /// - conseils pour l'utilisateur.
        /// </summary>
        private void ConstruireTendances()
        {
            var utilisateur = App.CurrentUser;
            if (utilisateur == null)
            {
                MessageBox.Show("Veuillez vous connecter.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Déterminer la période (7 ou 30 jours) en fonction de la sélection.
            int nombreJours = (CmbPeriode?.SelectedIndex ?? 0) == 1 ? 30 : 7;

            DateTime dateDebut = DateTime.Today.AddDays(-nombreJours + 1);
            DateTime dateFinIncluse = DateTime.Today.AddDays(1).AddSeconds(-1);

            // Récupérer les mesures de la période choisie.
            var mesures = App.Measures
                             .Filter(utilisateur.Id, dateDebut, dateFinIncluse, string.Empty, string.Empty)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            double cibleMin = utilisateur.TargetLow ?? 4.0;
            double cibleMax = utilisateur.TargetHigh ?? 10.0;

            // Si aucune mesure : afficher des valeurs par défaut et vider le graphique.
            if (mesures.Count == 0)
            {
                DefinirTexte("TxtMoyenne", "-");
                DefinirTexte("TxtMinMax", "-");
                DefinirTexte("TxtPourcZone", "-");
                DefinirTexte("TxtNbMesures", "0");

                AfficherConseil(mesures, cibleMin, cibleMax);

                if (PlotCourbes != null)
                {
                    PlotCourbes.Model = new PlotModel { Title = $"Glycémie sur {nombreJours} jours" };
                    PlotCourbes.InvalidatePlot(true);
                }

                return;
            }

            // Calcul des statistiques de base.
            double moyenne = mesures.Average(m => m.Glycemie);
            double valeurMin = mesures.Min(m => m.Glycemie);
            double valeurMax = mesures.Max(m => m.Glycemie);
            int nombreMesures = mesures.Count;
            int nombreEnZone = mesures.Count(m => m.Glycemie >= cibleMin && m.Glycemie <= cibleMax);
            double pourcentageZone = (double)nombreEnZone / nombreMesures * 100.0;

            // Affichage des statistiques dans les TextBlocks.
            DefinirTexte("TxtMoyenne", $"{moyenne:0.0}");
            DefinirTexte("TxtMinMax", $"{valeurMin:0.0} / {valeurMax:0.0}");
            DefinirTexte("TxtPourcZone", $"{pourcentageZone:0}%");
            DefinirTexte("TxtNbMesures", nombreMesures.ToString(CultureInfo.InvariantCulture));

            // Affichage du conseil.
            AfficherConseil(mesures, cibleMin, cibleMax);

            // Création du modèle de graphique.
            var modele = new PlotModel
            {
                Title = $"Glycémie sur {nombreJours} jours",
                PlotMargins = new OxyThickness(50, 20, 20, 40),
                Padding = new OxyThickness(10)
            };

            // Axe des dates (X).
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
            modele.Axes.Add(axeX);

            // Axe des valeurs de glycémie (Y).
            var axeY = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Glycémie (mmol/L)",
                Minimum = 3,   // bornes choisies pour un affichage lisible
                Maximum = 15,
                MajorStep = 1
            };
            modele.Axes.Add(axeY);

            // Série de la courbe des glycémies.
            var serieGlycemie = new LineSeries
            {
                Title = "Glycémie",
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerStroke = OxyColors.DarkBlue,
                MarkerFill = OxyColors.White,
                StrokeThickness = 2.5,
                Color = OxyColors.SteelBlue
            };

            foreach (var mesure in mesures)
            {
                serieGlycemie.Points.Add(DateTimeAxis.CreateDataPoint(mesure.DateMesure, mesure.Glycemie));
            }

            modele.Series.Add(serieGlycemie);

            // Lignes horizontales pour la zone cible (min et max).
            double xMin = DateTimeAxis.ToDouble(dateDebut);
            double xMax = DateTimeAxis.ToDouble(dateFinIncluse);

            var ligneCibleMin = new LineSeries
            {
                Title = "Cible min",
                Color = OxyColors.Firebrick,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneCibleMin.Points.Add(new DataPoint(xMin, cibleMin));
            ligneCibleMin.Points.Add(new DataPoint(xMax, cibleMin));
            modele.Series.Add(ligneCibleMin);

            var ligneCibleMax = new LineSeries
            {
                Title = "Cible max",
                Color = OxyColors.SteelBlue,
                LineStyle = LineStyle.Dash,
                StrokeThickness = 1
            };
            ligneCibleMax.Points.Add(new DataPoint(xMin, cibleMax));
            ligneCibleMax.Points.Add(new DataPoint(xMax, cibleMax));
            modele.Series.Add(ligneCibleMax);

            // Application du modèle au contrôle graphique.
            if (PlotCourbes != null)
            {
                PlotCourbes.Model = modele;
                PlotCourbes.InvalidatePlot(true);
            }
        }
    }
}
