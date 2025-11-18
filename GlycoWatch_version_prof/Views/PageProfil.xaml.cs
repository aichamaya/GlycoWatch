using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Profil utilisateur (nom, pseudo, naissance, cibles glycémiques).
    /// Code débutant + commentaires.
    /// </summary>
    public partial class PageProfil : Page
    {
        public PageProfil()
        {
            InitializeComponent();
            Loaded += PageProfil_Loaded;
        }

        /// <summary>
        /// Quand la page est chargée, on remplit les champs
        /// avec les infos de l'utilisateur courant.
        /// </summary>
        private void PageProfil_Loaded(object sender, RoutedEventArgs e)
        {
            var u = App.CurrentUser;
            if (u == null)
            {
                LblError.Text = "Aucun utilisateur connecté.";
                return;
            }

            TxtNom.Text = u.Nom;
            TxtPseudo.Text = u.Pseudo;

            if (u.DateNaissance.HasValue)
                DpNaissance.SelectedDate = u.DateNaissance.Value;

            if (u.TargetLow.HasValue)
                TxtCibleBasse.Text = u.TargetLow.Value.ToString("0.0", CultureInfo.InvariantCulture);

            if (u.TargetHigh.HasValue)
                TxtCibleHaute.Text = u.TargetHigh.Value.ToString("0.0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Bouton "Retour" : on revient à l'accueil.
        /// </summary>
        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
            {
                shell.AllerAccueil();
            }
        }

        /// <summary>
        /// Bouton "Enregistrer" : on valide les champs et on met
        /// à jour l'objet utilisateur en mémoire.
        /// (La sauvegarde en BD dépend de ton repository → à adapter si besoin.)
        /// </summary>
        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            LblError.Text = string.Empty;

            var u = App.CurrentUser;
            if (u == null)
            {
                LblError.Text = "Aucun utilisateur connecté.";
                return;
            }

            // 1) Récupérer les valeurs des champs
            u.Nom = TxtNom.Text.Trim();
            u.Pseudo = TxtPseudo.Text.Trim();
            u.DateNaissance = DpNaissance.SelectedDate;

            // 2) Parsing des cibles glycémiques (avec . ou ,)
            double? cibleBasse = null;
            double? cibleHaute = null;

            if (!string.IsNullOrWhiteSpace(TxtCibleBasse.Text))
            {
                if (double.TryParse(
                        TxtCibleBasse.Text.Replace(',', '.'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out double low))
                {
                    cibleBasse = low;
                }
                else
                {
                    LblError.Text = "Valeur de cible basse invalide.";
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(TxtCibleHaute.Text))
            {
                if (double.TryParse(
                        TxtCibleHaute.Text.Replace(',', '.'),
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out double high))
                {
                    cibleHaute = high;
                }
                else
                {
                    LblError.Text = "Valeur de cible haute invalide.";
                    return;
                }
            }

            u.TargetLow = cibleBasse;
            u.TargetHigh = cibleHaute;

            // 3) TODO : sauvegarde en base de données selon ton repository
            // Exemple (à adapter à ton projet) :
            // using (var cnx = App.Db.CreateConnection())
            // {
            //     App.UsersRepository.Update(cnx, u);
            // }

            MessageBox.Show("Profil mis à jour.",
                            "Profil",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
