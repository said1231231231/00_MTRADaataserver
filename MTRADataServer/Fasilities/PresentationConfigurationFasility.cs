/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: PresentationConfigurationFasility - базовый класс представления конфигурации уровня представления
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Fasilities\PresentationConfigurationFasility.cs
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
using PresentationConfigurationLib.PresentaionConfiguration;// MTRADataServer.PresentaionConfiguration;
using System.Diagnostics;

namespace MTRADataServer.Fasilities
{
    public class PresentationConfigurationFasility
    {
        /// <summary>
        /// конфигурация Native (ссылка)
        /// </summary>
        public NativeConfigurationLib.NativeConfiguration.DataConfiguration _dataConfiguration;

        IIntermediaDescription _intermDescr;

        public PresentationConfigurationFasility(NativeConfigurationLib.NativeConfiguration.DataConfiguration _dataConfiguration)
        {
            this._dataConfiguration = _dataConfiguration;
        }

        public _01Configuration GetConfiguration(string typesrc)
        {
            _01Configuration presentationConfiguration = null;

            /*
             * создаем промежуточное представление по типу источника хранения конфигурации
             * собираем конфигурацию представления на основе промежуточного представления
             */

            try
            {
                IntermediateDescriptionConfiguration_Classes.IntermediaDescriptionConfiguration idc = new IntermediateDescriptionConfiguration_Classes.IntermediaDescriptionConfiguration();
                _intermDescr = idc.Create_IntermediaDescriptionConfiguration("DSConfigInFiles");

                #region создание конфигурации
                presentationConfiguration = new _01Configuration();
                presentationConfiguration.NamePTK = _intermDescr.Get_NamePTK();
                presentationConfiguration.DSRouterServiceAddress = _intermDescr.Get_DSRouterServiceAddress();
                List<uint> lstDSsGUIDs = _intermDescr.Get_LstDataServersGUIDs();

                foreach( var uids in lstDSsGUIDs )
                {
                    #region создаем DS
                    _02DataServer presentds = new _02DataServer();
                    presentds.UniDS_GUID = uids;
                    presentds.PresentConfiguration = presentationConfiguration;
                    presentds.NameDataServer = _intermDescr.Get_NameDataServer(uids);

                    // сформировать список устройств для текущего DS
                    List<uint> lstDevice4DS = _intermDescr.Get_LstDevice4DS(uids);

                    foreach (uint numdev4ds in lstDevice4DS)
                    {
                        #region создаем устройство
                        _03Device device = new _03Device();
                        
                        // ссылка на DS
                        device.ThisDS = presentds;

                        device.ObjectGUID = numdev4ds;
                        
                        // игнорируем устр 1000 - его теги в сокращ конф нужно создавать отдельно
                        if (numdev4ds == 1000)
                        {
                            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 82, string.Format("Пропуск устр 1000 в конфигурации предсталения .", @"X:\Projects\00_MTRADataServer\MTRADataServer\Fasilities\PresentationConfigurationFasility.cs", "GetConfiguration()"));
                            continue;
                        }

                        device.Enable = _intermDescr.Get_EnableProperty(uids, numdev4ds);

                        if (!device.Enable)
                            continue;

                        device.DescriptInfo_DeviceBrandName = _intermDescr.Get_DescriptInfo_DeviceBrandNameProperty(uids, numdev4ds);
                        device.DescriptInfo_DeviceType = _intermDescr.Get_DescriptInfo_DeviceTypeProperty(uids, numdev4ds);
                        device.DescriptInfo_DeviceVersion = _intermDescr.Get_DescriptInfo_DeviceVersioneProperty(uids, numdev4ds);
                        device.DescriptInfo_DeviceFirmware = _intermDescr.Get_DescriptInfo_DeviceFirmwareProperty(uids, numdev4ds);
                        device.DescriptInfo_DeviceAttachmentDescribe = _intermDescr.Get_DescriptInfo_DeviceAttachmentDescribeProperty(uids, numdev4ds);
                        device.DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectState = _intermDescr.Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectStateProperty(uids, numdev4ds);
                        device.DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAState = _intermDescr.Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAStateProperty(uids, numdev4ds);
                        device.DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAMode = _intermDescr.Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAModeProperty(uids, numdev4ds);
                        device.DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOff = _intermDescr.Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOffProperty(uids, numdev4ds);
                        device.DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOn = _intermDescr.Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOnProperty(uids, numdev4ds);
                        device.DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceipt = _intermDescr.Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceiptProperty(uids, numdev4ds);

                        List<uint> lstGroup1Level = _intermDescr.GetGroup1Level(uids, numdev4ds);

                        foreach (uint numgr4dev in lstGroup1Level)
                        {
                            #region создаем иерархию групп
                            _04Group group = new _04Group();
                            group.GroupGUID = numgr4dev;
                            group.Enable = _intermDescr.Get_Group_EnableProperty(uids, numdev4ds, numgr4dev);
                            group.Name = _intermDescr.Get_Group_NameProperty(uids, numdev4ds, numgr4dev);
                            group.Category = _intermDescr.Get_Group_СategoryProperty(uids, numdev4ds, numgr4dev);

                            // ссылка на устройство
                            group.Thisdevice = device;
                            // проверяем наличие подгрупп
                            if (_intermDescr.IsSubgroupInGroup(uids, numdev4ds, numgr4dev))
                            {
                                List<uint> lstGroupNextLevel = _intermDescr.GetGroupNextLevel(uids, numdev4ds, numgr4dev);

                                foreach (uint numsgr4gr in lstGroupNextLevel)
                                    group.LstSubGroups.Add(CreateSubGroup(uids, numdev4ds, numgr4dev, numsgr4gr, device));
                            }
                            // ...  и тегов
                            if (_intermDescr.IsTagsInGroup(uids, numdev4ds, numgr4dev))
                            {
                                group.LstTags = CreateTags4Group(uids, numdev4ds, numgr4dev, group, device);
                            }
                            #endregion

                            device.LstGroups.Add(group);
                        }
                        #endregion

                        presentds.LstDevice.Add(device);
                    }                    
                    #endregion

                    presentationConfiguration.LstDataServers.Add(presentds);
                } 
	            #endregion
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return presentationConfiguration;
        }

        private _04Group CreateSubGroup(uint uids, uint numdev4ds, uint numgr4dev, uint numsgr4gr , _03Device device)
        {
            _04Group group = new _04Group();
            try
            {
                            group.GroupGUID = numsgr4gr;
                            group.Enable = _intermDescr.Get_Group_EnableProperty(uids, numdev4ds, numsgr4gr);
                            group.Name = _intermDescr.Get_Group_NameProperty(uids, numdev4ds, numsgr4gr);
                            group.Category = _intermDescr.Get_Group_СategoryProperty(uids, numdev4ds, numsgr4gr);
                            group.Thisdevice = device;

                            // проверяем наличие подгрупп
                            if (_intermDescr.IsSubgroupInGroup(uids, numdev4ds, numsgr4gr))
                            {
                                List<uint> lstGroupNextLevel = _intermDescr.GetGroupNextLevel(uids, numdev4ds, numsgr4gr);

                                foreach (uint numsgr4sgr in lstGroupNextLevel)
                                    group.LstSubGroups.Add(CreateSubGroup(uids, numdev4ds, numsgr4gr, numsgr4sgr, device));
                            }
                            // ...  и тегов
                            if (_intermDescr.IsTagsInGroup(uids, numdev4ds, numsgr4gr))
                            {
                                group.LstTags = CreateTags4Group(uids, numdev4ds, numsgr4gr, group, device);
                            }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return group;
        }
        private List<_05Tag> CreateTags4Group(uint uids, uint numdev4ds, uint numgr4dev, _04Group group, _03Device device)
        {
            List<_05Tag> lsttags = new List<_05Tag>();
            try
            {
                List<uint> lstTags4Group = _intermDescr.GetTagsGuid4Group(uids, numdev4ds, numgr4dev);
                foreach (uint guidtag in lstTags4Group)
                {
                    _05Tag tag = new _05Tag();
                    //ссылка на группп для даннго тега
                    tag.ThisGroup = group;

                    tag.Enable = _intermDescr.Get_Tag_EnableProperty( uids,  numdev4ds,  numgr4dev, guidtag);
                    tag.TagGUID = guidtag;
                    tag.Name = _intermDescr.Get_Tag_NameProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.UOM = _intermDescr.Get_Tag_UOMProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.PosPoint = _intermDescr.Get_Tag_PosPointProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.TagType = _intermDescr.Get_Tag_TagTypeProperty(uids, numdev4ds, numgr4dev, guidtag);
                    
                    tag.Access = _intermDescr.Get_Tag_AccessProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.MinValue = _intermDescr.Get_Tag_MinValueProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.MaxValue = _intermDescr.Get_Tag_MaxValueProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.RawType = _intermDescr.Get_Tag_RawTypeProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.RealAnlogType = _intermDescr.Get_Tag_RealAnlogTypeProperty(uids, numdev4ds, numgr4dev, guidtag);
                    tag.TransformationRatio = _intermDescr.Get_Tag_TransformationRatio(uids, numdev4ds, numgr4dev, guidtag);
                    tag.IsInverseBoolValue = _intermDescr.Get_Tag_InverseProperty(uids, numdev4ds, numgr4dev, guidtag);

                    // расчетный тег с формулой ?
                    tag.Formula4Calculated = _intermDescr.Get_Tag_Formula(uids, numdev4ds, numgr4dev, guidtag);
                    if (!string.IsNullOrWhiteSpace(tag.Formula4Calculated))
                    {
                        List<string> frmltags = CommonClassesLib.CommonClasses.ProjectCommonData.ParseFormula4ExtractTagDescribe(tag.Formula4Calculated, uids, numdev4ds);
                        CreateFormulaChanel(tag, device,tag.Formula4Calculated, frmltags);
                    }
                    else
                    {
                        // создаем канал для одиночного тега
                        CreateChanel(tag, device);
                    }

                    // тег - перечисление?
                    if (_intermDescr.IsTagsInGroupIsEnum(uids, numdev4ds, numgr4dev, guidtag))
                    {
                        // добавить значения перечисления в DictEnumValues
                        tag.DictEnumValues = _intermDescr.Get_Tag_EnumDictionary(uids, numdev4ds, numgr4dev, guidtag);
                    }

                    // тип в глобальную таблицу уровня PL
                    string idtag = string.Format("{0}.{1}.{2}", device.ThisDS.UniDS_GUID, device.ObjectGUID, tag.TagGUID);
                    if (!CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsType_PL.ContainsKey(idtag))
                        CommonClassesLib.CommonClasses.ProjectCommonData.slGlobalListTagsType_PL.Add(idtag, tag.TagType);
                    else
                    { 
                    }

                    lsttags.Add(tag);
                    if (!group.Thisdevice.dictTags4Parse.ContainsKey(tag.TagGUID))
                        group.Thisdevice.dictTags4Parse.Add(tag.TagGUID, tag);
                    else
                    { 
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return lsttags;
        }

        private void CreateFormulaChanel(_05Tag tag, _03Device dev, string formula, List<string> frmltags)
        {
            //LinksLib.LinksNT2PT.LinkNT2PTBase LinkConvertNT2PT = null;
            LinksLib.LinksNT2PT.Link_NatimeFormula LinkConvertNT2PT = null;

            try
            {
                switch (tag.TagType)
                {
                    case "Discret":
                        // создаем канал
                        LinkConvertNT2PT = new LinksLib.LinksNT2PT.Link_NativeFormula_2_Discret(_dataConfiguration, dev.ThisDS.PresentConfiguration);
                        break;
                    case "Analog":
                        // создаем канал                        
                        LinkConvertNT2PT = new LinksLib.LinksNT2PT.Link_NativeFormula_2_Analog(_dataConfiguration, dev.ThisDS.PresentConfiguration);
                        break;
                    default:
                        throw new Exception(string.Format(@"(277) ...\Fasilities\PresentationConfigurationFasility.cs: CreateFormulaChanel() : обработка типа тега {0} не реализована.", tag.TagType));

                }
                // общие действия при создании канала
                LinkConvertNT2PT.SetLink2TAGPL(dev.ObjectGUID, tag.TagGUID);
                // добавить список тегов для расчета - начтройка формулы по вызову CreateLink
                LinkConvertNT2PT.ListFrmlTags = frmltags;
                LinkConvertNT2PT.Formula = formula;

                // добавить канал в список - настройка после формирования конфигурации полностью
                App.LstLinksNT2PT.Add(LinkConvertNT2PT);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

        }

        private void CreateChanel(_05Tag tag, _03Device dev)
        {
            LinksLib.LinksNT2PT.LinkNT2PTBase LinkConvertNT2PT = null; 
            
            try
            {
                switch (tag.TagType)
                {
                    case "Discret":
                        // создаем канал
                        LinkConvertNT2PT = new LinksLib.LinksNT2PT.Link_NativeType_2_Discret (_dataConfiguration, dev.ThisDS.PresentConfiguration);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи
                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertNT2PT.SetLink2TAGND(dev.ObjectGUID, tag.TagGUID);
                        //LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertNT2PT.SetLink2TAGPL(dev.ObjectGUID, tag.TagGUID);
                        
                        // добавить канал в список - настройка после формирования конфигурации полностью
                        App.LstLinksNT2PT.Add(LinkConvertNT2PT);
                        break;
                    case "Analog":
                        // создаем канал
                        LinkConvertNT2PT = new LinksLib.LinksNT2PT.Link_NativeType_2_Analog(_dataConfiguration, dev.ThisDS.PresentConfiguration);

                        /*
                         * свяжем устройства и теги уровня hard с native на уровне объекта-канала связи
                         * 
                         * для установки тега hardware передаем адрес регистра, который 
                         * для данного источника будет явл. уник идент тега
                         */
                        LinkConvertNT2PT.SetLink2TAGND(dev.ObjectGUID, tag.TagGUID);
                        //LinkConvertHT2NT.SetLink2TAGHD(dev.DevGUID, uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value));
                        LinkConvertNT2PT.SetLink2TAGPL(dev.ObjectGUID, tag.TagGUID);

                        // добавить канал в список - настройка после формирования конфигурации полностью
                        App.LstLinksNT2PT.Add(LinkConvertNT2PT);
                        break;
                    default:
                         break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
