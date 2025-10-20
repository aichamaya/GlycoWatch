using System.Windows;
using System.Windows.Controls;

namespace GlycoWatch_version_prof.Views
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var u = App.CurrentUser;
                TxtWelcome.Text = u is null ? "GlycoWatch"
                                            : $"Bonjour, {u.Nom} 👋";
            };
        }

        private void GoProfil(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoProfile();
        }

        private void GoMesures(object s, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoMeasures();
        }

        private void Logout(object s, RoutedEventArgs e)
        {
            App.CurrentUser = null;
            if (Window.GetWindow(this) is MainWindow shell)
                shell.GoLogin();
        }
    }
}
