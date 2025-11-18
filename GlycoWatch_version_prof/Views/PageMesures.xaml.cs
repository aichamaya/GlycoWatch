using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Models;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page de gestion des mesures de glycémie (ajout, modification, suppression, filtres).
    /// </summary>
    public partial class PageMesures : Page
    {
        private int? _editionId = null;

        public PageMesures()
        {
            InitializeComponent();
            Loaded += Page_Chargee;
        }

        private void Page_Chargee(object sender, RoutedEventArgs e)
        {
            if (ChampDate.SelectedDate == null)
                ChampDate.SelectedDate = DateTime.Today;

            if (string.IsNullOrWhiteSpace(ChampHeure.Text))
                ChampHeure.Text = "12:00";

            RafraichirGrille();
        }

        private void RafraichirGrille()
        {
            var u = App.CurrentUser;
            if (u == null) return;

            Grille.ItemsSource = App.Measures
                                     .GetByUser(u.Id)
                                     .OrderBy(m => m.DateMesure)
                                     .ToList();
        }

        private void AjouterOuEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            if (!double.TryParse(ChampGlycemie.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double gly) || gly <= 0)
            {
                MessageBox.Show("La glycémie doit être un nombre positif.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime date = ChampDate.SelectedDate ?? DateTime.Today;
            TimeSpan heure = TimeSpan.TryParse(ChampHeure.Text, out var h) ? h : new TimeSpan(12, 0, 0);
            DateTime quand = date.Date + heure;

            string repas = string.IsNullOrWhiteSpace(ChampRepas.Text) ? null : ChampRepas.Text.Trim();
            string activite = string.IsNullOrWhiteSpace(ChampActivite.Text) ? null : ChampActivite.Text.Trim();

            if (_editionId == null)
            {
                App.Measures.Add(new MeasureDb
                {
                    UserId = u.Id,
                    DateMesure = quand,
                    Glycemie = gly,
                    ContexteRepas = repas,
                    Activite = activite
                });
            }
            else
            {
                App.Measures.Update(new MeasureDb
                {
                    Id = _editionId.Value,
                    UserId = u.Id,
                    DateMesure = quand,
                    Glycemie = gly,
                    ContexteRepas = repas,
                    Activite = activite
                });

                _editionId = null;
            }

            ChampGlycemie.Clear();
            ChampRepas.Clear();
            ChampActivite.Clear();

            RafraichirGrille();
        }

        private void ModifierSelection_Click(object sender, RoutedEventArgs e)
        {
            if (Grille.SelectedItem is MeasureDb m)
            {
                _editionId = m.Id;

                ChampGlycemie.Text = m.Glycemie.ToString(CultureInfo.InvariantCulture);
                ChampDate.SelectedDate = m.DateMesure.Date;
                ChampHeure.Text = m.DateMesure.ToString("HH:mm");
                ChampRepas.Text = m.ContexteRepas ?? "";
                ChampActivite.Text = m.Activite ?? "";

                MessageBox.Show("Mode édition : modifiez les champs puis cliquez sur « Ajouter / Enregistrer ».", "Information");
            }
            else
            {
                MessageBox.Show("Sélectionnez d’abord une ligne dans le tableau.", "Information");
            }
        }

        private void SupprimerSelection_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            if (Grille.SelectedItem is MeasureDb m)
            {
                if (MessageBox.Show("Supprimer cette mesure ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    App.Measures.Delete(m.Id, u.Id);

                    if (_editionId == m.Id)
                        _editionId = null;

                    RafraichirGrille();
                }
            }
            else
            {
                MessageBox.Show("Sélectionnez d’abord une ligne dans le tableau.", "Information");
            }
        }

        private void Filtrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            DateTime? depuis = FiltreDepuis.SelectedDate;
            DateTime? jusqua = FiltreJusqua.SelectedDate?.AddDays(1).AddSeconds(-1);

            string repasFiltre = string.IsNullOrWhiteSpace(FiltreRepas.Text) ? null : FiltreRepas.Text.Trim();
            string activiteFiltre = string.IsNullOrWhiteSpace(FiltreActivite.Text) ? null : FiltreActivite.Text.Trim();

            Grille.ItemsSource = App.Measures
                                     .Filter(u.Id, depuis, jusqua, repasFiltre, activiteFiltre)
                                     .OrderBy(m => m.DateMesure)
                                     .ToList();
        }

        private void ReinitialiserFiltres_Click(object sender, RoutedEventArgs e)
        {
            FiltreDepuis.SelectedDate = null;
            FiltreJusqua.SelectedDate = null;
            FiltreRepas.Clear();
            FiltreActivite.Clear();

            RafraichirGrille();
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
               
                shell.GoHome(this, e); 
        }
    }
}