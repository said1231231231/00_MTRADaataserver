using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareConfigurationLib.HardwareConfiguration;

namespace MTRADataServer.PartsFactoryHardware
{
    public abstract class ConfigurationPartsFactoryHardware
    {
        public abstract void CreateDataConfiguration(DataConfigurationHardware dc);
        public abstract void CreateDataserver(DataConfigurationHardware dc);
        public abstract void SetDSGuid(DataServerHardware ds);
        public abstract void CreateDataSource(DataServerHardware ds);
        //public abstract void SetDataSourceController(DataSourceHardware dsh);
        //public abstract void SetDevices4DataController(DataControllerHardware dch);
        //public abstract void CreateDeviceTags(DeviceHardware dev);
    }
}
