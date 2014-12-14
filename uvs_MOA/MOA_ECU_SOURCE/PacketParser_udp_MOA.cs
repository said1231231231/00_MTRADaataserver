/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: Класс для обработки входных пакетов для источника данных МОА
 *                                                                             
 *	Файл                     : X:\Projects\38_DS4BlockingPrg\ProviderCustomerExchangeLib\PacketParser_udp_MOA.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 17.10.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
//using InterfaceLibrary;
using System.Diagnostics;
using NativeConfigurationLib.NativeConfiguration;
using HardwareConfigurationLib.HardwareConfiguration;

namespace uvs_MOA.MOA_ECU_SOURCE
{
   public class PacketParser_udp_MOA// : IPacketParser
   {
      /// <summary>
      /// локальная очередь - создается для быстрого копирования
      /// входной очереди byteQueque
      /// </summary>
      Queue<byte[]> netPackQLoc;
      /// <summary>
      /// поток, обрабатывающий входную очередь
      /// </summary>
      BackgroundWorker bcwQ;
	   /// <summary>
	   /// конфигурация текущего DataServer
	   /// </summary>
	  //IConfiguration srcCfg;
       /// <summary>
       /// ссылка на класс источника
       /// </summary>
      HardwareConfiguration_MOA_ECU.DataSourceHardware_MOA_ECU dsh_moa_ecu;
	  /// <summary>
	  /// инициалзизация класс разбора
	  /// </summary>
      public void Init(HardwareConfiguration_MOA_ECU.DataSourceHardware_MOA_ECU dsh_moa_ecu)//IConfiguration srcCfg
      {
          this.dsh_moa_ecu = dsh_moa_ecu;

         netPackQLoc = new Queue<byte[]>();

         bcwQ = new BackgroundWorker();
         bcwQ.DoWork += new DoWorkEventHandler(bcwQ_DoWork);
      }

      public void byteQueque_packetAppearance(Queue<byte[]> pq)
      {
            byte[] tmp;

            try
            {
                lock (pq)
                {
                    // быстро добавляем пакеты в очередь на обработку
                    try
                    {
                        byte[][] pqarr = pq.ToArray();
                        if (pqarr.Length != pq.Count)
                        {
                        }
                        // и чистим входную
                        pq.Clear();

                        for (int i = 0; i < pqarr.Length; i++)
			            {
                            if (pqarr[i] == null)
                                continue;

                            //tmp = new byte[pq.ToArray()[i].Length];
                            //Buffer.BlockCopy(pq.ToArray()[i], 0, tmp, 0, pq.ToArray()[i].Length);
                            //netPackQLoc.Enqueue(tmp);			 
                            netPackQLoc.Enqueue(pqarr[i]);	
			            }
                        //// и чистим входную
                        //pq.Clear();
                    }
                    catch (Exception ex)
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 86, string.Format("{0} : {1} : {2} : ОШИБКА : {3}", DateTime.Now.ToString(),
                                    @"X:\Projects\00_DataServer\ProviderCustomerExchangeLib\PacketParser_udp_MOA.cs", "byteQueque_packetAppearance()", ex.Message));
                        pq.Clear();
                    }

                    // обработка считанных пакетов в отд. потоке
                    if (!bcwQ.IsBusy)
                    {
                        Queue<byte[]> q4Process = new Queue<byte[]>(netPackQLoc.ToArray());
                        netPackQLoc.Clear();

                        //bcwQ.RunWorkerAsync(netPackQLoc);
                        bcwQ.RunWorkerAsync(q4Process);
                    }
                    else
                    {
                        if (netPackQLoc.Count > 100)
                            Console.WriteLine(String.Format("************ Число пакетов во вх очереди на обработку > 100 и netPackQLoc.Count = {0} **************************", netPackQLoc.Count));
                        return;
                    }
                }
			}
			catch(Exception ex)
			{
			   TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
      }

      /// <summary>
      /// извлекает пакеты из очереди netPackQLoc
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void bcwQ_DoWork(object sender, DoWorkEventArgs e)
      {
         UInt16 lenpack = 0;
         UInt16 numdev = 0;
         UInt32 nf = 0;
         UInt32 nd = 0;

		Queue<byte[]> NetPackQ = (Queue<byte[]>)e.Argument;
        byte[] buf;

        try
        {
            if (NetPackQ.Count == 0)
            return;

           while (NetPackQ.Count > 0)
			{
                buf = (byte[])NetPackQ.Dequeue();
                if (buf == null || buf.Length == 0)
                    break;
                try
                {
                    using (MemoryStream msDev = new MemoryStream(buf))
                        using (BinaryReader binReader = new BinaryReader(msDev))
                        {
                            try
                            {
                                lenpack = (ushort)binReader.ReadUInt16();
                                // номер устройства в пакет с учетом ФК, поэтому вычленяем его
                                numdev = binReader.ReadUInt16();
                                nf = (UInt32)numdev / 256;
                                nd = (UInt32)numdev % 256;

                                if (numdev == 537)
                                {
                                }
                                if (numdev == 537)
                                {
                                }
                                binReader.BaseStream.Position -= 4;

                                dsh_moa_ecu.PacketHandler(nf, numdev, binReader.ReadBytes(lenpack));
                            }
                            catch (Exception ex)
                            {
                                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                            }
                        }
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
			} 
		}
		catch(Exception ex)
		{
			TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			NetPackQ.Clear();
		}
      }
   }
}