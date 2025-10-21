using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GlycoWatch_version_prof.Data;
using GlycoWatch_version_prof.Models;

namespace GlycoWatch_version_prof.Repositories
{
    /// <summary>
    /// Accès BD pour la table 'mesures'
    /// Colonnes: Id, DateMesure, Glycemie, ContexteRepas, Activite, UserId
    /// </summary>
    public class MeasureRepository
    {
        // AJOUT
        public void Add(MeasureDb m)
        {
            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand(@"
INSERT INTO mesures (DateMesure, Glycemie, ContexteRepas, Activite, UserId)
VALUES (@d, @g, @r, @a, @u)", cn);
            cmd.Parameters.AddWithValue("@d", m.DateMesure);
            cmd.Parameters.AddWithValue("@g", m.Glycemie);
            cmd.Parameters.AddWithValue("@r", string.IsNullOrWhiteSpace(m.ContexteRepas) ? (object)DBNull.Value : m.ContexteRepas);
            cmd.Parameters.AddWithValue("@a", string.IsNullOrWhiteSpace(m.Activite) ? (object)DBNull.Value : m.Activite);
            cmd.Parameters.AddWithValue("@u", m.UserId);
            cmd.ExecuteNonQuery();
        }

        // LECTURE TOUTES LES MESURES D'UN USER
        public IEnumerable<MeasureDb> GetByUser(int userId)
        {
            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand(
                "SELECT * FROM mesures WHERE UserId=@u ORDER BY DateMesure DESC", cn);
            cmd.Parameters.AddWithValue("@u", userId);
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) yield return Map(rd);
        }

        // FILTRE : par date (from/to), et textes partiels (repas, activité)
        public IEnumerable<MeasureDb> Filter(int userId, DateTime? from, DateTime? to,
                                             string? repasContains, string? activiteContains)
        {
            using var cn = Db.GetConnection();
            var where = "WHERE UserId=@u";
            if (from.HasValue) where += " AND DateMesure >= @from";
            if (to.HasValue) where += " AND DateMesure <= @to";
            if (!string.IsNullOrWhiteSpace(repasContains)) where += " AND ContexteRepas LIKE @r";
            if (!string.IsNullOrWhiteSpace(activiteContains)) where += " AND Activite LIKE @a";

            var sql = $@"SELECT * FROM mesures {where} ORDER BY DateMesure DESC";
            using var cmd = new MySqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@u", userId);
            if (from.HasValue) cmd.Parameters.AddWithValue("@from", from.Value);
            if (to.HasValue) cmd.Parameters.AddWithValue("@to", to.Value);
            if (!string.IsNullOrWhiteSpace(repasContains)) cmd.Parameters.AddWithValue("@r", $"%{repasContains}%");
            if (!string.IsNullOrWhiteSpace(activiteContains)) cmd.Parameters.AddWithValue("@a", $"%{activiteContains}%");

            using var rd = cmd.ExecuteReader();
            while (rd.Read()) yield return Map(rd);
        }

        // MISE À JOUR
        public void Update(MeasureDb m)
        {
            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand(@"
UPDATE mesures
SET DateMesure=@d, Glycemie=@g, ContexteRepas=@r, Activite=@a
WHERE Id=@id AND UserId=@u", cn);
            cmd.Parameters.AddWithValue("@d", m.DateMesure);
            cmd.Parameters.AddWithValue("@g", m.Glycemie);
            cmd.Parameters.AddWithValue("@r", string.IsNullOrWhiteSpace(m.ContexteRepas) ? (object)DBNull.Value : m.ContexteRepas);
            cmd.Parameters.AddWithValue("@a", string.IsNullOrWhiteSpace(m.Activite) ? (object)DBNull.Value : m.Activite);
            cmd.Parameters.AddWithValue("@id", m.Id);
            cmd.Parameters.AddWithValue("@u", m.UserId);
            cmd.ExecuteNonQuery();
        }

        // SUPPRESSION
        public void Delete(int id, int userId)
        {
            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand(
                "DELETE FROM mesures WHERE Id=@id AND UserId=@u", cn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.ExecuteNonQuery();
        }

        // Mapping d'une ligne SQL -> objet C#
        private static MeasureDb Map(MySqlDataReader rd)
        {
            string GetString(string col) =>
                rd.IsDBNull(rd.GetOrdinal(col)) ? "" : rd.GetString(col);

            return new MeasureDb
            {
                Id = rd.GetInt32("Id"),
                DateMesure = rd.GetDateTime("DateMesure"),
                Glycemie = rd.GetDouble("Glycemie"),
                ContexteRepas = GetString("ContexteRepas"),
                Activite = GetString("Activite"),
                UserId = rd.GetInt32("UserId")
            };
        }
    }
}
