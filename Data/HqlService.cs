using System;
using MySql.Data.MySqlClient;

namespace Hql.Data
{
    public class HqlService
    {
        private static MySqlConnection _connection;

        public static void Connect(String url)
        {
            _connection = new MySqlConnection(url);
        }

        public static HqlModel<T> GetModel<T>(String table) where T : class, new()
        {
            return new HqlModel<T>(table, _connection);
        }
    }
}