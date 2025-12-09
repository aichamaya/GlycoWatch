using MySql.Data.MySqlClient;
using System.Configuration;

namespace GlycoWatch_version_prof.Data
{
    public static class Db
    {
        private const string ConnStr =
      "Server=127.0.0.1;" +
      "Port=3306;Database=glyco;Uid=glyco_user;Pwd=TonPass!123;" +
            "SslMode=None;AllowPublicKeyRetrieval=True;";
       
        public static MySqlConnection GetConnection()
        {
            var cn = new MySqlConnection(ConnStr);
            cn.Open();
            return cn;
        }
    }
}
