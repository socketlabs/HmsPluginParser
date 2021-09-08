using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsPluginParser
{
    public class SessionState
    {
        public SessionState(string ipAddress)
        {
            this.IpAddress = ipAddress;
        }
        public string IpAddress { get; set; }
        public bool ReturnTemporaryFailure { get; set; } = false;
    }
}
