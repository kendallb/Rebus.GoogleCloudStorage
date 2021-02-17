using System;
using Google.Cloud.Storage.V1;
using Rebus.GoogleCloudStorage;
using Rebus.DataBus;
using Rebus.Logging;
using Rebus.Time;

namespace Rebus.Config
{
    /// <summary>
    /// Provides extensions methods for configuring the Google Cloud Storage for the data bus
    /// </summary>
    public static class GoogleCloudStorageDataBusConfigurationExtensions
    {
        /// <summary>
        /// Configures the data bus to store data in Google Cloud Storage
        /// </summary>
        /// <param name="configurer">Reference to the configuration for fluent syntax</param>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="options">Options to configure the storage bus</param>
        public static void StoreInGoogleCloudStorage(this StandardConfigurer<IDataBusStorage> configurer, StorageClient storageClient, GoogleCloudStorageDataBusOptions options)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (storageClient == null) throw new ArgumentNullException(nameof(storageClient));
            if (options == null) throw new ArgumentNullException(nameof(options));
            Configure(configurer, storageClient, options);
        }

        /// <summary>
        /// Configures the data bus to store data in Google Cloud Storage
        /// </summary>
        /// <param name="configurer">Reference to the configuration for fluent syntax</param>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="projectId">ID of the Google project required to auto create buckets</param>
        /// <param name="bucketName">Name of the bucket to store the data in</param>
        public static void StoreInGoogleCloudStorage(this StandardConfigurer<IDataBusStorage> configurer, StorageClient storageClient, string projectId, string bucketName)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (storageClient == null) throw new ArgumentNullException(nameof(storageClient));
            if (projectId == null) throw new ArgumentNullException(nameof(projectId));
            if (bucketName == null) throw new ArgumentNullException(nameof(bucketName));
            Configure(configurer, storageClient, new GoogleCloudStorageDataBusOptions(projectId, bucketName));
        }

        /// <summary>
        /// Performs the configuration and registers it
        /// </summary>
        /// <param name="configurer">Reference to the configuration for fluent syntax</param>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="options">Options to configure the storage bus</param>
        static void Configure(StandardConfigurer<IDataBusStorage> configurer, StorageClient storageClient, GoogleCloudStorageDataBusOptions options)
        {
            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                var rebusTime = c.Get<IRebusTime>();
                return new GoogleCloudStorageDataBusStorage(storageClient, rebusLoggerFactory, rebusTime, options);
            });
        }
    }
}
