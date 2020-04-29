using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

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

            // Open sql conneciton to build post object
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                // Build post query
                var post_sql_sb = new StringBuilder();
                post_sql_sb.Append("SELECT p.Id, p.Title, p.Slug, p.PubDate, p.IsPublished, p.CategoriesCsv, p.Content, u.DisplayName, u.Email");
                post_sql_sb.Append("FROM cs_Post p LEFT JOIN cs_User u ON p.Author = u.Email");

                if (!string.IsNullOrEmpty(config.blogid))
                    post_sql_sb.Append("WHERE p.BlogId = '" + config.blogid + "'");

                var post_sql = post_sql_sb.ToString();

                // Read each post
                using (var post_command = new SqlCommand(post_sql, connection))
                {
                    using (var post_reader = post_command.ExecuteReader())
                    {
                        while (post_reader.Read())
                        {

                        }
                    }
                }

                // Next
            }
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

    public class Post
    {
        public string id { get; set; }
        public string title { get; set; }
        public string slug { get; set; }
        public string pubdate { get; set; }
        public bool isPublished { get; set; }
        public string categoriescsv { get; set; }
        public string content { get; set; }
        public string displayname { get; set; }
        public string email { get; set; }
        public List<Comment> comments { get; set; }
    }

    public class Comment
    {
        public string id { get; set; }
        public string author { get; set; }
        public string email { get; set; }
        public string website { get; set; }
        public string pubdate { get; set; }
        public string content { get; set; }
    }
}
