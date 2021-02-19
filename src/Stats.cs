using System;
using System.Collections.Generic;

namespace Stressi
{
    public class Stats
    {
        #region Timing

        /// <summary>
        /// When the run started.
        /// </summary>
        public DateTimeOffset? Started { get; set; }

        /// <summary>
        /// When the run ended.
        /// </summary>
        public DateTimeOffset? Ended { get; set; }

        /// <summary>
        /// How long the run took.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        #endregion

        #region Responses

        public long TotalRequests { get; set; }

        /// <summary>
        /// Number of successful requests. (2xx)
        /// </summary>
        public long SuccessfulRequests { get; set; }

        /// <summary>
        /// Number of 'further action needed' responses. (3xx)
        /// </summary>
        public long FurtherActionResponses { get; set; }

        /// <summary>
        /// Number of user errors. (4xx)
        /// </summary>
        public long UserErrors { get; set; }

        /// <summary>
        /// Number of server errors. (5xx)
        /// </summary>
        public long ServerErrors { get; set; }

        /// <summary>
        /// Number of unhandled exceptions.
        /// </summary>
        public long Exceptions { get; set; }

        #endregion

        #region Response Times

        /// <summary>
        /// All response times.
        /// </summary>
        public List<long> ResponseTimes { get; set; }

        #endregion

        #region Request/Response Sizes

        /// <summary>
        /// Total bytes sent.
        /// </summary>
        public long BytesSent { get; set; }

        /// <summary>
        /// Total bytes received.
        /// </summary>
        public long BytesReceived { get; set; }

        #endregion
    }
}