using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.Interfaces
{
    public interface INotificationDispatcher
    {
        Task DispatchAsync(Guid orderId,string notificationType,string subject,Func<UserInfo, OrderInfo, string> buildBody);
    }
}
