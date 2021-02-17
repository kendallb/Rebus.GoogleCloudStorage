using Rebus.Subscriptions;
using System;
using Google.Cloud.Storage.V1;
using Rebus.GoogleCloudStorage;
using Rebus.Logging;

namespace Rebus.Config
{
    /// <summary>
    /// Provides extensions methods for configuring the Google Cloud Storage subscription storage
    /// </summary>
    public static class GoogleCloudStorageSubscriptionsConfigurationExtensions
    {
        /// <summary>
        /// Configures the storage of subscriptions in Google Cloud Storage
        /// </summary>
        /// <param name="configurer">Reference to the configuration for fluent syntax</param>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="options">Options to configure the subscription storage</param>
        public static void StoreInGoogleCloudStorage(this StandardConfigurer<ISubscriptionStorage> configurer, StorageClient storageClient, GoogleCloudStorageSubscriptionOptions options)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (storageClient == null) throw new ArgumentNullException(nameof(storageClient));
            if (options == null) throw new ArgumentNullException(nameof(options));
            Configure(configurer, storageClient, options);
        }

        /// <summary>
        /// Configures the storage of subscriptions in Google Cloud Storage
        /// </summary>
        /// <param name="configurer">Reference to the configuration for fluent syntax</param>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="projectId">ID of the Google project required to auto create buckets</param>
        /// <param name="bucketName">Name of the bucket to store the data in</param>
        public static void StoreInGoogleCloudStorage(this StandardConfigurer<ISubscriptionStorage> configurer, StorageClient storageClient, string projectId, string bucketName)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (storageClient == null) throw new ArgumentNullException(nameof(storageClient));
            var options = projectId != null && bucketName != null ? new GoogleCloudStorageSubscriptionOptions(projectId, bucketName) : null;
            Configure(configurer, storageClient, options);
        }

        /// <summary>
        /// Performs the configuration and registers it
        /// </summary>
        /// <param name="configurer">Reference to the configuration for fluent syntax</param>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="options">Options to configure the subscription storage</param>
        static void Configure(StandardConfigurer<ISubscriptionStorage> configurer, StorageClient storageClient, GoogleCloudStorageSubscriptionOptions options)
        {
            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                return new GoogleCloudStorageSubscriptionsStorage(storageClient, rebusLoggerFactory, options);
            });
        }
    }
}
