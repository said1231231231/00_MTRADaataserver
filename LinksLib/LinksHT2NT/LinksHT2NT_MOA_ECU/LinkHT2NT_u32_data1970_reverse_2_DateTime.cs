/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkHT2NT_u32_data1970_reverse_2_DateTime -  - класс для конкретизации действий по преобразованию значения hardware в значение типа c#
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\LinksHT2NT\LinksHT2NT_MOA_ECU\LinkHT2NT_u32_data1970_reverse_2_DateTime.cs
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
using System.IO;
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib.CommonClasses;

namespace LinksLib.LinksHT2NT_MOA_ECU
{
    public class LinkHT2NT_u32_data1970_reverse_2_DateTime : LinkHT2NT_MOA_ECU
    {
        public LinkHT2NT_u32_data1970_reverse_2_DateTime(DataConfigurationHardware dchHC, DataConfiguration dcNC)
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
            DateTime varMT_Value = DateTime.MinValue;

            if (valueHT == null)
                return;

            try
            {
                // быстро анализируем месяц, он не может быть нулем, поэтому считаем что вместо времени пришли 0
                if ((valueHT as byte[])[1] != 0)
                {
                    switch ((valueHT as byte[]).Length)
                    {
                        case 4: // DateTimeUTC_FieldMT
                            UInt32 varMT_temp = BitConverter.ToUInt32(valueHT as byte[], 0);
                            varMT_Value = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                            varMT_Value = varMT_Value.AddSeconds(varMT_temp);
                            break;
                        case 6: // DateTime3_FieldMT
                            varMT_Value = new System.DateTime(
                                                 2000 + Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[0])),  // Year32
                                                 Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[1])),         // Month32
                                                 Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[2])),         // Day32
                                                 Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[3])),         // Hour32
                                                 Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[4])),         // Minute32
                                                 Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[5])));        // Second32   
                            break;
                        case 8:
                            varMT_Value = new System.DateTime(
                                                       2000 + Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[0])),  // Year32
                                                       Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[1])),			// Month32
                                                       Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[2])),			// Day32
                                                       Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[3])),			// Hour32
                                                       Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[4])),			// Minute32
                                                       Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[5])),			// ,Second32
                                                       Convert.ToInt32(ConvertBCDtoINT((valueHT as byte[])[6]) * 10));			// Millisec
                            break;
                        default:
                            break;
                    }
                }
                // значение
                TagValueNT = varMT_Value;
                //качество и время
                TagQualityNT = TagQualityHT;
                TimeStampNT = TimeStampHT;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                TraceSourceLib.TraceSourceDiagMes.WriteDump(System.Diagnostics.TraceEventType.Error, 664, "Содержимое дампа памяти тега", new MemoryStream(valueHT as byte[]));
            }
        }

        /// <summary>
        /// public int ConvertBCDtoINT(byte b)
        ///     преобразование BCD-байта в эквивалентное двоичное значение
        /// </summary>
        public int ConvertBCDtoINT(byte b)
        {
            byte t = b;
            return (t >> 4) * 10 + (b & 0x0f);
        }
    }
}
