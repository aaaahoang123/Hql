using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Hql.Ultility
{
    public class HqlCommand
    {
        private MySqlCommand cmd;

        public HqlCommand(MySqlCommand cmd)
        {
            this.cmd = cmd;
        }

        public bool AddValue<T>(List<string> fields, T item)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                cmd.Parameters.AddWithValue("@" + fields[i],
                    item.GetType().GetMethod("get_" + fields[i])?.Invoke(item, null));
            }

            return true;
        }

        public bool AddValue<T>(string key, T value)
        {
            cmd.Parameters.AddWithValue(key, value);

            return true;
        }

        public bool AddValue<T>(Dictionary<string, T> key)
        {
            List<string> listKey = new List<string>(key.Keys);
            foreach (string k in listKey)
            {
                cmd.Parameters.AddWithValue("@" + k, key[k]);
            }
            
            return true;
        }

        public int ExecuteNonQuery()
        {
            try
            {
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
            
            return 0;
        }

        public MySqlDataReader ExecuteReader()
        {
            MySqlDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return reader;
        }
    }
}