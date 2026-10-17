using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineStore.Enums;

public enum OrderStatus
{
    Pending = 1,
    Shipped,
    Delivered,
    Cancelled
}
