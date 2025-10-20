using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Models;

namespace GlycoWatch_version_prof.Views
{
    public partial class MeasuresPage : Page
    {
        // null = mode AJOUT ; sinon = Id de la mesure en cours d’édition
        private int? _editingId = null;

        public MeasuresPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DpDate.SelectedDate == null) DpDate.SelectedDate = DateTime.Today;
            if (string.IsNullOrWhiteSpace(TxtTime.Text)) TxtTime.Text = "12:00";
            Refresh();
        }

        private void Refresh()
        {
            var u = App.CurrentUser; if (u == null) return;
            Grid.ItemsSource = App.Measures.GetByUser(u.Id).ToList();
        }

        // AJOUT ou MISE À JOUR selon _editingId
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;

            if (!double.TryParse(TxtGly.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var gly) || gly <= 0)
            {
                MessageBox.Show("Glycémie invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = DpDate.SelectedDate ?? DateTime.Today;
            var time = TimeSpan.TryParse(TxtTime.Text, out var t) ? t : new TimeSpan(12, 0, 0);
            var when = date.Date + time;

            var repas = (TxtRepas.Text ?? "").Trim();
            var act = (TxtActivite.Text ?? "").Trim();
            if (repas.Length == 0) repas = null;
            if (act.Length == 0) act = null;

            if (_editingId is null)
            {
                // AJOUT
                App.Measures.Add(new MeasureDb
                {
                    UserId = u.Id,
                    DateMesure = when,
                    Glycemie = gly,
                    ContexteRepas = repas,
                    Activite = act
                });
            }
            else
            {
                // MISE À JOUR
                App.Measures.Update(new MeasureDb
                {
                    Id = _editingId.Value,
                    UserId = u.Id,
                    DateMesure = when,
                    Glycemie = gly,
                    ContexteRepas = repas,
                    Activite = act
                });
                _editingId = null; // retour en mode ajout
            }

            // reset & refresh
            TxtGly.Clear();
            TxtRepas.Clear();
            TxtActivite.Clear();
            Refresh();
        }

        // Remplit le formulaire avec la ligne sélectionnée et passe en mode édition
        private void EditSelected_Click(object sender, RoutedEventArgs e)
        {
            if (Grid.SelectedItem is MeasureDb m)
            {
                _editingId = m.Id;
                TxtGly.Text = m.Glycemie.ToString(CultureInfo.InvariantCulture);
                DpDate.SelectedDate = m.DateMesure.Date;
                TxtTime.Text = m.DateMesure.ToString("HH:mm");
                TxtRepas.Text = m.ContexteRepas ?? "";
                TxtActivite.Text = m.Activite ?? "";
                MessageBox.Show("Mode édition : modifiez les champs puis cliquez sur « Ajouter » pour enregistrer.",
                                "Édition", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Sélectionnez d'abord une ligne dans le tableau.");
            }
        }

        // Supprime la ligne sélectionnée
        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;

            if (Grid.SelectedItem is MeasureDb m)
            {
                var confirm = MessageBox.Show("Supprimer cette mesure ?", "Confirmation",
                                              MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    App.Measures.Delete(m.Id, u.Id);
                    if (_editingId == m.Id) _editingId = null;
                    Refresh();
                }
            }
            else
            {
                MessageBox.Show("Sélectionnez d'abord une ligne dans le tableau.");
            }
        }

        // Filtrage par date et textes (repas/activité)
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser; if (u == null) return;

            DateTime? from = DpFrom.SelectedDate;
            DateTime? to = DpTo.SelectedDate?.AddDays(1).AddSeconds(-1);
            string repasContains = (TxtFilterRepas.Text ?? "").Trim();
            string actContains = (TxtFilterActivite.Text ?? "").Trim();

            Grid.ItemsSource = App.Measures
                .Filter(u.Id,
                        from, to,
                        string.IsNullOrEmpty(repasContains) ? null : repasContains,
                        string.IsNullOrEmpty(actContains) ? null : actContains)
                .ToList();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
            TxtFilterRepas.Clear();
            TxtFilterActivite.Clear();
            Refresh();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoHome();
        }
    }
}
