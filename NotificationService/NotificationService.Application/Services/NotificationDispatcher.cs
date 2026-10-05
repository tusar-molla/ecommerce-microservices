using NotificationService.Application.Interfaces;
using NotificationService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Application.Services
{
    public class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly IIdentityServiceClient _identityServiceClient;
        private readonly INotificationLogRepository _logRepository;
        private readonly IEmailSender _emailSender;

        public NotificationDispatcher(
            IOrderServiceClient orderServiceClient,
            IIdentityServiceClient identityServiceClient,
            INotificationLogRepository logRepository,
            IEmailSender emailSender)
        {
            _orderServiceClient = orderServiceClient;
            _identityServiceClient = identityServiceClient;
            _logRepository = logRepository;
            _emailSender = emailSender;
        }

        public async Task DispatchAsync(Guid orderId,string notificationType,string subject,Func<UserInfo, OrderInfo, string> buildBody)
        {
            if (await _logRepository.HasBeenSentAsync(orderId, notificationType))
            {
                return;
            }

            var order = await _orderServiceClient.GetOrderAsync(orderId);
            if (order is null)
            {
                return;
            }

            var user = await _identityServiceClient.GetUserAsync(order.UserId);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var log = new NotificationLog
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                RecipientEmail = user.Email,
                NotificationType = notificationType,
                Subject = subject,
                Status = NotificationStatus.Pending
            };

            await _logRepository.CreateAsync(log);

            try
            {
                await _emailSender.SendAsync(user.Email, subject, buildBody(user, order));
                await _logRepository.UpdateStatusAsync(log.Id, NotificationStatus.Sent, null);
            }
            catch (Exception ex)
            {
                await _logRepository.UpdateStatusAsync(log.Id, NotificationStatus.Failed, ex.Message);
            }
        }
    }
}
