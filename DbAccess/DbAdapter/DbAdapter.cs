using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using DbAccess.Tools;

namespace DbAccess.DbAdapter
{
    public class DbAdapter : IDbAdapter
    {
        public IDbConnection Conn { get; private set; } //dependancy injection to make this class more reusable and flexiable
        public IDbCommand Cmd { get; private set; }

        public DbAdapter(IDbCommand command, IDbConnection conn)
        {
            Cmd = command;
            Conn = conn;
        }

        public List<T> LoadObject<T>(string storedProcedure, IDbDataParameter[] parameters = null) where T : class // this will say this will ONLY take a class (validation)
        {
            List<T> list = new List<T>();


            using (IDbConnection conn = Conn) //using statement will open the connection for us and once the function is done it will close the connection
            using (IDbCommand cmd = Cmd) // does a try catch inside wrapped in one statement --- if fails it'll throw an exception
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open(); // if it's not open execute connectionstate open

                //these cmd are the min requirements for ADO.NET 
                cmd.Connection = conn;
                cmd.CommandTimeout = 5000; // keep open for 5 seconds
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcedure;
                if (parameters != null) // need to assign parameters. if not null then do...
                {
                    foreach (IDbDataParameter parameter in parameters)
                        cmd.Parameters.Add(parameter);
                }
                IDataReader reader = cmd.ExecuteReader(); // the method that will hold our string - this will send to DB and we will get back 1 at a time
                while (reader.Read()) // while it's reading it'll do something for us
                {
                    //TODO: WE NEED TO MAP TO THE OBJECT
                    list.Add(DataMapper<T>.Instance.MapToObject(reader));
                }
            }

            return list;
        }

        public int ExecuteQuery(string storedProcedure, IDbDataParameter[] parameters, 
            //action is our delegate
            Action<IDbDataParameter[]> returnParameters = null) //action is good when you want to get an id back or for a delete
        {
            using (IDbConnection conn = Conn)
            using (IDbCommand cmd = Cmd)
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;
                cmd.CommandTimeout = 5000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcedure;
                foreach (IDbDataParameter parameter in parameters)
                    cmd.Parameters.Add(parameter);
                
                int returnValue = cmd.ExecuteNonQuery();
                if (returnParameters != null)
                {
                    returnParameters(parameters);
                }
                return returnValue; //this will return how many rows that are effected
            }
        }

        public T ExecuteDBScalar<T>(string storedProcedure, IDbDataParameter[] parameters)
        {
            using (IDbConnection conn = Conn)
            using (IDbCommand cmd = Cmd)
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;
                cmd.CommandTimeout = 5000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = storedProcedure;
                foreach (IDbDataParameter parameter in parameters)
                    cmd.Parameters.Add(parameter);

                object obj = cmd.ExecuteScalar(); // only returns ONE column ONE row --- only used when we want only 1 specific data
                return (T)Convert.ChangeType(obj, typeof(T)); // this will convert T to whatever type we have
                // we can also return it as return (T)obj;
            }
        }
    }
}
