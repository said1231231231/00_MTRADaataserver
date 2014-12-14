/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _05Tag - класс представления тега уровня Presentation
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentaionConfiguration\_05Tag.cs
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
using CommonClassesLib.CommonClasses;

namespace PresentationConfigurationLib.PresentaionConfiguration
{
    public delegate void ChangeTagPT(string stridtag, object value, ProjectCommonData.VarQuality varquality, DateTime dt);

    public class _05Tag
    {
        public event ChangeTagPT OnChangeTagPT;

        public bool Enable { get; set; }
        public uint TagGUID { get; set; }
        public string Name { get; set; }
        public string UOM { get; set; }
        public uint PosPoint { get; set; }
        public string TagType { get; set; }
        public string Access { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public string RawType { get; set; }
        /// <summary>
        /// atp
        /// </summary>
        public string RealAnlogType { get; set; }
        public string Formula4CalculatedRaw { get; set; }
        /// <summary>
        /// словарь значений перечисления
        /// </summary>
        public Dictionary<int, string> DictEnumValues = new Dictionary<int, string>();
        /// <summary>
        /// ссылка на группу для этого тега
        /// </summary>
        public _04Group ThisGroup = null;

        #region VTQ
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
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// коэффициент пересчета
        /// </summary>
        public Single TransformationRatio { get; set; }
        /// <summary>
        /// признак инверсности
        /// </summary>
        public bool IsInverseBoolValue { get; set; }
        /// <summary>
        /// строка с формулой
        /// </summary>
        public string Formula4Calculated { get; set; }
	    #endregion

        public _05Tag()
        {
            DictEnumValues = new Dictionary<int, string>();
        }
        /// <summary>
        /// функция установки значения по событию от линка
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        public void linkNT2PT_OnChangeTagPT(object value, ProjectCommonData.VarQuality varquality, DateTime dt)
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
                var tmp = OnChangeTagPT;
                string stridtag = string.Format("{0}.{1}.{2}", ThisGroup.Thisdevice.ThisDS.UniDS_GUID, ThisGroup.Thisdevice.ObjectGUID, this.TagGUID);
                if (tmp != null)
                    tmp(stridtag, TagValue, TagQuality, TimeStamp);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
