using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Storage.V1;
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace Rebus.GoogleCloudStorage.Tests
{
    internal class CleanupUtil
    {
        private readonly ConnectionInfo _connectionInfo;

        /// <summary>
        /// Constructor for the cleanup utility
        /// </summary>
        /// <param name="connectionInfo"></param>
        public CleanupUtil(ConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        /// <summary>
        /// Cleans up after unit testing has been run by clearing out the bucket
        /// </summary>
        public void Cleanup()
        {
            var storageClient = StorageClient.Create();
            var keys = GetObjectKeys(storageClient);
            DeleteObjects(storageClient, keys);
        }

        /// <summary>
        /// Gets a list of all objects in the bucket
        /// </summary>
        /// <param name="client">Storage client</param>
        /// <returns>Enumeration of all items in the bucket</returns>
        private List<string> GetObjectKeys(StorageClient client)
        {
            var storageObjects = client.ListObjects(_connectionInfo.BucketName);
            return storageObjects.Select(obj => obj.Name).ToList();
        }

        /// <summary>
        /// Deletes all the objects from the bucket
        /// </summary>
        /// <param name="client">Storage client</param>
        /// <param name="keys">Keys of objects to delete</param>
        private void DeleteObjects(StorageClient client, List<string> keys)
        {
            foreach (var key in keys)
            {
                client.DeleteObject(_connectionInfo.BucketName, key);
            }
        }
    }
}
