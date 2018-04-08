using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hql.Ultility
{
    public class SqlStringBuilder
    {
        public static string GetInsertString(string table, List<string> fields)
        {
            StringBuilder isb = new StringBuilder();
            StringBuilder vsb = new StringBuilder();
            
            isb.Append("INSERT INTO ")
                .Append(table)
                .Append("(");
            
            vsb.Append("VALUES(");
            for (int i = 0; i < fields.Count; i++)
            {
                isb.Append(Char.ToLowerInvariant(fields[i][0]) + fields[i].Substring(1));
                vsb.Append("@").Append(fields[i]);
                if (i != fields.Count - 1)
                {
                    isb.Append(",");
                    vsb.Append(",");
                }
            }

            isb.Append(") ");
            vsb.Append(")");

            isb.Append(vsb);
            return isb.ToString();
        }

        public static string GetUpdateString<T>(string table, Dictionary<string, T> key, List<string> fields)
        {
            StringBuilder usb = new StringBuilder();

            usb.Append("UPDATE ")
                .Append(table)
                .Append(" SET ");
            
            foreach (string f in fields)
            {
                usb.Append(Char.ToLowerInvariant(f[0]))
                    .Append(f.Substring(1))
                    .Append("=@")
                    .Append(f);
                if (!fields.Last().Equals(f))
                {
                    usb.Append(",");
                }
            }
            
            string keyField = new List<string>(key.Keys)[0];
            
            usb.Append(" WHERE ")
                .Append(keyField)
                .Append("=@")
                .Append(key[keyField]);
            
            return usb.ToString();
        }

        public static string GetSelectString(string table, List<string> listKeys)
        {
            StringBuilder ssb = new StringBuilder();
            ssb.Append("SELECT * FROM ")
                .Append(table);
            if (listKeys.Count > 0)
            {
                ssb.Append(" WHERE ");
                foreach (string k in listKeys)
                {
                    ssb.Append(k)
                        .Append("=@")
                        .Append(k);
                    if (!listKeys.Last().Equals(k))
                    {
                        ssb.Append(" AND ");
                    }
                }
            }
            
            return ssb.ToString();
        }

        public static string GetDeleteString(string table, List<string> listKey)
        {
            StringBuilder dsb = new StringBuilder();
            dsb.Append("DELETE FROM ")
                .Append(table)
                .Append(" WHERE ");

            for (int i = 0; i < listKey.Count; i++)
            {
                dsb.Append(listKey[i])
                    .Append("=@")
                    .Append(listKey[i]);
                if (i < listKey.Count - 1)
                {
                    dsb.Append(" AND ");
                }
            }

            return dsb.ToString();
        }
    }
}