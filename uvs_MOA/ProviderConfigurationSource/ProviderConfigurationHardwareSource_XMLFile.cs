/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: ProviderConfigurationSource_XMLFile - реализация интерфейса конфигурирования источника DS на базе xml-файлов
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\uvs_MOA\ProviderConfigurationSource\ProviderConfigurationSource_XMLFile.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С# 5.0, Framework 4.5                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : xx.xx.2014
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Легенда:
* 
*#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceLibrary;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;

namespace uvs_MOA.ProviderConfigurationSource
{
    public class ProviderConfigurationHardwareSource_XMLFile : IProviderConfiguration4HardwareSources
    {
        #region создать источник
        /// <summary>
        /// Создать источник
        /// </summary>
        /// <returns></returns>
        public HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware CreateDataSourceHardware(string name_src)
        {
            //HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware dsrchard_moa_ecu = null;
            uvs_MOA.HardwareConfiguration_MOA_ECU.DataSourceHardware_MOA_ECU dsrchard_moa_ecu = null;
            try
            {
                /*
                 * работаем с совокупностью файлов по заранее 
                 * определенному местоположению :
                 * папка \Project в папке запуска DS
                 */
                string PathToConfigurationFile = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathToConfigurationFile();

                /*
                 * в файле Configuration.cfg секция Sources - 
                 * перечисляем источники, находим нужный по заданному имени
                 * и конфигурируем его
                 */
                XDocument xdocPathToConfigurationFile = XDocument.Load(PathToConfigurationFile);
                XElement xe_srcs = xdocPathToConfigurationFile.Element("MTRA").Element("Configuration").Element("Object").Element("Sources");

                var xe_Sources = xe_srcs.Elements("Source");

                foreach (XElement xe_src in xe_Sources)
                {
                    if (xe_src.Element("SourceDriver").Attribute("nameSourceDriver").Value != name_src)
                        continue;

                    //HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware dsrc = null;

                    dsrchard_moa_ecu = new uvs_MOA.HardwareConfiguration_MOA_ECU.DataSourceHardware_MOA_ECU();

                    int udpserver_port = 20000;

                    //прочитаем номер порта
                    if (!int.TryParse(xe_src.Element("SourceDriver").Element("CustomiseDriverInfo").Element("Port").Attribute("value").Value, out udpserver_port))
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 110, "Не задано значение порта для входящих соединений TCP-сервера.\n Порт по умолчанию : ." + udpserver_port.ToString());

                    dsrchard_moa_ecu.udpserver_port = udpserver_port;
                    //(dsrc as uvs_MOA.HardwareConfiguration_MOA_ECU.DataSourceHardware_MOA_ECU).udpserver_port = udpserver_port;

                    dsrchard_moa_ecu.SrcGuid = xe_src.Attribute("SrcGuid").Value;

                    dsrchard_moa_ecu.NameSourceDriver = xe_src.Element("SourceDriver").Attribute("nameSourceDriver").Value;
                    //dsrc.DataServerParent = ds;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dsrchard_moa_ecu;
        }        
        #endregion

        #region инициализация контроллеров
        /// <summary>
        /// инициализация контроллеров
        /// </summary>
        /// <param name="dsh"></param>
        public void SetDataSourceController(HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware dsh)
        {
            try
            {
                /*
                 * по имени источника заходим в папку источников, 
                 * открываем файл PrgDev.cfg и формируем список контроллеров 
                 * и устройств этого источника
                 */
                string PathTo_PrgDevCFG_cdp_File = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(dsh.NameSourceDriver);
                if (!File.Exists(PathTo_PrgDevCFG_cdp_File))
                    throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                XDocument xdoc_PathTo_PrgDevCFG_cdp_File = XDocument.Load(PathTo_PrgDevCFG_cdp_File);
                var xe_cntrls = xdoc_PathTo_PrgDevCFG_cdp_File.Element("MTRA").Element("Configuration").Elements("SourceECU");

                HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU dch = null;

                foreach (XElement xe_cntrl in xe_cntrls)
                {
                    dch = GetController(dsh.NameSourceDriver);// new DataControllerHardware();
                    dch.СontrollerNumber = xe_cntrl.Attribute("NumECU").Value;
                    dch.DataSourceParent = dsh;

                    dsh.ListDataControllerHardware.Add(dch);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU GetController(string namesrc)
        {
            HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU dch = null;
            try
            {
                switch (namesrc)
                {
                    case "MOA_ECU":
                        dch = new uvs_MOA.HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dch;
        }        
        #endregion        

        #region Инициализация устройств
        /// <summary>
        /// Инициализация устройств
        /// </summary>
        /// <param name="dch"></param>
        public void SetDevices4DataController(HardwareConfigurationLib.HardwareConfiguration.DataControllerHardware dch)
        {
            try
            {
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = dch.DataSourceParent.NameSourceDriver;
                string numcontroller = dch.СontrollerNumber;

                string PathTo_PrgDevCFG_cdp_File = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(namesource);
                if (!File.Exists(PathTo_PrgDevCFG_cdp_File))
                    throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                XDocument xdoc_PathTo_PrgDevCFG_cdp_File = XDocument.Load(PathTo_PrgDevCFG_cdp_File);
                var xe_cntrls = xdoc_PathTo_PrgDevCFG_cdp_File.Element("MTRA").Element("Configuration").Elements("SourceECU");

                HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU dh = null;

                foreach (XElement xe_cntrl in xe_cntrls)
                {
                    if (xe_cntrl.Attribute("NumECU").Value == numcontroller)
                    {
                        /*
                         * создать устройство для самого контроллера, 
                         * но понимать что его может и не быть реально
                         */
                        switch (namesource)
                        {
                            case "MOA_ECU":
                                dh = new uvs_MOA.HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU();
                                dh.DevGUID = uint.Parse(xe_cntrl.Attribute("objectGUID").Value);
                                dh.Enable = bool.Parse(xe_cntrl.Attribute("enable").Value);
                                dh.DeviceType = xe_cntrl.Attribute("typeECU").Value;
                                dh.DataControllerHardwareParent = dch;

                                HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU dcme = (HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU)dch;

                                AddDevice2Lists(dcme, dh);
                                break;
                            default:
                                break;
                        }

                        var xe_devs = xe_cntrl.Element("ECUDevices").Elements("Device");
                        foreach (var xe_dev in xe_devs)
                        {
                            /*
                             * создание устройства по его ParsingVariant
                             * для учета особенностей описания устройства
                             */
                            if (xe_dev.Attributes("ParsingVariant").Count() == 0)
                            {
                                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 199, string.Format("{0} : {1} : Устройство {2} не создано.", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "SetDevices4DataController()", xe_dev.Attribute("objectGUID").Value));
                                continue;
                            }
                            else if (string.IsNullOrWhiteSpace(xe_dev.Attribute("ParsingVariant").Value))
                            {
                                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 204, string.Format("{0} : {1} : Устройство {2} не создано.", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "SetDevices4DataController()", xe_dev.Attribute("objectGUID").Value));
                                continue;
                            }

                            dh.ParsingVariant = xe_dev.Attribute("ParsingVariant").Value;

                            switch (xe_dev.Attribute("ParsingVariant").Value)
                            {
                                case "BMRZDescrMOA":
                                    dh = new uvs_MOA.HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU_BMRZDescrMOA();
                                    break;
                                case "BMRZ_100_DescrMOA":
                                    dh = new uvs_MOA.HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU_BMRZ_100_DescrMOA();
                                    break;
                                case "BlockingVirtualDevice":
                                    break;
                                default:
                                    break;
                            }

                            dh.DevGUID = uint.Parse(xe_dev.Attribute("objectGUID").Value);
                            dh.Enable = bool.Parse(xe_dev.Attribute("enable").Value);
                            dh.DeviceType = xe_dev.Attribute("TypeName").Value;
                            dh.DataControllerHardwareParent = dch;

                            HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU dcme = (HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU)dch;

                            AddDevice2Lists(dcme, dh);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }
        }
        /// <summary>
        /// добавить устройство в списки контроллера, dataserver
        /// </summary>
        /// <param name="dch"></param>
        /// <param name="dh"></param>
        private void AddDevice2Lists(HardwareConfiguration_MOA_ECU.DataControllerHardware_MOA_ECU dch, HardwareConfiguration_MOA_ECU.DeviceHardware_MOA_ECU dh)
        {
            try
            {
                dch.ListDevice4DataController.Add(dh);
                dch.DataSourceParent.DataServerParent.ListDevice4DS.Add(dh);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        } 
	    #endregion

        #region инициализация тегов
        /// <summary>
        /// инициализация тегов устройств
        /// </summary>
        /// <param name="dev"></param>
        public void CreateDeviceTags(HardwareConfigurationLib.HardwareConfiguration.DeviceHardware dev)
        {
            try
            {
                // для некоторых устройств (виртуальные устройства) - теги hardware не создаются
                switch (dev.ParsingVariant)
                {
                    case "BlockingVirtualDevice":
                        return;
                    default:
                        break;
                }
                // открыть файл устройства и инициализировать теги
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = dev.DataControllerHardwareParent.DataSourceParent.NameSourceDriver;
                string numcontroller = dev.DataControllerHardwareParent.СontrollerNumber;
                uint numdev = dev.DevGUID;

                string PathTo_DevCFG_File = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_DevCFG_File(numdev);

                if (!File.Exists(PathTo_DevCFG_File))
                    throw new FileNotFoundException("Файл описания устройства не существует");

                XDocument xdoc_DevCFG_File = XDocument.Load(PathTo_DevCFG_File);

                var xe_tags = xdoc_DevCFG_File.Element("Device").Element("Tags").Elements("Tag");

                /*
                 *  в списке д.б. только уник теги
                 *  для моа ключом является адрес регистра, 
                 *  поэтому нужно отслеживать их
                 */
                List<uint> guids = new List<uint>();

                foreach (XElement xe_tag in xe_tags)
                {
                    HardwareConfigurationLib.HardwareConfiguration.TagHardware newtag = dev.CreateTagHardware(xe_tag);
                    if (newtag != null)
                    {
                        if (guids.Contains(newtag.TagGuid))
                            continue;

                        // учтем и добавим в список тегов устройства
                        guids.Add(newtag.TagGuid);
                        dev.dictTags4Parse.Add(newtag.TagGuid, newtag);

                        dev.LstTags.Add(newtag);
                    }
                    else
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 218, string.Format("{0} : {1} : Тег не создан .", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "CreateDeviceTags()"));
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion
    }
}
