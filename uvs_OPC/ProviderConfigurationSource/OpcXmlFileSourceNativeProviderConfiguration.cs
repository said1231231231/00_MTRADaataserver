using CommonClassesLib.CommonClasses;
using InterfaceLibrary;
using LinksLib.LinksHT2NT;
using LinksLib.LinksHT2NT_OPC_ECU;
using NativeConfigurationLib.NativeConfiguration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace uvs_OPC.ProviderConfigurationSource
{
    public class OpcXmlFileSourceNativeProviderConfiguration : IProviderConfiguration4NativeSource
    {
        List<LinkHT2NTBase> _linksList;

        #region IProviderConfiguration4NativeSource implementation

        public void CreateDataSourceNative(DataServer ds, string name_src, List<LinkHT2NTBase> lstLinksHT2NT)
        {
            _linksList = lstLinksHT2NT;

            try
            {
                ds.DATASOURCES.Add(name_src);

                CreateDataController(ds, name_src);

                //на каждом контроллере выяснить устройства
                foreach (DataController datacnrl in ds.DATACONTROLLER)
                {
                    if (datacnrl.DataSourceName4ThisController != name_src)
                        continue;

                    // создадим устройства
                    CreateDevice(datacnrl);
                    // создадим теги, иерархию групп, команды
                    foreach (var vdev in datacnrl.ListDevice4DataController)
                    {
                        // на каждом устройстве сформировать теги
                        CreateDeviceTag(vdev);
                        // иерархию групп
                        CreateDeviceHierarchyGroup(vdev);
                        // команды
                        CreateDeviceCommand(vdev);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        #endregion

        #region инициализация контроллеров

        /// <summary>
        /// Создаем все контроллеры
        /// </summary>
        private void CreateDataController(DataServer ds, string name_src /*имя источника*/)
        {
            string pathToPrgDevCfgFile = ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(name_src);
            if (!File.Exists(pathToPrgDevCfgFile))
                throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

            XDocument prgDevCfgFileXDocument = XDocument.Load(pathToPrgDevCfgFile);
            var sourcesXElements = prgDevCfgFileXDocument.Element("MTRA")
                .Element("Configuration")
                .Elements("SourceECU");

            foreach (var sourceXElement in sourcesXElements)
            {
                try
                {
                    var sourceNative = new DataController();

                    sourceNative.СontrollerNumber = sourceXElement.Attribute("objectGUID").Value;
                    sourceNative.DataSourceName4ThisController = name_src;
                    sourceNative.DataServer4ThisDevice = ds;

                    ds.DATACONTROLLER.Add(sourceNative);
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
            }
        }

        #endregion

        #region инициализация устройства
        /// <summary>
        /// набить список устройств контроллера
        /// устройствами
        /// </summary>
        private void CreateDevice(DataController dataControllerNative)
        {
            try
            {
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = dataControllerNative.DataSourceName4ThisController;
                string numcontroller = dataControllerNative.СontrollerNumber;

                string pathToPrgDevCfgCdpFile = ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(namesource);

                if (!File.Exists(pathToPrgDevCfgCdpFile))
                    throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                XDocument deviceConfigurationFileXDocument = XDocument.Load(pathToPrgDevCfgCdpFile);
                var sourcesXElements = deviceConfigurationFileXDocument.Element("MTRA").Element("Configuration").Elements("SourceECU");

                foreach (XElement sourceXElement in sourcesXElements)
                {
                    if (sourceXElement.Attribute("objectGUID").Value == numcontroller)
                    {
                        var deviceXElements = sourceXElement.Element("ECUDevices").Elements("Device");
                        foreach (var deviceXElement in deviceXElements)
                            try
                            {
                                var deviceNative = new Device();

                                deviceNative.DevGUID = uint.Parse(deviceXElement.Attribute("objectGUID").Value);
                                deviceNative.Enable = bool.Parse(deviceXElement.Attribute("enable").Value);

                                if (deviceNative.Enable == false)
                                    continue;

                                //deviceNative.DeviceType = deviceXElement.Attribute("TypeName").Value;
                                deviceNative.DataControllerParent = dataControllerNative;
                                deviceNative.DataServer4ThisDevice = dataControllerNative.DataServer4ThisDevice;

                                dataControllerNative.ListDevice4DataController.Add(deviceNative);
                                dataControllerNative.DataServer4ThisDevice.ListDevice4DS.Add(deviceNative);
                            }
                            catch (Exception ex)
                            {
                                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                                throw;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw;
            }
        }
        #endregion

        #region инициализация тега
        private void CreateDeviceTag(Device deviceNative)
        {
            try
            {
                // открыть файл устройства и инициализировать теги
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = deviceNative.DataControllerParent.DataSourceName4ThisController;
                string numcontroller = deviceNative.DataControllerParent.СontrollerNumber;
                uint numdev = deviceNative.DevGUID;

                string pathToPrgDevCfgCdpFile = ProjectCommonData.GetPathTo_DevCFG_File(numdev);

                if (!File.Exists(pathToPrgDevCfgCdpFile))
                    throw new FileNotFoundException("Файл описания устройства не существует");

                XDocument deviceConfigurationFileXDocument = XDocument.Load(pathToPrgDevCfgCdpFile);

                var tagsXElements = deviceConfigurationFileXDocument.Element("Device").Element("Tags").Elements("Tag");

                foreach (XElement tagXElement in tagsXElements)
                {
                    var tagNative = new Tag();

                    var deviceLevelDescribeXElement = tagXElement.Element("Device_level_Describe");

                    tagNative.TagGUID = uint.Parse(tagXElement.Attribute("TagGUID").Value);
                    tagNative.TagName = deviceLevelDescribeXElement.Element("name").Value;
                    tagNative.TypeTag = deviceLevelDescribeXElement.Element("type").Value;

                    CreateLink(deviceNative, tagNative);

                    // учтем и добавим в список тегов устройства
                    deviceNative.dictTags4Parse.Add(tagNative.TagGUID, tagNative);
                    deviceNative.Tags.Add(tagNative);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        private void CreateLink(Device deviceNative, Tag tagNative)
        {
            var link = new SimpleLinkHT2NT(deviceNative.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware,
                deviceNative.DataServer4ThisDevice.DATACONFIGURATION);

            link.SetLink2TAGHD(deviceNative.DevGUID, tagNative.TagGUID);
            link.SetLink2TAGND(deviceNative.DevGUID, tagNative.TagGUID);

            _linksList.Add(link);
        }

        #endregion

        #region инициализация групп устройства
        private void CreateDeviceHierarchyGroup(Device dev)
        {
            try
            {
                string PathTo_DevCFG_File = ProjectCommonData.GetPathTo_DevCFG_File(dev.DevGUID);

                if (!File.Exists(PathTo_DevCFG_File))
                    throw new FileNotFoundException("Файл описания устройства не существует");

                XDocument xdoc_DevCFG_File = XDocument.Load(PathTo_DevCFG_File);

                var xdev_groups = xdoc_DevCFG_File.Element("Device").Element("Groups");

                CreateGroupHierarchy(dev, xdev_groups);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        private void CreateGroupHierarchy(Device dev, XElement xeGroupHierarchy)
        {
            try
            {
                // список групп первого уровня
                List<Group> RTUGroupsList = new List<Group>();

                IEnumerable<XElement> xe_groups = xeGroupHierarchy.Elements("Group");

                // создаем список групп первого уровня
                foreach (XElement xe_group in xe_groups)
                {
                    Group ng = CreateGroup(xe_group, dev);

                    CreateSubItemInGroup(ng, xe_group, dev);

                    RTUGroupsList.Add(ng);
                }

                //foreach (var group in RTUGroupsList)
                //    CreateSubItemInGroup(group);

                dev.Groups = RTUGroupsList;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        private Group CreateGroup(XElement xe_group, Device dev)
        {
            Group group = new Group();

            try
            {
                group.GroupName = xe_group.Attribute("Name").Value;
                //group.GroupGUID = uint.Parse(xe_group.Attribute("GroupGUID").Value);
                group.SubGroupList = new List<Group>();
                group.TagList = new List<Tag>();
                //group.GroupXElement = new XElement(xe_group);

                if (xe_group.Attributes("enable").Count() > 0)
                    group.Enable = bool.Parse(xe_group.Attribute("enable").Value);

                IEnumerable<XElement> xe_tags;

                string str = string.Empty;

                if (xe_group.Elements("Tags").Count() != 0)
                {
                    xe_tags = xe_group.Element("Tags").Elements("TagGuid");
                    if (xe_tags != null)
                        foreach (XElement xe_tag in xe_tags)
                            if (xe_tag.Attribute("enable").Value.ToLower() == "true")
                            {
                                if (!dev.dictTags4Parse.ContainsKey(uint.Parse(xe_tag.Attribute("value").Value)))
                                {
                                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 698, string.Format("{0} : {1} : Ссылка из группы на тег {2} не создана.", @"X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactory\FileConfigurationPartsFactory.cs", "CreateGroup()", xe_tag.Attribute("value").Value));
                                    continue;
                                }

                                Tag tag = dev.dictTags4Parse[uint.Parse(xe_tag.Attribute("value").Value)];
                                group.TagList.Add(tag);
                            }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return group;
        }

        private void CreateSubItemInGroup(Group group, XElement xe_subgrop, Device dev)
        {
            try
            {
                IEnumerable<XElement> xe_groups = xe_subgrop.Elements("Group");

                foreach (var xe_group in xe_groups)
                {
                    Group gr = CreateGroup(xe_group, dev);

                    group.SubGroupList.Add(gr);

                    IEnumerable<XElement> xe_subgroups = xe_group.Elements("Group");
                    if (xe_subgroups.Count() != 0)
                        foreach (XElement xe_subsubgrop in xe_subgroups)
                            CreateSubItemInGroup(gr, xe_subsubgrop, dev);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

        #region инициализация команды
        private void CreateDeviceCommand(Device dev)
        {
            Command cmd = null;
            try
            {
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

    }
}
