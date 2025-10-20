using System;
using MySql.Data.MySqlClient;
using GlycoWatch_version_prof.Data;
using GlycoWatch_version_prof.Models;
using GlycoWatch_version_prof.Security;

namespace GlycoWatch_version_prof.Repositories
{
    public class AuthRepository
    {
        public UserDb? Login(string pseudo, string password)
        {
            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand("SELECT * FROM users WHERE Pseudo=@p", cn);
            cmd.Parameters.AddWithValue("@p", pseudo.Trim());

            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            var user = Map(rd);

            if (!PasswordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
                return null;

            return user;
        }

        public UserDb Register(string nom, string pseudo, string password, DateTime? dob)
        {
            PasswordHasher.CreateHash(password, out var hash, out var salt);

            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand(@"
INSERT INTO users (Nom, Pseudo, PasswordHash, PasswordSalt, DateNaissance, Unite, Notifications)
VALUES (@n, @p, @h, @s, @d, 'mmol/L', 1);
SELECT LAST_INSERT_ID();", cn);

            cmd.Parameters.AddWithValue("@n", nom.Trim());
            cmd.Parameters.AddWithValue("@p", pseudo.Trim());
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@d", (object?)dob ?? DBNull.Value);

            var id = Convert.ToInt32(cmd.ExecuteScalar());
            return new UserDb { Id = id, Nom = nom, Pseudo = pseudo, DateNaissance = dob };
        }

        private static UserDb Map(MySqlDataReader rd)
        {
            string GetString(string col) =>
                rd.IsDBNull(rd.GetOrdinal(col)) ? "" : rd.GetString(col);

            double? GetDouble(string col) =>
                rd.IsDBNull(rd.GetOrdinal(col)) ? (double?)null : rd.GetDouble(col);

            bool GetBool(string col)
            {
                var i = rd.IsDBNull(rd.GetOrdinal(col)) ? 0 : Convert.ToInt32(rd[col]);
                return i != 0;
            }

            return new UserDb
            {
                Id = rd.GetInt32("Id"),
                Nom = GetString("Nom"),
                Pseudo = GetString("Pseudo"),
                PasswordHash = (byte[])rd["PasswordHash"],
                PasswordSalt = (byte[])rd["PasswordSalt"],
                DateNaissance = rd.IsDBNull(rd.GetOrdinal("DateNaissance")) ? (DateTime?)null : rd.GetDateTime("DateNaissance"),
                Email = GetString("Email"),
                TargetLow = GetDouble("TargetLow"),
                TargetHigh = GetDouble("TargetHigh"),
                Unite = GetString("Unite"),
                Notifications = GetBool("Notifications")
            };
        }
    }
}
