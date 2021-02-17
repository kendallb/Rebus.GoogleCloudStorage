using Rebus.Auditing.Sagas;
using Rebus.Logging;
using System;
using Google.Cloud.Storage.V1;
using Rebus.GoogleCloudStorage;

namespace Rebus.Config
{
    /// <summary>
    /// Configuration extensions for Google Cloud storage
    /// </summary>
    public static class GoogleCloudStorageSagaConfigurationExtensions
    {
        /// <summary>
        /// Configures Rebus to store saga data snapshots in Google Cloud Storage
        /// </summary>
        public static void StoreInGoogleCloudStorage(this StandardConfigurer<ISagaSnapshotStorage> configurer, StorageClient storageClient, GoogleCloudStorageSagaSnapshotOptions options)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (storageClient == null) throw new ArgumentNullException(nameof(storageClient));
            if (options == null) throw new ArgumentNullException(nameof(options));
            configurer.Register(c =>
            {
                var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                return new GoogleCloudStorageSagaSnapshotStorage(storageClient, rebusLoggerFactory, options);
            });
        }
    }
}