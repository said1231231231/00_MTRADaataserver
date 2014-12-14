/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkNT2PTBase - базовый класс связи тегов Native (NT) и Presentation (PT)
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\LinksLib\LinksNT2PT\LinkNT2PTBase.cs
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
using NativeConfigurationLib.NativeConfiguration;
using PresentationConfigurationLib.PresentaionConfiguration;

namespace LinksLib.LinksNT2PT
{
    public delegate void ChangeTagPT(object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt);

    public class LinkNT2PTBase
    {
        /// <summary>
        /// событие для вызова обновления тега на уровне Native
        /// </summary>
        public event ChangeTagPT OnChangeTagPT;

        public LinksHT2NT.MODE Mode { get; set; }

        /// <summary>
        /// конфигурация Native
        /// </summary>
        protected NativeConfigurationLib.NativeConfiguration.DataConfiguration DCNC { get; set; }
        /// <summary>
        /// конфигурация Presentation
        /// </summary>
        protected PresentationConfigurationLib.PresentaionConfiguration._01Configuration DCPL { get; set; }

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

        #region привязка Presentation
        /// <summary>
        /// привязка к устройству 
        /// уровня Presentation
        /// </summary>
        protected _03Device DEVICEPL { get; set; }
        /// <summary>
        /// идентификатор устр. Presentation
        /// </summary>
        protected uint deviceplid { get; set; }
        /// <summary>
        /// привязка к тегу 
        /// уровня Presentation
        /// </summary>
        public _05Tag TAGPL { get; set; }
        /// <summary>
        /// идентификатор тега Presentation
        /// </summary>
        protected uint tagplid { get; set; }        
        #endregion

        #region значение тега (VTQ) на входе (от NativeTags)
        /// <summary>
        /// значение тега на входе
        /// </summary>
        public object TagValueNT { get; set; }
        /// <summary>
        /// качество тега  на входе
        /// </summary>
        public CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality TagQualityNT { get; set; }
        /// <summary>
        /// метка времени  на входе
        /// </summary>
        public DateTime TimeStampNT { get; set; }
        #endregion

        #region значение тега (VTQ) на выходе (для Presentstion)
        /// <summary>
        /// значение тега на выходе
        /// </summary>
        public object TagValuePL { get; set; }
        /// <summary>
        /// качество тега  на выходе
        /// </summary>
        public CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality TagQualityPL { get; set; }
        /// <summary>
        /// метка времени  на выходе
        /// </summary>
        public DateTime TimeStampPL {get;set;}
        /// <summary>
        /// коэф пересчета для
        /// аналоговых тегов
        /// </summary>
        public Single TransformationRatioPL { get; set; }
	    #endregion

        public LinkNT2PTBase(DataConfiguration dcNC, _01Configuration dcPL)
        {
            Mode = LinksHT2NT.MODE.Natural;    // по умолчанию

            DCPL = dcPL;
            DCNC = dcNC;
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
        /// <summary>
        /// привязать канал к тегу 
        /// уровня presentstion
        /// </summary>
        /// <param name="devguid">уник номер устр</param>
        /// <param name="tagguid">уник номер тега</param>
        /// <returns>признак успешности привязки</returns>
        public virtual bool SetLink2TAGPL(uint devguid, uint tagguid)
        {
            bool rezlink = false;
            try
            {
                deviceplid = devguid;
                tagplid = tagguid;

                rezlink = true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rezlink;
        }
        /// <summary>
        /// настроить ссылку на тег
        /// </summary>
        /// <returns></returns>
        public virtual bool CreateLink()
        {
            bool rezlink = false;
            try
            {
                DEVICEND = DCNC.GetDeviceByGUID(devicendid);
                if (DEVICEND == null)
                    throw new Exception(string.Format(@"(231) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));
                TAGND = DEVICEND.GetTagByTagGUID(tagndid);
                if (TAGND == null)
                    throw new Exception(string.Format(@"(234) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));

                TAGND.OnChangeTagNT += TAGND_OnChangeTagPT;// TAGPL.linkHT2NT_OnChangeTagPT;

                DEVICEPL = DCPL.GetDeviceByGUID(deviceplid);
                if ( DEVICEPL == null )
                    throw new Exception(string.Format(@"(224) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));
                TAGPL = DEVICEPL.GetTagByTagGUID(tagplid);
                if ( TAGPL == null )
                    throw new Exception(string.Format(@"(227) ...LinksLib\LinksNT2PT\LinkNT2PTBase.cs: CreateLink() : ошибка настройки канала."));

                // установить коэф пересчета
                TransformationRatioPL = TAGPL.TransformationRatio;

                // установить значение в зависимости от режима работы Link
                SetVTQBylinkMode(TAGND.TagValue, TAGND.TagQuality, TAGND.TimeStamp);

                this.OnChangeTagPT += TAGPL.linkNT2PT_OnChangeTagPT;// TAGND.l TAGHD_OnChangeTagPT;

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
        protected virtual void SetVTQBylinkMode(object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                switch( Mode )
                {
                    case LinksHT2NT.MODE.Natural:
                        if (TagValuePL != null)
                        { 
                        }

                        // установка VTQ native
                        TagQualityNT = varquality;
                        TagValueNT = value;
                        TimeStampNT = dt;

                        ReNewPTValue();

                        TAGPL.TagValue = TagValuePL;
                        TAGPL.TagQuality = TagQualityPL;
                        TAGPL.TimeStamp = TimeStampPL;
                        
                        //TAGND.TagValue = TagValueNT;
                        //TAGND.TagQuality = TagQualityNT;
                        //TAGND.TimeStamp = TimeStampNT;

                        //if (this.OnChangeTagPT != null)
                        //    OnChangeTagPT(TagValuePL, TagQualityPL, TimeStampPL);//

                        SetVTQ2Client(TagValuePL, TagQualityPL, TimeStampPL);
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
        /// тега native
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        void TAGND_OnChangeTagPT(string stridtag, object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                SetVTQBylinkMode(value, varquality, dt);// // TagValuePL, TagQualityPL, TimeStampPL
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// вычислить значение PL по значению NT
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        protected virtual void ReNewPTValue()//object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        protected void SetVTQ2Client(object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt)
        {
            try
            {
                if (this.OnChangeTagPT != null)
                    OnChangeTagPT(TagValuePL, TagQualityPL, TimeStampPL);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
