/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _03Device - класс представления устройства на уровне Presentation
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentaionConfiguration\_03Device.cs
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

namespace PresentationConfigurationLib.PresentaionConfiguration
{
    public class _03Device
    {
        public uint ObjectGUID { get; set; }
        public bool Enable { get; set; }
        public string DescriptInfo_DeviceBrandName { get; set; }
        public string DescriptInfo_DeviceType { get; set; }
        public string DescriptInfo_DeviceVersion { get; set; }
        public string DescriptInfo_DeviceFirmware { get; set; }
        public string DescriptInfo_DeviceAttachmentDescribe { get; set; }
        public string DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_ConnectState { get; set; }
        public string DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAState { get; set; }
        public string DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_KAMode { get; set; }
        public string DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOff { get; set; }
        public string DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandOn { get; set; }
        public string DescriptInfo_SpecificDeviceValues_SpecificTags_Tag_CommandReceipt { get; set; }

        /// <summary>
        /// список тегов для удобства доступа
        /// </summary>
        public Dictionary<uint, _05Tag> dictTags4Parse = new Dictionary<uint, _05Tag>();

        public List<_04Group> LstGroups { get; set; }

        /// <summary>
        /// ссылка на DS для этого устройства
        /// </summary>
        public _02DataServer ThisDS = null;

        public _03Device()
        {
            LstGroups = new List<_04Group>();
        }

        /// <summary>
        /// получить тег по идентификатору
        /// </summary>
        /// <param name="tagguid"></param>
        /// <returns></returns>
        public _05Tag GetTagByTagGUID(uint tagguid)
        {
            _05Tag th = null;
            try
            {
                if (this.dictTags4Parse.ContainsKey(tagguid))
                    th = this.dictTags4Parse[tagguid];
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return th;
        }
    }
}


