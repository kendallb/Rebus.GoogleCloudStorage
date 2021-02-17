using Rebus.Exceptions;
using System;
using System.IO;

namespace Rebus.GoogleCloudStorage.Tests
{
    internal static class GoogleCloudStorageConnectionInfoUtil
    {
        private const string ConnectionInfoFile = "google_connectioninfo.txt";
        private const string ConnectionInfoEnvironment = "REBUS_GOOGLE_CONNECTIONINFO";

        /// <summary>
        /// Lazily loads the connection information when needed
        /// </summary>
        public static readonly Lazy<ConnectionInfo> ConnectionInfo = new Lazy<ConnectionInfo>(() =>
        {
            // First try to load load from file in the unit test directory
            var file = GetFilePath(ConnectionInfoFile);
            if (File.Exists(file))
            {
                try
                {
                    return Tests.ConnectionInfo.CreateFromString(File.ReadAllText(file));
                }
                catch (Exception exception)
                {
                    throw new RebusConfigurationException(exception, $"Could not get connection string information from file {file}");
                }
            }

            // If that fails, try loading from an environment variable
            var env = Environment.GetEnvironmentVariable(ConnectionInfoEnvironment);
            if (env == null) throw new RebusConfigurationException("Missing Google Cloud Storage connection info");
            try
            {
                return Tests.ConnectionInfo.CreateFromString(env);
            }
            catch (Exception exception)
            {
                throw new RebusConfigurationException(exception, "Could not get connection string information");
            }
        });

        /// <summary>
        /// Gets the full path of the filename
        /// </summary>
        /// <param name="filename">Filename to get the full path for</param>
        /// <returns>Full path for filename</returns>
        private static string GetFilePath(string filename)
        {
            var baseDirectory = AppContext.BaseDirectory;

            // added because of test run issues on MacOS
            var indexOfBin = baseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase);
            var connectionStringFileDirectory = baseDirectory[..(indexOfBin > 0 ? indexOfBin : baseDirectory.Length)];
            return Path.Combine(connectionStringFileDirectory, filename);
        }
    }
}
