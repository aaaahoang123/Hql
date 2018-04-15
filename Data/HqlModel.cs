using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Hql.Ultility;
using MySql.Data.MySqlClient;

namespace Hql.Data
{
    public class HqlModel<T> where T : class, new()
    {
        private MySqlTransaction tran;
        private String table;
        private PropertyInfo[] pi;
        private MySqlConnection con;
        private MySqlCommand mySqlCommand;

        public HqlModel(string table, MySqlConnection con)
        {
            this.table = table;
            this.con = con;
            mySqlCommand = con.CreateCommand();
            this.pi = typeof(T).GetProperties();
        }

        // In default, the insert one method will close the connection
        public int InsertOne(T item)
        {
            return InsertOne(item, false);
        }
        
        public int InsertOne(T item, bool isInTransaction)
        {
            int af = 0;
            // if item and schema isn't the same type, return 0
            if (typeof(T) != item.GetType())
            {
                Console.WriteLine("The input item and the model isn't the same type");
                return af;
            }

            List<string> listField = FieldsHandle.FilterField(item);
            
            // gen the insert string
            string insertString = SqlStringBuilder.GetInsertString(table, listField);

            // check if the connection is opened
            bool conOpened = con.State == ConnectionState.Open;
            // if not, open it
            if (!conOpened) conOpened = OpenCon();
            
            if (conOpened)
            {
                mySqlCommand.CommandText = insertString; mySqlCommand.Parameters.Clear();
                if (isInTransaction) mySqlCommand.Transaction = tran;
                else mySqlCommand.Transaction = null;
                HqlCommand cmd = new HqlCommand(mySqlCommand);
                cmd.AddValue(listField, item);
                af = cmd.ExecuteNonQuery();
                // if the param willCloseCon == true, close the connection
                if (!isInTransaction) CloseCon();
            }
            return af;
        }

        public int UpdateOne<U>(Dictionary<string, U> key, T item)
        {
            return UpdateOne(key, item, false);
        }
        // return 1 if success, 0 if failed
        public int UpdateOne<U>(Dictionary<string, U> key, T item, bool isInTransaction)
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

            bool conOpened = con.State == ConnectionState.Open;
            if (!conOpened) conOpened = OpenCon();
            
            if (conOpened)
            {
                mySqlCommand.CommandText = updateString; mySqlCommand.Parameters.Clear();
                if (isInTransaction) mySqlCommand.Transaction = tran;
                else mySqlCommand.Transaction = null;
                HqlCommand cmd = new HqlCommand(mySqlCommand);

                cmd.AddValue(listFields, item);
                string keyFields = new List<string>(key.Keys)[0];
                cmd.AddValue(keyFields, key[keyFields]);
                
                af = cmd.ExecuteNonQuery();
                
                if (!isInTransaction) CloseCon();
            }

            return af;
        }

        public List<T> Find()
        {
            return Find<string>(null);
        }

        public List<T> Find<TKey>(Dictionary<string, TKey> condition)
        {
            return Find<TKey>(condition, false);
        }

        public List<T> Find<TKey>(Dictionary<string, TKey> condition, bool isInTransaction)
        {
            List<T> listItem = new List<T>();
            List<string> listField = new List<string>();
            if (condition != null) listField = new List<string>(condition.Keys);

            string selectString = SqlStringBuilder.GetSelectString(table, listField);
            
            bool conOpened = con.State == ConnectionState.Open;
            if (!conOpened) conOpened = OpenCon();
            
            if (conOpened)
            {
                mySqlCommand.CommandText = selectString; mySqlCommand.Parameters.Clear();
                if (isInTransaction) mySqlCommand.Transaction = tran;
                else mySqlCommand.Transaction = null;
                HqlCommand cmd = new HqlCommand(mySqlCommand);
                if (condition != null)
                {
                    cmd.AddValue(condition);
                }

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

                reader.Close();
                if (!isInTransaction) CloseCon();
            }

            return listItem;
        }

        public int Delete<TKey>(Dictionary<string, TKey> condition)
        {
            int af = 0;
            string deleteString = SqlStringBuilder.GetDeleteString(table, new List<string>(condition.Keys));
            if (OpenCon())
            {
                mySqlCommand.CommandText = deleteString; mySqlCommand.Parameters.Clear();
                HqlCommand cmd = new HqlCommand(mySqlCommand);
                cmd.AddValue(condition);
                af = cmd.ExecuteNonQuery();
                CloseCon();
            }
            
            return af; 
        }
        
        /// <Note>
        /// ExecuteNonQuery with another command string, that can work with another table, that difference from this.table
        /// This method help the user work with multi table when open a transaction
        /// </Note>
        public int ExecuteNonQuery<TKey>(string scmd)
        {
            return ExecuteNonQuery<TKey>(scmd, null);
        }

        public int ExecuteNonQuery<TKey>(string scmd, Dictionary<string, TKey> value)
        {
            return ExecuteNonQuery<TKey>(scmd, value, false);
        }
        
        public int ExecuteNonQuery<TKey>(string scmd, Dictionary<string, TKey> value, bool isInTransaction)
        {
            int af = 0;
            bool conOpened = con.State == ConnectionState.Open;
            if (!conOpened) conOpened = OpenCon();

            if (conOpened)
            {
                mySqlCommand.CommandText = scmd; mySqlCommand.Parameters.Clear();
                HqlCommand cmd = new HqlCommand(mySqlCommand);
                if (value != null) cmd.AddValue(value);
                af = cmd.ExecuteNonQuery();
                if (!isInTransaction) CloseCon();
            }

            return af;
        }

        public MySqlDataReader ExecuteReader<TKey>(string scmd)
        {
            return ExecuteReader<TKey>(scmd, null);
        }
        
        public MySqlDataReader ExecuteReader<TKey>(string scmd, Dictionary<string, TKey> value)
        {
            return ExecuteReader<TKey>(scmd, value, false);
        }

        public MySqlDataReader ExecuteReader<TKey>(string scmd, Dictionary<string, TKey> value, bool isInTransaction)
        {
            MySqlDataReader reader = null;
            bool conOpened = con.State == ConnectionState.Open;
            if (!conOpened) conOpened = OpenCon();

            if (conOpened)
            {
                mySqlCommand.CommandText = scmd; mySqlCommand.Parameters.Clear();
                HqlCommand cmd = new HqlCommand(mySqlCommand);
                if (value != null) cmd.AddValue(value);
                reader = cmd.ExecuteReader();
                if (!isInTransaction) CloseCon();
            }

            return reader;
        }

        public void HqlTransaction(Action<MySqlTransaction> action)
        {
            OpenCon();
            tran = con.BeginTransaction();
            action(tran);
            
            CloseCon();
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