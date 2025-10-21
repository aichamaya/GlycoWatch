using GlycoWatch_version_prof.Models; // MeasureDb
using Microsoft.Win32;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// Historique des mesures + filtres + export CSV/JSON.
    public partial class PageHistorique : Page
    {
        public PageHistorique()
        {
            InitializeComponent();
            Loaded += PageHistorique_Loaded;
        }

        private void PageHistorique_Loaded(object? sender, RoutedEventArgs e)
        {
            Charger();
        }

        private void Charger()
        {
            var u = App.CurrentUser;
            if (u == null) return;

            DateTime? depuis = FiltreDepuis.SelectedDate;
            DateTime? jusqua = FiltreJusqua.SelectedDate?.AddDays(1).AddSeconds(-1);
            string r = (FiltreRepas.Text ?? "").Trim();
            string a = (FiltreActivite.Text ?? "").Trim();

            Grille.ItemsSource = App.Measures
                .Filter(u.Id,
                        depuis, jusqua,
                        string.IsNullOrEmpty(r) ? null : r,
                        string.IsNullOrEmpty(a) ? null : a)
                .OrderBy(m => m.DateMesure)
                .ToList();
        }

        private void Filtrer_Click(object sender, RoutedEventArgs e) => Charger();

        private void Reinitialiser_Click(object sender, RoutedEventArgs e)
        {
            FiltreDepuis.SelectedDate = null;
            FiltreJusqua.SelectedDate = null;
            FiltreRepas.Clear();
            FiltreActivite.Clear();
            Charger();
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell) shell.AllerAccueil();
        }

        // -------- Exports --------

        private void ExporterCsv_Click(object sender, RoutedEventArgs e)
        {
            var mesures = Grille.ItemsSource?.Cast<MeasureDb>().ToList() ?? [];
            if (mesures.Count == 0) { MessageBox.Show("Aucune donnée à exporter."); return; }

            var dlg = new SaveFileDialog
            {
                Filter = "Fichier CSV|*.csv",
                FileName = "historique_glycemies.csv"
            };
            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();
            sb.AppendLine("DateMesure;Glycemie;Repas;Activite");
            foreach (var m in mesures)
            {
                string date = m.DateMesure.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                string gly = m.Glycemie.ToString(CultureInfo.InvariantCulture);
                string repas = (m.ContexteRepas ?? "").Replace(';', ',');
                string act = (m.Activite ?? "").Replace(';', ',');
                sb.AppendLine($"{date};{gly};{repas};{act}");
            }
            System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Export CSV terminé.");
        }

        private void ExporterJson_Click(object sender, RoutedEventArgs e)
        {
            var mesures = Grille.ItemsSource?.Cast<MeasureDb>().ToList() ?? [];
            if (mesures.Count == 0) { MessageBox.Show("Aucune donnée à exporter."); return; }

            var dlg = new SaveFileDialog
            {
                Filter = "Fichier JSON|*.json",
                FileName = "historique_glycemies.json"
            };
            if (dlg.ShowDialog() != true) return;

            var json = JsonSerializer.Serialize(mesures, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            System.IO.File.WriteAllText(dlg.FileName, json, Encoding.UTF8);
            MessageBox.Show("Export JSON terminé.");
        }
    }
}
