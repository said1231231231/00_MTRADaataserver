/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: IIntermediaDescription - интерфейс доступа к конфигурации уровня представления
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\InterfaceLibrary\IIntermediaDescription.cs
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

namespace InterfaceLibrary
{
    public interface IIntermediaDescription
    {
        /// <summary>
        /// инициализировать описание
        /// конфигурации представления
        /// </summary>
        void InitPresentationConfig();
        /// <summary>
        /// получить имя проекта
        /// </summary>
        /// <returns></returns>
        string Get_NamePTK();
        /// <summary>
        /// DSRouterServiceAddress
        /// </summary>
        /// <returns></returns>
        string Get_DSRouterServiceAddress();
        /// <summary>
        /// список идентификаторов DataServers
        /// </summary>
        /// <returns></returns>
        List<uint> Get_LstDataServersGUIDs();
        /// <summary>
        /// имя DS
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        string Get_NameDataServer(uint uids);
        /// <summary>
        /// сформировать список устройств для конкретного DS
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        List<uint> Get_LstDevice4DS(uint uids);
        /// <summary>
        /// получить значение свойства Enable
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        bool Get_EnableProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceBrandName
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_DeviceBrandNameProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceType
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_DeviceTypeProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceVersione
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_DeviceVersioneProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceFirmware
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_DeviceFirmwareProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_DeviceAttachmentDescribe
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_DeviceAttachmentDescribeProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectState
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectStateProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAState
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAStateProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAMode
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAModeProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOff
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOffProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOn
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOnProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить значение свойства DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceipt
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        string Get_DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceiptProperty(uint uids, uint numdev4ds);
        /// <summary>
        /// получить список групп верхнего уровня
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <returns></returns>
        List<uint> GetGroup1Level(uint uids, uint numdev4ds);
        /// <summary>
        /// получить свойство Enable для группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        bool Get_Group_EnableProperty(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// имя группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        string Get_Group_NameProperty(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// Category группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        uint Get_Group_СategoryProperty(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// тест - есть ли подгруппы в группе
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        bool IsSubgroupInGroup(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// получить список подгрупп
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        List<uint> GetGroupNextLevel(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// тест - есть ли теги в группе
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        bool IsTagsInGroup(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// список guid для тегов группы
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <returns></returns>
        List<uint> GetTagsGuid4Group(uint uids, uint numdev4ds, uint numgr4dev);
        /// <summary>
        /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        bool Get_Tag_EnableProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_NameProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_UOMProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        uint Get_Tag_PosPointProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_TagTypeProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_AccessProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        object Get_Tag_MinValueProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        object Get_Tag_MaxValueProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_RawTypeProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_RealAnlogTypeProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// коэф пересчета для аналоговых величин
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        Single Get_Tag_TransformationRatio(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// признак инверсности
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        bool Get_Tag_InverseProperty(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// формула для расчетного тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        string Get_Tag_Formula(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        bool IsTagsInGroupIsEnum(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
        /// <summary>
        /// /// получить свойство тега
        /// </summary>
        /// <param name="uids"></param>
        /// <param name="numdev4ds"></param>
        /// <param name="numgr4dev"></param>
        /// <param name="guidtag"></param>
        /// <returns></returns>
        Dictionary<int, string> Get_Tag_EnumDictionary(uint uids, uint numdev4ds, uint numgr4dev, uint guidtag);
    }
}
