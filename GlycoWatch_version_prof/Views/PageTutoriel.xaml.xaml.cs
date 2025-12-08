using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    /// <summary>
    /// Page qui affiche une vidéo de tutoriel locale pour l'utilisateur.
    /// </summary>
    public partial class PageTutoriel : Page
    {
        public PageTutoriel()
        {
            InitializeComponent();
            ChargerVideo();
        }

        /// <summary>
        /// Charge la vidéo "tutoriel_glycowatch.mp4" depuis le dossier Videos du projet.
        /// </summary>
        private void ChargerVideo()
        {
            try
            {

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dossierVideo = Path.Combine(baseDir, "Videos");
                string cheminVideo = Path.Combine(dossierVideo, "tutoriel_glycowatch.mp4");

                if (File.Exists(cheminVideo))
                {
                    VideoTutoriel.Source = new Uri(cheminVideo, UriKind.Absolute);
                    TxtInfo.Text = "La vidéo est prête. Cliquez sur Lecture pour démarrer.";
                }
                else
                {
                    TxtInfo.Text = "Vidéo introuvable. Placez « tutoriel_glycowatch.mp4 » dans le dossier Videos.";
                }
            }
            catch
            {
                TxtInfo.Text = "Erreur lors du chargement de la vidéo.";
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            VideoTutoriel.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            VideoTutoriel.Pause();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            VideoTutoriel.Stop();
            VideoTutoriel.Play();
        }

        private void Retour_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
            {
                shell.AllerAccueil();
            }
        }
    }
}
