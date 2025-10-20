using System;

namespace GlycoWatch_version_prof.Models
{
    public class MeasureDb
    {
        public int Id { get; set; }
        public DateTime DateMesure { get; set; }
        public double Glycemie { get; set; }

        // doit matcher la colonne MySQL `ContexteRepas`
        public string? ContexteRepas { get; set; }

        public string? Activite { get; set; }
        public int UserId { get; set; }
    }
}
