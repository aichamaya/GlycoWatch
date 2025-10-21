namespace GlycoWatch_version_prof.Models
{
    public class Patient : Utilisateur
    {
        public double CibleMin { get; set; } = 4.0;   // mmol/L
        public double CibleMax { get; set; } = 10.0;  // mmol/L

        public override string Role() => "Patient";
    }
}
