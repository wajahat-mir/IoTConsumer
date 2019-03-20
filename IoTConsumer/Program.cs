using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using IoTConsumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace IoTConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            CompositionRoot().Resolve<MessageService>().Run().GetAwaiter().GetResult();
        }

        static private IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MessageService>().As<IMessageService>();
            builder.RegisterType<DBService>().As<IDBService>();

            var config = new ConfigurationBuilder();
            config.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var module = new ConfigurationModule(config.Build());
            builder.RegisterModule(module);

            var services = new ServiceCollection();
            services.AddLogging();
            builder.Populate(services);

            return builder.Build();
        }
    }
}
