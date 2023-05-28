using System;
using System.Collections.Generic;

namespace Alert_Server.Models
{
    public partial class ObdSubSystemCode
    {
        public char SubsystemId { get; set; }
        public string Description { get; set; } = null!;
    }
}
