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
            Apps = new HashSet<App>();
        }

        public string? IpAddress { get; set; }
        public long TcuId { get; set; }
        public string UserId { get; set; } = null!;
        public string Mac { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
        public byte[]? Challenge { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public long? ModelId { get; set; }

        public virtual Model? Model { get; set; }
        public virtual AspNetUser User { get; set; } = null!;
        public virtual ICollection<Alert> Alerts { get; set; }
        public virtual ICollection<ConnectionRequest> ConnectionRequests { get; set; }
        public virtual ICollection<DevicesTcu> DevicesTcus { get; set; }
        public virtual ICollection<LockRequest> LockRequests { get; set; }

        public virtual ICollection<App> Apps { get; set; }
    }
}
