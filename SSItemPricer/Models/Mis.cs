using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace SSItemPricer.Models
{
    public class Mis : ObservableObject
    {
        private const string ConnectionString = "Server=10.0.0.25,1400; Database=MIS; Uid=web123; Pwd=web123";

        public static T FindOne<T>(string sql) where T : Mis
        {
            var result = FindOne(sql);

            return result.Count == 0 ? default : Create<T>(result);
        }

        public static List<T> FindMany<T>(string sql) where T : Mis
        {
            var results = FindMany(sql);

            return results.Select(Create<T>).ToList();
        }

        private static T Create<T>(IReadOnlyDictionary<string, object> result) where T : Mis
        {
            var type = typeof(T);
            var record = (T) Activator.CreateInstance(type);

            foreach (var propertyInfo in type.GetProperties())
                if (result.ContainsKey(propertyInfo.Name))
                {
                    var value = result[propertyInfo.Name];
                    
                    if(value != DBNull.Value) 
                        propertyInfo.SetValue(record,  value);
                }

            return record;
        }

        private static Dictionary<string, object> FindOne(string query)
        {
            using var conn = new SqlConnection(ConnectionString);
            
            conn.Open();

            var command = new SqlCommand(query, conn);

            using var reader = command.ExecuteReader();
            
            reader.Read();

            return reader.HasRows
                ? GetRecord(reader)
                : new Dictionary<string, object>();
        }

        private static List<Dictionary<string, object>> FindMany(string query)
        {
            var results = new List<Dictionary<string, object>>();

            using var conn = new SqlConnection(ConnectionString);
            
            conn.Open();

            var command = new SqlCommand(query, conn);

            using var reader = command.ExecuteReader();
            
            while (reader.Read())
                results.Add(GetRecord(reader));

            return results;
        }

        private static Dictionary<string, object> GetRecord(IDataRecord reader)
        {
            var result = new Dictionary<string, object>();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader[i];

                result[name] = value;
            }

            return result;
        }

    }
}