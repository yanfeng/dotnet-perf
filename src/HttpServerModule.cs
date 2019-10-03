using Common.Logging;
using DotNet.Perf.Models;
using Marten;
using Nancy;
using Nancy.Responses;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DotNet.Perf
{
    public class HttpServerModule : NancyModule
    {
        public HttpServerModule(Program.Options options, IDocumentStore documentStore)
        {
            Get["HttpServer", "/servers/default"] = HttpServer;

            Get["HttpServer to test npgsql query operation", "/servers/npgsql"] = HttpServerWithNpgsqlQuery;
            Post["HttpServer to test npgsql insert operation", "/servers/npgsql-insert"] = HttpServerWithNpgsqlInsert;
            Put["HttpServer to test npgsql update operation", "/servers/npgsql-update"] = HttpServerWithNpgsqlUpdate;

            Get["HttpServer to test marten query operation", "/servers/marten"] = HttpServerWithMarten;

            this.options = options;
            this.documentStore = documentStore;
        }

        private Response HttpServer(dynamic parameters)
        {
            return Response.AsJson(HttpStatusCode.OK);
        }

        private Response HttpServerWithNpgsqlQuery(dynamic parameters)
        {
            var response = RunDbFunction(options.Database, () =>
            {
                using (var conn = new NpgsqlConnection(options.Database))
                {
                    conn.Open();

                    // Retrieve all rows
                    using (var cmd = new NpgsqlCommand(QuerySQL, conn))
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                        {
                            reader.GetString(0);
                            //Log.Info(reader.GetString(0));
                        }

                    Thread.Sleep(options.Timewait);
                }
            });

            return response;
        }

        private Response HttpServerWithNpgsqlInsert(dynamic parameters)
        {
            var response = RunDbFunction(options.Database, () =>
            {
                Product prod = new Product("Test Prod", PROD_DESC);

                using (var conn = new NpgsqlConnection(options.Database))
                {
                    conn.Open();

                    // Insert some data
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = InsertSQL;
                        //@id, @data, @last_modified, @version, @dotnet_type
                        cmd.Parameters.AddWithValue("id", prod.Id);
                        cmd.Parameters.AddWithValue("data", NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(prod));
                        cmd.Parameters.AddWithValue("last_modified", DateTimeOffset.Now);
                        cmd.Parameters.AddWithValue("version", Guid.NewGuid());
                        cmd.Parameters.AddWithValue("dotnet_type", prod.GetType().ToString());
                        cmd.ExecuteNonQuery();
                    }
                }
            });

            return response;
        }

        private Response HttpServerWithNpgsqlUpdate(dynamic parameters)
        {
            // cache the ids of the inserted records
            if (InsertedIds.IsEmpty())
            {
                InsertedIds.AddRange(GetInsertedIds());
            }

            if (InsertedIds.Count == 0)
            {
                throw new Exception("Please call 'insert' api to insert some records for updating!");
            }

            int randomIndex = new Random().Next(0, InsertedIds.Count);
            string theId = InsertedIds[randomIndex];
            Product prod = new Product("Updated: Test Prod", "Updated: " + PROD_DESC);
            prod.SetId(theId);

            var response = RunDbFunction(options.Database, () =>
            {
                using (var conn = new NpgsqlConnection(options.Database))
                {
                    conn.Open();

                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = UpdateSQL;
                        //@data, @id
                        cmd.Parameters.AddWithValue("data", NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(prod));
                        cmd.Parameters.AddWithValue("id", prod.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            });

            return response;
        }

        private Response HttpServerWithMarten(dynamic parameters)
        {
            var response = RunDbFunction(options.Database, () =>
            {
                using (var session = documentStore.QuerySession())
                {
                    session.Query<string>(QuerySQL);

                    Thread.Sleep(options.Timewait);
                }
            });

            return response;
        }

        private List<string> GetInsertedIds()
        {
            List<string> theIds = new List<string>();

            RunDbFunction(options.Database, () =>
            {
                using (var conn = new NpgsqlConnection(options.Database))
                {
                    conn.Open();

                    // Retrieve all rows
                    using (var cmd = new NpgsqlCommand("SELECT id FROM perf_testing_product", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            theIds.Add(reader.GetString(0));
                        }
                    }
                }
            });

            return theIds;
        }

        private Response RunDbFunction(string conn, Action func)
        {
            if (string.IsNullOrEmpty(conn))
            {
                var errorResponse = new JsonResponse(new { Message = "DB Error" }, new DefaultJsonSerializer())
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = "Database connection string is missing."
                };

                return errorResponse;
            }

            try
            {
                func();
            }
            catch (Exception ex)
            {
                Log.Error(ex);

                var errorResponse = new JsonResponse(new { Message = ex.ToString() }, new DefaultJsonSerializer())
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = ex.Message
                };

                return errorResponse;
            }

            return Response.AsJson(HttpStatusCode.OK);
        }

        private readonly Program.Options options;
        private readonly IDocumentStore documentStore;
        private static readonly ILog Log = LogManager.GetLogger<HttpServerModule>();
        private static readonly List<string> InsertedIds = new List<string>();
        private static readonly string QuerySQL = "SELECT '1' FROM pg_stat_activity";
        private static readonly string InsertSQL = @"
INSERT INTO public.perf_testing_product(
	id, data, mt_last_modified, mt_version, mt_dotnet_type)
	VALUES (@id, @data, @last_modified, @version, @dotnet_type);";
        private static readonly string UpdateSQL = @"
UPDATE public.perf_testing_product
	SET data=@data
	WHERE id=@id;";
        private static readonly string PROD_DESC = @"
A product description is the marketing copy used to describe a product’s value proposition to potential customers. A compelling product description provides customers with details around features, problems it solves and other benefits to help generate a sale.It’s no wonder they are worried — the quality of a product description can make or break a sale, especially if it doesn’t include the information a shopper needs to make a purchase decision. Providing key product details is critical if you want the shopper to click “Add to Cart” and differentiate your ecommerce website from the competition.Whether your products have a specific function, like a camera, or a personal purpose, like fashion, all products exist to enhance or improve the purchaser’s quality of life in one way or another. As the shopper browses, they instinctively imagine having each product in hand, using it and enjoying it.
";
    }
}
