/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _07PresentatonTag - класс представления тега уровня интерфейса пользователя
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentationTreeItems\_07PresentatonTag.cs
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
using System.Diagnostics;
using System.Xml.Linq;
using System.ComponentModel;
using CommonClassesLib.CommonClasses;

namespace MTRADataServer.PresentationTreeItems
{
    public class _07PresentatonTag : INotifyPropertyChanged
    {        
        private uint tagGuid;
        /// <summary>
        /// идентификатор тега тега - 
        /// им м.б. адрес регистра, 
        /// индекс тега в описании САФ,
        /// идентификатор OPC
        /// </summary>
        public uint TagGuid
        {
            get { return tagGuid; }
            set
            {
                tagGuid = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagGuid"));
            }
        }
        private string tagName;
        /// <summary>
        /// Название тега
        /// для регистров модбас 
        /// представляющих битовые поля это м.б.
        /// название первого поля
        /// </summary>
        public string TagName
        {
            get { return tagName; }
            set
            {
                tagName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagName"));
            }
        }
        private string tagType;
        /// <summary>
        /// Название типа тега
        /// в устройстве
        /// </summary>
        public string TagType
        {
            get { return tagType; }
            set
            {
                tagType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagType"));
            }
        }

        private object tagValue;
        /// <summary>
        /// значение тега
        /// </summary>
        public object TagValue
        {
            get { return tagValue; }
            set
            {
                tagValue = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagValue"));
            }
        }
        private string tagValueAsString = string.Empty;
        /// <summary>
        /// значение тега
        /// </summary>
        public string TagValueAsString
        {
            get { return tagValueAsString; }
            set
            {
                tagValueAsString = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagValueAsString"));
            }
        }
        private ProjectCommonData.VarQuality tagQuality = ProjectCommonData.VarQuality.vqUndefined;
        /// <summary>
        /// качество тега
        /// </summary>
        public ProjectCommonData.VarQuality TagQuality
        {
            get { return tagQuality; }
            set
            {
                tagQuality = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagQuality"));
            }
        }
        private DateTime timeStamp;
        /// <summary>
        /// метка времени
        /// </summary>
        public DateTime TimeStamp
        {

            get { return timeStamp; }
            set
            {
                timeStamp = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TimeStamp"));
            }
        }

        public void tg_OnChangeTagHT(string stridtag, object value, ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                TagQuality = varquality;
                if (value != null)
                    TagValue = value;
                TimeStamp = dt;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            try
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, e);
                if (e.PropertyName == "TagValue")
                {
                    if (TagValue == null)
                        return;

                    string typet = TagValue.GetType().Name;
                    switch(typet.ToLower())
                    {
                        case "byte[]":
                            TagValueAsString = "0x" + BitConverter.ToString(TagValue as byte[]);
                            break;
                        case "string":
                            TagValueAsString = TagValue as string;
                            break;
                        case "single":
                        case "int16":
                        case "uint16":
                            TagValueAsString = TagValue.ToString();
                            break;
                        case "datetime":
                            TagValueAsString = TagValue.ToString();
                            break;
                        case "boolean":
                            TagValueAsString = TagValue.ToString();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        }
    }
}
