/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: TagHardware - класс представления тега уровня hardware
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\TagHardware.cs
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

namespace HardwareConfigurationLib.HardwareConfiguration
{
    public delegate void ChangeTagHT(string strTagIdent,object value, CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality varquality, DateTime dt);

    public class TagHardware
    {
        public event ChangeTagHT OnChangeTagHT;

        /// <summary>
        /// идентификатор тега тега - 
        /// им м.б. адрес регистра, 
        /// индекс тега в описании САФ,
        /// идентификатор OPC
        /// </summary>
        public uint TagGuid {get;set;}
        /// <summary>
        /// Название тега
        /// для регистров модбас 
        /// представляющих битовые поля это м.б.
        /// название первого поля
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// Название типа тега
        /// в устройстве
        /// </summary>
        public string TagType {get;set;}

        /// <summary>
        /// последнее сформированное
        /// значение тега - требуется для 
        /// точек кода где сравниваются 
        /// новое и предыдущее значение тега
        /// </summary>
        private CommonClassesLib.CommonClasses.ProjectCommonData.DSTagValueCompare DSTAGVALUECOMPARE { get; set; }

        private object tagValue;
        /// <summary>
        /// значение тега
        /// </summary>
        public object TagValue { get; private set; }
        private CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality tagQuality = CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality.vqUndefined;
        /// <summary>
        /// качество тега
        /// </summary>
        public CommonClassesLib.CommonClasses.ProjectCommonData.VarQuality TagQuality  { get; private set; }
        /// <summary>
        /// метка времени
        /// </summary>
        public DateTime TimeStamp  { get; private set; }

        /// <summary>
        /// длина в единицах источника
        /// это м.б. байты, регистры modbus - 
        /// логика интерпретации заложена в классах 
        /// устройств
        /// </summary>
        public Int32 VendorLengthTag { get; set; }

        #region конструкторы
        public TagHardware()
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
        /// Устанавливает значение тега
        /// </summary>
        public void SetTagValue(object tagValue, ProjectCommonData.VarQuality tagValueQuality)
        {
            try
            {
                CommonClassesLib.CommonClasses.ProjectCommonData.DSTagValueCompare dstNew = new CommonClassesLib.CommonClasses.ProjectCommonData.DSTagValueCompare(tagValueQuality, tagValue);
                DSTAGVALUECOMPARE = new CommonClassesLib.CommonClasses.ProjectCommonData.DSTagValueCompare(TagQuality, TagValue);

                if (CommonClassesLib.CommonClasses.ProjectCommonData.IsTagAsObjectsIsEqual(DSTAGVALUECOMPARE, dstNew))
                    return;

                TagValue = tagValue;
                TagQuality = tagValueQuality;
                TimeStamp = DateTime.Now;

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
                var tmp = OnChangeTagHT;
                if (tmp != null)
                    tmp(string.Empty, TagValue, TagQuality, TimeStamp);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
