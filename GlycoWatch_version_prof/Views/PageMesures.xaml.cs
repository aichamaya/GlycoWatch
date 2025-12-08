using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GlycoWatch_version_prof.Models;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page de gestion des mesures de glycémie.
    /// Ajout, modification, suppression et filtres pour l'utilisateur connecté.
    /// </summary>
    public partial class PageMesures : Page
    {
        /// <summary>Identifiant de la mesure en cours d'édition (null = ajout).</summary>
        private int? _editionId = null;

        public PageMesures()
        {
            InitializeComponent();
            Loaded += Page_Chargee;
        }

        /// <summary>
        /// Affiche un message dans la barre de statut.
        /// estErreur = true pour un message en rouge, false pour un message en vert.
        /// </summary>
        private void AfficherStatut(string message, bool estErreur = false)
        {
            if (LblEtatMesures == null) return;

            LblEtatMesures.Text = message;
            LblEtatMesures.Foreground = estErreur ? Brushes.DarkRed : Brushes.DarkGreen;
        }

        /// <summary>
        /// Appelé au chargement de la page.
        /// Initialise les champs par défaut et charge les mesures.
        /// </summary>
        private void Page_Chargee(object sender, RoutedEventArgs e)
        {
            ChampDate.SelectedDate ??= DateTime.Today;

            if (string.IsNullOrWhiteSpace(ChampHeure.Text))
                ChampHeure.Text = "12:00";

            RafraichirGrille();
        }

        /// <summary>
        /// Recharge la grille avec toutes les mesures de l'utilisateur courant.
        /// </summary>
        /// <summary>
        /// Recharge la grille avec toutes les mesures de l'utilisateur courant.
        /// </summary>
        private void RafraichirGrille()
        {
            var u = App.CurrentUser;
            if (u == null) return;

            try
            {
                Grille.ItemsSource = App.Measures
                                         .GetByUser(u.Id)
                                         .OrderBy(m => m.DateMesure)
                                         .ToList();

            }
            catch
            {
                AfficherStatut("Erreur lors de l'accès aux mesures. Réessayez plus tard.", true);
            }
        }


        /// <summary>
        /// Ajoute une nouvelle mesure ou enregistre la mesure en cours d'édition.
        /// </summary>
        /// <summary>
        /// Ajoute une nouvelle mesure ou enregistre la mesure en cours d'édition.
        /// </summary>
        private void AjouterOuEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            AfficherStatut("");


            if (!double.TryParse(ChampGlycemie.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double gly) || gly <= 0)
            {
                AfficherStatut("La glycémie doit être un nombre positif.", true);
                return;
            }


            DateTime date = ChampDate.SelectedDate ?? DateTime.Today;
            TimeSpan heure = TimeSpan.TryParse(ChampHeure.Text, out var h) ? h : new TimeSpan(12, 0, 0);
            DateTime quand = date.Date + heure;


            string repas = string.IsNullOrWhiteSpace(ChampRepas.Text) ? null : ChampRepas.Text.Trim();
            string activite = string.IsNullOrWhiteSpace(ChampActivite.Text) ? null : ChampActivite.Text.Trim();

            try
            {
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

                    RafraichirGrille();
                    AfficherStatut("Mesure enregistrée.");
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

                    RafraichirGrille();
                    AfficherStatut("Mesure mise à jour.");
                }


                ChampGlycemie.Clear();
                ChampRepas.Clear();
                ChampActivite.Clear();
            }
            catch
            {
                AfficherStatut("Erreur lors de l'enregistrement dans la base de données.", true);
            }
        }


        /// <summary>
        /// Passe en mode édition à partir de la ligne sélectionnée.
        /// </summary>
        private void ModifierSelection_Click(object sender, RoutedEventArgs e)
        {
            AfficherStatut("");

            if (Grille.SelectedItem is MeasureDb m)
            {
                _editionId = m.Id;

                ChampGlycemie.Text = m.Glycemie.ToString(CultureInfo.InvariantCulture);
                ChampDate.SelectedDate = m.DateMesure.Date;
                ChampHeure.Text = m.DateMesure.ToString("HH:mm");
                ChampRepas.Text = m.ContexteRepas ?? "";
                ChampActivite.Text = m.Activite ?? "";

                AfficherStatut("Modification en cours : validez avec « Ajouter / Enregistrer ».");
            }
            else
            {
                AfficherStatut("Sélectionnez une ligne d'abord.", true);
            }
        }

        /// <summary>
        /// Supprime la mesure sélectionnée.
        /// </summary>
        private void SupprimerSelection_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            AfficherStatut("");

            if (Grille.SelectedItem is not MeasureDb m)
            {
                AfficherStatut("Sélectionnez une ligne d'abord.", true);
                return;
            }



            if (MessageBox.Show("Supprimer cette mesure ?",
                                "Confirmation",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                AfficherStatut("Suppression annulée.");
                return;
            }

            try
            {
                App.Measures.Delete(m.Id, u.Id);

                if (_editionId == m.Id)
                    _editionId = null;

                RafraichirGrille();
                AfficherStatut("Mesure supprimée.");
            }
            catch
            {
                AfficherStatut("Erreur lors de la suppression dans la base de données.", true);
            }
        }


        /// <summary>
        /// Applique les filtres (dates, repas, activité) sur les mesures.
        /// </summary>
        private void Filtrer_Click(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null) return;

            AfficherStatut("");

            DateTime? depuis = FiltreDepuis.SelectedDate;
            DateTime? jusqua = FiltreJusqua.SelectedDate?.AddDays(1).AddSeconds(-1);

            string repas = string.IsNullOrWhiteSpace(FiltreRepas.Text) ? null : FiltreRepas.Text.Trim();
            string activite = string.IsNullOrWhiteSpace(FiltreActivite.Text) ? null : FiltreActivite.Text.Trim();

            try
            {
                Grille.ItemsSource = App.Measures
                                         .Filter(u.Id, depuis, jusqua, repas, activite)
                                         .OrderBy(m => m.DateMesure)
                                         .ToList();

                AfficherStatut("Filtres appliqués.");
            }
            catch
            {
                AfficherStatut("Erreur lors de l'accès aux mesures filtrées.", true);
            }
        }

        /// <summary>
        /// Réinitialise les filtres et recharge toutes les mesures.
        /// </summary>
        private void ReinitialiserFiltres_Click(object sender, RoutedEventArgs e)
        {
            FiltreDepuis.SelectedDate = null;
            FiltreJusqua.SelectedDate = null;
            FiltreRepas.Clear();
            FiltreActivite.Clear();

            RafraichirGrille();
            AfficherStatut("Filtres réinitialisés.");
        }

        /// <summary>
        /// Retour à la page d'accueil.
        /// </summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoHome();
        }
    }
}
