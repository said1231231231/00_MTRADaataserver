/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkHT2NT_Int_2_Short - класс для конкретизации действий по преобразованию значения hardware в значение типа c#
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\LinksHT2NT\LinkHT2NT_Int_2_Short.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib.CommonClasses;

namespace LinksLib.LinksHT2NT_MOA_ECU
{
    public class LinkHT2NT_Int_2_Short : LinkHT2NT_MOA_ECU
    {
        public LinkHT2NT_Int_2_Short(DataConfigurationHardware dchHC, DataConfiguration dcNC)
            : base(dchHC, dcNC)
        {
        }

        /// <summary>
        /// вычислить значение NT по значению HT
        /// </summary>
        /// <param name="value"></param>
        /// <param name="varquality"></param>
        /// <param name="dt"></param>
        protected override void ReNewNTValue(object valueHT, ProjectCommonData.VarQuality varqualityHT, DateTime dtHT)
        {
            if (valueHT == null)
                return;

             try
             {
                 // значение
                 Array.Reverse(valueHT as byte[]);
                 TagValueNT = BitConverter.ToInt16(valueHT as byte[], 0);
                 //качество и время
                 TagQualityNT = TagQualityHT;
                 TimeStampNT = TimeStampHT;
             }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
