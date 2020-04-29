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
            // Create Posts object
            var Posts = new List<Post>();

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
                post_sql_sb.Append("SELECT p.Id, p.Title, p.Slug, p.PubDate, p.IsPublished, p.CategoriesCsv, p.Content, u.DisplayName, u.Email ");
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
                            Posts.Add(new Post()
                            {
                                id = post_reader[0].ToString(),
                                title = post_reader[1].ToString(),
                                slug = post_reader[2].ToString(),
                                pubdate = post_reader[3].ToString(),
                                isPublished = (post_reader[4].ToString() == "1"),
                                categoriescsv = post_reader[5].ToString(),
                                content = post_reader[6].ToString(),
                                displayname = post_reader[7].ToString(),
                                email = post_reader[8].ToString()
                            });
                        }
                    }
                }

                // If we need to get comments, let's grab the comments and update accordingly
                if (config.comments)
                {
                    // Build comment query
                    var post_comment_sb = new StringBuilder();
                    post_comment_sb.Append("SELECT Id, Content, PubDate, Author, Email, Website FROM cs_PostComment ");
                    post_comment_sb.Append("WHERE IsApproved = 1 AND PostEntityId = '{0}'");

                    var post_comment = post_comment_sb.ToString();

                    foreach (var post_for_comment in Posts)
                    {
                        post_for_comment.comments = new List<Comment>();

                        // Read each post
                        using (var comment_command = new SqlCommand(post_comment.Replace("{0}", post_for_comment.id), connection))
                        {
                            using (var comment_reader = comment_command.ExecuteReader())
                            {
                                while (comment_reader.Read())
                                {
                                    post_for_comment.comments.Add(new Comment()
                                    {
                                        id = comment_reader[0].ToString(),
                                        content = comment_reader[1].ToString(),
                                        pubdate = comment_reader[2].ToString(),
                                        author = comment_reader[3].ToString(),
                                        email = comment_reader[4].ToString(),
                                        website = comment_reader[5].ToString()
                                    });
                                }
                            }
                        }
                    } // end foreach
                } //end if comments
            }

            // Now that SQL is done, we can start processing the files.
            
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
        public string jekyll_location { get; set; }
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
