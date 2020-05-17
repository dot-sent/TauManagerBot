using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TauManager.BusinessLogic;

namespace TauManagerBot
{
    public class NotifierService : BackgroundService
    {
        private IServiceProvider _serviceProvider;
        private readonly int _refreshDelay = 6000;

        public NotifierService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                CheckNotificationsToSend();
                await Task.Delay(_refreshDelay, stoppingToken);
            }
        }

        private void CheckNotificationsToSend()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationLogic = scope.ServiceProvider.GetRequiredService<INotificationLogic>();
                var notifications = notificationLogic.GetCurrentNotifications();
                var queueService = _serviceProvider.GetRequiredService<INotificationQueueService>();
                foreach (var notification in notifications)
                {
                    queueService.Enequeue(notification);
                }
            }
        }
    }
}