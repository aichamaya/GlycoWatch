using MySql.Data.MySqlClient;
using GlycoWatch_version_prof.Data;

namespace GlycoWatch_version_prof.Repositories
{
    public class ProfileRepository
    {
        public void UpdateTargets(int userId, double low, double high)
        {
            using var cn = Db.GetConnection();
            using var cmd = new MySqlCommand("UPDATE users SET TargetLow=@lo, TargetHigh=@hi WHERE Id=@id", cn);
            cmd.Parameters.AddWithValue("@lo", low);
            cmd.Parameters.AddWithValue("@hi", high);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.ExecuteNonQuery();
        }
    }
}
