/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DeviceHardware - класс представления устройства в конфигурации DS (уровень hardware)
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DeviceHardware.cs
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommonClassesLib;

namespace HardwareConfigurationLib.HardwareConfiguration
{
    public class DeviceHardware
    {
        /// <summary>
        /// доступность устройства для работы
        /// </summary>
        public bool Enable {get;set;}
        /// <summary>
        /// уник номер устройства
        /// </summary>
        public uint DevGUID {get;set;}
        /// <summary>
        /// имя типа устройства
        /// </summary>
        public string DeviceType {get;set;}
        /// <summary>
        /// список тегов устройства
        /// </summary>
        public List<TagHardware> LstTags { get; set; }
        /// <summary>
        /// список тегов для удобства работы механизма
        /// разбора (guid, класс тега)
        /// </summary>
        public Dictionary<uint, TagHardware> dictTags4Parse = new Dictionary<uint, TagHardware>();
        /// <summary>
        /// ссылка на контроллер кот. принадлежит устройство
        /// </summary>
        public DataControllerHardware DataControllerHardwareParent { get; set; }
        /// <summary>
        /// алгоритм работы с устройством
        /// </summary>
        public string ParsingVariant { get; set; }

        public DeviceHardware()
        {
            LstTags = new List<TagHardware>();
        }

        public virtual TagHardware CreateTagHardware(XElement xe_tag)
        {
            TagHardware th = new TagHardware();
            try
            {
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return th;
        }
        
        public void ResetAllTagsToUndefinedStatus()
        {
            try
            {
                foreach (TagHardware th in this.LstTags)
                    //th.TagQuality = CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality.vqUndefined;
                    th.SetTagValue(null, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality.vqUndefined);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// получить тег по идентификатору
        /// </summary>
        /// <param name="tagguid"></param>
        /// <returns></returns>
        public TagHardware GetTagByTagGUID(uint tagguid)
        {
            TagHardware th = null;
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
