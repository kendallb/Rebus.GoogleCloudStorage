using System;
using System.Collections.Generic;
using System.Linq;

namespace Rebus.GoogleCloudStorage.Tests
{
    public class ConnectionInfo
    {
        /// <summary>
        /// Constructor for the connection information
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="bucketName"></param>
        private ConnectionInfo(string projectId, string bucketName)
        {
            ProjectId = projectId;
            BucketName = bucketName;
        }

        public string ProjectId { get; }
        public string BucketName { get; }

        /// <summary>
        /// Loads the connection information from a string and parses it
        /// </summary>
        /// <param name="textString">String to parse with connection information</param>
        /// <returns>Connection information to use</returns>
        public static ConnectionInfo CreateFromString(string textString)
        {
            var keyValuePairs = textString.Split("; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            try
            {
                var keysAndValues = keyValuePairs.Select(kvp => kvp.Split('=')).ToDictionary(kv => kv.First(), kv => kv.Last());
                return new ConnectionInfo(GetValue(keysAndValues, "ProjectId"), GetValue(keysAndValues, "BucketName"));
            }
            catch (Exception exception)
            {
                throw new FormatException("Could not extract project ID bucket name from string - expected the form \'ProjectId=blah; BucketName=something-unique\'", exception);
            }
        }

        /// <summary>
        /// Gets the value from a key value pair throwing an exception if not found
        /// </summary>
        /// <param name="keysAndValues">Keys and value</param>
        /// <param name="key">Key to extract</param>
        /// <returns>Extracts value, or exception on error</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found with details about the key</exception>
        private static string GetValue(Dictionary<string, string> keysAndValues, string key)
        {
            try
            {
                return keysAndValues[key];
            }
            catch
            {
                throw new KeyNotFoundException($"Could not find key '{key}' - got these keys: {string.Join(", ", keysAndValues.Keys)}");
            }
        }
    }
}