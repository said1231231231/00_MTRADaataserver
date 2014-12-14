/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkHT2NT_BCDPack_2_Int32 - класс для конкретизации действий по преобразованию значения hardware в значение типа c#
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\LinksHT2NT\LinksHT2NT_MOA_ECU\LinkHT2NT_BCDPack_2_Int32.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С# 5.0, Framework 4.5                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : xx.xx.2014
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Легенда:
*  - Двоично-десятичное упакованное целое число - (без преобразования в десятичное - этот тип для уставок)
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
    public class LinkHT2NT_BCDPack_2_Int32 : LinkHT2NT_MOA_ECU
    {
        /// <summary>
        /// 
        /// </summary>
        public string pospoint { get; set; }


        public LinkHT2NT_BCDPack_2_Int32(DataConfigurationHardware dchHC, DataConfiguration dcNC)
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

            //try
            //{
            //    Array.Reverse(valueHT as byte[]); 

            //    string pospoint = tag.XMLTagDescriptAsClass.GetValueByName("Device_level_Describe_PosPointInTBCDPackVariable");

            //    UInt32 tmpui32 = ConvertBCDstoUINT(tag.ValueAsMemXRaw2);

            //    Single tmp = Convert.ToSingle(tmpui32);

            //    //вставить точку в соответсвии с типом
            //    switch (pospoint)
            //    {
            //        case "0":
            //            // без точки
            //            break;
            //        case "2":
            //            //Value = pospoint.Insert(1, ".");
            //            tmp /= 1000;
            //            break;
            //        case "3":
            //            //Value = Value.Insert(2, ".");
            //            tmp /= 100;
            //            break;
            //        case "4":
            //            //Value = Value.Insert(3, ".");
            //            tmp /= 10;
            //            break;
            //        default:
            //            throw new Exception(string.Format(@"(390) : X:\Projects\00_DataServer\uvs_MOA\GetValue4TypeClassDev_BMRZ.cs : Transform_TBCDPackVariableMOA_2Turple() : Ошибка указания положения точки в BCD числе = {0}", pospoint));
            //    }

            //    byte[] tmpb = new byte[4];

            //    tmpb = BitConverter.GetBytes(tmp);



            //    TagValueNT = BitConverter.ToInt16(valueHT as byte[], 0);
            //    //качество и время
            //    TagQualityNT = TagQualityHT;
            //    TimeStampNT = TimeStampHT;
            //}
            //catch (Exception ex)
            //{
            //    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            //    TraceSourceLib.TraceSourceDiagMes.WriteDump(System.Diagnostics.TraceEventType.Error, 415, "Содержимое дампа памяти тега", new MemoryStream(rawval_));
            //}
        }
        /// <summary>
        /// public int ConvertBCDstoUINT(byte[] b)
        ///     преобразование последовательности BCD-байт в эквивалентное двоичное значение типа uint
        /// </summary>
        public uint ConvertBCDstoUINT(byte[] bs)
        {
            uint rez = 0;
            uint stepen100 = 1;
            for (int i = bs.Length - 1; i >= 0; i--)
            {
                rez += ConvertBCDtoUINT(bs[i]) * stepen100;
                stepen100 *= 100;
            }
            return rez;
        }
        /// <summary>
        /// public int ConvertBCDtoINT(byte b)
        ///     преобразование BCD-байта в эквивалентное двоичное значение без знака
        /// </summary>
        public uint ConvertBCDtoUINT(byte b)
        {
            byte t = b;
            return (uint)((t >> 4) * 10 + (b & 0x0f));
        }
    }
}
