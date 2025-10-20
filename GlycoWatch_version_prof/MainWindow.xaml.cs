using System.Windows;
using GlycoWatch_version_prof.Views;
using GlycoWatch_version_prof.Models;


namespace GlycoWatch_version_prof
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage()); // page d’accueil
        }


        public void GoHome() => MainFrame.Navigate(new Views.HomePage());
        public void GoLogin() => MainFrame.Navigate(new Views.LoginPage());
        public void GoRegister() => MainFrame.Navigate(new Views.RegisterPage());
        public void GoProfile() => MainFrame.Navigate(new Views.ProfilePage());
        public void GoMeasures() => MainFrame.Navigate(new Views.MeasuresPage());


    }
}
