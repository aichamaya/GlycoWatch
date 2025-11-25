using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Models; 
using Microsoft.Win32;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page d'historique des mesures de glycémie.
    /// Permet de :
    /// - afficher toutes les mesures;
    /// - filtrer par dates, repas et activité;
    /// - exporter les données en CSV ou en JSON.
    /// </summary>
    public partial class PageHistorique : Page
    {
        /// <summary>
        /// Constructeur. Initialise la page et abonne l'événement de chargement.
        /// </summary>
        public PageHistorique()
        {
            InitializeComponent();
            Loaded += PageHistorique_Loaded;
        }

        /// <summary>
        /// Événement appelé lorsque la page est chargée.
        /// On remplit le tableau avec les données de l'utilisateur connecté.
        /// </summary>
        private void PageHistorique_Loaded(object? sender, RoutedEventArgs e)
        {
            Charger();
        }

        /// <summary>
        /// Charge les mesures de l'utilisateur connecté en tenant compte des filtres.
        /// </summary>
        private void Charger()
        {
            var utilisateur = App.CurrentUser;
            if (utilisateur == null)
            {
                MessageBox.Show("Aucun utilisateur connecté. Veuillez vous connecter d'abord.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            DateTime? depuis = FiltreDepuis.SelectedDate;
            DateTime? jusqua = FiltreJusqua.SelectedDate?.AddDays(1).AddSeconds(-1);

            string texteRepas = (FiltreRepas.Text ?? string.Empty).Trim();
            string texteActivite = (FiltreActivite.Text ?? string.Empty).Trim();

            string? filtreRepas = string.IsNullOrEmpty(texteRepas) ? null : texteRepas;
            string? filtreActivite = string.IsNullOrEmpty(texteActivite) ? null : texteActivite;

            try
            {
                var mesures = App.Measures
                                 .Filter(utilisateur.Id, depuis, jusqua, filtreRepas, filtreActivite)
                                 .OrderBy(m => m.DateMesure)
                                 .ToList();

                Grille.ItemsSource = mesures;
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenue lors du chargement de l'historique.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Applique les filtres (dates, repas, activité) et recharge la grille.
        /// </summary>
        private void Filtrer_Click(object sender, RoutedEventArgs e)
        {
            Charger();
        }

        /// <summary>
        /// Réinitialise tous les filtres et recharge toutes les mesures.
        /// </summary>
        private void Reinitialiser_Click(object sender, RoutedEventArgs e)
        {
            FiltreDepuis.SelectedDate = null;
            FiltreJusqua.SelectedDate = null;
            FiltreRepas.Clear();
            FiltreActivite.Clear();

            Charger();
        }

        /// <summary>
        /// Retourne à la page d'accueil de l'application.
        /// </summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
                fenetrePrincipale.AllerAccueil();
            }
        }

        // -------- Exports --------

        /// <summary>
        /// Exporte les mesures affichées dans la grille au format CSV.
        /// </summary>
        private void ExporterCsv_Click(object sender, RoutedEventArgs e)
        {
            var mesures = Grille.ItemsSource?.Cast<MeasureDb>().ToList() ?? new List<MeasureDb>();

            if (mesures.Count == 0)
            {
                MessageBox.Show("Aucune donnée à exporter.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var dialogue = new SaveFileDialog
            {
                Filter = "Fichier CSV|*.csv",
                FileName = "historique_glycemies.csv"
            };

            if (dialogue.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var constructeur = new StringBuilder();
                constructeur.AppendLine("DateMesure;Glycemie;Repas;Activite");

                foreach (var mesure in mesures)
                {
                    string date = mesure.DateMesure.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    string glycemie = mesure.Glycemie.ToString(CultureInfo.InvariantCulture);
                    string repas = (mesure.ContexteRepas ?? string.Empty).Replace(';', ',');
                    string activite = (mesure.Activite ?? string.Empty).Replace(';', ',');

                    constructeur.AppendLine($"{date};{glycemie};{repas};{activite}");
                }

                System.IO.File.WriteAllText(dialogue.FileName, constructeur.ToString(), Encoding.UTF8);

                MessageBox.Show("Export CSV terminé.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenue lors de l'export CSV.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exporte les mesures affichées dans la grille au format JSON.
        /// </summary>
        private void ExporterJson_Click(object sender, RoutedEventArgs e)
        {
            var mesures = Grille.ItemsSource?.Cast<MeasureDb>().ToList() ?? new List<MeasureDb>();

            if (mesures.Count == 0)
            {
                MessageBox.Show("Aucune donnée à exporter.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var dialogue = new SaveFileDialog
            {
                Filter = "Fichier JSON|*.json",
                FileName = "historique_glycemies.json"
            };

            if (dialogue.ShowDialog() != true)
            {
                return;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(mesures, options);
                System.IO.File.WriteAllText(dialogue.FileName, json, Encoding.UTF8);

                MessageBox.Show("Export JSON terminé.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenue lors de l'export JSON.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}
