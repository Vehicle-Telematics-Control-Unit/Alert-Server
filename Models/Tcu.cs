using System;
using System.Collections.Generic;

namespace Alert_Server.Models
{
    public partial class Tcu
    {
        public Tcu()
        {
            Alerts = new HashSet<Alert>();
            ConnectionRequests = new HashSet<ConnectionRequest>();
            DevicesTcus = new HashSet<DevicesTcu>();
            LockRequests = new HashSet<LockRequest>();
        }

        public string? IpAddress { get; set; }
        public long TcuId { get; set; }
        public string UserId { get; set; } = null!;
        public string Mac { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public byte[]? Challenge { get; set; }

        public virtual AspNetUser User { get; set; } = null!;
        public virtual ICollection<Alert> Alerts { get; set; }
        public virtual ICollection<ConnectionRequest> ConnectionRequests { get; set; }
        public virtual ICollection<DevicesTcu> DevicesTcus { get; set; }
        public virtual ICollection<LockRequest> LockRequests { get; set; }
    }
}
