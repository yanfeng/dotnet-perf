using Common.Logging;
using Marten;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Nancy.TinyIoc;
using Npgsql;

namespace DotNet.Perf
{
    class Bootstrapper : DefaultNancyBootstrapper
    {
        public Bootstrapper(Program.Options options)
        {
            this.options = options;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer existingContainer)
        {
            base.ConfigureApplicationContainer(existingContainer);

            existingContainer.Register<Program.Options>(options);

            var store = InitializeDocumentStore(options.Database);
            existingContainer.Register<IDocumentStore>(store).AsSingleton();
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Log.InfoFormat("Application starting...");

            base.ApplicationStartup(container, pipelines);

            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;

            DropAndCreateTestingTable();
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            pipelines.BeforeRequest += ctx =>
            {
                //Log.InfoFormat("Request starting. [{0}] {1}", ctx.Request.Method, ctx.Request.Url);

                return null;
            };

            pipelines.AfterRequest += ctx =>
            {
                //Log.InfoFormat("Request ended. [{0}]{1} {2}",
                //    (int) ctx.Response.StatusCode,
                //    !string.IsNullOrEmpty(ctx.Response.ReasonPhrase) ? $" [{ctx.Response.ReasonPhrase}]" : "",
                //    ctx.Request.Url);
            };

            pipelines.OnError += (ctx, ex) =>
            {
                Log.ErrorFormat("An error occurred when processing [{0}] [{1}]", ex, ctx.Request.Method, ctx.Request.Url);

                var error = ex.Message;

                var errorResponse = new JsonResponse(new {Message = error}, new DefaultJsonSerializer())
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = error
                };

                return errorResponse;
            };
            base.RequestStartup(container, pipelines, context);
        }

        private Marten.IDocumentStore InitializeDocumentStore(string connectionString)
        {
            try
            {
                var serializer = new Marten.Services.JsonNetSerializer();
                serializer.Customize(_ =>
                {
                    _.ContractResolver = new PrivateSetterContractResolver();
                });

                var store = DocumentStore
                    .For(_ =>
                    {
                        _.Connection(connectionString);
                        _.Serializer(serializer);
                        _.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
                    });
                return store;
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "3D000")
                {
                    Log.Error("Database does not exist.");
                }

                throw;
            }
        }

        private void DropAndCreateTestingTable()
        {
            string connString = options.Database;
            ExecuteSQL(connString, SQL_DROP_TABLE);
            ExecuteSQL(connString, SQL_CREATING_TABLE_SCHEMA);
        }

        private void ExecuteSQL(string connString, string SQL)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = SQL;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger<Bootstrapper>();
        private readonly Program.Options options;

        #region SQL
        private static readonly string SQL_DROP_TABLE = @"
BEGIN;
    DROP TABLE IF EXISTS public.perf_testing_product;
    DROP TABLE IF EXISTS public.mt_doc_testproduct;
COMMIT;
";

        private static readonly string SQL_CREATING_TABLE_SCHEMA = @"
BEGIN;
    -- Table: public.perf_testing_product

    CREATE TABLE IF NOT EXISTS public.perf_testing_product
    (
        id character varying COLLATE pg_catalog.""default"" NOT NULL,
        data jsonb NOT NULL,
        mt_last_modified timestamp with time zone DEFAULT transaction_timestamp(),
        mt_version uuid NOT NULL DEFAULT(md5(((random())::text || (clock_timestamp())::text)))::uuid,
        mt_dotnet_type character varying COLLATE pg_catalog.""default"",
        CONSTRAINT pk_perf_testing_product PRIMARY KEY(id)
    )
    WITH(
        OIDS = FALSE
    )
    TABLESPACE pg_default;

    -- Index: perf_testing_product_gin_index

    CREATE INDEX IF NOT EXISTS perf_testing_product_gin_index
        ON public.perf_testing_product USING gin
        (data)
        TABLESPACE pg_default;
COMMIT;
";
        #endregion
    }
}