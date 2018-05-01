using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection; //allows us to read classes and extract methods
using System.Data;

namespace DbAccess.Tools
{
   public class DataMapper<T> where T: class
    {
        private static readonly DataMapper<T> _instance = new DataMapper<T>();
        //first thing we need to do is set up properties
        PropertyInfo[] properties;
        private DataMapper()
        {
            properties = typeof(T).GetProperties();
        }
        static DataMapper() { }

        public static DataMapper<T> Instance { get { return _instance; } }

        public T MapToObject(IDataReader reader) // this will return 1 item - take in datareader and return 1 generic type out
        {
            IEnumerable<string> columns = reader.GetSchemaTable().Rows.Cast<DataRow>().Select(
                c => c["ColumnName"].ToString().ToLower()).ToList(); //this will take the data reader take in the rows, cast it to one data row and select one column name. extract as a list

            T obj = Activator.CreateInstance<T>(); // allows us to create an instance (new class)

            foreach (PropertyInfo pinfo in properties)
            {
                if(columns.Contains(pinfo.Name.ToLower())) //contains will check for prop name and lower it
                {
                    if(reader[pinfo.Name] != DBNull.Value) //if null give default value
                    {
                        if(reader[pinfo.Name].GetType() == typeof(decimal))
                        {
                            pinfo.SetValue(obj, reader.GetDouble(pinfo.Name));
                        }
                        else
                        {
                            pinfo.SetValue(obj, (reader.GetValue(reader.GetOrdinal(pinfo.Name)) ?? null), null);
                        }
                    }
                }
            }

            return obj;
        }

    }

    public static class DataHelper
    {
        //extension methods
        public static double GetDouble(this IDataReader reader /*key word is this*/, string columnName)
        {
            double dbl = 0;
            double.TryParse(reader[columnName].ToString(), out dbl);
            return dbl;
        }

        public static double GetDouble(this IDataReader reader, int columnIndex)
        {
            double dbl = 0;
            double.TryParse(reader[columnIndex].ToString(), out dbl);
            return dbl;
        }

    }
}
