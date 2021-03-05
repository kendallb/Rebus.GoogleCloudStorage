using System;
using Google.Cloud.Storage.V1;
using Rebus.Config;
using Rebus.DataBus;
using Rebus.Logging;
using Rebus.Tests.Contracts.DataBus;

namespace Rebus.GoogleCloudStorage.Tests.DataBus
{
    public class GoogleCloudStorageDataBusStorageFactory : IDataBusStorageFactory
    {
        private readonly FakeRebusTime _fakeRebusTime = new FakeRebusTime();

        /// <summary>
        /// Creates the data bus storage container
        /// </summary>
        /// <returns>Data bus storage to use</returns>
        public IDataBusStorage Create()
        {
            var connectionInfo = GoogleCloudStorageConnectionInfoUtil.ConnectionInfo.Value;
            var storageClient = StorageClient.Create();

            // We need a longer delay here as one of the tests will cause throttling
            var options = new GoogleCloudStorageDataBusOptions(connectionInfo.ProjectId, connectionInfo.BucketName)
            {
                MedianFirstRetryDelay = TimeSpan.FromMilliseconds(500)
            };
            return new GoogleCloudStorageDataBusStorage(storageClient, new ConsoleLoggerFactory(false), _fakeRebusTime, options);
        }

        /// <summary>
        /// Cleans up when the tests are done
        /// </summary>
        public void CleanUp()
        {
            var connectionInfo = GoogleCloudStorageConnectionInfoUtil.ConnectionInfo.Value;
            new CleanupUtil(connectionInfo).Cleanup();
        }

        /// <summary>
        /// Fakes out the current time with a new timestamp value
        /// </summary>
        /// <param name="fakeTime">New fake time to set</param>
        public void FakeIt(DateTimeOffset fakeTime) => _fakeRebusTime.SetNow(fakeTime);
    }
}