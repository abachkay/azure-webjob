using Microsoft.Azure.WebJobs;
using System;

namespace azuredbtest1
{
    internal class Program
    {
        private static void Main()
        {
            var config = new JobHostConfiguration();
            config.Queues.MaxDequeueCount = 5;
            config.Queues.MaxPollingInterval = TimeSpan.FromSeconds(2);
            config.Queues.BatchSize = 32;

            var host = new JobHost(config);
            host.RunAndBlock();            
        }
    }
}
