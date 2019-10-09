using CommandLine;
using CommandLine.Text;
using Common.Logging;
using Nancy.Hosting.Self;
using System;
using System.Diagnostics;

namespace DotNet.Perf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();

            try
            {
                Parser.Default.ParseArgumentsStrict(args, options);
                if (options.ShowVersionInformation)
                {
                    var assembly = typeof(Program).Assembly;
                    var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                    Console.WriteLine(versionInfo.ProductVersion);
                }
                else
                {
                    var bootstrapper = new Bootstrapper(options);

                    var hostConfiguration = new HostConfiguration
                    {
                        UrlReservations = new UrlReservations() { CreateAutomatically = true }
                    };

                    using (var host = new NancyHost(bootstrapper, hostConfiguration, new Uri(options.ServiceUri)))
                    {
                        host.Start();
                        Log.Info(m => m(
                            "{0} is running on {1}", options.ServiceId, options.ServiceUri));
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(m => m(
                    "{0} service starting failed! Error: {1}", options.ServiceId, ex.ToString()));
            }
        }

        private static readonly ILog Log = LogManager.GetLogger<Program>();

        public class Options
        {
            [Option('i', "id", HelpText = "Unique Service ID", DefaultValue = "dotnet-perf")]
            public string ServiceId { get; set; }

            [Option('u', "uri", HelpText = "Service URI, e.g. http://localhost:7002/api/v1/", DefaultValue = "http://localhost:7002/api/v1/")]
            public string ServiceUri { get; set; }

            [Option('d', "db", HelpText = "Database Connection String, e.g. Host=localhost;Port=5444;Database=perf_testing;Username=postgres;Password=123456;Pooling=false;", DefaultValue = "")]
            public string Database { get; set; }

            [Option('t', "timewait", DefaultValue = 0, HelpText = "Timewait (ms) after DB Query operation is done but before the connection is disposed by application.")]
            public int Timewait { get; set; }

            [Option('v', "version", Required = false, HelpText = "Get current version information")]
            public bool ShowVersionInformation { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(
                    this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}
