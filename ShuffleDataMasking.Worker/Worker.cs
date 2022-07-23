using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using ShuffleDataMasking.Domain.Masking.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FKSolutionsSource.Infra.MessagingBroker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumeDispatcherBuilder _messageConsumeDispatcherBuilder;

        public Worker(ILogger<Worker> logger, IMessageConsumeDispatcherBuilder messageConsumeDispatcherBuilder)
        {
            _logger = logger;
            _messageConsumeDispatcherBuilder = messageConsumeDispatcherBuilder;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            var consumeDispatcher = _messageConsumeDispatcherBuilder
                .ForQueues()
                .WithConfigurationFromSettings("DataMasking")
                .ForService<IShuffleDataMaskingService>()
                .ForListener<ShuffleDataMaskingMessage>((serviceInstance, message) => serviceInstance.Process(message))
                .Build();

            using (consumeDispatcher.StartConsume())
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("Worker stoped at: {time}", DateTimeOffset.Now);
        }
    }
}

