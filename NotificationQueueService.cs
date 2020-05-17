using System.Collections.Generic;
using TauManager.ViewModels;

namespace TauManagerBot
{
    public class NotificationQueueService : INotificationQueueService
    {
        private readonly Queue<NotificationViewModel> _notificationQueue;
        public NotificationQueueService()
        {
            _notificationQueue = new Queue<NotificationViewModel>();
        }

        public bool Any()
        {
            return _notificationQueue.Count > 0;
        }

        public NotificationViewModel Dequeue()
        {
            return _notificationQueue.Count == 0 ? null : _notificationQueue.Dequeue();
        }

        public void Enequeue(NotificationViewModel notification)
        {
            _notificationQueue.Enqueue(notification);
        }
    }
}