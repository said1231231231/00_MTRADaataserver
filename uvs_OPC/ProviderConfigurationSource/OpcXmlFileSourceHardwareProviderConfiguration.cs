using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardwareConfigurationLib.HardwareConfiguration;
using InterfaceLibrary;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using uvs_OPC.HardwareConfiguration;

namespace uvs_OPC.ProviderConfigurationSource
{
    public class OpcXmlFileSourceHardwareProviderConfiguration : IProviderConfiguration4HardwareSources
    {
        #region IProviderConfiguration4HardwareSources implementation

        public DataSourceHardware CreateDataSourceHardware(string name)
        {
            OpcDataSourceHardware opcSource = null;

            try
            {
                opcSource = new OpcDataSourceHardware();
                opcSource.NameSourceDriver = name;
                opcSource.SrcGuid = "dsfsdfsdfsdf";
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return opcSource;
        }

        public void SetDataSourceController(DataSourceHardware dataSourceHardware)
        {
            try
            {
                string pathToPrgDevCfgFile = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(dataSourceHardware.NameSourceDriver);
                if (!File.Exists(pathToPrgDevCfgFile))
                    throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                XDocument prgDevCfgFileXDocument = XDocument.Load(pathToPrgDevCfgFile);
                var sourcesXElements = prgDevCfgFileXDocument.Element("MTRA").Element("Configuration").Elements("SourceECU");

                foreach (var sourceXElement in sourcesXElements)
                {
                    var opcControllerHardware = new OpcControllerHardware();
                    opcControllerHardware.DataSourceParent = dataSourceHardware;
                    opcControllerHardware.СontrollerNumber = sourceXElement.Attribute("objectGUID").Value;
                    opcControllerHardware.OpcServerUrl = sourceXElement.Attribute("Url").Value;
                    opcControllerHardware.ObjectGUID = sourceXElement.Attribute("objectGUID").Value;
                    opcControllerHardware.UpdateRate = int.Parse(sourceXElement.Attribute("UpdateRate").Value);

                    dataSourceHardware.ListDataControllerHardware.Add(opcControllerHardware);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw;
            }
        }

        public void SetDevices4DataController(DataControllerHardware dataControllerHardware)
        {
            try
            {
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = dataControllerHardware.DataSourceParent.NameSourceDriver;
                string numcontroller = dataControllerHardware.СontrollerNumber;

                string pathToPrgDevCfgFile = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(namesource);
                if (!File.Exists(pathToPrgDevCfgFile))
                    throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                XDocument prgDevCfgFileXDocument = XDocument.Load(pathToPrgDevCfgFile);
                var sourcesXElements = prgDevCfgFileXDocument.Element("MTRA").Element("Configuration").Elements("SourceECU");

                foreach (XElement sourceXElement in sourcesXElements)
                {
                    if (sourceXElement.Attribute("objectGUID").Value != numcontroller)
                        continue;

                    var devicesXElements = sourceXElement.Element("ECUDevices").Elements("Device");
                    foreach (var deviceXElement in devicesXElements)
                    {
                        var deviceHardware = new DeviceHardware();

                        deviceHardware.DevGUID = uint.Parse(deviceXElement.Attribute("objectGUID").Value);
                        deviceHardware.Enable = bool.Parse(deviceXElement.Attribute("enable").Value);
                        //deviceHardware.DeviceType = deviceXElement.Attribute("TypeName").Value;
                        deviceHardware.DataControllerHardwareParent = dataControllerHardware;

                        dataControllerHardware.ListDevice4DataController.Add(deviceHardware);
                        dataControllerHardware.DataSourceParent.ListDevice4DataSource.Add(deviceHardware);
                        dataControllerHardware.DataSourceParent.DataServerParent.ListDevice4DS.Add(deviceHardware);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw;
            }
        }

        public void CreateDeviceTags(DeviceHardware deviceHardware)
        {
            try
            {
                // открыть файл устройства и инициализировать теги
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = deviceHardware.DataControllerHardwareParent.DataSourceParent.NameSourceDriver;
                string numcontroller = deviceHardware.DataControllerHardwareParent.СontrollerNumber;
                uint numdev = deviceHardware.DevGUID;

                string pathToDeviceConfigurationFile = CommonClassesLib.CommonClasses.ProjectCommonData.GetPathTo_DevCFG_File(numdev);

                if (!File.Exists(pathToDeviceConfigurationFile))
                    throw new FileNotFoundException("Файл описания устройства не существует");

                XDocument deviceConfigurationFileXDocument = XDocument.Load(pathToDeviceConfigurationFile);

                // Сразу загружаем только включенные теги.
                var tagsXElements = deviceConfigurationFileXDocument.Element("Device")
                    .Element("Tags")
                    .Elements("Tag")
                    .Where(element => element.Attribute("TagEnable").Value.Equals("true", StringComparison.InvariantCultureIgnoreCase));

                deviceHardware.dictTags4Parse = new Dictionary<uint, TagHardware>(tagsXElements.Count());
                foreach (XElement tagXElement in tagsXElements)
                {
                    try
                    {
                        var deviceLevelDescribeXElement = tagXElement.Element("Device_level_Describe");

                        var tagHardware = new OpcTagHardware();
                        tagHardware.TagGuid = uint.Parse(tagXElement.Attribute("TagGUID").Value);
                        tagHardware.TagName = deviceLevelDescribeXElement.Element("name").Value;
                        tagHardware.TagType = deviceLevelDescribeXElement.Element("type").Value;
                        tagHardware.Path = deviceLevelDescribeXElement.Element("path").Value;

                        deviceHardware.LstTags.Add(tagHardware);
                        deviceHardware.dictTags4Parse.Add(tagHardware.TagGuid, tagHardware);
                    }
                    catch (Exception ex)
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Warning, 0, "Не удалось иницилизировать тег: " + ex.Message);
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
