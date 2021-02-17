using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Polly.Retry;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Subscriptions;

namespace Rebus.GoogleCloudStorage
{
    /// <summary>
    /// Implementation of <see cref="ISubscriptionStorage"/> that stores subscriptions in Google Cloud Storage
    /// </summary>
    public class GoogleCloudStorageSubscriptionsStorage : ISubscriptionStorage
    {
        private readonly StorageClient _storageClient;
        private readonly GoogleCloudStorageSubscriptionOptions _options;
        private readonly AsyncRetryPolicy _retry;

        /// <summary>
        /// Constructor for the Google Cloud Storage subscription storage
        /// </summary>
        /// <param name="storageClient">Reference to the single instance of Google StorageClient</param>
        /// <param name="loggerFactory">Reference to the logger factory to create the logger</param>
        /// <param name="options">Options to configure the subscription storage</param>
        public GoogleCloudStorageSubscriptionsStorage(StorageClient storageClient, IRebusLoggerFactory loggerFactory, GoogleCloudStorageSubscriptionOptions options)
        {
            _storageClient = storageClient ?? throw new ArgumentNullException(nameof(storageClient));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // Auto create the bucket when we start up, if required
            CloudUtils.CreateBucketIfNotExists(storageClient, loggerFactory, options);

            // Create our retry policy with Polly with a exponential back off strategy for retries, with jitter and retrying immediately on the first failure
            _retry = CloudUtils.CreateRetryPolicy(options);
        }

        /// <summary>
        /// This is a centralized subscription storage type
        /// </summary>
        public bool IsCentralized => true;

        /// <summary>
        /// Gets the subscriber addresses for a specific topic
        /// </summary>
        /// <param name="topic">Topic to get the subscribers for</param>
        /// <returns>Array of subscriber addresses</returns>
        public async Task<string[]> GetSubscriberAddresses(string topic)
        {
            return await RetryCatchAsync(topic, async objectPrefix =>
            {
                // Get all the objects that match this topic
                var storageObjects = _storageClient.ListObjectsAsync(_options.BucketName, objectPrefix);

                // Now build the list and trim off the topic directory from the front
                var prefixLength = objectPrefix.Length + 1;
                var keys = new List<string>();
                await foreach (var obj in storageObjects)
                {
                    keys.Add(obj.Name.Substring(prefixLength));
                }
                return keys.ToArray();
            });
        }

        /// <summary>
        /// Register a subscriber for a topic
        /// </summary>
        /// <param name="topic">Topic to register the subscriber for</param>
        /// <param name="subscriberAddress">Address of the subscriber</param>
        public async Task RegisterSubscriber(string topic, string subscriberAddress)
        {
            // Create the name of the object to store the subscription under
            topic = $"{topic}/{subscriberAddress}";

            // Upload an empty memory stream. We don't actually need to store anything here, we just need the object to be present.
            await RetryCatchAsync(topic, async objectName =>
            {
                using var ms = new MemoryStream();
                await _storageClient.UploadObjectAsync(_options.BucketName, objectName, null, ms);
                return true;
            });
        }

        /// <summary>
        /// Unregister a subscriber for a topic
        /// </summary>
        /// <param name="topic">Topic to unregister the subscriber for</param>
        /// <param name="subscriberAddress">Address of the subscriber</param>
        public async Task UnregisterSubscriber(string topic, string subscriberAddress)
        {
            // Create the name of the object to store the subscription under
            topic = $"{topic}/{subscriberAddress}";

            await RetryCatchAsync(topic, async objectName =>
            {
                await _storageClient.DeleteObjectAsync(_options.BucketName, objectName);
                return true;
            });
        }

        /// <summary>
        /// Calls the Google function via the Polly retry handler
        /// </summary>
        /// <param name="topic">Topic for the subscriptions</param>
        /// <param name="func">Callback to run the function</param>
        /// <typeparam name="T">Type of the return value for the function</typeparam>
        /// <returns>Return value for the function</returns>
        private async Task<T> RetryCatchAsync<T>(string topic, Func<string, Task<T>> func)
        {
            var objectName = $"{_options.ObjectKeyPrefix}{topic}";
            return await _retry.ExecuteAsync(
                _ => func(objectName),
                new Dictionary<string, object> {{"objectName", objectName}}
            );
        }
    }
}
