using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Rebus.Config;
using Rebus.Exceptions;
using Rebus.Logging;
// ReSharper disable InvertIf

namespace Rebus.GoogleCloudStorage
{
    internal static class CloudUtils
    {
        /// <summary>
        /// Creates the bucket in Google if it does not exist
        /// </summary>
        /// <param name="client">Google storage client</param>
        /// <param name="loggerFactory">Reference to the logger factory to create the logger</param>
        /// <param name="options">Options to configure the storage bus</param>
        internal static void CreateBucketIfNotExists(StorageClient client, IRebusLoggerFactory loggerFactory, GoogleCloudStorageOptions options)
        {
            if (options.AutoCreateBucket)
            {
                var log = loggerFactory?.GetLogger<GoogleCloudStorageDataBusStorage>() ?? throw new ArgumentNullException(nameof(loggerFactory));
                if (!BucketExists(client, options.BucketName))
                {
                    log.Info("Bucket {0} does not exist - will create it now", options.BucketName);
                    CreateBucket(client, options.ProjectId, options.BucketName);
                }
            }
        }

        /// <summary>
        /// Checks if a bucket exists
        /// </summary>
        /// <param name="client">Google storage client</param>
        /// <param name="bucketName">Name of the bucket</param>
        /// <returns>True if bucket exists, false if not</returns>
        private static bool BucketExists(StorageClient client, string bucketName)
        {
            try
            {
                return client.GetBucket(bucketName) != null;
            }
            catch (GoogleApiException e) when (e.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates the bucket
        /// </summary>
        /// <param name="client">Google storage client</param>
        /// <param name="projectId">Project ID to create the bucket in</param>
        /// <param name="bucketName">Name of the bucket</param>
        private static void CreateBucket(StorageClient client, string projectId, string bucketName)
        {
            try
            {
                client.CreateBucket(projectId, bucketName);
            }
            catch (GoogleApiException e)
            {
                if (e.HttpStatusCode != HttpStatusCode.Conflict)
                {
                    throw new RebusApplicationException(e, "Unexpected Google Cloud Storage exception occurred");
                }
            }
        }

        /// <summary>
        /// Function to stream the download the results of a bucket as a stream
        /// </summary>
        /// <param name="client">Google storage client</param>
        /// <param name="bucketName">Name of the bucket</param>
        /// <param name="objectName">Object name to download</param>
        /// <returns>Open stream for reading the data</returns>
        internal static async Task<Stream> DownloadAsStreamAsync(StorageClient client, string bucketName, string objectName)
        {
            var o = await client.GetObjectAsync(bucketName, objectName);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(o.MediaLink)
            };
            var response = await client.Service.HttpClient.SendAsync(request);
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Serialize and upload JSON to a Google Cloud Storage bucket
        /// </summary>
        /// <param name="client">Google storage client</param>
        /// <param name="bucketName">Name of the bucket</param>
        /// <param name="objectName">Name of the object to upload to</param>
        /// <param name="data">Data to serialize as JSON and upload</param>
        /// <param name="settings">JSON serializer settings to use</param>
        /// <param name="encoding">Text encoding to use</param>
        internal static async Task UploadJsonAsync(StorageClient client, string bucketName, string objectName, object data, JsonSerializerSettings settings, Encoding encoding)
        {
            // Serialize the JSON to a memory stream
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, encoding);
            using var jsonWriter = new JsonTextWriter(writer);
            var serializer = JsonSerializer.Create(settings);
            serializer.Serialize(jsonWriter, data);
            await jsonWriter.FlushAsync();

            // Now upload the stream
            ms.Seek(0, SeekOrigin.Begin);
            await client.UploadObjectAsync(bucketName, objectName, "application/json", ms);
        }

        /// <summary>
        /// Download and deserialize a JSON file from a Google Cloud Storage bucket
        /// </summary>
        /// <param name="client">Google storage client</param>
        /// <param name="bucketName">Name of the bucket</param>
        /// <param name="objectName">Name of the object to upload to</param>
        /// <param name="settings">JSON serializer settings to use</param>
        /// <param name="encoding">Text encoding to use</param>
        internal static async Task<T> DownloadJsonAsync<T>(StorageClient client, string bucketName, string objectName, JsonSerializerSettings settings, Encoding encoding)
        {
            using var s = await DownloadAsStreamAsync(client, bucketName, objectName);
            using var reader = new StreamReader(s, encoding);
            using var jsonReader = new JsonTextReader(reader);
            var serializer = JsonSerializer.Create(settings);
            return serializer.Deserialize<T>(jsonReader);
        }

        /// <summary>
        /// Create a retry policy with Polly with a exponential back off strategy for retries, with jitter and retrying immediately on the first failure
        /// </summary>
        /// <param name="options">Options to configure the data storage component</param>
        /// <returns>Async retry policy to use</returns>
        internal static AsyncRetryPolicy CreateRetryPolicy(GoogleCloudStorageOptions options)
        {
            var delay = Polly.Contrib.WaitAndRetry.Backoff.DecorrelatedJitterBackoffV2(options.MedianFirstRetryDelay, options.MaxRetries, fastFirst: true);
            return Policy
                .Handle<GoogleApiException>()
                .WaitAndRetryAsync(delay, OnRetryAsync);
        }

        /// <summary>
        /// Polly retry callback handler called when we have an exception. We will get these if we are being throttled (HTTP status code 429), but we can just run the normal retry
        /// policy with exponential back off for those as well.
        /// </summary>
        /// <param name="exception">Exception that occurred</param>
        /// <param name="timeSpan">Timespan we will currently be waiting</param>
        /// <param name="context">Context passed to the execution function</param>
        private static Task OnRetryAsync(Exception exception, TimeSpan timeSpan, Context context)
        {
            if (exception is GoogleApiException {HttpStatusCode: HttpStatusCode.NotFound})
            {
                // If we received an object not found error, no need to retry just fail out
                throw new ArgumentException($"Object {context["objectName"]} not found in bucket");
            }
            return Task.CompletedTask;
        }
    }
}
