using System.Text.Json;
using MemeGen.Contracts.Messaging.V1;
using MemeGen.Contracts.Messaging.V1.Requests;
using MemeGen.ImageProcessor.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MemeGen.ImageProcessor;

public class ImageProcessingWorker(ILogger<ImageProcessingWorker> logger, IConnection connection,
    IImageProcessor imageProcessor) : BackgroundService
{
    private IModel? _channel;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = connection.CreateModel();
        _channel.QueueDeclare(MessagingContractConstants.ContentProcessingQueueName, durable: true, exclusive: false,
            autoDelete: false);

        logger.LogInformation("Image processing requests processing starting... ");

        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            logger.LogInformation("Received image processing request");
            var message = JsonSerializer.Deserialize<ImageProcessingRequest>(ea.Body.Span);
            
            if (message == null)
            {
                logger.LogWarning("Received message is null.");
                return;
            }

            try
            {
                await imageProcessor.ProcessImageAsync(message, stoppingToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing image processing request");
            }
            
            logger.LogInformation("Image processing request processed.");
        };

        _channel?.BasicConsume(queue: MessagingContractConstants.ContentProcessingQueueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        connection.Close();

        logger.LogInformation("Image processing stopped.");

        return base.StopAsync(cancellationToken);
    }
}