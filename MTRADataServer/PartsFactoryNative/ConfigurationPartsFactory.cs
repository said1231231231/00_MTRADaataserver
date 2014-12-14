using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeConfigurationLib;
using NativeConfigurationLib.NativeConfiguration;

namespace MTRADataServer.PartsFactory
{
    public abstract class ConfigurationPartsFactory
    {
        public abstract void CreateDataConfiguration(DataConfiguration dc);
        public abstract void SetNamePTK(DataConfiguration dc);
        public abstract void CreateDataserver(DataConfiguration dc);
        public abstract void SetDSName(DataServer ds);
        public abstract void SetDSGuid(DataServer ds);
        public abstract void CreateDataSource(DataServer ds);
        //public abstract void GetDataSourceName(DataServer ds);
        //public abstract void CreateDataController(DataServer ds, string dsh /*имя источника*/ );
        //public abstract void CreateDevice(DataController dsrc);
        //public abstract void CreateDeviceTag(Device dev);
        //public abstract void CreateDeviceHierarchyGroup(Device dev); 
        //public abstract void CreateDeviceCommand(Device dev);
    }
}
