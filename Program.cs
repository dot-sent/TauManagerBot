using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TauManager;
using TauManager.BusinessLogic;
using TauManager.Utils;

namespace TauManagerBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(options => {})
                .ConfigureHostConfiguration(configuration =>
                {
                    configuration.AddJsonFile("appsettings.json");
                    configuration.AddEnvironmentVariables();
                    configuration.AddEnvironmentVariables("TAUBOT_");
                })
                .ConfigureServices((hostContext, services) => 
                {
                    services.AddSingleton<IServiceProvider, ServiceProvider>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddHostedService<NotifierService>();
                    services.AddDbContext<TauDbContext>(options =>
                            options
                                .UseLazyLoadingProxies()
                                .UseNpgsql(hostContext.Configuration.GetConnectionString("TauDbContextConnection")));
                    services.AddScoped<ITauHeadClient, TauHead>();
                    services.AddScoped<ICampaignLogic, CampaignLogic>();
                    services.AddScoped<IPlayerLogic, PlayerLogic>();
                    services.AddScoped<INotificationLogic, NotificationLogic>();
                    services.AddScoped<IIntegrationLogic, IntegrationLogic>();
                    services.AddSingleton<IRegisteredDiscordUsersService, RegisteredDiscordUsersService>();
                    services.AddSingleton<INotificationQueueService, NotificationQueueService>();
                    services.AddSingleton<IFuelTrackerService, FuelTrackerService>();
                });
    }
}
