/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: PresentationConfiguration_DSConfigFile - конфигурация уровня представления на основе файла DSConfig.cfg
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\IntermediateDescriptionConfiguration_Classes\PresentationConfiguration_DSConfigFile.cs
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
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Threading;

namespace MTRADataServer.IntermediateDescriptionConfiguration_Classes
{
    public class PresentationConfiguration_DSConfigFile : InterfaceLibrary.IIntermediaDescription
    {
        /// <summary>
        /// xml-представление проекта из файла 
        /// DSConfig.cfg
        /// </summary>
        XDocument xdoc_dev = null;

        #region инициализация
        /// <summary>
        /// инициализировать описание
        /// конфигурации представления
        /// </summary>
        public void InitPresentationConfig()
        {
            try
            {
                string path2DSConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Project", "DSConfig.cfg");

                if (!File.Exists(path2DSConfigFile))
                    throw new Exception(string.Format(@"(43) ...\MTRADataServer\IntermediateDescriptionConfiguration_Classes\PresentationConfiguration_DSConfigFile.cs: InitPresentationConfig() : файл {} не существует", path2DSConfigFile));

                xdoc_dev = XDocument.Load(path2DSConfigFile);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }        
        #endregion

        #region проект
        /// <summary>
        /// получить имя проекта
        /// </summary>
        /// <returns></returns>
        public string Get_NamePTK()
        {
            string nameptk = string.Empty;

            try
            {
                nameptk = xdoc_dev.Element("MTRA").Element("ProjectInfo").Element("NamePTK").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return nameptk;
        }
        /// <summary>
        /// DSRouterServiceAddress
        /// </summary>
        /// <returns></returns>
        public string Get_DSRouterServiceAddress()
        {
            string DSRouterServiceAddress = string.Empty;

            try
            {
                DSRouterServiceAddress = xdoc_dev.Element("MTRA").Element("ProjectInfo").Element("DSRouterServiceAddress").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return DSRouterServiceAddress;
        }        
        #endregion

        #region DataServer
        /// <summary>
        /// список уник номеров DS
        /// </summary>
        /// <returns></returns>
        public List<uint> Get_LstDataServersGUIDs()
        {
            List<uint> lstDataServersGUIDs = new List<uint>();

            try
            {
                var xe_dss = xdoc_dev.Element("MTRA").Element("Configuration").Elements("Object");

                foreach (var xe_ds in xe_dss)
                    lstDataServersGUIDs.Add(uint.Parse(xe_ds.Attribute("UniDS_GUID").Value));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return lstDataServersGUIDs;
        }
        /// <summary>
        /// имя DS по его номеру
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        public string Get_NameDataServer(uint uids)
        {
            string NameDataServer = string.Empty;

            try
            {
                var xe_dss = xdoc_dev.Element("MTRA").Element("Configuration").Elements("Object");

                foreach (var xe_ds in xe_dss)
                    if (xe_ds.Attribute("UniDS_GUID").Value == uids.ToString())
                    {
                        NameDataServer = xe_ds.Attribute("name").Value;
                        break;
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return NameDataServer;
        }
        /// <summary>
        /// сформировать список устройств для конкретного DS
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public List<uint> Get_LstDevice4DS(uint uids)
        {
            List<uint> lstDeviceGUIDs = new List<uint>();

            try
            {
                var xe_dss = xdoc_dev.Element("MTRA").Element("Configuration").Elements("Object");

                foreach (var xe_ds in xe_dss)
                    if (xe_ds.Attribute("UniDS_GUID").Value == uids.ToString())
                    {
                        var xe_devs = xe_ds.Elements("Device");
                        foreach (var xe_dev in xe_devs)
                            lstDeviceGUIDs.Add(uint.Parse(xe_dev.Attribute("objectGUID").Value));
                        break;
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return lstDeviceGUIDs;
        }        
        #endregion

        #region Устройство
        /// <summary>
        /// получить значение свойства Enable
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public bool Get_EnableProperty(uint uids, uint numdev4ds)
        {
            bool enable = false;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                enable = bool.Parse(xe_dev.Attribute("enable").Value);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return enable;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceBrandName
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_DeviceBrandNameProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("DeviceBrandName").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceType
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_DeviceTypeProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("DeviceType").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceVersion
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_DeviceVersioneProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("DeviceVersion").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceFirmware
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_DeviceFirmwareProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("DeviceFirmware").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceAttachmentDescribe
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_DeviceAttachmentDescribeProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("DeviceAttachmentDescribe").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectState
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectStateProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("SpecificDeviceValues").Element("SpecificTags").Element("Tag_ConnectState").Attribute("tagguid").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAState
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAStateProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("SpecificDeviceValues").Element("SpecificTags").Element("Tag_KAState").Attribute("tagguid").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAMode
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAModeProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("SpecificDeviceValues").Element("SpecificTags").Element("Tag_KAMode").Attribute("tagguid").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOff
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOffProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("SpecificDeviceValues").Element("SpecificTags").Element("Tag_CommandOff").Attribute("tagguid").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOn
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOnProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("SpecificDeviceValues").Element("SpecificTags").Element("Tag_CommandOn").Attribute("tagguid").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceipt
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceiptProperty(uint uids, uint numdev4ds)
        {
            string rez = string.Empty;

            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);
                rez = xe_dev.Element("DescriptInfo").Element("SpecificDeviceValues").Element("SpecificTags").Element("Tag_CommandReceipt").Attribute("tagguid").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }        
        #endregion

        #region группы
        /// <summary>
        /// получить список групп верхнего уровня
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        public List<uint> GetGroup1Level(uint uids, uint numdev4ds)
        {
            List<uint> lstgroupsnum = new List<uint>();
            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);

                // словарь групп (XElement) по их guid'у
                Dictionary<uint, XElement> dictGroupInPlainOrder = new Dictionary<uint, XElement>();

                var xe_groups = xe_dev.Element("Groups").Descendants("Group");
                foreach (var xe_group in xe_groups)
                    dictGroupInPlainOrder.Add(uint.Parse(xe_group.Attribute("GroupGUID").Value), xe_group);

                var xe_groups1level = xe_dev.Element("Groups").Elements("Group");
                foreach (var xe_group1level in xe_groups1level)
                    lstgroupsnum.Add(uint.Parse(xe_group1level.Attribute("GroupGUID").Value));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return lstgroupsnum;
        }
        /// <summary>
        /// получить свойство Enable для группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public bool Get_Group_EnableProperty(uint uids, uint numdev4ds, uint numgr4dev)
        {
            bool rez = false;
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);
                rez = bool.Parse(xe_group.Attribute("enbl").Value);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить свойство Name для группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public string Get_Group_NameProperty(uint uids, uint numdev4ds, uint numgr4dev)
        {
            string rez = string.Empty;
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);
                rez = xe_group.Attribute("nm").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить свойство category для группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public uint Get_Group_СategoryProperty(uint uids, uint numdev4ds, uint numgr4dev)
        {
            uint rez = 0;
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);

                if (xe_group.Attributes("category").Count() > 0)
                    rez = uint.Parse(xe_group.Attribute("category").Value);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// тест - есть ли подгруппы в группе
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public bool IsSubgroupInGroup(uint uids, uint numdev4ds, uint numgr4dev)
        {
            bool rez = false;
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);
                if (xe_group.Elements("Group").Count() > 0)
                    rez = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить список подгрупп
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public List<uint> GetGroupNextLevel(uint uids, uint numdev4ds, uint numgr4dev)
        {
            List<uint> lstgroupsnum = new List<uint>();
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);

                var xe_groups1level = xe_group.Elements("Group");
                foreach (var xe_group1level in xe_groups1level)
                    lstgroupsnum.Add(uint.Parse(xe_group1level.Attribute("GroupGUID").Value));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return lstgroupsnum;
        }
        #endregion
        #region теги
        /// <summary>
        /// тест - есть ли теги в группе
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public bool IsTagsInGroup(uint uids, uint numdev4ds, uint numgr4dev)
        {
            bool rez = false;
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);
                if (xe_group.Elements("Tags").Count() > 0)
                    rez = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// список guid для тегов группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        public List<uint> GetTagsGuid4Group(uint uids, uint numdev4ds, uint numgr4dev)
        {
            List<uint> rez = new List<uint>();
            try
            {
                var xe_group = Get_Xe4Group(uids, numdev4ds, numgr4dev);
                if (xe_group == null)
                { 
                }

                var xe_tags = xe_group.Element("Tags").Elements("Tag");
                foreach (var xe_tag in xe_tags)
                    // для тегов группы команды атрибута tg нет
                    if (xe_tag.Attributes("tg").Count() > 0)
                        rez.Add(uint.Parse(xe_tag.Attribute("tg").Value));
                    else
                    { 
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public bool Get_Tag_EnableProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            bool rez = false;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);

                if (!bool.TryParse(xe_tag.Attribute("enbl").Value, out rez))
                    throw new Exception(string.Format(@"(656) ...\IntermediateDescriptionConfiguration_Classes\PresentationConfiguration_DSConfigFile.cs: Get_Tag_EnableProperty() : ошибка преобразования типа Boolean"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_NameProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("nm").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_UOMProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("uom").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public uint Get_Tag_PosPointProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            uint rez = 0;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = uint.Parse(xe_tag.Attribute("pt").Value);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_TagTypeProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("tp").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_AccessProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("acc").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public object Get_Tag_MinValueProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            object rez = false;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("min").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public object Get_Tag_MaxValueProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            object rez = false;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("max").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_RawTypeProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("rawtp").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_RealAnlogTypeProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                rez = xe_tag.Attribute("atp").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить коэф пересчета
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public Single Get_Tag_TransformationRatio(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            Single rez = 1;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);

                if (xe_tag.Attributes("TrRatio").Count() > 0)
                {
                    string trr = xe_tag.Attribute("TrRatio").Value;
                    // установка десятичного разделителя:
                    CultureInfo cinfo = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                    // проверим десятичный разделитель
                    if (trr.Contains("."))
                        cinfo.NumberFormat.NumberDecimalSeparator = ".";
                    else if (trr.Contains(","))
                        cinfo.NumberFormat.NumberDecimalSeparator = ",";

                    Thread.CurrentThread.CurrentCulture = cinfo;

                    rez = Single.Parse(xe_tag.Attribute("TrRatio").Value);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить признак инверсии
        /// дискрета
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public bool Get_Tag_InverseProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            bool rez = false;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);

                if (xe_tag.Attributes("IsInverseBoolValue").Count() > 0)
                {
                    bool trr = bool.Parse(xe_tag.Attribute("IsInverseBoolValue").Value);

                    rez = bool.Parse(xe_tag.Attribute("IsInverseBoolValue").Value);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// формула для расчетного тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public string Get_Tag_Formula(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            string rez = string.Empty;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);

                if (xe_tag.Attributes("formula4calculatedRaw").Count() > 0)
                    rez = xe_tag.Attribute("formula4calculatedRaw").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }            

        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public bool IsTagsInGroupIsEnum(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            bool rez = false;
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                if (xe_tag.Elements("CBItemsList").Count() > 0)
                    rez = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        public Dictionary<int, string> Get_Tag_EnumDictionary(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            Dictionary<int, string> rez = new Dictionary<int,string>();
            try
            {
                var xe_tag = Get_Xe4Tag(uids, numdev4ds, numgr4dev, guidtag);
                var xe_cbitems = xe_tag.Element("CBItemsList").Elements("CBItem");

                foreach (var xe_cbitem in xe_cbitems)
                    rez.Add(int.Parse(xe_cbitem.Attribute("intvalue").Value), xe_cbitem.Value);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }        
        #endregion
        #region вспомогательные функции
        /// <summary>
        /// получить секцию XElement для устройства
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        private XElement Get_Xe4Device(uint uids, uint numdev4ds)
        {
            XElement xed = null;

            try
            {
                var xe_dss = xdoc_dev.Element("MTRA").Element("Configuration").Elements("Object");

                foreach (var xe_ds in xe_dss)
                    if (xe_ds.Attribute("UniDS_GUID").Value == uids.ToString())
                    {
                        var xe_devs = xe_ds.Elements("Device");
                        foreach (var xe_dev in xe_devs)
                            if (uint.Parse(xe_dev.Attribute("objectGUID").Value) == numdev4ds)
                            {
                                xed = xe_dev;
                                break;
                            }
                        break;
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return xed;
        }
        /// <summary>
        /// получить XElement для группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        private XElement Get_Xe4Group(uint uids, uint numdev4ds, uint numgr4dev)
        {
            XElement xe_gr = null;
            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);

                // словарь групп (XElement) по их guid'у
                Dictionary<uint, XElement> dictGroupInPlainOrder = new Dictionary<uint, XElement>();

                var xe_groups = xe_dev.Element("Groups").Descendants("Group");
                foreach (var xe_group in xe_groups)
                    dictGroupInPlainOrder.Add(uint.Parse(xe_group.Attribute("GroupGUID").Value), xe_group);

                if (dictGroupInPlainOrder.ContainsKey(numgr4dev))
                    xe_gr = dictGroupInPlainOrder[numgr4dev];
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return xe_gr;
        }
        /// <summary>
        /// получить XElement для тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        private XElement Get_Xe4Tag(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag)
        {
            XElement xe_tag = null;
            XElement xe_gr = null;
            try
            {
                var xe_dev = Get_Xe4Device(uids, numdev4ds);

                // словарь групп (XElement) по их guid'у
                Dictionary<uint, XElement> dictGroupInPlainOrder = new Dictionary<uint, XElement>();

                var xe_groups = xe_dev.Element("Groups").Descendants("Group");
                foreach (var xe_group in xe_groups)
                    dictGroupInPlainOrder.Add(uint.Parse(xe_group.Attribute("GroupGUID").Value), xe_group);

                if (dictGroupInPlainOrder.ContainsKey(numgr4dev))
                    xe_gr = dictGroupInPlainOrder[numgr4dev];
                else
                    throw new Exception(string.Format(@"(976) ...\IntermediateDescriptionConfiguration_Classes\PresentationConfiguration_DSConfigFile.cs: Get_Xe4Tag() : группа {0} не найдена.", numgr4dev));

                var xe_tags = xe_gr.Element("Tags").Elements("Tag");

                foreach (var xe_tg in xe_tags)
                    if (uint.Parse(xe_tg.Attribute("tg").Value) == guidtag)
                    {
                        xe_tag = xe_tg;
                        break;
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return xe_tag;
        }
        #endregion
    }
}
