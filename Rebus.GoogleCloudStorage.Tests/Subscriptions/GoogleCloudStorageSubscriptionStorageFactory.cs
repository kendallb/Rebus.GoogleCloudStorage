using Google.Cloud.Storage.V1;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Subscriptions;
using Rebus.Tests.Contracts.Subscriptions;

namespace Rebus.GoogleCloudStorage.Tests.Subscriptions
{
    public class GoogleCloudStorageSubscriptionStorageFactory : ISubscriptionStorageFactory
    {
        /// <summary>
        /// Creates the subscription storage container
        /// </summary>
        /// <returns>Data bus storage to use</returns>
        public ISubscriptionStorage Create()
        {
            var connectionInfo = GoogleCloudStorageConnectionInfoUtil.ConnectionInfo.Value;
            var storageClient = StorageClient.Create();
            var options = new GoogleCloudStorageSubscriptionOptions(connectionInfo.ProjectId, connectionInfo.BucketName);
            return new GoogleCloudStorageSubscriptionsStorage(storageClient, new ConsoleLoggerFactory(false), options);
        }

        /// <summary>
        /// Cleans up when the tests are done
        /// </summary>
        public void Cleanup()
        {
            var connectionInfo = GoogleCloudStorageConnectionInfoUtil.ConnectionInfo.Value;
            new CleanupUtil(connectionInfo).Cleanup();
        }
    }
}
