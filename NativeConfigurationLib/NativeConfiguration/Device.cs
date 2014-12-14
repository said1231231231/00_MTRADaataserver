/*#############################################################################
 *    Copyright (C) 2006-2013 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Device - класс представления устройства
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\Device.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С#, Framework 4.0                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : 31.07.2013
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HardwareConfigurationLib;

namespace NativeConfigurationLib.NativeConfiguration
{
    public class Device
    {
        #region общая информация об устройстве
        /// <summary>
        /// время последнего изменения описания устройства
        /// </summary>
        public DateTime LastChangeTime { get; set; }
        /// <summary>
        /// производитель
        /// </summary>
        public string DeviceBrandName { get; set; }
        ///// <summary>
        ///// тип устройства
        ///// </summary>
        //public string DeviceType { get; set; }
        /// <summary>
        /// номер версии устройства
        /// </summary>
        public string DeviceVersion { get; set; }
        /// <summary>
        /// прошивка устройства
        /// </summary>
        public string DeviceFirmware { get; set; }
        /// <summary>
        /// имя устройства (для использования в коде)
        /// </summary>
        public string DeviceHMIName { get; set; }
        /// <summary>
        /// диспетчерское название устройства
        /// </summary>
        public string DeviceHMINameR { get; set; }
        /// <summary>
        /// алгоритм работы с устройством
        /// </summary>
        public string ParsingVariant { get; set; }
        /// <summary>
        /// серийный номер устройства
        /// </summary>
        public string SerNumber { get; set; }
        #endregion общая информация об устройстве

        #region специяическая информация об устройстве
        /// <summary>
        /// номер тега состояния свзи с устр
        /// </summary>
        public string Tag_ConnectState {get; set;}
        /// <summary>
        /// id номер тега состояния (разчет)
        /// </summary>
        public string Tag_КАState {get; set;}
        /// <summary>
        /// id номер иега режима (расчет)
        /// </summary>
        public string Tag_KAMode {get; set;}
        /// <summary>
        /// id команда отключения
        /// </summary>
        public string Tag_CommandOff {get; set;}
        /// <summary>
        /// id команда включения
        /// </summary>
        public string Tag_CommandOn {get; set;}
        /// <summary>
        /// id команда кситирования
        /// </summary>
        public string Tag_CommandReceipt {get; set;}
        /// <summary>
        /// класс описания уставок
        /// </summary>
        //public UstavkiDescripton USTAVKiDeSCRIPTION { get; set; }
        #endregion

        #region содержимое устройства - теги, группы, команды
        /// <summary>
        /// список тегов устройства
        /// </summary>
        public List<Tag> Tags{get;set;}
        /// <summary>
        /// список тегов для удобства работы механизма
        /// разбора (guid, класс тега)
        /// </summary>
        public Dictionary<uint, Tag> dictTags4Parse = new Dictionary<uint, Tag>();
        /// <summary>
        /// группы устройства
        /// </summary>
        public List<Group> Groups { get; set; }
        /// <summary>
        /// команды устройства
        /// </summary>
        public List<Command> Commands { get; set; }        
        #endregion

        /// <summary>
        /// доступность устройства для работы
        /// </summary>
        public bool Enable{get;set;}
        /// <summary>
        /// уник номер устройства
        /// </summary>
        public uint DevGUID {get;set;}
        /// <summary>
        /// имя типа устройства
        /// </summary>
        public string DeviceType{get;set;}
        /// <summary>
        /// список подгрупп
        /// </summary>
        public List<Group> GroupList4Request{get;set;}
        /// <summary>
        /// DataServer для этого устройства
        /// </summary>
        public DataServer DataServer4ThisDevice { get; set; }
        /// <summary>
        /// контроллер для этого устройства
        /// </summary>
        public DataController DataControllerParent { get; set; }
        /// <summary>
        /// устройство Hardawre-уровня
        /// </summary>
        //public HardwareConfiguration.DeviceHardware DEVICEHARDWARE { get; set; }

        public Device()
        {
            Tags = new List<Tag>();
            dictTags4Parse = new Dictionary<uint, Tag>();
        }

        /// <summary>
        /// получить тег по идентификатору
        /// </summary>
        /// <param name="tagguid"></param>
        /// <returns></returns>
        public Tag GetTagByTagGUID(uint tagguid)
        {
            Tag th = null;
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
