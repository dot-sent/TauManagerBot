using TauManager.ViewModels;

namespace TauManagerBot
{
    public interface INotificationQueueService
    {
        void Enequeue(NotificationViewModel notification);
        NotificationViewModel Dequeue();
        bool Any();
    }
}