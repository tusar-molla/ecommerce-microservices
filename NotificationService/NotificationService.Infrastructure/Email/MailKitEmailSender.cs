using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NotificationService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Infrastructure.Email
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public MailKitEmailSender(IConfiguration configuration)
        {
            _host = configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host is missing.");
            _port = int.Parse(configuration["Smtp:Port"] ?? "2525");
            _username = configuration["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username is missing.");
            _password = configuration["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password is missing.");
            _fromEmail = configuration["Smtp:FromEmail"] ?? "noreply@ecommerce-demo.com";
            _fromName = configuration["Smtp:FromName"] ?? "E-Commerce Platform";
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
