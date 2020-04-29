using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;

namespace CloudScribeToJekyll
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read configuration file
            var config = JsonConvert.DeserializeObject<CloudScribeSettings>(File.ReadAllText("cloudscribe.json"));

            // Create sql connection
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = config.server,
                UserID = config.username,
                Password = config.password,
                InitialCatalog = config.database
            };
        }
    }

    public class CloudScribeSettings
    {
        public string server { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string database { get; set; }
        public string filetype { get; set; }
        public string blogid { get; set; }
        public bool comments { get; set; }
        public bool categories { get; set; } 
    }
}
