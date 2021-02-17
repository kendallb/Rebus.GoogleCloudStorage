using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Polly.Retry;
using Rebus.Config;
using Rebus.DataBus;
using Rebus.Logging;
using Rebus.Time;

namespace Rebus.GoogleCloudStorage
{
    /// <summary>
    /// Implementation of <see cref="IDataBusStorage"/> that stores data in Google Cloud Storage
    /// </summary>
    public class GoogleCloudStorageDataBusStorage : IDataBusStorage
    {
        private readonly StorageClient _storageClient;
        private readonly IRebusTime _rebusTime;
        private readonly GoogleCloudStorageDataBusOptions _options;
        private readonly AsyncRetryPolicy _retry;

        /// <summary>
        /// Constructor for the Google Cloud Storage data bus
        /// </summary>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="loggerFactory">Reference to the logger factory to create the logger</param>
        /// <param name="rebusTime">Reference to the rebus time interface for getting the current time</param>
        /// <param name="options">Options to configure the storage bus</param>
        public GoogleCloudStorageDataBusStorage(StorageClient storageClient, IRebusLoggerFactory loggerFactory, IRebusTime rebusTime, GoogleCloudStorageDataBusOptions options)
        {
            _rebusTime = rebusTime ?? throw new ArgumentNullException(nameof(rebusTime));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));

            // Auto create the bucket when we start up, if required
            CloudUtils.CreateBucketIfNotExists(storageClient, loggerFactory, options);

            // Create our retry policy with Polly with a exponential back off strategy for retries, with jitter and retrying immediately on the first failure
            _retry = CloudUtils.CreateRetryPolicy(options);
        }

        /// <summary>
        /// Saves data to the data bus
        /// </summary>
        /// <param name="id">ID of the data bus object</param>
        /// <param name="source">Stream to read the data to save from</param>
        /// <param name="metadata">Optional metadata for the data bus object</param>
        public async Task Save(string id, Stream source, Dictionary<string, string> metadata = null)
        {
            await RetryCatchAsync(id, async objectName =>
            {
                // Build the meta data to save with the object
                var metadataToSave = new Dictionary<string, string>
                {
                    {MetadataKeys.SaveTime, _rebusTime.Now.ToString("O")}
                };
                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        metadataToSave[kvp.Key] = kvp.Value;
                    }
                }

                // Now upload the object
                await _storageClient.UploadObjectAsync(new Google.Apis.Storage.v1.Data.Object
                {
                    Bucket = _options.BucketName,
                    Name = objectName,
                    Metadata = metadataToSave,
                }, source);
                return true;
            });
        }

        /// <summary>
        /// Reads data from the data bus
        /// </summary>
        /// <param name="id">ID of the data bus object</param>
        /// <returns>Stream to read the object from</returns>
        public async Task<Stream> Read(string id)
        {
            return await RetryCatchAsync(id, async objectName =>
            {
                // Make sure we update the read time for the object
                if (!_options.DoNotUpdateLastReadTime)
                {
                    await UpdateLastReadTimeAsync(objectName);
                }

                // Build an HTTP request to be able to stream the object from the bucket as the API does not have a
                // direct function for that.
                return await CloudUtils.DownloadAsStreamAsync(_storageClient, _options.BucketName, objectName);
            });
        }

        /// <summary>
        /// Update the last read metadata for an object in the data bus
        /// </summary>
        /// <param name="objectName">Name of the Google Cloud object</param>
        private async Task UpdateLastReadTimeAsync(string objectName)
        {
            var metadata = await GetObjectMetadataAsync(objectName, false);
            metadata[MetadataKeys.ReadTime] = _rebusTime.Now.ToString("O");
            await _storageClient.PatchObjectAsync(new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _options.BucketName,
                Name = objectName,
                Metadata = metadata
            });
        }

        /// <summary>
        /// Reads current metadata for a data bus object
        /// </summary>
        /// <param name="id">ID of the data bus object</param>
        /// <returns>Dictionary of object metadata</returns>
        public async Task<Dictionary<string, string>> ReadMetadata(string id)
        {
            return await RetryCatchAsync(id, async objectName => await GetObjectMetadataAsync(objectName, true));
        }

        /// <summary>
        /// Reads the metadata for the object in Google Cloud Storage
        /// </summary>
        /// <param name="objectName">Name of the Google Cloud object</param>
        /// <param name="addContentLength">True to add in the content length metadata</param>
        /// <returns>Metadata for the object</returns>
        private async Task<Dictionary<string, string>> GetObjectMetadataAsync(string objectName, bool addContentLength)
        {
            var o = await _storageClient.GetObjectAsync(_options.BucketName, objectName);
            var metadata = new Dictionary<string, string>(o.Metadata);
            if (addContentLength)
            {
                metadata[MetadataKeys.Length] = o.Size.ToString();
            }
            return metadata;
        }

        /// <summary>
        /// Calls the Google function via the Polly retry handler
        /// </summary>
        /// <param name="id">ID of the data bus object</param>
        /// <param name="func">Callback to run the function</param>
        /// <typeparam name="T">Type of the return value for the function</typeparam>
        /// <returns>Return value for the function</returns>
        private async Task<T> RetryCatchAsync<T>(string id, Func<string, Task<T>> func)
        {
            var objectName = $"{_options.ObjectKeyPrefix}{id}{_options.ObjectKeySuffix}";
            return await _retry.ExecuteAsync(
                _ => func(objectName),
                new Dictionary<string, object> {{"objectName", objectName}}
            );
        }
    }
}
