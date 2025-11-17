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
        // Id de la mesure en cours d’édition (null = on ajoute une nouvelle mesure)
        private int? _editionId = null;

        public PageMesures()
        {
            InitializeComponent();
            Loaded += Page_Chargee;
        }

        /// <summary>
        /// Quand la page est chargée : on initialise quelques champs et on charge la grille.
        /// </summary>
        private void Page_Chargee(object sender, RoutedEventArgs e)
        {
            if (ChampDate.SelectedDate == null)
                ChampDate.SelectedDate = DateTime.Today;

            if (string.IsNullOrWhiteSpace(ChampHeure.Text))
                ChampHeure.Text = "12:00";

            RafraichirGrille();
        }

        /// <summary>
        /// Recharge toutes les mesures de l’utilisateur connecté dans la grille.
        /// </summary>
        private void RafraichirGrille()
        {
            var u = App.CurrentUser;
            if (u == null) return;

            // On récupère toutes les mesures de cet utilisateur
            Grille.ItemsSource = App.Measures
                                     .GetByUser(u.Id)
                                     .OrderBy(m => m.DateMesure)
                                     .ToList();
        }

        /// <summary>
        /// Bouton "Ajouter / Enregistrer".
        /// Si _editionId est null -> on ajoute.
        /// Sinon -> on met à jour la mesure.
        /// </summary>
        private void AjouterOuEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            // 1) Lecture + validation de la glycémie
            if (!double.TryParse(ChampGlycemie.Text,
                                 NumberStyles.Float,
                                 CultureInfo.InvariantCulture,
                                 out double gly) || gly <= 0)
            {
                MessageBox.Show("La glycémie doit être un nombre positif.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // 2) Date + heure
            DateTime date = ChampDate.SelectedDate ?? DateTime.Today;
            TimeSpan heure;
            if (!TimeSpan.TryParse(ChampHeure.Text, out heure))
                heure = new TimeSpan(12, 0, 0);   // 12:00 par défaut

            DateTime quand = date.Date + heure;

            // 3) Contexte repas / activité (on met null si vide)
            string repas = (ChampRepas.Text ?? "").Trim();
            string activite = (ChampActivite.Text ?? "").Trim();
            if (repas.Length == 0) repas = null;
            if (activite.Length == 0) activite = null;

            // 4) Ajout ou modification
            if (_editionId == null)
            {
                // Ajout d’une nouvelle mesure
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
                // Mise à jour de la mesure existante
                App.Measures.Update(new MeasureDb
                {
                    Id = _editionId.Value,
                    UserId = u.Id,
                    DateMesure = quand,
                    Glycemie = gly,
                    ContexteRepas = repas,
                    Activite = activite
                });

                _editionId = null; // on sort du mode édition
            }

            // 5) On vide les champs texte et on recharge la grille
            ChampGlycemie.Clear();
            ChampRepas.Clear();
            ChampActivite.Clear();

            RafraichirGrille();
        }

        /// <summary>
        /// Bouton "Modifier la sélection" :
        /// on copie la ligne sélectionnée dans les champs du haut.
        /// </summary>
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

                MessageBox.Show("Mode édition : modifiez les champs puis cliquez sur « Ajouter / Enregistrer ».",
                                "Information");
            }
            else
            {
                MessageBox.Show("Sélectionnez d’abord une ligne dans le tableau.",
                                "Information");
            }
        }

        /// <summary>
        /// Bouton "Supprimer la sélection".
        /// </summary>
        private void SupprimerSelection_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            if (Grille.SelectedItem is MeasureDb m)
            {
                if (MessageBox.Show("Supprimer cette mesure ?",
                                    "Confirmation",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question)
                    == MessageBoxResult.Yes)
                {
                    App.Measures.Delete(m.Id, u.Id);

                    if (_editionId == m.Id)
                        _editionId = null;

                    RafraichirGrille();
                }
            }
            else
            {
                MessageBox.Show("Sélectionnez d’abord une ligne dans le tableau.",
                                "Information");
            }
        }

        /// <summary>
        /// Bouton "Filtrer" (zone du bas) :
        /// on applique les filtres sur la liste des mesures.
        /// </summary>
        private void Filtrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            // Dates de filtre (on laisse null si vide)
            DateTime? depuis = FiltreDepuis.SelectedDate;
            DateTime? jusqua = FiltreJusqua.SelectedDate?.AddDays(1).AddSeconds(-1);

            // Textes de filtre
            string r = (FiltreRepas.Text ?? "").Trim();
            string a = (FiltreActivite.Text ?? "").Trim();

            string repasFiltre = string.IsNullOrEmpty(r) ? null : r;
            string activiteFiltre = string.IsNullOrEmpty(a) ? null : a;

            Grille.ItemsSource = App.Measures
                                     .Filter(u.Id,
                                             depuis,
                                             jusqua,
                                             repasFiltre,
                                             activiteFiltre)
                                     .OrderBy(m => m.DateMesure)
                                     .ToList();
        }

        /// <summary>
        /// Bouton "Réinitialiser les filtres".
        /// On efface les champs de filtre et on recharge toutes les mesures.
        /// </summary>
        private void ReinitialiserFiltres_Click(object sender, RoutedEventArgs e)
        {
            FiltreDepuis.SelectedDate = null;
            FiltreJusqua.SelectedDate = null;
            FiltreRepas.Clear();
            FiltreActivite.Clear();

            RafraichirGrille();
        }

        /// <summary>
        /// Bouton "← Retour" 
        /// </summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.AllerAccueil();
        }
    }
}
