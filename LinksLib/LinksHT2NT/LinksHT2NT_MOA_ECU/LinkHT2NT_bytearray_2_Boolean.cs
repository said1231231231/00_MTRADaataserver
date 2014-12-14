/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkHT2NT_bytearray_2_Boolean -  - класс для конкретизации действий по преобразованию значения hardware в значение типа c#
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\LinksHT2NT\LinkHT2NT_bytearray_2_Boolean.cs
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
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib.CommonClasses;

namespace LinksLib.LinksHT2NT_MOA_ECU
{
    public class LinkHT2NT_bytearray_2_Boolean : LinkHT2NT_MOA_ECU
    {
        /// <summary>
        /// битовая маска для извлечения значения тега
        /// </summary>
        public string bitMask { get; set; }

        public LinkHT2NT_bytearray_2_Boolean(DataConfigurationHardware dchHC, DataConfiguration dcNC) : base (dchHC, dcNC)
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
                 byte bbm = 0;

                 if (!string.IsNullOrWhiteSpace(bitMask))
                 {
                     // выделим крайний символ строки - это д.б. 16-ная цифра
                     Regex re = new Regex(@"[0-9a-fA-F]$");
                     MatchCollection mc = re.Matches(bitMask);
                     int iCountMatchs = mc.Count;

                     if (mc.Count == 0)
                         throw new Exception(string.Format(@"(64) : ...\LinksHT2NT\LinkHT2NT_bytearray_2_Boolean.cs : ReNewNTValue() : некорректная битовая маска ля создания TagGUID = {0}", bitMask));

                     string bm = mc[0].ToString();

                     bbm = Convert.ToByte(bm, 16);

                     //if (bbm > 7)
                     //    bbm -= 8;
                 }
                 else
                     throw new Exception(string.Format(@"(74) : ...\LinksHT2NT\LinkHT2NT_bytearray_2_Boolean.cs : ReNewNTValue() : Некорректная битовая маска поля = {0}", bitMask));


                 if (valueHT.GetType().Name != "Byte[]")
                     throw new Exception(string.Format(@"(80) ...\LinksHT2NT\LinksHT2NT_MOA_ECU\LinkHT2NT_bytearray_2_Boolean.cs: ReNewNTValue() : тип данных должен быть байтовым массивом."));

                 // установим заданный порядок байт   
                 byte[] tmpb = new byte[(valueHT as byte[]).Length];
                 Buffer.BlockCopy((valueHT as byte[]), 0, tmpb, 0, (valueHT as byte[]).Length);

                 #region костыли
                 /*
                  * в старой конфигурации (по крайней мере для БМРЗ)
                  * адреса умножались на 2 - т.е. как бы плоская модель памяти
                  * поэтому нужно адаптировать уровень hardware в регистрах modbus
                  * под  это описание, что ниже и делается
                  */
                 //if ((valueHT as byte[]).Length != BYTEORDER.Count())
                 //{
                 //    if (BYTEORDER.Count() == 1 && (valueHT as byte[]).Length == 2)
                 //    {
                 //        /*
                 //         * если регистр четный, то работаем с младшим (?)
                 //         * байтом регистра, и наоборот
                 //         * если регистр нечетный, то работаем со старшим (?)
                 //         * байтом регистра
                 //         */

                 //        //ReorderingByteArray(ref tmpb, (valueHT as byte[]), BYTEORDER);   // переупорядочивать нечего т.к. 1 байт, который определяем по четности регистра:                 
                 #endregion
                        
                         // копия исходного байтового массива в локальную переменную valVar
                         byte[] valVar = new byte[1];

                         // массив в кот уст маска
                         byte[] mTmp = new byte[1];

                         //if ((ADDRESS % 2) == 0)
                         //{
                         //    // регистр четный
                             Buffer.BlockCopy(tmpb, 0, valVar, 0, 1);
                         //}
                         //else
                         //{ 
                         //    // регистр нечетный
                         //    Buffer.BlockCopy(tmpb, 1, valVar, 0, 1);
                         //}


                         // сравниваем массивы бит
                         System.Collections.BitArray mBits = new BitArray(mTmp);
                         mBits.Set((int)bbm, true);
                         System.Collections.BitArray vBits = new BitArray(valVar);
                         vBits.And(mBits);

                         for (int i = 0; i < vBits.Count; i++)
                         {
                             if (vBits.Get(i) == true)
                             {
                                 TagValueNT = Boolean.TrueString;
                                 break;
                             }
                             else
                             {
                                 TagValueNT = Boolean.FalseString;
                             }
                         }
                 //    }
                 //    else
                 //        throw new Exception(string.Format(@"(97) ...\LinksHT2NT_MOA_ECU\LinkHT2NT_bytearray_2_Boolean.cs : ReNewNTValue() : несовместимая комбинация длин "));
                 //}

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
