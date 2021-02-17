using Rebus.Auditing.Sagas;
using Rebus.Logging;
using Rebus.Tests.Contracts.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Storage.V1;
using Rebus.Config;

namespace Rebus.GoogleCloudStorage.Tests.Sagas
{
    public class GoogleCloudStorageSagaSnapshotStorageFactory : ISagaSnapshotStorageFactory
    {
        private readonly ConnectionInfo _connectionInfo;
        private readonly GoogleCloudStorageSagaSnapshotStorage _storage;

        /// <summary>
        /// Constructor for the saga snapshot factory for unit testing
        /// </summary>
        public GoogleCloudStorageSagaSnapshotStorageFactory()
        {
            _connectionInfo = GoogleCloudStorageConnectionInfoUtil.ConnectionInfo.Value;
            var storageClient = StorageClient.Create();
            var options = new GoogleCloudStorageSagaSnapshotOptions(_connectionInfo.ProjectId, _connectionInfo.BucketName);
            _storage = new GoogleCloudStorageSagaSnapshotStorage(storageClient, new ConsoleLoggerFactory(false), options);
        }

        /// <summary>
        /// Creates the saga snapshot factory
        /// </summary>
        /// <returns>Reference to the snapshot storage instance</returns>
        public ISagaSnapshotStorage Create()
        {
            new CleanupUtil(_connectionInfo).Cleanup();
            return _storage;
        }

        /// <summary>
        /// Returns an enumeration of all existing snapshot storage data
        /// </summary>
        /// <returns>Enumeration of all existing snapshot data</returns>
        public IEnumerable<SagaDataSnapshot> GetAllSnapshots()
        {
            var allBlobs = _storage.ListAllObjects()
                .Select(name => new
                {
                    Parts = name.Split('/')
                })
                .Where(x => x.Parts.Length == 3)
                .Select(b =>
                {
                    var guid = Guid.Parse(b.Parts[0]);
                    var revision = int.Parse(b.Parts[1]);
                    var part = b.Parts[2];
                    return new
                    {
                        Id = guid,
                        Revision = revision,
                        Part = part,
                    };
                })
                .GroupBy(b => new { b.Id, b.Revision })
                .Select(g => new SagaDataSnapshot
                {
                    SagaData = _storage.GetSagaData(g.Key.Id, g.Key.Revision),
                    Metadata = _storage.GetSagaMetaData(g.Key.Id, g.Key.Revision)
                })
                .ToList();
            return allBlobs;
        }
    }
}