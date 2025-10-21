namespace GlycoWatch_version_prof.Models
{
    public class UserDb
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string Pseudo { get; set; } = "";
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public DateTime? DateNaissance { get; set; }
        public string Email { get; set; } = "";       // vide si NULL en BD
        public double? TargetLow { get; set; }
        public double? TargetHigh { get; set; }
        public string Unite { get; set; } = "mmol/L";
        public bool Notifications { get; set; } = true;
    }
}
