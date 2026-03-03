using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.Astronomy.TheSkyX.Models
{
    internal class TheSkyXClient_InitializeResult
    {
        public string Version { get; set; } = string.Empty;
        public string Build { get; set; } = string.Empty;
        public TheSkyXOperatingSystem OperatingSystem { get; set; }
    }


}
