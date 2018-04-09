using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hql.Ultility;
using MySql.Data.MySqlClient;

namespace Hql.Data
{
    public class HqlModel<T> where T : class, new()
    {
        private String table;
        private PropertyInfo[] pi;
        private MySqlConnection con;

        public HqlModel(string table, MySqlConnection con)
        {
            this.table = table;
            this.con = con;
            this.pi = typeof(T).GetProperties();
        }

        public int InsertOne(T item)
        {
            int af = 0;
            // if item and schema isn't the same type, return 0
            if (typeof(T) != item.GetType())
            {
                Console.WriteLine("The input item and the model isn't the same type");
                return af;
            }

            List<string> listField = FieldsHandle.FilterField(item);

            string insertString = SqlStringBuilder.GetInsertString(table, listField);
            
            if (OpenCon())
            {
                HqlCommand cmd = new HqlCommand(new MySqlCommand(insertString, con));
                cmd.AddValue(listField, item);
                af = cmd.ExecuteNonQuery();
                CloseCon();
            }
            return af;
        }
       
        // return 1 if success, 0 if failed
        public int UpdateOne<U>(Dictionary<string, U> key, T item)
        {
            int af = 0;
            
            // if the item isn't the same type with schema, return 0
            if (typeof(T) != item.GetType())
            {
                Console.WriteLine("The input item and the model isn't the same type");
                return af;
            }

            List<string> listFields = FieldsHandle.FilterField(item);
            string updateString = SqlStringBuilder.GetUpdateString(table, key, listFields);

            if (OpenCon())
            {
                HqlCommand cmd = new HqlCommand(new MySqlCommand(updateString, con));

                cmd.AddValue(listFields, item);
                string keyFields = new List<string>(key.Keys)[0];
                cmd.AddValue(keyFields, key[keyFields]);
                
                af = cmd.ExecuteNonQuery();
                
                CloseCon();
            }

            return af;
        }

        public List<T> Find()
        {
            List<T> list = Find<string>(null);
            return list;
        }

        public List<T> Find<TKey>(Dictionary<string, TKey> condition)
        {
            List<T> listItem = new List<T>();
            List<string> listField = new List<string>();
            if (condition != null) listField = new List<string>(condition.Keys);

            string selectString = SqlStringBuilder.GetSelectString(table, listField);
            if (OpenCon())
            {
                HqlCommand cmd = new HqlCommand(new MySqlCommand(selectString, con));
                if (condition != null)
                {
                    cmd.AddValue(condition);
                }

                Console.WriteLine();
                MySqlDataReader reader = cmd.ExecuteReader();
                
                T obj;

                object[] param;
                while (reader.Read())
                {
                    obj = new T();

                    for (int i = 0; i < pi.Length; i++)
                    {
                        param = new[] {reader[i]};
                        try
                        {
                            obj.GetType().GetMethod("set_" + pi[i].Name)?.Invoke(obj, param.Length == 0 ? null : param);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error when set value for" + pi[i].Name + ": " + reader[i].GetType());
                        }
                    }

                    listItem.Add(obj);
                }

                CloseCon();
            }

            return listItem;
        }

        public int Delete<TKey>(Dictionary<string, TKey> condition)
        {
            int af = 0;
            string deleteString = SqlStringBuilder.GetDeleteString(table, new List<string>(condition.Keys));
            if (OpenCon())
            {
                HqlCommand cmd = new HqlCommand(new MySqlCommand(deleteString));
                cmd.AddValue(condition);
                af = cmd.ExecuteNonQuery();
                CloseCon();
            }
            
            return af; 
        }

        private bool OpenCon()
        {
            try
            {
                con.Open();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        

        private bool CloseCon()
        {
            try
            {
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}