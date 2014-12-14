/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkHT2NTBase - базовый класс связи тегов Hardware (HT) и Native (NT)
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\LinksHT2NT\LinkHT2NTBase.cs
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
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib;
using CommonClassesLib.CommonClasses;

namespace LinksLib.LinksHT2NT
{
    public delegate void ChangeTagNT(object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt);

    public enum MODE 
    {
        /// <summary>
        /// естественный обмен 
        /// с реальным устройством
        /// </summary>
        Natural = 0,
        /// <summary>
        /// режим эмуляции 
        /// </summary>
        Emulator = 1,
        /// <summary>
        /// ручная установка тега
        /// </summary>
        Handles = 2
    }

    public class LinkHT2NTBase
    {
        /// <summary>
        /// событие для вызова обновления тега на уровне Native
        /// </summary>
        public event ChangeTagNT OnChangeTagNT;

        public MODE Mode { get; set; }

        /// <summary>
        /// конфигурация Hardware
        /// </summary>
        private HardwareConfigurationLib.HardwareConfiguration.DataConfigurationHardware DCHHC { get; set; }
        /// <summary>
        /// конфигурация Native
        /// </summary>
        private NativeConfigurationLib.NativeConfiguration.DataConfiguration DCNC { get; set; }

        #region привязка HARDWARE
        /// <summary>
        /// привязка к устройству 
        /// уровня HARDWARE
        /// </summary>
        private DeviceHardware DEVICEHD { get; set; }
        /// <summary>
        /// идентификатор устр. hardware
        /// </summary>
        private uint devicehdid { get; set; }
        /// <summary>
        /// привязка к тегу 
        /// уровня HARDWARE
        /// </summary>
        public TagHardware TAGHD { get; set; }
        /// <summary>
        /// идентификатор тега hardware
        /// </summary>
        private uint taghdid { get; set; }        
        #endregion

        #region привязка NATIVE
        /// <summary>
        /// привязка к устройству 
        /// уровня NATIVE
        /// </summary>
        private Device DEVICEND { get; set; }
        /// <summary>
        /// привязка к устр
        /// уровня native
        /// </summary>
        private uint devicendid { get; set; }
        /// <summary>
        /// привязка к тегу 
        /// уровня NATIVE
        /// </summary>
        public Tag TAGND { get; set; }
        /// <summary>
        /// идентификатор тега native
        /// </summary>
        private uint tagndid { get; set; }        
        #endregion

        #region значение тега (VTQ) на входе (от HardwareTag)
        private object tagValueHT;
        /// <summary>
        /// значение тега на входе
        /// </summary>
        public object TagValueHT
        {
            get { return tagValueHT; }
            set { tagValueHT = value; }
        }
        private ProjectCommonData.VarQuality tagQualityHT;
        /// <summary>
        /// качество тега  на входе
        /// </summary>
        public ProjectCommonData.VarQuality TagQualityHT
        {
            get { return tagQualityHT; }
            set { tagQualityHT = value; }
        }
        private DateTime timeStampHT;
        /// <summary>
        /// метка времени  на входе
        /// </summary>
        public DateTime TimeStampHT
        {
            get { return timeStampHT; }
            set { timeStampHT = value; }
        } 
	    #endregion

        #region значение тега (VTQ) на выходе (для NativeTags)
        private object tagValueNT;
        /// <summary>
        /// значение тега на входе
        /// </summary>
        public object TagValueNT
        {
            get { return tagValueNT; }
            set { tagValueNT = value; }
        }
        private ProjectCommonData.VarQuality tagQualityNT;
        /// <summary>
        /// качество тега  на входе
        /// </summary>
        public ProjectCommonData.VarQuality TagQualityNT
        {
            get { return tagQualityNT; }
            set { tagQualityNT = value; }
        }
        private DateTime timeStampNT;
        /// <summary>
        /// метка времени  на входе
        /// </summary>
        public DateTime TimeStampNT
        {

            get { return timeStampNT; }
            set { timeStampNT = value; }
        }
        #endregion

        public LinkHT2NTBase( DataConfigurationHardware dchHC, DataConfiguration dcNC )
        {
            Mode = MODE.Natural;    // по умолчанию

            DCHHC = dchHC;
            DCNC = dcNC;
        }
        /// <summary>
        /// привязать канал к тегу 
        /// уровня hardware
        /// </summary>
        /// <param name="devguid">уник номер устр</param>
        /// <param name="tagguid">уник номер тега</param>
        /// <returns>признак успешности привязки</returns>
        public virtual bool SetLink2TAGHD(uint devguid, uint tagguid)
        {
            bool rezlink = false;
            try
            {
                devicehdid = devguid;
                taghdid = tagguid;

                rezlink = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rezlink;
        }
        /// <summary>
        /// привязать канал к тегу 
        /// уровня native
        /// </summary>
        /// <param name="devguid">уник номер устр</param>
        /// <param name="tagguid">уник номер тега</param>
        /// <returns>признак успешности привязки</returns>
        public virtual bool SetLink2TAGND(uint devguid, uint tagguid)
        {
            bool rezlink = false;
            try
            {
                devicendid = devguid;
                tagndid = tagguid;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rezlink;
        }
        public bool CreateLink()
        {
            bool rezlink = false;
            try
            {
                DEVICEHD = DCHHC.GetDeviceByGUID(devicehdid);
                if ( DEVICEHD == null )
                    throw new Exception(string.Format(@"(224) ...\LinksHT2NT\LinkHT2NTBase.cs: CreateLink() : ошибка настройки канала."));
                TAGHD = DEVICEHD.GetTagByTagGUID(taghdid);
                if ( TAGHD == null )
                    throw new Exception(string.Format(@"(227) ...\LinksHT2NT\LinkHT2NTBase.cs: CreateLink() : ошибка настройки канала."));

                if (TAGHD.TagValue != null)
                { 

                }

                TAGHD.OnChangeTagHT += TAGHD_OnChangeTagHT;

                DEVICEND = DCNC.GetDeviceByGUID(devicendid);
                if (DEVICEND == null)
                    throw new Exception(string.Format(@"(231) ...\LinksHT2NT\LinkHT2NTBase.cs: CreateLink() : ошибка настройки канала."));
                TAGND = DEVICEND.GetTagByTagGUID(tagndid);
                if (TAGND == null)
                    throw new Exception(string.Format(@"(234) ...\LinksHT2NT\LinkHT2NTBase.cs: CreateLink() : ошибка настройки канала."));

                // установить значение в зависимости от режима работы Link
                SetVTQBylinkMode(TAGHD.TagValue, TAGHD.TagQuality, TAGHD.TimeStamp);

                this.OnChangeTagNT += TAGND.linkHT2NT_OnChangeTagNT;

                rezlink = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rezlink;
        }
        /// <summary>
        /// установить значение согласно текущему режиму работы
        /// </summary>
        private void SetVTQBylinkMode(object value, ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                switch( Mode )
                {
                    case MODE.Natural:
                        if (TagValueHT != null)
                        { 
                        }

                        // установка VTQ hardware
                        TagQualityHT = varquality;
                        TagValueHT = value;
                        TimeStampHT = dt;

                        ReNewNTValue(TagValueHT, TagQualityHT, TimeStampHT);

                        TAGND.TagValue = TagValueNT;
                        TAGND.TagQuality = TagQualityNT;
                        TAGND.TimeStamp = TimeStampNT;

                        if (this.OnChangeTagNT != null)
                            OnChangeTagNT(TagValueNT, TagQualityNT, TimeStampNT);
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
        /// <summary>
        /// вызов по событию изменения 
        /// тега hardware
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        void TAGHD_OnChangeTagHT(string stridtag, object value, ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                SetVTQBylinkMode(value,varquality, dt);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// вычислить значение NT по значению HT
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        protected virtual void ReNewNTValue(object value, ProjectCommonData.VarQuality varquality, DateTime dt)
        {
        }
    }
}
