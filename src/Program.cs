using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Stressi
{
    public class Program
    {
        #region Properties

        /// <summary>
        /// Loaded config.
        /// </summary>
        public static Config LoadedConfig { get; set; }

        /// <summary>
        /// Stats for this run.
        /// </summary>
        public static Stats RunStats { get; set; }

        #endregion

        /// <summary>
        /// Init all the things..
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        private static void Main(string[] args)
        {
            // Setup a new stats object.
            RunStats = new Stats
            {
                ResponseTimes = new List<long>()
            };

            // Store the cmd-args for later use.
            ConsoleEx.Init(args);

            // Show app version?
            if (ConsoleEx.IsSwitchPresent("v", "version"))
            {
                ShowVersion();
                return;
            }

            // Show app usage and info?
            if (ConsoleEx.IsSwitchPresent("h", "help") ||
                ConsoleEx.NoOptions())
            {
                ShowHelp();
                return;
            }

            // Prepare config.
            LoadedConfig = new Config
            {
                Url = ConsoleEx.GetArgValue("u", "url"),
                HttpMethod = ConsoleEx.GetArgValue("m", "method"),
                ConcurrentUsers = ConsoleEx.GetArgValueAsInt32("s", "users"),
                Repetitions = ConsoleEx.GetArgValueAsInt32("r", "reps"),
                Verbose = ConsoleEx.IsSwitchPresent("b", "verbose"),
                UserAgent = ConsoleEx.GetArgValue("a", "user-agent"),
                Headers = ConsoleEx.GetArgValueAsDictionary("e", "headers"),
                Timeout = ConsoleEx.GetArgValueAsInt32("t", "timeout")
            };

            // Check for max values.
            if (LoadedConfig.ConcurrentUsers.HasValue &&
                LoadedConfig.ConcurrentUsers.Value == -1)
            {
                LoadedConfig.ConcurrentUsers = long.MaxValue;
            }

            if (LoadedConfig.Repetitions.HasValue &&
                LoadedConfig.Repetitions.Value == -1)
            {
                LoadedConfig.Repetitions = long.MaxValue;
            }

            // Do we have a URL?
            if (LoadedConfig.Url == null)
            {
                ConsoleEx.WriteError("URL param is required!");
                return;
            }

            // Run the actual app.
            RunStressi();
        }

        #region Helper functions

        /// <summary>
        /// Show app usage and info.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Usage: stressi [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -v | --version               Show the app version.");
            Console.WriteLine("  -h | --help                  Show the app usage and help information.");
            Console.WriteLine("  -u | --url <url>             The URL to use for each request. Required!");
            Console.WriteLine("  -m | --method <method>       The HTTP method to use for each request. Defaults to 'GET'.");
            Console.WriteLine("  -s | --users <number>        Number of concurrent users to simulate. Defaults to 10.");
            Console.WriteLine("  -r | --reps <number>         Number of repetitions pr. user. Defaults to 10.");
            Console.WriteLine("  -b | --verbose               Turn on verbose mode, which shows a lot more console output.");
            Console.WriteLine("  -a | --user-agent <string>   Set the user-agent to use.");
            Console.WriteLine("  -e | --headers <string>      Comma-list of key:value, like so: key1:value1,key2:value2");
            Console.WriteLine("  -t | --timeout <number>      Set the timeout for each request to N ms.");

            Console.WriteLine();
            ConsoleEx.WriteLineWordWrapped("If a value for one of the options has spaces in it, you can use quotation marks around the string, " +
                                           "like so: \"this will all be the same value\"");

            Console.WriteLine();
            ConsoleEx.WriteLineWordWrapped("Number of users and repetitions pr. user determines the total number of requests that will be performed. " +
                                           "They both default to 10, which means 100 total requests.");

            Console.WriteLine();
            ConsoleEx.WriteLineWordWrapped($"For both options -u and -r you can supply -1 as a value to indicate it to use the max value of a " +
                                           $"int64, which is {long.MaxValue}, which will basically run forever..");

            Console.WriteLine();
        }

        /// <summary>
        /// Show app version.
        /// </summary>
        private static void ShowVersion()
        {
            Console.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        #endregion

        #region Main application functions

        /// <summary>
        /// Run the actual app.
        /// </summary>
        private static void RunStressi()
        {
            var maxUsers = LoadedConfig.ConcurrentUsers ??
                           DefaultConfig.ConcurrentUsers;

            var maxRepetitions = LoadedConfig.Repetitions ??
                                 DefaultConfig.Repetitions;

            var total = maxUsers * maxRepetitions;

            Console.WriteLine($" # Spinning up {maxUsers} users with {maxRepetitions} requests per user " +
                              $"for a total of {total} request against {LoadedConfig.Url}");

            Console.WriteLine();

            RunStats.Started = DateTimeOffset.Now;

            Parallel.For(0, maxUsers, _ => CreateUser());

            RunStats.Ended = DateTimeOffset.Now;
            RunStats.Duration = RunStats.Ended - RunStats.Started;

            if (LoadedConfig.Verbose)
            {
                Console.WriteLine();
            }

            // Show stats.
            Console.WriteLine("Timing:");
            Console.WriteLine($" # Started:                     {RunStats.Started}");
            Console.WriteLine($" # Ended:                       {RunStats.Ended}");
            Console.WriteLine($" # Duration:                    {RunStats.Duration}");
            Console.WriteLine();
            Console.WriteLine("Responses:");
            Console.WriteLine($" # Total Requests:              {RunStats.TotalRequests}");
            Console.WriteLine($" # Successful Requests (2xx):   {RunStats.SuccessfulRequests}");
            Console.WriteLine($" # Further Action Needed (3xx): {RunStats.FurtherActionResponses}");
            Console.WriteLine($" # User Errors (4xx):           {RunStats.UserErrors}");
            Console.WriteLine($" # Server Errors (5xx):         {RunStats.ServerErrors}");
            Console.WriteLine($" # Unhandled Exceptions:        {RunStats.Exceptions}");
            Console.WriteLine();
            Console.WriteLine("Response Times:");
            Console.WriteLine($" # Average:                     {RunStats.ResponseTimes.Sum() / RunStats.ResponseTimes.Count} ms");
            Console.WriteLine($" # Min:                         {RunStats.ResponseTimes.Min()} ms");
            Console.WriteLine($" # Max:                         {RunStats.ResponseTimes.Max()} ms");
            Console.WriteLine();
            Console.WriteLine("Request/Response Sizes:");
            Console.WriteLine($" # Total Bytes Sent:            {RunStats.BytesSent} ({FormatBytes(RunStats.BytesSent)})");
            Console.WriteLine($" # Total Bytes Received:        {RunStats.BytesReceived} ({FormatBytes(RunStats.BytesReceived)})");
        }

        /// <summary>
        /// Create a new user which runs repetitions.
        /// </summary>
        private static void CreateUser()
        {
            var maxRepetitions = LoadedConfig.Repetitions ??
                                 DefaultConfig.Repetitions;

            Parallel.For(0, maxRepetitions, _ => MakeRequest());
        }

        /// <summary>
        /// Make a request for a 'user'.
        /// </summary>
        private static void MakeRequest()
        {
            lock (RunStats)
            {
                RunStats.TotalRequests++;
            }

            try
            {
                var sw = new Stopwatch();
                var size = 50L;

                sw.Start();

                if (!(WebRequest.Create(LoadedConfig.Url) is HttpWebRequest req))
                {
                    throw new Exception($"Unable to create HttpWebRequest for {LoadedConfig.Url}");
                }

                req.AllowAutoRedirect = false;
                req.KeepAlive = false;

                // HTTP method.
                req.Method = LoadedConfig.HttpMethod ?? DefaultConfig.HttpMethod;

                // Timeout.
                if (LoadedConfig.Timeout.HasValue)
                {
                    req.Timeout = LoadedConfig.Timeout.Value;
                    size += 12;
                }

                // User agent.
                if (LoadedConfig.UserAgent != null)
                {
                    req.UserAgent = LoadedConfig.UserAgent;
                    size += 12 + LoadedConfig.UserAgent.Length;
                }

                // Headers.
                if (LoadedConfig.Headers != null)
                {
                    foreach (var (key, value) in LoadedConfig.Headers)
                    {
                        req.Headers.Add(key, value);
                        size += key.Length + value.Length + 5;
                    }
                }

                // Get response.
                if (!(req.GetResponse() is HttpWebResponse res))
                {
                    throw new Exception($"Unable to get HttpWebResponse from HttpWebRequest for {LoadedConfig.Url}");
                }

                sw.Stop();

                lock (RunStats)
                {
                    RunStats.BytesSent += size;
                    RunStats.ResponseTimes.Add(sw.ElapsedMilliseconds);
                    RunStats.BytesReceived += res.ContentLength;
                }

                AnalyzeStatusCode(res.StatusCode);

                if (LoadedConfig.Verbose)
                {
                    Console.WriteLine($" > {((int) res.StatusCode)} {res.StatusDescription}");
                }
            }
            catch (WebException ex)
            {
                try
                {
                    if (!(ex.Response is HttpWebResponse res))
                    {
                        throw new Exception($"Unable to get HttpWebResponse from WebException for {LoadedConfig.Url}");
                    }

                    AnalyzeStatusCode(res.StatusCode);

                    if (LoadedConfig.Verbose)
                    {
                        Console.WriteLine($" > {((int) res.StatusCode)} {res.StatusDescription}");
                    }
                }
                catch
                {
                    lock (RunStats)
                    {
                        RunStats.Exceptions++;
                    }

                    if (LoadedConfig.Verbose)
                    {
                        Console.WriteLine($" > [ERROR] {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                lock (RunStats)
                {
                    RunStats.Exceptions++;
                }

                if (LoadedConfig.Verbose)
                {
                    Console.WriteLine($" > [ERROR] {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Analyze the given HTTP status code and update stats.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        private static void AnalyzeStatusCode(HttpStatusCode statusCode)
        {
            var sc = (int) statusCode;

            if (sc >= 200 &&
                sc < 300)
            {
                lock (RunStats)
                {
                    RunStats.SuccessfulRequests++;
                }
            }
            else if (sc >= 300 &&
                     sc < 400)
            {
                lock (RunStats)
                {
                    RunStats.FurtherActionResponses++;
                }
            }
            else if (sc >= 400 &&
                     sc < 500)
            {
                lock (RunStats)
                {
                    RunStats.UserErrors++;
                }
            }
            else if (sc >= 500 &&
                     sc < 600)
            {
                lock (RunStats)
                {
                    RunStats.ServerErrors++;
                }
            }
        }

        /// <summary>
        /// Format bytes as a more human-readable version.
        /// </summary>
        /// <param name="bytes">Total bytes.</param>
        /// <returns>Formatted.</returns>
        private static string FormatBytes(long bytes)
        {
            var exts = new[]
            {
                "",
                "KB",
                "MB",
                "GB",
                "TB"
            };

            for (var i = 5; i > 0; i--)
            {
                var divisor = Math.Pow(1024, i);

                if (divisor > bytes)
                {
                    continue;
                }

                var nv = bytes / divisor;

                return $"{nv:0.00} {exts[i]}";
            }

            return $"{bytes}";
        }

        #endregion
    }
}