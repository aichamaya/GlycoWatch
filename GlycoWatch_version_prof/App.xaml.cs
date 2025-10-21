using GlycoWatch_version_prof.Models;
using GlycoWatch_version_prof.Repositories;
using System.Windows;

namespace GlycoWatch_version_prof
{
    public partial class App : Application
    {
        public static AuthRepository Auth { get; } = new AuthRepository();
        public static ProfileRepository Profiles { get; } = new ProfileRepository();
        public static MeasureRepository Measures { get; } = new MeasureRepository();

        public static UserDb? CurrentUser { get; set; }   // ← remplace PatientCourant
    }
}
