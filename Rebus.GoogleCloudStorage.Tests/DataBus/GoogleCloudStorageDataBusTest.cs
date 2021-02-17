using NUnit.Framework;
using Rebus.Tests.Contracts.DataBus;

namespace Rebus.GoogleCloudStorage.Tests.DataBus
{
    [TestFixture]
    public class GoogleCloudStorageDataBusTest : GeneralDataBusStorageTests<GoogleCloudStorageDataBusStorageFactory>
    {
    }
}