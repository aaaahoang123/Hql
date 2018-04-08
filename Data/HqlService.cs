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

        public static HqlModel GetModel(String table, Type schema)
        {
            return new HqlModel(table, schema, _connection);
        }
    }
}