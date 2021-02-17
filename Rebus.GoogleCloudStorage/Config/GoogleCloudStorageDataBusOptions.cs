namespace Rebus.Config
{
    /// <summary>
    /// Holds all of the exposed options which can be applied using the Google Cloud Storage data bus.
    /// </summary>
    public class GoogleCloudStorageDataBusOptions : GoogleCloudStorageOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rebus.Config.GoogleCloudStorageDataBusOptions"/> class.
        /// </summary>
        /// <param name="projectId">ID of the Google project required to auto create buckets</param>
        /// <param name="bucketName">Name of the bucket to store the data in</param>
        public GoogleCloudStorageDataBusOptions(string projectId, string bucketName)
            : base(projectId, bucketName)
        {
            ObjectKeyPrefix = "data-";
            ObjectKeySuffix = ".dat";
        }

        /// <summary>
        /// Prefix for object keys. Defaults to "data-".
        /// </summary>
        public string ObjectKeyPrefix { get; set; }

        /// <summary>
        /// Suffix for object keys. Defaults to ".dat".
        /// </summary>
        public string ObjectKeySuffix { get; set; }
    }
}
