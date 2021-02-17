using Google.Cloud.Storage.V1;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Auditing.Sagas;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Tests.Contracts;
using Rebus.Transport.InMem;

namespace Rebus.GoogleCloudStorage.Tests
{
    [TestFixture]
    [Ignore("just some code to post on GitHub")]
    public class GoogleCloudStorageCodeExample : FixtureBase
    {
        [Test]
        public void ConfigureDataBus()
        {
            // Use a single instance of the storage client
            var storageClient = StorageClient.Create();

            var activator = new BuiltinHandlerActivator();

            Using(activator);

            Configure.With(activator)
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "api"))
                .DataBus(d =>
                {
                    var options = new GoogleCloudStorageDataBusOptions("my-project-id", "my-bucket")
                    {
                        DoNotUpdateLastReadTime = true,
                        AutoCreateBucket = false,
                        ObjectKeyPrefix = "my-prefix",
                        ObjectKeySuffix = ".my-suffix",
                    };
                   d.StoreInGoogleCloudStorage(storageClient, options);
                })
                .Start();
        }

        [Test]
        public void ConfigureSubscriptions()
        {
            // Use a single instance of the storage client
            var storageClient = StorageClient.Create();

            var activator = new BuiltinHandlerActivator();

            Using(activator);

            Configure.With(activator)
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "api"))
                .Subscriptions(d =>
                {
                    var options = new GoogleCloudStorageSubscriptionOptions("my-project-id", "my-bucket")
                    {
                        DoNotUpdateLastReadTime = true,
                        AutoCreateBucket = false,
                        ObjectKeyPrefix = "my-prefix-folder/",
                    };
                    d.StoreInGoogleCloudStorage(storageClient, options);
                })
                .Start();
        }

        [Test]
        public void ConfigureSagaSnapshots()
        {
            // Use a single instance of the storage client
            var storageClient = StorageClient.Create();

            var activator = new BuiltinHandlerActivator();

            Using(activator);

            Configure.With(activator)
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "api"))
                .Sagas(s => s.StoreInMemory())
                .Options(o =>
                {
                    var options = new GoogleCloudStorageSagaSnapshotOptions("my-project-id", "my-bucket")
                    {
                        DoNotUpdateLastReadTime = true,
                        AutoCreateBucket = false,
                        ObjectKeyPrefix = "my-snapshots-folder/",
                    };
                    o.EnableSagaAuditing().StoreInGoogleCloudStorage(storageClient, options);
                })
                .Start();
        }
    }
}