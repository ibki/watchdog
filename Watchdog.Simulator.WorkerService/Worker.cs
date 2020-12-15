using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Watchdog.Simulator.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[{DateTime.Now}] Hello World!");
            _logger.LogInformation($"[{DateTime.Now}] Environment.OSVersion.VersionString = {Environment.OSVersion.VersionString}");
            _logger.LogInformation($"[{DateTime.Now}] Environment.Is64BitOperatingSystem = {Environment.Is64BitOperatingSystem}");
            _logger.LogInformation($"[{DateTime.Now}] Environment.Is64BitProcess = {Environment.Is64BitProcess}");

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
