using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareConfigurationLib.HardwareConfiguration;
using LinksLib.LinksHT2NT;
using NativeConfigurationLib.NativeConfiguration;

namespace LinksLib.LinksHT2NT_OPC_ECU
{
    public class SimpleLinkHT2NT : LinksHT2NT.LinkHT2NTBase
    {
        public SimpleLinkHT2NT(DataConfigurationHardware dchHC, DataConfiguration dcNC)
            : base(dchHC, dcNC)
        {
        }
    }
}
