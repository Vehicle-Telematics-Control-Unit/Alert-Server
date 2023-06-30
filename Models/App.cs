using System;
using System.Collections.Generic;

namespace Alert_Server.Models
{
    public partial class App
    {
        public App()
        {
            Tcus = new HashSet<Tcu>();
        }

        public string Repo { get; set; } = null!;
        public string Tag { get; set; } = null!;
        public DateTime ReleaseDate { get; set; }
        public DateTime LatestUpdate { get; set; }
        public string HexDigest { get; set; } = null!;
        public decimal[]? ExposedPorts { get; set; }
        public string[]? Volumes { get; set; }
        public string[]? EnvVariables { get; set; }
        public long AppId { get; set; }

        public virtual ICollection<Tcu> Tcus { get; set; }
    }
}
