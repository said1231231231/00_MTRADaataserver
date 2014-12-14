/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: ProviderConfigurationNativeSource_XMLFile - класс реализующий заполнение конфигурации Native источником uvs_moa
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\uvs_MOA\ProviderConfigurationSource\ProviderConfigurationNativeSource_XMLFile.cs
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
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib.CommonClasses;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using LinksLib.LinksHT2NT;

namespace uvs_MOA.ProviderConfigurationSource
{
    public class ProviderConfigurationNativeSource_XMLFile : IProviderConfiguration4NativeSource
    {
        List<LinkHT2NTBase> LstLinksHT2NT = new List<LinkHT2NTBase>();

        public void CreateDataSourceNative(DataServer ds, string name_src, List<LinkHT2NTBase> lstLinksHT2NT)
        {
            LstLinksHT2NT = lstLinksHT2NT;

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

        #region инициализация контроллеров
        private void CreateDataController(DataServer ds, string name_src /*имя источника*/)
        {
            try
            {
                /*
                 * по имени источника заходим в папку источников, 
                 * открываем файл PrgDev.cfg и формируем список контроллеров 
                 * и устройств этого источника
                 */
                string PathTo_PrgDevCFG_cdp_File = ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(name_src);
                    if (!File.Exists(PathTo_PrgDevCFG_cdp_File))
                        throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                    XDocument xdoc_PathTo_PrgDevCFG_cdp_File = XDocument.Load(PathTo_PrgDevCFG_cdp_File);
                    var xe_cntrls = xdoc_PathTo_PrgDevCFG_cdp_File.Element("MTRA").Element("Configuration").Elements("SourceECU");

                    DataController dch = null;

                    foreach (XElement xe_cntrl in xe_cntrls)
                    {
                        dch = new DataController();

                        dch.СontrollerNumber = xe_cntrl.Attribute("NumECU").Value;
                        dch.DataSourceName4ThisController = name_src;
                        dch.DataServer4ThisDevice = ds;

                        ds.DATACONTROLLER.Add(dch);
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }
        }
        #endregion

        #region инициализация устройства
        /// <summary>
        /// набить список устройств контроллера
        /// устройствами
        /// </summary>
        /// <param name="dsrc"></param>
        private void CreateDevice(DataController dsrc)
        {
            try
            {
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = dsrc.DataSourceName4ThisController;
                string numcontroller = dsrc.СontrollerNumber;

                string PathTo_PrgDevCFG_cdp_File = ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(namesource);

                if (!File.Exists(PathTo_PrgDevCFG_cdp_File))
                    throw new FileNotFoundException("Файл PrgDevCFG.cdp не существует");

                XDocument xdoc_PathTo_PrgDevCFG_cdp_File = XDocument.Load(PathTo_PrgDevCFG_cdp_File);
                var xe_cntrls = xdoc_PathTo_PrgDevCFG_cdp_File.Element("MTRA").Element("Configuration").Elements("SourceECU");

                Device dh = null;

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
                                dh = new Device();
                                dh.ParsingVariant = "MOA_ECU";
                                dh.DevGUID = uint.Parse(xe_cntrl.Attribute("objectGUID").Value);
                                dh.Enable = bool.Parse(xe_cntrl.Attribute("enable").Value);
                                dh.DeviceType = xe_cntrl.Attribute("typeECU").Value;
                                dh.DataControllerParent = dsrc;
                                dh.DataServer4ThisDevice = dsrc.DataServer4ThisDevice;

                                dsrc.ListDevice4DataController.Add(dh);
                                dsrc.DataServer4ThisDevice.ListDevice4DS.Add(dh);
                                break;
                            default:
                                break;
                        }

                        var xe_devs = xe_cntrl.Element("ECUDevices").Elements("Device");
                        foreach (var xe_dev in xe_devs)
                            try
                            {
                                dh = new Device();

                                dh.DevGUID = uint.Parse(xe_dev.Attribute("objectGUID").Value);
                                dh.Enable = bool.Parse(xe_dev.Attribute("enable").Value);

                                if (dh.Enable == false)
                                    continue;

                                dh.DeviceType = xe_dev.Attribute("TypeName").Value;
                                if (xe_dev.Attributes("ParsingVariant").Count() > 0)
                                    dh.ParsingVariant = xe_dev.Attribute("ParsingVariant").Value;
                                else
                                    throw new Exception(string.Format(@"(276) ...\MTRADataServer\PartsFactory\FileConfigurationPartsFactory.cs : CreateDevice() : ParsingVariant для устройства {0} не задан.", xe_dev.Attribute("TypeName").Value));

                                dh.DataControllerParent = dsrc;
                                dh.DataServer4ThisDevice = dsrc.DataServer4ThisDevice;

                                dsrc.ListDevice4DataController.Add(dh);
                                dsrc.DataServer4ThisDevice.ListDevice4DS.Add(dh);
                            }
                            catch (Exception ex)
                            {
                                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                                throw ex;
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
        #endregion

        #region инициализация тега
        private void CreateDeviceTag(Device dev)
        {
            try
            {
                // открыть файл устройства и инициализировать теги
                // по ссылке на контроллер найти и сохранить в список устройства
                string namesource = dev.DataControllerParent.DataSourceName4ThisController;
                string numcontroller = dev.DataControllerParent.СontrollerNumber;
                uint numdev = dev.DevGUID;

                string PathTo_DevCFG_File = ProjectCommonData.GetPathTo_DevCFG_File(numdev);

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
                    Tag newtag = CreateTag(xe_tag, dev);

                    if (newtag != null)
                    {
                        if (guids.Contains(newtag.TagGUID))
                            continue;

                        // учтем и добавим в список тегов устройства
                        guids.Add(newtag.TagGUID);
                        dev.dictTags4Parse.Add(newtag.TagGUID, newtag);

                        dev.Tags.Add(newtag);
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

        private Tag CreateTag(XElement xe_tag, Device dev)
        {
            Tag tag = null;
            try
            {
                // особенности создания тега в ParsingVariant
                switch (dev.ParsingVariant)
                {
                    case "MOA_ECU":
                        tag = MOA_ECU_Create(xe_tag, dev);
                        break;
                    case "BMRZDescrMOA":
                        tag = BMRZDescrMOACreate(xe_tag, dev);
                        break;
                    case "BlockingVirtualDevice":
                        tag = BlockingVirtualDevice_Create(xe_tag);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            string idtag = string.Format("{0}.{1}.{2}", dev.DataServer4ThisDevice.UniDS_GUID, dev.DevGUID, tag.TagGUID);// tag.TagGUID.ToString());
            if (!CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsTypeNativeLevel.ContainsKey(idtag))
                CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsTypeNativeLevel.Add(idtag, tag.TypeTag);
            else
            {
            }

            // формируем StrTagIdent
            tag.StrTagIdent = idtag;

            return tag;
        }

        /// <summary>
        /// создание тега в устройстве типа 
        /// вирт устр связи 1000
        /// </summary>
        /// <param name="xe_tag"></param>
        /// <returns></returns>
        private Tag BlockingVirtualDevice_Create(XElement xe_tag)
        {
            Tag tag = new Tag();
            try
            {
                tag.TagGUID = uint.Parse(xe_tag.Attribute("TagGUID").Value);
                tag.TagEnable = bool.Parse(xe_tag.Element("Configurator_level_Describe").Element("TagEnable").Value);
                tag.TagName = xe_tag.Element("Configurator_level_Describe").Element("TagName").Value;
                tag.TypeTag = GetTypeTag_BlockingVirtualDevice(xe_tag.Element("Configurator_level_Describe").Element("Type").Value);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                tag = null;
            }
            return tag;
        }
        /// <summary>
        /// вернуть тип c# по исходному типу
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string GetTypeTag_BlockingVirtualDevice(string p)
        {
            string tp = string.Empty;

            try
            {
                switch (p)
                {
                    case "Discret":
                        tp = "Boolean";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return tp;
        }


        /// <summary>
        /// создание тега в ФК МОА
        /// </summary>
        /// <param name="xe_tag"></param>
        /// <returns></returns>
        private Tag MOA_ECU_Create(XElement xe_tag, Device dev)
        {
            Tag tag = new Tag();
            try
            {
                tag.TagGUID = uint.Parse(xe_tag.Attribute("TagGUID").Value);
                //tag.TagEnable = bool.Parse(xe_tag.Attribute("TagEnable").Value);
                tag.TagName = xe_tag.Element("Configurator_level_Describe").Element("TagName").Value;
                tag.TypeTag = GetTypeTag_MOA_ECU(xe_tag.Element("Device_level_Describe").Element("regtype").Value);
                tag.AccessToValue = xe_tag.Element("Configurator_level_Describe").Element("AccessToValue").Value;
                tag.Comment = xe_tag.Element("Configurator_level_Describe").Element("Comment").Value;
                tag.DefValue = xe_tag.Element("Configurator_level_Describe").Element("DefValue").Value;
                // перечисление ?
                if (xe_tag.Element("Configurator_level_Describe").Elements("CBItemsList").Count() > 0)
                {
                    tag.EnumValue = new Dictionary<int, string>();
                    foreach (XElement xe_enum in xe_tag.Element("Configurator_level_Describe").Elements("CBItemsList").Elements("CBItem"))
                        tag.EnumValue.Add(int.Parse(xe_enum.Attribute("intvalue").Value), xe_enum.Value);
                }
                tag.MaxValue = xe_tag.Element("Configurator_level_Describe").Element("MaxValue").Value;
                tag.MinValue = xe_tag.Element("Configurator_level_Describe").Element("MinValue").Value;
                tag.UOM = xe_tag.Element("Configurator_level_Describe").Element("Unit").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                tag = null;
            }
            return tag;
        }
        /// <summary>
        /// вернуть тип c# по исходному типу
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string GetTypeTag_MOA_ECU(string p)
        {
            string tp = string.Empty;

            try
            {
                switch (p)
                {
                    case "UInt_FieldMT":
                        tp = "ushort";
                        break;
                    case "IPAdress_FieldMT":
                        tp = "IPAddress";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return tp;
        }

        /// <summary>
        /// создание тега в устройстве типа 
        /// BMRZDescrMOA
        /// </summary>
        /// <param name="xe_tag"></param>
        /// <returns></returns>
        private Tag BMRZDescrMOACreate(XElement xe_tag, Device dev)
        {
            Tag tag = new Tag();
            try
            {
                tag.TagGUID = uint.Parse(xe_tag.Attribute("TagGUID").Value);
                tag.TagEnable = bool.Parse(xe_tag.Attribute("TagEnable").Value);
                tag.TagName = xe_tag.Element("DataServer_level_Describe").Element("Name").Value;
                tag.TypeTag = GetTypeTag_BMRZDescrMOA(tag, dev, xe_tag);
                tag.AccessToValue = xe_tag.Element("DataServer_level_Describe").Element("Access").Value;
                tag.Comment = xe_tag.Element("Configurator_level_Describe").Element("Comment").Value;
                tag.DefValue = xe_tag.Element("DataServer_level_Describe").Element("Default").Value;
                // перечисление ?
                if (xe_tag.Element("Configurator_level_Describe").Elements("CBItemsList").Count() > 0)
                {
                    tag.EnumValue = new Dictionary<int, string>();
                    foreach (XElement xe_enum in xe_tag.Element("Configurator_level_Describe").Elements("CBItemsList").Elements("CBItem"))
                        tag.EnumValue.Add(int.Parse(xe_enum.Attribute("intvalue").Value), xe_enum.Value);
                }
                tag.MaxValue = xe_tag.Element("DataServer_level_Describe").Element("Max").Value;
                tag.MinValue = xe_tag.Element("DataServer_level_Describe").Element("Min").Value;
                tag.UOM = xe_tag.Element("DataServer_level_Describe").Element("UOM").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                tag = null;
            }
            return tag;
        }

        /// <summary>
        /// вернуть тип c# по исходному типу
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private string GetTypeTag_BMRZDescrMOA(Tag tag, Device dev, XElement xe_tag)
        {
            string tp = string.Empty;
            LinksLib.LinksHT2NT.LinkHT2NTBase LinkConvertHT2NT = null;

            try
            {
                string p = xe_tag.Element("DataServer_level_Describe").Element("Type").Value;
                switch (p)
                {
                    case "BitField":
                        tp = "Boolean";
                        // создаем канал
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_bytearray_2_Boolean(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи
                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_bytearray_2_Boolean).bitMask = xe_tag.Element("DataServer_level_Describe").Element("BitMask").Value;
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_bytearray_2_Boolean).BYTEORDER = "0";
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_bytearray_2_Boolean).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    case "Byte":
                        tp = "Byte";
                        break;
                    case "Int":
                        tp = "Int16";
                        // создаем канал
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_Int_2_Short(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_Int_2_Short).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    case "UInt":
                        tp = "UInt16";
                        // создаем канал
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_UInt_2_UInt16(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_UInt_2_UInt16).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    case "DInt":
                        tp = "Int32";
                        break;
                    case "UDInt":
                        tp = "UInt32";
                        break;
                    case "Real":
                        tp = "float";
                        // создаем канал
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_Real_2_Single(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_Real_2_Single).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    case "bit":
                        tp = "Boolean";
                        break;
                    case "Stringz":
                        tp = "string";
                        break;
                    case "BCDPack":
                        tp = "int";
                        // создаем канал
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_BCDPack_2_Int32(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_BCDPack_2_Int32).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    case "BCD":
                        tp = "int";
                        break;
                    case "Enum":
                        tp = "int";
                        break;
                    case "DateTime8":
                        tp = "DateTime";
                        break;
                    case "DateTime6":
                        tp = "DateTime";
                        break;
                    case "DateTime4_FieldMT":
                        tp = "DateTime";
                        // создаем канал типа
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_DateTime4_FieldMT_2_DateTime(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_DateTime4_FieldMT_2_DateTime).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    case "u32_data1970":
                        tp = "DateTime";
                        break;
                    case "u32_ipV4":
                        tp = "IPAddress";
                        break;
                    case "u32_data1970_reverse":
                        tp = "DateTime";
                        // создаем канал
                        LinkConvertHT2NT = new LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_u32_data1970_reverse_2_DateTime(dev.DataServer4ThisDevice.DATACONFIGURATION._dataConfigurationHardware, dev.DataServer4ThisDevice.DATACONFIGURATION);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertHT2NT.SetLink2TAGND(dev.DevGUID, tag.TagGUID);

                        // установим битовую маску и порядок байт для преобразования
                        (LinkConvertHT2NT as LinksLib.LinksHT2NT_MOA_ECU.LinkHT2NT_u32_data1970_reverse_2_DateTime).ADDRESS = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        LstLinksHT2NT.Add(LinkConvertHT2NT);
                        break;
                    default:
                        if (p.Contains("text"))
                            tp = "String";
                        else
                            break;
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return tp;
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
                group.GroupGUID = uint.Parse(xe_group.Attribute("GroupGUID").Value);
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
