using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Models
{
    public enum OrderStatus
    {
        Pending,
        StockReserved,
        PaymentProcessing,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
}
