using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hql.Ultility
{
    public class FieldsHandle
    {
        public static List<string> FilterField<T>(T item)
        {
            List<string> listField = new List<string>();

            foreach (PropertyInfo p in item.GetType().GetProperties())
            {
                if (NotNullStringProperty(p, item) && NotZeroIntProperty(p, item))
                {
                    listField.Add(p.Name);
                }
            }

            return listField;
        }

        private static bool NotNullStringProperty<T>(PropertyInfo p, T item)
        {
            return p.GetValue(item) != null;
        }

        private static bool NotZeroIntProperty<T>(PropertyInfo p, T item)
        {
            try
            {
                return (int) p.GetValue(item) != 0;
            }
            catch (Exception e)
            {
                try
                {
                    return (long) p.GetValue(item) != 0;
                }
                catch (Exception exception)
                {
                    return true;
                }
            }
        }
    }
}