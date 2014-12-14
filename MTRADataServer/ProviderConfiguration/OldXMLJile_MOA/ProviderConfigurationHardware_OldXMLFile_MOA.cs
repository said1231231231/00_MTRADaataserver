/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: ProviderConfigurationHardware_OldXMLFile_MOA - реализация интерфейса конфигурирования DS  (уровень Hardware) на базе xml-файлов
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\ProviderConfiguration\OldXMLJile_MOA\ProviderConfigurationHardware_OldXMLFile_MOA.cs
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
using System.IO;
using System.Xml.Linq;
using HardwareConfigurationLib.HardwareConfiguration;
using System.Diagnostics;
using InterfaceLibrary;
using uvs_OPC.ProviderConfigurationSource;

namespace MTRADataServer.ProviderConfiguration.OldXMLJile_MOA
{
    public class ProviderConfigurationHardware_OldXMLFile_MOA : InterfaceLibrary.IProviderConfigurationHardware
    {
        #region private-поля
        /// <summary>
        /// путь к файлу проекта Project.cfg
        /// </summary>
        string PathToPrjFile = string.Empty;
        /// <summary>
        /// путь к файлу проекта Configuration.cfg
        /// </summary>
        string PathToConfigurationFile = string.Empty;
        #endregion

        /// <summary>
        /// создать инициализировать 
        /// провайдер
        /// </summary>
        public void CreateProvider()
        {
            /*
             * работаем с совокупностью файлов по заранее 
             * определенному местоположению :
             * папка \Project в папке запуска DS
             */
            PathToPrjFile = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathToPrjFile();

            if (!File.Exists(PathToPrjFile))
                throw new FileNotFoundException("Файл Project.cfg не существует");

            PathToConfigurationFile = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathToConfigurationFile();

            if (!File.Exists(PathToConfigurationFile))
                throw new FileNotFoundException("Файл Configuration.cfg не существует");
        }
        /// <summary>
        /// идентификатор DS
        /// </summary>
        /// <returns></returns>
        public string Get_UniDS_GUID()
        {
            string UniDS_GUID = string.Empty;
            try
            {
                XDocument xdocPathToConfigurationFile = XDocument.Load(PathToConfigurationFile);
                UniDS_GUID = xdocPathToConfigurationFile.Element("MTRA").Element("Configuration").Element("Object").Attribute("UniDS_GUID").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return UniDS_GUID;
        }
        /// <summary>
        /// Создать источники
        /// </summary>
        /// <returns></returns>
        public List<HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware> GetDataSources(HardwareConfigurationLib.HardwareConfiguration.DataServerHardware ds)
        {
            List<DataSourceHardware> lstDATASOURCES = new List<DataSourceHardware>();
            IProviderConfiguration4HardwareSources ipcs = null;

            try
            {
                /*
                 * в файле Configuration.cfg секция Sources - 
                 * перечисляем и создаем источник - его в список 
                 * источников для DS
                 */
                XDocument xdocPathToConfigurationFile = XDocument.Load(PathToConfigurationFile);
                XElement xe_srcs = xdocPathToConfigurationFile.Element("MTRA").Element("Configuration").Element("Object").Element("Sources");

                var xe_Sources = xe_srcs.Elements("Source");

                foreach (XElement xe_src in xe_Sources)
                {
                    HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware dsrc = null;
                    string name_src = xe_src.Element("SourceDriver").Attribute("nameSourceDriver").Value;

                    if (xe_src.Element("SourceDriver").Attribute("enable").Value.ToLower() == "false")
                        break;

                    switch (name_src)
                    {
                        case "MOA_ECU":
                            /*
                             * в этом месте возможна фабрика,
                             * кот возвращает реализацию формирования
                             * источника по типу его описания, 
                             * пока же задаем это прямо
                             */
                            ipcs = new uvs_MOA.ProviderConfigurationSource.ProviderConfigurationHardwareSource_XMLFile();
                            // создать источник Hardware по его имени
                            dsrc = ipcs.CreateDataSourceHardware(name_src);
                            dsrc.DataServerParent = ds;
                            /*
                            * создать контроллеры, устройства, теги
                            */
                            CreateCntrlDevsTags4Source(ipcs, dsrc);
                            break;
                        case "OPC_ECU":
                            ipcs = new OpcXmlFileSourceHardwareProviderConfiguration();
                            // создать источник Hardware по его имени
                            dsrc = ipcs.CreateDataSourceHardware(name_src);
                            dsrc.DataServerParent = ds;
                            /*
                            * создать контроллеры, устройства, теги
                            */
                            CreateCntrlDevsTags4Source(ipcs, dsrc);
                            break;
                        default:
                            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 117, string.Format("{0} : {1} : Источник {2} не создан .", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactoryHardware\FileConfigurationPartsFactoryHardware.cs", "CreateDeviceTags()", xe_src.Element("SourceDriver").Attribute("nameSourceDriver").Value));
                            break;
                    }

                    if (dsrc != null)
                        lstDATASOURCES.Add(dsrc);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return lstDATASOURCES;
        }

        private void CreateCntrlDevsTags4Source(IProviderConfiguration4HardwareSources ipcs, DataSourceHardware dsh)
        {
            try
            {
                //на каждом источнике настроить контроллеры
                ipcs.SetDataSourceController(dsh);

                //на каждом контроллере настроить устройства
                foreach (DataControllerHardware datacnrlHW in dsh.ListDataControllerHardware)
                {
                    ipcs.SetDevices4DataController(datacnrlHW);

                    foreach (var vdev in datacnrlHW.ListDevice4DataController)
                    {
                        // на каждом устройстве сформировать теги
                        ipcs.CreateDeviceTags(vdev);
                        // группы формировать для уровня Hardware не нужно
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}