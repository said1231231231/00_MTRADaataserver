using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareConfigurationLib.HardwareConfiguration
{
    public class DataConfigurationHardware
    {
        /// <summary>
        /// DataServer
        /// </summary>
        public DataServerHardware DATASERVER;

        public virtual void Configure()
        { 
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        }

        /// <summary>
        /// получить устройство Hardware
        /// </summary>
        /// <param name="devguid"></param>
        /// <returns></returns>
        public DeviceHardware GetDeviceByGUID(uint devguid)
        {
            DeviceHardware dh = null;
            try
            {
                foreach (DeviceHardware dha in this.DATASERVER.ListDevice4DS)
                    if (dha.DevGUID == devguid)
                    {
                        dh = dha;
                        break;
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dh;
        }
    }
}
