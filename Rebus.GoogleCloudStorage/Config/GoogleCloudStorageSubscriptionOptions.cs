namespace Rebus.Config
{
    /// <summary>
    /// Holds all of the exposed options which can be applied using the Google Cloud Storage subscriptions.
    /// </summary>
    public class GoogleCloudStorageSubscriptionOptions : GoogleCloudStorageOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rebus.Config.GoogleCloudStorageSubscriptionOptions"/> class.
        /// </summary>
        /// <param name="projectId">ID of the Google project required to auto create buckets</param>
        /// <param name="bucketName">Name of the bucket to store the data in</param>
        public GoogleCloudStorageSubscriptionOptions(string projectId, string bucketName)
            : base(projectId, bucketName)
        {
            ObjectKeyPrefix = "subscriptions/";
        }

        /// <summary>
        /// Prefix for object keys. Defaults to "subscriptions/".
        /// </summary>
        public string ObjectKeyPrefix { get; set; }
    }
}
