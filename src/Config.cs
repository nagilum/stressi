using System.Collections.Generic;

namespace Stressi
{
    public class Config
    {
        /// <summary>
        /// URL to stress test.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// HTTP method to use.
        /// </summary>
        public string HttpMethod { get; set; }

        /// <summary>
        /// Number of concurrent users.
        /// </summary>
        public long? ConcurrentUsers { get; set; }

        /// <summary>
        /// Number of repetitions pr. user.
        /// </summary>
        public long? Repetitions { get; set; }

        /// <summary>
        /// Whether to run with verbose output.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// User-agent to use.
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Headers to include.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Set timeout for each request.
        /// </summary>
        public int? Timeout { get; set; }
    }

    public class DefaultConfig
    {
        /// <summary>
        /// Default value for HTTP method.
        /// </summary>
        public const string HttpMethod = "GET";

        /// <summary>
        /// Default value for concurrent users.
        /// </summary>
        public const int ConcurrentUsers = 10;

        /// <summary>
        /// Default value for repetitions.
        /// </summary>
        public const int Repetitions = 10;
    }
}