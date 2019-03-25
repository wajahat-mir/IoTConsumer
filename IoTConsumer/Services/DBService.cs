using Cassandra;
using Cassandra.Mapping;
using IoTConsumer.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace IoTConsumer.Services
{
    class DBService : IDBService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        Cluster cluster;
        ISession session;

        public DBService(IConfiguration configuration, ILogger<DBService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
            options.SetHostNameResolver((ipAddress) => _configuration["DatabaseSettings:Endpoint"]);

            Cluster cluster = Cluster.Builder().WithCredentials(_configuration["DatabaseSettings:UserName"], _configuration["DatabaseSettings:Password"])
                .WithPort(Convert.ToInt32(_configuration["DatabaseSettings:Port"])).AddContactPoint(_configuration["DatabaseSettings:Endpoint"]).WithSSL(options).Build();
            ISession session = cluster.Connect();
        }

        public bool SaveMessagetoDatabase(SaveIoTMessage iotmessage)
        {
            session = cluster.Connect(_configuration["DatabaseSettings:KeySpace"]);
            IMapper mapper = new Mapper(session);

            try
            { 
                mapper.Insert<SaveIoTMessage>(new SaveIoTMessage(iotmessage.DeviceId, iotmessage.TimeStamp, iotmessage.Status, iotmessage.message));
            }
            catch(Exception ex)
            {
                _logger.LogError("Error in saving to Database: {0}", ex.Message);
                return false;
            }
            return true;
        }

        public bool ValidateServerCertificate(
           object sender,
           X509Certificate certificate,
           X509Chain chain,
           SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            _logger.LogError("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
    }

    interface IDBService
    {
        bool SaveMessagetoDatabase(SaveIoTMessage iotmessage);
        bool ValidateServerCertificate(object sender,
           X509Certificate certificate,
           X509Chain chain,
           SslPolicyErrors sslPolicyErrors);
    }



}
