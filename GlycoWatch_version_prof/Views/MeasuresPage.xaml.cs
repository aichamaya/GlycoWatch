using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Models; // MeasureDb
// App.CurrentUser et App.Measures sont exposés dans App.xaml.cs

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page Mesures (CRUD + Filtres) connectée à MySQL
    /// </summary>
    public partial class MeasuresPage : Page
    {
        // Si null => on est en mode "AJOUT"
        // Si non null => on édite la mesure avec cet Id
        private int? _editingId = null;

        public MeasuresPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Quand la page se charge : valeurs par défaut + affichage des mesures
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DpDate.SelectedDate == null) DpDate.SelectedDate = DateTime.Today; // date d'aujourd'hui par défaut
            if (string.IsNullOrWhiteSpace(TxtTime.Text)) TxtTime.Text = "12:00";  // heure par défaut
            Refresh(); // on remplit le tableau
        }

        /// <summary>
        /// Recharge la grille avec toutes les mesures de l'utilisateur courant
        /// </summary>
        private void Refresh()
        {
            var u = App.CurrentUser;
            if (u == null) return; // sécurité : pas d'utilisateur connecté

            // Lecture en BD via le repository
            Grid.ItemsSource = App.Measures.GetByUser(u.Id).ToList();
        }

        /// <summary>
        /// Bouton "Ajouter / Enregistrer"
        /// - Si _editingId == null => on AJOUTE
        /// - Sinon => on MET À JOUR la mesure correspondante
        /// </summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            // 1) Valider la glycémie (nombre > 0)
            if (!double.TryParse(TxtGly.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var gly) || gly <= 0)
            {
                MessageBox.Show("Glycémie invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2) Récupérer date et heure (si heure invalide, on met 12:00)
            var date = DpDate.SelectedDate ?? DateTime.Today;
            var time = TimeSpan.TryParse(TxtTime.Text, out var t) ? t : new TimeSpan(12, 0, 0);
            var when = date.Date + time;

            // 3) Repas / Activité (null si vide)
            var repas = (TxtRepas.Text ?? "").Trim();
            var act = (TxtActivite.Text ?? "").Trim();
            if (repas.Length == 0) repas = null;
            if (act.Length == 0) act = null;

            // 4) Ajout ou mise à jour en BD
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

                _editingId = null; // retour au mode AJOUT
            }

            // 5) Nettoyer le formulaire + recharger la grille
            TxtGly.Clear();
            TxtRepas.Clear();
            TxtActivite.Clear();
            Refresh();
        }

        /// <summary>
        /// Bouton "Modifier sélection" :
        /// charge la ligne sélectionnée dans le formulaire et passe en mode édition
        /// </summary>
        private void EditSelected_Click(object sender, RoutedEventArgs e)
        {
            if (Grid.SelectedItem is MeasureDb m)
            {
                _editingId = m.Id; // on passe en mode EDITION
                TxtGly.Text = m.Glycemie.ToString(CultureInfo.InvariantCulture);
                DpDate.SelectedDate = m.DateMesure.Date;
                TxtTime.Text = m.DateMesure.ToString("HH:mm");
                TxtRepas.Text = m.ContexteRepas ?? "";
                TxtActivite.Text = m.Activite ?? "";
                MessageBox.Show("Mode édition activé : modifiez les champs puis cliquez sur « Ajouter / Enregistrer ».",
                                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Sélectionnez d'abord une ligne dans le tableau.");
            }
        }

        /// <summary>
        /// Bouton "Supprimer sélection" : supprime la ligne en BD
        /// </summary>
        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            if (Grid.SelectedItem is MeasureDb m)
            {
                var confirm = MessageBox.Show("Supprimer cette mesure ?", "Confirmation",
                                              MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.Yes)
                {
                    App.Measures.Delete(m.Id, u.Id);
                    if (_editingId == m.Id) _editingId = null; // si on supprimait celle en cours d'édition
                    Refresh();
                }
            }
            else
            {
                MessageBox.Show("Sélectionnez d'abord une ligne dans le tableau.");
            }
        }

        /// <summary>
        /// Bouton "Filtrer" : applique les filtres et recharge la grille
        /// </summary>
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

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

        /// <summary>
        /// Bouton "Réinitialiser" : efface les filtres et recharge tout
        /// </summary>
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
            TxtFilterRepas.Clear();
            TxtFilterActivite.Clear();
            Refresh();
        }

        /// <summary>
        /// Bouton "← Retour" : revient à la page d'accueil
        /// </summary>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoHome();
        }
    }
}
