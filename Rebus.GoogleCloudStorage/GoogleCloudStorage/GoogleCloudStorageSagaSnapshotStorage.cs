using Newtonsoft.Json;
using Rebus.Auditing.Sagas;
using Rebus.Logging;
using Rebus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Polly.Retry;
using Rebus.Config;

namespace Rebus.GoogleCloudStorage
{
    /// <summary>
    /// Implementation of <see cref="ISagaSnapshotStorage"/> that uses Google Cloud Storage to store saga data snapshots
    /// </summary>
    public class GoogleCloudStorageSagaSnapshotStorage : ISagaSnapshotStorage
    {
        private static readonly JsonSerializerSettings DataSettings = new() { TypeNameHandling = TypeNameHandling.All };
        private static readonly JsonSerializerSettings MetadataSettings = new() { TypeNameHandling = TypeNameHandling.None };
        private static readonly Encoding TextEncoding = Encoding.UTF8;
        private readonly StorageClient _storageClient;
        private readonly GoogleCloudStorageSagaSnapshotOptions _options;
        private readonly AsyncRetryPolicy _retry;

        /// <summary>
        /// Creates the storage
        /// </summary>
        public GoogleCloudStorageSagaSnapshotStorage(StorageClient storageClient, IRebusLoggerFactory loggerFactory, GoogleCloudStorageSagaSnapshotOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));

            // Auto create the bucket when we start up, if required
            CloudUtils.CreateBucketIfNotExists(storageClient, loggerFactory, options);

            // Create our retry policy with Polly with a exponential back off strategy for retries, with jitter and retrying immediately on the first failure
            _retry = CloudUtils.CreateRetryPolicy(options);
        }

        /// <summary>
        /// Archives the given saga data under its current ID and revision
        /// </summary>
        /// <param name="sagaData">Saga data to archive</param>
        /// <param name="sagaAuditMetadata">Saga audit metadata to store</param>
        public async Task Save(ISagaData sagaData, Dictionary<string, string> sagaAuditMetadata)
        {
            var dataRef = $"{sagaData.Id:N}/{sagaData.Revision:0000000000}/data.json";
            var metaDataRef = $"{sagaData.Id:N}/{sagaData.Revision:0000000000}/metadata.json";
            await _retry.ExecuteAsync(async () =>
            {
                await CloudUtils.UploadJsonAsync(_storageClient, _options.BucketName, dataRef, sagaData, DataSettings, TextEncoding);
                await CloudUtils.UploadJsonAsync(_storageClient, _options.BucketName, metaDataRef, sagaAuditMetadata, MetadataSettings, TextEncoding);
            });
        }

        /// <summary>
        /// Gets all blobs in the snapshot container
        /// </summary>
        public IEnumerable<string> ListAllObjects()
        {
            var storageObjects = _storageClient.ListObjects(_options.BucketName);
            return storageObjects.Select(obj => obj.Name).ToList();
        }

        /// <summary>
        /// Loads the saga data with the given id and revision
        /// </summary>
        /// <param name="sagaDataId">Saga data id to load</param>
        /// <param name="revision">Revision to load</param>
        /// <returns>Saga data loaded from snapshot storage</returns>
        public ISagaData GetSagaData(Guid sagaDataId, int revision)
        {
            var dataRef = $"{sagaDataId:N}/{revision:0000000000}/data.json";
            return (ISagaData)CloudUtils.DownloadJsonAsync<object>(_storageClient, _options.BucketName, dataRef, DataSettings, TextEncoding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Loads the saga metadata for the saga with the given id and revision
        /// </summary>
        /// <param name="sagaDataId">Saga data id to load</param>
        /// <param name="revision">Revision to load</param>
        /// <returns>Saga metadata loaded from snapshot storage</returns>
        public Dictionary<string, string> GetSagaMetaData(Guid sagaDataId, int revision)
        {
            var metaDataRef = $"{sagaDataId:N}/{revision:0000000000}/metadata.json";
            return CloudUtils.DownloadJsonAsync<Dictionary<string, string>>(_storageClient, _options.BucketName, metaDataRef, MetadataSettings, TextEncoding).GetAwaiter().GetResult();
        }
    }
}