using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;
using HardwareConfigurationLib;
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib;
using NativeConfigurationLib.NativeConfiguration;
using uvs_MOA;
using InterfaceLibrary;

namespace MTRADataServer.PartsFactoryHardware
{
    public class FileConfigurationPartsFactoryHardware : ConfigurationPartsFactoryHardware
    {
        IProviderConfigurationHardware PROVIDERCONFIGURATION;

        #region конструктор
		public FileConfigurationPartsFactoryHardware(IProviderConfigurationHardware provconf)
        {
            PROVIDERCONFIGURATION = provconf;
            try
            {
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        } 
	    #endregion

        #region инициализация конфигурации DataConfiguration
        /// <summary>
        /// инициализация конфигурации
        /// </summary>
        /// <param name="dc"></param>
        public override void CreateDataConfiguration(DataConfigurationHardware dch)
        {
            try
            {
                PROVIDERCONFIGURATION.CreateProvider();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                Process.GetCurrentProcess().Kill();
            }
        }
        #endregion
        #region инициализация DataServer
        public override void CreateDataserver(DataConfigurationHardware dc)
        {
            DataServerHardware ds = new DataServerHardware();

            try
            {

            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }
            dc.DATASERVER = ds;
        }
       public override void SetDSGuid(HardwareConfigurationLib.HardwareConfiguration.DataServerHardware ds)
        {
            try
            {
                ds.UniDS_GUID = PROVIDERCONFIGURATION.Get_UniDS_GUID();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

       #region инициализация источников
       public override void CreateDataSource(HardwareConfigurationLib.HardwareConfiguration.DataServerHardware ds)
       {
           try
           {
               List<DataSourceHardware> lstDsSources = PROVIDERCONFIGURATION.GetDataSources(ds);

               if (lstDsSources != null)
               { 
                   ds.DATASOURCES = lstDsSources;

                    //foreach ( DataSourceHardware dstc in ds.DATASOURCES )
                    //    dstc.DataServerParent = ds;
               }
           }
           catch (Exception ex)
           {
               TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
               throw ex;
           }
       }
       //public override void SetDataSourceController(HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware dsh)
       //{ 
       //     try
       //     {
       //         /*
       //          * по имени источника заходим в папку источников, 
       //          * открываем файл PrgDev.cfg и формируем список контроллеров 
       //          * и устройств этого источника
       //          */
       //         string PathTo_PrgDevCFG_cdp_File = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(dsh.NameSourceDriver);
       //         if (!File.Exists(PathTo_PrgDevCFG_cdp_File))
       //             throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

       //         XDocument xdoc_PathTo_PrgDevCFG_cdp_File = XDocument.Load(PathTo_PrgDevCFG_cdp_File);
       //         var xe_cntrls = xdoc_PathTo_PrgDevCFG_cdp_File.Element("MTRA").Element("Configuration").Elements("SourceECU");

       //         HardwareConfigurationLib.HardwareConfiguration.DataControllerHardware dch = null;

       //         foreach( XElement xe_cntrl in xe_cntrls )
       //         {
       //             dch = GetController(dsh.NameSourceDriver);// new DataControllerHardware();
       //             dch.СontrollerNumber = xe_cntrl.Attribute("NumECU").Value;
       //             dch.DataSourceParent = dsh;

       //             dsh.ListDataControllerHardware.Add(dch);
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
       //         throw ex;
       //     }
       //}
       ///// <summary>
       ///// 
       ///// </summary>
       ///// <param name="p"></param>
       ///// <returns></returns>
       //private HardwareConfigurationLib.HardwareConfiguration.DataControllerHardware GetController(string namesrc)
       //{
       //    HardwareConfigurationLib.HardwareConfiguration.DataControllerHardware dch = null;
       //     try
       //     {
       //         switch (namesrc)
       //         { 
       //             case "MOA_ECU":
       //                 dch = new  uvs_MOA.HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU();
       //                 break;
       //             default:
       //                 break;
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
       //     }
       //     return dch;
       //}
       //public override void SetDevices4DataController(HardwareConfigurationLib.HardwareConfiguration.DataControllerHardware dch)
       //{ 
       //     try
       //     {
       //         // по ссылке на контроллер найти и сохранить в список устройства
       //         string namesource = dch.DataSourceParent.NameSourceDriver;
       //         string numcontroller = dch.СontrollerNumber;

       //         string PathTo_PrgDevCFG_cdp_File = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(namesource);
       //         if (!File.Exists(PathTo_PrgDevCFG_cdp_File))
       //             throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

       //         XDocument xdoc_PathTo_PrgDevCFG_cdp_File = XDocument.Load(PathTo_PrgDevCFG_cdp_File);
       //         var xe_cntrls = xdoc_PathTo_PrgDevCFG_cdp_File.Element("MTRA").Element("Configuration").Elements("SourceECU");

       //         HardwareConfigurationLib.HardwareConfiguration.DeviceHardware dh = null;

       //         foreach (XElement xe_cntrl in xe_cntrls)
       //         {
       //             if (xe_cntrl.Attribute("NumECU").Value == numcontroller)
       //             {
       //                 /*
       //                  * создать устройство для самого контроллера, 
       //                  * но понимать что его может и не быть реально
       //                  */
       //                 switch (namesource)
       //                 {
       //                     case "MOA_ECU":
       //                         dh = new uvs_MOA.HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU();
       //                         dh.DevGUID = uint.Parse(xe_cntrl.Attribute("objectGUID").Value);
       //                         dh.Enable = bool.Parse(xe_cntrl.Attribute("enable").Value);
       //                         dh.DeviceType = xe_cntrl.Attribute("typeECU").Value;
       //                         dh.DataControllerHardwareParent = dch;

       //                         AddDevice2Lists(dch,dh);
       //                         break;
       //                     default:
       //                         break;
       //                 }

       //                 var xe_devs = xe_cntrl.Element("ECUDevices").Elements("Device");
       //                 foreach (var xe_dev in xe_devs)
       //                 {
       //                     /*
       //                      * создание устройства по его ParsingVariant
       //                      * для учета особенностей описания устройства
       //                      */
       //                     if (xe_dev.Attributes("ParsingVariant").Count() == 0)
       //                     {
       //                         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 199, string.Format("{0} : {1} : Устройство {2} не создано.", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "SetDevices4DataController()", xe_dev.Attribute("objectGUID").Value));
       //                         continue;
       //                     }
       //                     else if (string.IsNullOrWhiteSpace(xe_dev.Attribute("ParsingVariant").Value))
       //                     {
       //                         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 204, string.Format("{0} : {1} : Устройство {2} не создано.", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "SetDevices4DataController()", xe_dev.Attribute("objectGUID").Value));
       //                         continue;
       //                     }

       //                     dh.ParsingVariant = xe_dev.Attribute("ParsingVariant").Value;

       //                     switch (xe_dev.Attribute("ParsingVariant").Value)
       //                     {
       //                         case "BMRZDescrMOA":
       //                             dh = new uvs_MOA.HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU_BMRZDescrMOA();
       //                             break;
       //                         case "BMRZ_100_DescrMOA":
       //                             dh = new uvs_MOA.HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU_BMRZ_100_DescrMOA();
       //                             break;
       //                         case "BlockingVirtualDevice":
       //                             break;
       //                         default:
       //                             break;
       //                     }

       //                     dh.DevGUID = uint.Parse(xe_dev.Attribute("objectGUID").Value);
       //                     dh.Enable = bool.Parse(xe_dev.Attribute("enable").Value);
       //                     dh.DeviceType = xe_dev.Attribute("TypeName").Value;
       //                     dh.DataControllerHardwareParent = dch;

       //                     AddDevice2Lists(dch, dh);
       //                 }
       //             }
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
       //         throw ex;
       //     }
       //}
        /// <summary>
        /// добавить устройство в списки контроллера, dataserver
        /// </summary>
        /// <param name="dch"></param>
        /// <param name="dh"></param>
       //private void AddDevice2Lists(DataControllerHardware dch, DeviceHardware dh)
       //{
       //     try
       //     {
       //         dch.ListDevice4DataController.Add(dh);
       //         dch.DataSourceParent.DataServerParent.ListDevice4DS.Add(dh);
       //     }
       //     catch (Exception ex)
       //     {
       //         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
       //     }            
       //}
       #endregion
        #region инициализация тегов
       //public override void CreateDeviceTags(DeviceHardware dev)
       //{ 
       //     try
       //     {
       //         // для некоторых устройств (виртуальные устройства) - теги hardware не создаются
       //         switch (dev.ParsingVariant)
       //         {
       //             case "BlockingVirtualDevice":
       //                 return;
       //             default:
       //                 break;
       //         }
       //         // открыть файл устройства и инициализировать теги
       //         // по ссылке на контроллер найти и сохранить в список устройства
       //         string namesource = dev.DataControllerHardwareParent.DataSourceParent.NameSourceDriver;
       //         string numcontroller = dev.DataControllerHardwareParent.СontrollerNumber;
       //         uint numdev = dev.DevGUID;

       //         string PathTo_DevCFG_File = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_DevCFG_File(numdev);

       //         if (!File.Exists(PathTo_DevCFG_File))
       //             throw new FileNotFoundException("Файл описания устройства не существует");

       //         XDocument xdoc_DevCFG_File = XDocument.Load(PathTo_DevCFG_File);

       //         var xe_tags = xdoc_DevCFG_File.Element("Device").Element("Tags").Elements("Tag");

       //         /*
       //          *  в списке д.б. только уник теги
       //          *  для моа ключом является адрес регистра, 
       //          *  поэтому нужно отслеживать их
       //          */
       //         List<uint> guids = new List<uint>();

       //         foreach (XElement xe_tag in xe_tags)
       //         {
       //             HardwareConfigurationLib.HardwareConfiguration.TagHardware newtag = dev.CreateTagHardware(xe_tag);
       //             if (newtag != null)
       //             {
       //                 if (guids.Contains(newtag.TagGuid))
       //                     continue;

       //                 // учтем и добавим в список тегов устройства
       //                 guids.Add(newtag.TagGuid);
       //                 dev.dictTags4Parse.Add(newtag.TagGuid, newtag);

       //                 dev.LstTags.Add(newtag);
       //             }
       //             else
       //             {
       //                 TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 218, string.Format("{0} : {1} : Тег не создан .", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "CreateDeviceTags()"));
       //             }
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
       //     }
       //}
	   #endregion
    }
}
