using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace OrderService.Application.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public int Attempts { get; set; }
        public string? LastError { get; set; }

        public static OutboxMessage From<TEvent>(TEvent @event) where TEvent : class
        {
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = typeof(TEvent).FullName!,
                Payload = JsonSerializer.Serialize(@event)
            };
        }
    }
}
