using System;
using Rebus.DataBus;

namespace Rebus.Config
{
    /// <summary>
    /// Holds all of the exposed options which can be applied using the Google Cloud Storage data bus.
    /// </summary>
    public class GoogleCloudStorageOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rebus.Config.GoogleCloudStorageOptions"/> class.
        /// </summary>
        /// <param name="projectId">ID of the Google project required to auto create buckets</param>
        /// <param name="bucketName">Name of the bucket to store the data in</param>
        protected GoogleCloudStorageOptions(string projectId, string bucketName)
        {
            if (string.IsNullOrWhiteSpace(projectId)) throw new ArgumentException("Cannot be null or empty", nameof(projectId));
            if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentException("Cannot be null or empty", nameof(bucketName));
            ProjectId = projectId;
            BucketName = bucketName;
            DoNotUpdateLastReadTime = false;
            AutoCreateBucket = true;
            MaxRetries = 5;
            MedianFirstRetryDelay = TimeSpan.FromMilliseconds(200);
        }

        /// <summary>
        /// ID of the project containing the buckets
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Name of bucket used to store attachments
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// If true, the <see cref="MetadataKeys.ReadTime"/> metadata key will NOT be updated on each read operation. This will save some requests.
        /// Defaults to false.
        /// </summary>
        public bool DoNotUpdateLastReadTime { get; set; }

        /// <summary>
        /// Whether or not to automatically create the bucket, if it doesn't already exist. Defaults to true.
        /// </summary>
        public bool AutoCreateBucket { get; set; }

        /// <summary>
        /// Maximum number of times to retry operations before reporting failure. Defaults to 5.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// The median delay to target before the first retry. Defaults to 100ms.
        /// </summary>
        public TimeSpan MedianFirstRetryDelay { get; set; }
    }
}
