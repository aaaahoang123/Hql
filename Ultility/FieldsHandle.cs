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
            return item.GetType().GetMethod("get_" + p.Name)?.Invoke(item, null) != null;
        }

        private static bool NotZeroIntProperty<T>(PropertyInfo p, T item)
        {
            try
            {
                return (int) item.GetType().GetMethod("get_" + p.Name)?.Invoke(item, null) != 0;
            }
            catch (Exception e)
            {
                try
                {
                    return (long) item.GetType().GetMethod("get_" + p.Name)?.Invoke(item, null) != 0;
                }
                catch (Exception exception)
                {
                    return true;
                }
            }
        }
    }
}