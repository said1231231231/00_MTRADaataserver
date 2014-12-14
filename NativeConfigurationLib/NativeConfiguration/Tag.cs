/*#############################################################################
 *    Copyright (C) 2006-2013 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Tag - класс представления тега устройства
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\Tag.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С#, Framework 4.0                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : 05.10.2013
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * предлагается описать тег след образом:
 * базовый класс Tag - поля из секции Configurator_level_Describe
 * класс-наследник Dev_Tag - поля из секции Device_level_Describe + 
 *                          класс Dev_Tag знает секцию Device_level_Describe
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.ComponentModel;
using CommonClassesLib.CommonClasses;

namespace NativeConfigurationLib.NativeConfiguration
{
    public delegate void ChangeTagNT(string stridtag, object value, ProjectCommonData.VarQuality varquality, DateTime dt);

    public class Tag
    {
        public event ChangeTagNT OnChangeTagNT;

        #region свойства из секции Tag | Configurator_level_Describe
        /// <summary>
        /// доступность тега
        /// </summary>
        public bool TagEnable {get;set;}
        /// <summary>
        /// уник номер тега
        /// </summary>
        public uint TagGUID {get;set;}
        /// <summary>
        /// имя тега
        /// </summary>
        public string TagName {get;set;}
        /// <summary>
        /// тип тега
        /// </summary>
        public string TypeTag {get;set;}
        /// <summary>
        /// единица измерения
        /// </summary>
        public string UOM {get;set;}
        /// <summary>
        /// минимальное значение
        /// </summary>
        public object MinValue {get;set;}
        /// <summary>
        /// максимальное значение
        /// </summary>
        public object MaxValue {get;set;}
        /// <summary>
        /// значение по умолчанию
        /// </summary>
        public object DefValue {get;set;}
        /// <summary>
        /// доступ к тегу
        /// </summary>
        public string AccessToValue {get;set;}
        /// <summary>
        /// строка комментария
        /// </summary>
        public string Comment{get;set;}
        /// <summary>
        /// видимость тега в интерфейсе пользователя
        /// </summary>
        public bool Visible {get;set;}
        #endregion

        #region дополнительные свойства
        /// <summary>
        /// строка идентификации тега
        /// в формате
        /// dsguid.devguid.tagguid
        /// </summary>
        public string StrTagIdent {get;set;}
        /// <summary>
        /// значение тега
        /// </summary>
        public object TagValue { get; set; }
        /// <summary>
        /// качество
        /// </summary>
        public ProjectCommonData.VarQuality TagQuality { get; set; }
        /// <summary>
        /// метка времени
        /// </summary>
        public DateTime TimeStamp {get;set;}
        /// <summary>
        /// словарь значений перечисления
        /// </summary>
        public Dictionary<int, string> EnumValue = new Dictionary<int, string>();
        // перенесено на уровень конфигурации в отдельный список каналов IO
        //public LinksHT2NT.LinkHT2NTBase LinkConvertHT2NT;
        #endregion

        #region private
        private XElement xe_tag;        
        #endregion

        #region конструкторы
        public Tag()
        {
            try
            {
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }        
        #endregion
        /// <summary>
        /// функция установки значения по событию от линка
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        public void linkHT2NT_OnChangeTagNT(object value, ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                SetTagValue(value, varquality, dt);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        private void SetTagValue(object value, ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                //if (value == TagValue && varquality == TagQuality)
                //    return;

                TagValue = value;
                TagQuality = varquality;
                TimeStamp = dt;

                OnTagValueChanged();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        }

        private void OnTagValueChanged()
        {
            try
            {
                var tmp = OnChangeTagNT;
                if (tmp != null)
                    tmp(StrTagIdent, TagValue, TagQuality, TimeStamp);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
