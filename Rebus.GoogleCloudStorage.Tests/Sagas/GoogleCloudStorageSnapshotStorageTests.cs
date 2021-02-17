using NUnit.Framework;
using Rebus.Tests.Contracts.Sagas;

namespace Rebus.GoogleCloudStorage.Tests.Sagas
{
    [TestFixture]
    public class GoogleCloudStorageSnapshotStorageTests : SagaSnapshotStorageTest<GoogleCloudStorageSagaSnapshotStorageFactory>
    {
    }
}