using IoTConsumer.Entities;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IoTConsumer.Services
{
    class MessageService : IMessageService
    {
        string ServiceBusConnectionString;
        string QueueName;
        static IQueueClient queueClient;
        private readonly IConfiguration _configuration;
        private IDBService _dbService;
        private readonly ILogger _logger;

        //public MessageService(IConfiguration configuration, IDBService dBService)
        public MessageService(IConfiguration configuration, ILogger<MessageService> logger)
        {
            _configuration = configuration;
            //_dbService = dBService;
            _logger = logger;

            ServiceBusConnectionString = _configuration["MessageBusSettings:ConnectionString"];
            QueueName = _configuration["MessageBusSettings:QueueName"];
        }

        public async Task Run()
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await queueClient.CloseAsync();
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,

                AutoComplete = false
            };

            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            SaveIoTMessage iotmessage = JsonConvert.DeserializeObject<SaveIoTMessage>(Encoding.UTF8.GetString(message.Body));

            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber}");
            Console.WriteLine("IoT Device Id: " + iotmessage.DeviceId);
            Console.WriteLine("IoT Timestamp: " + iotmessage.TimeStamp.ToLongDateString());
            Console.WriteLine("IoT Status: " + iotmessage.Status.ToString());
            Console.WriteLine("IoT Message: " + iotmessage.message);

            // save to DB
            //_dbService.SaveMessagetoDatabase(iotmessage);

            await queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogError("Exception context for troubleshooting:");
            _logger.LogError($"- Endpoint: {context.Endpoint}");
            _logger.LogError($"- Entity Path: {context.EntityPath}");
            _logger.LogError($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }

    interface IMessageService
    {
       Task Run();
       void RegisterOnMessageHandlerAndReceiveMessages();
       Task ProcessMessagesAsync(Message message, CancellationToken token);
       Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs);
    }
}
