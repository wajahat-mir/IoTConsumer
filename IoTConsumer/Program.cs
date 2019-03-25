using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using IoTConsumer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace IoTConsumer
{
    class Program
    {
        private static readonly IServiceProvider ServiceProvider;

        static void Main(string[] args)
        {
            var container = CompositionRoot();

            using (var scope = container.BeginLifetimeScope())
            {
                var service = scope.Resolve<IMessageService>();
                service.Run().GetAwaiter().GetResult();
            }
        }

        static private IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MessageService>().As<IMessageService>();
            builder.RegisterType<DBService>().As<IDBService>();

            var configBuilder = new ConfigurationBuilder();
            configBuilder
                .SetBasePath(GetApplicationRoot())
                .AddJsonFile("appsettings.json");
            var config = configBuilder.Build();

            builder.Register(context => config).As<IConfiguration>();

            var services = new ServiceCollection();
            services.AddLogging();
            builder.Populate(services);

            return builder.Build();
        }

        public static string GetApplicationRoot()
        {
            var exePath = Path.GetDirectoryName(System.Reflection
                              .Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var appRoot = appPathMatcher.Match(exePath).Value;
            return appRoot;
        }
    }
}
