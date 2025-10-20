using GlycoWatch_version_prof.Models;

namespace GlycoWatch_version_prof.Models

{
    public class Utilisateur
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NomComplet { get; set; } = string.Empty;
        public DateTime? DateNaissance { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public virtual string Role() => "Utilisateur";
        public override string ToString() => $"{NomComplet} ({Email})";
    }
}
