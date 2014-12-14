/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: ProviderConfigurationNative_OldXMLFile_MOA - реализация интерфейса конфигурирования DS (уровень Native) на базе xml-файлов
 *                                                                             
 *Файл                     :X:\Projects\00_MTRADataServer\MTRADataServer\ProviderConfiguration\OldXMLJile_MOA\ProviderConfigurationNative_OldXMLFile_MOA.cs
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
using NativeConfigurationLib.NativeConfiguration;
using System.Diagnostics;
using InterfaceLibrary;
using uvs_OPC.ProviderConfigurationSource;

namespace MTRADataServer.ProviderConfiguration.OldXMLJile_MOA
{
    public class ProviderConfigurationNative_OldXMLFile_MOA : InterfaceLibrary.IProviderConfigurationNative
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
        /// имя птк от провайдера
        /// </summary>
        public string GetNamePTK()
        {
            string name_ptk = string.Empty;
            try
            {
                XDocument XDoc4PathToPrjFile = XDocument.Load(PathToPrjFile);
                name_ptk = XDoc4PathToPrjFile.Element("Project").Element("NamePTK").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return name_ptk;
        }
                /// <summary>
        /// имя DS от провайдера
        /// </summary>
        public string Get_NameDS_GUID()
        {
            string nameDS_GUID = string.Empty;
            try
            {
                XDocument xdocPathToConfigurationFile = XDocument.Load(PathToConfigurationFile);
                nameDS_GUID = xdocPathToConfigurationFile.Element("MTRA").Element("Configuration").Element("Object").Attribute("name").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return nameDS_GUID;
        }
        /// <summary>
        /// уник номер DS
        /// </summary>
        /// <returns></returns>
        public string GetDSGuid()
        {
            string dS_GUID = string.Empty;
            try
            {
                XDocument xdocPathToConfigurationFile = XDocument.Load(PathToConfigurationFile);
                dS_GUID = xdocPathToConfigurationFile.Element("MTRA").Element("Configuration").Element("Object").Attribute("UniDS_GUID").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dS_GUID;
        }
        /// <summary>
        /// инициализировать описание DataServer
        /// </summary>
        /// <returns></returns>
        public void InitDataServerDescription(NativeConfigurationLib.NativeConfiguration.DataServer ds)
        { 
            try
            {
                InitDataSources(ds);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #region Информация об источниках
        /// <summary>
        /// Создать источники
        /// </summary>
        /// <returns></returns>
        private void InitDataSources(NativeConfigurationLib.NativeConfiguration.DataServer ds)
        {
            IProviderConfiguration4NativeSource ipcns = null;

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
                /*
                 * пройти по источникам:
                 * создать контроллеры, устройства, теги
                */

                foreach (XElement xe_src in xe_Sources)
                {
                    //HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware dsrc = null;
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
                            ipcns = new uvs_MOA.ProviderConfigurationSource.ProviderConfigurationNativeSource_XMLFile();
                            // создать источник и добавить его элементы в DataServer
                            ipcns.CreateDataSourceNative(ds, name_src, MTRADataServer.App.LstLinksHT2NT);
                            break;
                        case "OPC_ECU":
                            ipcns = new OpcXmlFileSourceNativeProviderConfiguration();
                            ipcns.CreateDataSourceNative(ds, name_src, MTRADataServer.App.LstLinksHT2NT);
                            break;
                        default:
                            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 176, string.Format("{0} : {1} : Источник {2} не создан .", @"X:\Projects\00_MTRADataServer\MTRADataServer\ProviderConfiguration\OldXMLJile_MOA\ProviderConfigurationNative_OldXMLFile_MOA.cs", "InitDataSources()", xe_src.Element("SourceDriver").Attribute("nameSourceDriver").Value));
                            break;
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
