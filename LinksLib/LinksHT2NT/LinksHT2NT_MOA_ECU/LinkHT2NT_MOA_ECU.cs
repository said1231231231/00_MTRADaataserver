/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: LinkHT2NT_MOA_ECU - канал для источника МОА
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\LinksHT2NT\LinkHT2NT_MOA_ECU.cs
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
using LinksLib.LinksHT2NT;
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib.CommonClasses;

namespace LinksLib.LinksHT2NT_MOA_ECU
{
    public class LinkHT2NT_MOA_ECU :  LinkHT2NTBase
    {
        /// <summary>
        /// порядок байт для преобразования
        /// в значение
        /// </summary>
        public string BYTEORDER { get; set; }
        public uint ADDRESS { get; set; }

        public LinkHT2NT_MOA_ECU( HardwareConfigurationLib.HardwareConfiguration.DataConfigurationHardware dchHC, NativeConfigurationLib.NativeConfiguration.DataConfiguration dcNC)
            : base(dchHC, dcNC)
        { 
        }
        /// <summary>
        /// переупорядочивание исходного массива
        /// </summary>
        /// <param name="tmpb"></param>
        /// <param name="rawval"></param>
        /// <param name="byteorder"></param>
        protected void ReorderingByteArray(ref byte[] tmpb, byte[] rawval, string byteorder)
        {
            int iorder = 0;

            try
            { 
                if (string.IsNullOrWhiteSpace(byteorder))
                {
                    Buffer.BlockCopy(rawval, 0, tmpb, 0, rawval.Length);
                    return;
                }

                if (!byteorder.Contains("f"))
                {
                    char[] arr = byteorder.ToCharArray();
                    Array.Reverse(arr);
                    byteorder = new string(arr);
                    byteorder = byteorder.Trim();
                    // выполняем перестановку по заданному порядку byteorder
                    for (int i = 0; i < tmpb.Length; i++)
                    {
                        iorder = int.Parse(byteorder[i].ToString());
                        tmpb[iorder] = rawval[i];
                    }
                }
                else
                    Buffer.BlockCopy(rawval, 0, tmpb, 0, rawval.Length);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
