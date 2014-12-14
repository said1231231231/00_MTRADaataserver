using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareConfigurationLib.HardwareConfiguration;

namespace uvs_OPC.HardwareConfiguration
{
    public class OpcDataSourceHardware : DataSourceHardware
    {
        #region Constructors
        #endregion

        #region Public metods

        public override void StartDataCommunicationExchange()
        {
            foreach (var dataControllerHardware in ListDataControllerHardware)
            {
                (dataControllerHardware as OpcControllerHardware).StartSubscribbe();
            }
        }

        #endregion
    }
}
