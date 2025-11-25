using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GlycoWatch_version_prof.Models;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page de gestion des mesures de glycémie.
    /// Permet :
    /// - d'ajouter une nouvelle mesure;
    /// - de modifier une mesure existante;
    /// - de supprimer une mesure;
    /// - de filtrer les mesures affichées.
    /// </summary>
    public partial class PageMesures : Page
    {
        /// <summary>
        /// Identifiant de la mesure en cours d'édition.
        /// Null si on est en mode "ajout".
        /// </summary>
        private int? _editionId = null;

        /// <summary>
        /// Constructeur. Initialise la page et abonne l'événement de chargement.
        /// </summary>
        public PageMesures()
        {
            InitializeComponent();
            Loaded += Page_Chargee;
        }

        /// <summary>
        /// Événement appelé lorsque la page est chargée.
        /// Initialise les champs par défaut et remplit le tableau.
        /// </summary>
        private void Page_Chargee(object? sender, RoutedEventArgs e)
        {
            if (ChampDate.SelectedDate == null)
            {
                ChampDate.SelectedDate = DateTime.Today;
            }

            if (string.IsNullOrWhiteSpace(ChampHeure.Text))
            {
                ChampHeure.Text = "12:00";
            }

            RafraichirTableau();
        }

        /// <summary>
        /// Recharge les mesures de l'utilisateur connecté dans le tableau,
        /// sans filtre particulier.
        /// </summary>
        private void RafraichirTableau()
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

            var mesures = App.Measures
                             .GetByUser(utilisateur.Id)
                             .OrderBy(m => m.DateMesure)
                             .ToList();

            Grille.ItemsSource = mesures;
        }

        /// <summary>
        /// Ajoute une nouvelle mesure ou enregistre la modification d'une mesure existante.
        /// </summary>
        private void AjouterOuEnregistrer_Click(object sender, RoutedEventArgs e)
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

            
            string texteGlycemie = (ChampGlycemie.Text ?? string.Empty).Replace(',', '.');

            if (!double.TryParse(texteGlycemie, NumberStyles.Float, CultureInfo.InvariantCulture, out double valeurGlycemie)
                || valeurGlycemie <= 0)
            {
                MessageBox.Show("La valeur de la glycémie doit être un nombre positif.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Date de la mesure
            DateTime date = ChampDate.SelectedDate ?? DateTime.Today;

            // Heure de la mesure
            TimeSpan heure;
            if (!TimeSpan.TryParse(ChampHeure.Text, out heure))
            {
                MessageBox.Show("L'heure saisie est invalide. Utilisez le format HH:MM, par exemple 08:30.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            DateTime momentMesure = date.Date + heure;

            // Contexte : repas et activité (facultatifs)
            string? repas = string.IsNullOrWhiteSpace(ChampRepas.Text) ? null : ChampRepas.Text.Trim();
            string? activite = string.IsNullOrWhiteSpace(ChampActivite.Text) ? null : ChampActivite.Text.Trim();

            try
            {
                if (_editionId == null)
                {
                    // Ajout d'une nouvelle mesure
                    var nouvelleMesure = new MeasureDb
                    {
                        UserId = utilisateur.Id,
                        DateMesure = momentMesure,
                        Glycemie = valeurGlycemie,
                        ContexteRepas = repas,
                        Activite = activite
                    };

                    App.Measures.Add(nouvelleMesure);
                    MessageBox.Show("Mesure ajoutée avec succès.",
                                    "Information",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                else
                {
                    // Mise à jour d'une mesure existante
                    var mesureModifiee = new MeasureDb
                    {
                        Id = _editionId.Value,
                        UserId = utilisateur.Id,
                        DateMesure = momentMesure,
                        Glycemie = valeurGlycemie,
                        ContexteRepas = repas,
                        Activite = activite
                    };

                    App.Measures.Update(mesureModifiee);
                    _editionId = null;

                    MessageBox.Show("Mesure mise à jour avec succès.",
                                    "Information",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }

                // Réinitialisation des champs de saisie
                ChampGlycemie.Clear();
                ChampRepas.Clear();
                ChampActivite.Clear();
                ChampHeure.Text = "12:00";
                ChampDate.SelectedDate = DateTime.Today;

                RafraichirTableau();
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenue lors de l'enregistrement de la mesure. Veuillez réessayer.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Charge la mesure sélectionnée dans les champs pour la modifier.
        /// </summary>
        private void ModifierSelection_Click(object sender, RoutedEventArgs e)
        {
            if (Grille.SelectedItem is MeasureDb mesureSelectionnee)
            {
                _editionId = mesureSelectionnee.Id;

                ChampGlycemie.Text = mesureSelectionnee.Glycemie.ToString(CultureInfo.InvariantCulture);
                ChampDate.SelectedDate = mesureSelectionnee.DateMesure.Date;
                ChampHeure.Text = mesureSelectionnee.DateMesure.ToString("HH:mm");
                ChampRepas.Text = mesureSelectionnee.ContexteRepas ?? string.Empty;
                ChampActivite.Text = mesureSelectionnee.Activite ?? string.Empty;

                MessageBox.Show("Mode édition : modifiez les champs puis cliquez sur « Ajouter / Enregistrer ».",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Sélectionnez d’abord une ligne dans le tableau.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Supprime la mesure sélectionnée après confirmation.
        /// </summary>
        private void SupprimerSelection_Click(object sender, RoutedEventArgs e)
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

            if (Grille.SelectedItem is MeasureDb mesureSelectionnee)
            {
                var reponse = MessageBox.Show("Voulez-vous vraiment supprimer cette mesure ?",
                                              "Confirmation",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);

                if (reponse == MessageBoxResult.Yes)
                {
                    try
                    {
                        App.Measures.Delete(mesureSelectionnee.Id, utilisateur.Id);

                        if (_editionId == mesureSelectionnee.Id)
                        {
                            _editionId = null;
                        }

                        RafraichirTableau();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Une erreur est survenue lors de la suppression de la mesure.",
                                        "Erreur",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Sélectionnez d’abord une ligne dans le tableau.",
                                "Information",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Applique les filtres choisis (dates, repas, activité) sur les mesures affichées.
        /// </summary>
        private void Filtrer_Click(object sender, RoutedEventArgs e)
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

            string? repasFiltre = string.IsNullOrWhiteSpace(FiltreRepas.Text) ? null : FiltreRepas.Text.Trim();
            string? activiteFiltre = string.IsNullOrWhiteSpace(FiltreActivite.Text) ? null : FiltreActivite.Text.Trim();

            try
            {
                var mesuresFiltrees = App.Measures
                                         .Filter(utilisateur.Id, depuis, jusqua, repasFiltre, activiteFiltre)
                                         .OrderBy(m => m.DateMesure)
                                         .ToList();

                Grille.ItemsSource = mesuresFiltrees;
            }
            catch (Exception)
            {
                MessageBox.Show("Une erreur est survenue lors de l'application des filtres.",
                                "Erreur",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Réinitialise tous les filtres et recharge toutes les mesures.
        /// </summary>
        private void ReinitialiserFiltres_Click(object sender, RoutedEventArgs e)
        {
            FiltreDepuis.SelectedDate = null;
            FiltreJusqua.SelectedDate = null;
            FiltreRepas.Clear();
            FiltreActivite.Clear();

            RafraichirTableau();
        }

        /// <summary>
        /// Retourne à la page d'accueil.
        /// </summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow fenetrePrincipale)
            {
               
                fenetrePrincipale.GoHome();
            }
        }
    }
}
