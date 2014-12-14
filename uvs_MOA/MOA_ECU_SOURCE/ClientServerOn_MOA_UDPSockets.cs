/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: класс для реализации асинхронного обмена по протоколу UDP (MOA)
 *                                                                             
 *	Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\MOA_ECU_SOURCE\ClientServerOn_MOA_UDPSockets.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 23.09.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации: 
 *#############################################################################*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;
using System.Net.Sockets;
using System.Xml.Linq;
using CommonClassesLib.CommonClasses;

namespace uvs_MOA.MOA_ECU_SOURCE
{
    public delegate void ByteArrayPacketAppearance(byte[] pq);

	public class ClientServerOn_MOA_UDPSockets
	{
		#region События
		#endregion

		#region Свойства
		PacketQueque netPackQ;
		public PacketQueque NetPackQ
		{
			set
			{
				netPackQ = value;
			}
			get
			{
				return netPackQ;
			}
		}
		#endregion

		#region public
		#endregion

		#region private
        static object lockKey = new object();
		/// <summary>
		/// массив для входных пакетов
		/// </summary>
		//ArrayForExchange arrForReceiveData;
		/// <summary>
		/// порт для получения данных от сервера
		/// по UDP
		/// </summary>
		int udpserver_port = 0;
		private ChatUdpListener _listener;
		/// <summary>
		/// StringBuilder для формирования ip-адреса UDP-пакетов
		/// </summary>
		StringBuilder adr = new StringBuilder();
		/// <summary>
		/// список новеров фк и их ip-адресов
		/// </summary>
		Dictionary<string, int> dictECUIPAdresses = new Dictionary<string, int>();
        /// <summary>
        /// список ноvеров фк и числа поступивших пакетов от них
        /// (для определения связи с ФК - по таймеру вызывается функция 
        /// кот формирует и подкидывает пакеты с состоянием связи с фк)
        /// </summary>
        //Dictionary<byte, Int16> dictECUIDNumPakets = new Dictionary<byte, Int16>();
        SortedList slECUIDNumPakets = new SortedList();
		/// <summary>
		/// поток в памяти для приема информации последовательностей, формируемых ФК
		/// </summary>
		private BinaryWriter foutMemCurrent = new BinaryWriter(new MemoryStream());
        /// <summary>
        /// timer для отслеживания связи с ФК
        /// </summary>
        System.Timers.Timer tmrFCConnection = new System.Timers.Timer();
        /// <summary>
        /// список пакетов для дампирования обмена
        /// </summary>
        List<byte[]> lstDumps = new List<byte[]>();
        /// <summary>
        /// поток для фонового вывода 
        /// пакетов в файл
        /// </summary>
        BackgroundWorker bgw4DumpPackets;
        /// <summary>
        /// состояние записи пакетов
        /// для последующего анализа
        /// </summary>
        public static bool PacketRecordStatus = false;
        #endregion

		#region конструктор(ы)
		/// <summary>
		/// конструктор класса для обмена по udp 
		/// c  ист. данных МОА
		/// </summary>
		/// <param name="srcinfo">инф для настройки соединения tcp\ip</param>
        public ClientServerOn_MOA_UDPSockets(int port, string namesource )
		{
			CreateConnect2Server(port, namesource);

            bgw4DumpPackets = new BackgroundWorker();
            bgw4DumpPackets.DoWork += new DoWorkEventHandler(bgw4DumpPackets_DoWork);

            tmrFCConnection.Elapsed += new System.Timers.ElapsedEventHandler(tmrFCConnection_Elapsed);
            tmrFCConnection.Interval = 5000;
            tmrFCConnection.Start();
        }
		#endregion					

		#region public-методы
		#endregion

		#region public-методы реализации интерфейса IProviderCustomer
		/// <summary>
		/// событие появления данных на входе клиента(потребителя)
		/// </summary>
		public event ByteArrayPacketAppearance OnByteArrayPacketAppearance;

		/// <summary>
		/// посылка данных поставщику
		/// </summary>
		/// <param name="pq"></param>
		public void SendData(byte[] pq)
		{
			//Send(client, pq);
		}
		#endregion

		#region private-методы
		/// <summary>
		/// установка связи с сервером
		/// </summary>
        private void CreateConnect2Server(int port, string namesource) //XElement srcinfo
		{
			ReadCFGInfo(port, namesource);

			CreateConnection();
		}

		/// <summary>
		/// читаем настройки соединения из конф файла
		/// </summary>
        private void ReadCFGInfo(int port, string namesource)  //XElement srcinfo
		{
			try
			{
                udpserver_port = port;
                
    			// сформируем список с адресами фк, от которых ждем пакетов
				CreateListECUAdresses(namesource);
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
			}
		}

		/// <summary>
		/// Сформировать список адресов ФК
		/// от которых ждем пакеты
		/// </summary>
		/// <param name="srcinfo"></param>
        private void CreateListECUAdresses(string namesource)
		{
			try
			{
				// путь к файлу конфигурации источника
                string path2SrcPrgDevCfg = ProjectCommonData.GetPathTo_PrgDevCFG_cdp_File(namesource);

				if (!File.Exists(path2SrcPrgDevCfg))
					throw new Exception("(141) : ClientServerOn_MOA_UDPSockets.cs : CreateListECUAdresses() : Ошибка открытия файла : " + path2SrcPrgDevCfg);

				XDocument xdoc_SrcPrgDevCfg = XDocument.Load(path2SrcPrgDevCfg);

				var xe_xml_ecus = xdoc_SrcPrgDevCfg.Descendants("SourceECU");

				foreach( XElement xe_ecu in xe_xml_ecus )
				{
					dictECUIPAdresses.Add(xe_ecu.Attribute("fcadr").Value,int.Parse(xe_ecu.Attribute("numFC").Value) );
                    //dictECUIDNumPakets.Add(byte.Parse(xe_ecu.Attribute("numFC").Value), 0);
                    slECUIDNumPakets.Add(byte.Parse(xe_ecu.Attribute("numFC").Value), (UInt16)0);
				}
			}
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
		}

		/// <summary>
		/// создать соединение
		/// </summary>
		private void CreateConnection()
		{
			try
			{
				_listener = new ChatUdpListener((int)udpserver_port);
			}
			catch
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 110, "ClientServerOn_MOA_UDPSockets.cs : ClientServerOn_MOA_UDPSockets.CreateConnection() : Неправильный номер UDP-порта для получения данных от ФК: " + udpserver_port.ToString());
				
				return;
			}

			_listener.NewMessage += new EventHandler(_listener_NewMessage);
		}

		/// <summary>
		/// Обработчик события UdpListener.NewMessage -
		/// выделение пакета, ip-адрес в начало пакета, 
		/// пакет на обработку
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void _listener_NewMessage(object sender, EventArgs e)
		{
			byte[] ipadrinbyte;	// адрес фк от которого пришел пакет
			try
			{
				#region пакет от нужного ФК?
				adr.Length = 0;
				adr.Append((e as NewMessage).EndPoint.ToString());
				char[] delim = { ':' };

				string[] stdelim = (adr.ToString()).Split(delim);
				adr.Length = 0;
				adr.Append(stdelim[0]);

				IPAddress ipudpserver = IPAddress.Loopback;
				// сворачиваем адрес и в начало пакета - передаем на верхний уровень
				if (!IPAddress.TryParse(adr.ToString(), out ipudpserver))
				{
					TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 163, "ClientServerOn_MOA_UDPSockets.cs : ClientServerOn_MOA_UDPSockets.CreateConnection() : Неправильный ip-адрес: " + adr.ToString());
					return;
				}

				if (!dictECUIPAdresses.ContainsKey(ipudpserver.ToString()))
					return;
				else
					ipadrinbyte = ipudpserver.GetAddressBytes();
				#endregion

                // пакет наш - на обработку
                ProcessPakets((e as NewMessage).Message, ipadrinbyte);
			}
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}
		}

        private void bgw4DumpPackets_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string nf = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PacketIncommingDumps.dmp");
                using( FileStream fs = new FileStream(nf,FileMode.Create))
                {
                    lock (lstDumps)
                    {
                        BinaryWriter bw = new BinaryWriter(fs);
                        foreach( byte[] bt in lstDumps )
                            bw.Write(bt);

                        bw.Close();
                    }
                }

                // сбрасываем и инициализируем список заново
                lstDumps = new List<byte[]>();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        void ProcessPakets(byte[] dataNetRaw, byte[] ipaInBytes)
        {
            MemoryStream mspacket = new MemoryStream();
            BinaryWriter br = new BinaryWriter(mspacket);

            lock (lockKey)
            {
                try
                {
                    #region специально для ПС-304 и Вырицы
                    /* специально ПС-304  и Вырицы где один-два фк (№0,1) со старыми заголовками:
                 * подбрасываем 0 вместо 0x7a
                 */
                    // вычислим индекс, он и будет номером фк для подставы
                    byte index = 0xff;

                    if (dataNetRaw.Length == 20)
                        index = 0xff;


                    if (dataNetRaw[0] == 0x7a)
                    {
                        index = Convert.ToByte(dictECUIPAdresses[adr.ToString()]);
                        dataNetRaw[0] = index;
                    }
                    //else if (HMI_MT_Settings.HMI_Settings.IsPTK147Project)
                    //{
                    //    index = Convert.ToByte(dictECUIPAdresses[adr.ToString()]);
                    //    dataNetRaw[0] = index;
                    //}
                    else
                    {
                    }
                    #endregion

                    #region запись в дамп пакетов
                    if (ipaInBytes.Length > 0)
                        try
                        {
                            if (PacketRecordStatus)
                            {
                                if (!bgw4DumpPackets.IsBusy)
                                {
                                    // собираем пакеты
                                    br.Write(ipaInBytes);   // ip-адрес откуда пакет
                                    byte[] lenp = BitConverter.GetBytes((uint)(dataNetRaw.Length));
                                    br.Write(lenp);
                                    br.Write(dataNetRaw);
                                    mspacket.Position = 0;
                                    lstDumps.Add(mspacket.ToArray());
                                }
                                else
                                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 305, @"...\ProviderCustomerExchangeLib\ClientServerOn_MOA_UDPSockets.cs : ProcessPakets() : Поток записи дампа пакетов в файл занят.");
                            }
                            else
                            {
                                if (lstDumps.Count > 0 && !bgw4DumpPackets.IsBusy)
                                    // пакеты в файл в отдельном потоке
                                    bgw4DumpPackets.RunWorkerAsync();
                                else
                                    lstDumps.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                        }
                    
                    #endregion

                    // номер фк
                    byte nfc = dataNetRaw[0];

                    // инкремент числа пакетов по этому фк
                    //if (dictECUIDNumPakets.ContainsKey(nfc))
                    //    dictECUIDNumPakets[nfc] += 1;
                    if (slECUIDNumPakets.ContainsKey(nfc))
                        slECUIDNumPakets[nfc] = (UInt16)((UInt16)slECUIDNumPakets[nfc] + 1);

                    if (nfc == 3)
                        nfc = 3;
                    BinaryReader fin = new BinaryReader(new MemoryStream(dataNetRaw));
                    fin.BaseStream.Position = 0;
                    if (fin.BaseStream.Length == 0)
                        return;

                    // читаем последовательности из потока: читаем заголовок первой последовательности , потом ее пакеты.
                    // проверка номера протокола 0x7aa7 (для 304 и вырицы) и 0xnna7 (для более поздних проектов, nn - номер фк):
                    byte[] protocol = new byte[2];
                    protocol = fin.ReadBytes(2);

                    // время пакета и данных в нем - его мы бераем в качестве метки времени тегов
                    UInt32 m_uiTemp = fin.ReadUInt32();		// читаем количество секунд - по этому времени ФК осуществляем синхронизацию
                    byte uiTemp2 = fin.ReadByte();           // читаем количество сотых долей от начала секунды

                    // номер последовательности
                    ushort usNumSeq = fin.ReadByte();
                    ushort usNumMes = fin.ReadUInt16();	// номер сообщения

                    // далее можно читать пакеты из тела сообщения
                    ushort usLenPack;
                    do
                    {
                        // читаем длину очередного фрагмента
                        usLenPack = fin.ReadUInt16();	// число байт в очередном фрагменте исходного пакета

                        if (usLenPack == 0 || (fin.PeekChar() == -1))   // если длина пакета 0 или поток пустой, то выходим
                            break;

                        // сформируем номер устройства с учетом фк
                        ushort numdev = (ushort)fin.ReadInt16();
                        numdev = (ushort)(Convert.ToUInt16(nfc) * 256 + numdev);
                        // преобразовать в поток байт и в поток на место номера устройства
                        byte[] byte_numdev = new byte[2];
                        byte_numdev = BitConverter.GetBytes(numdev);
                        fin.BaseStream.Position -= 2;
                        fin.BaseStream.Write(byte_numdev, 0, 2);
                        fin.BaseStream.Position -= 4;

                        if (numdev == 519)
                            numdev = 519;

                        byte[] copyBlockDev = new byte[usLenPack + 5]; //5 - метка времени  lenpack
                        fin.Read(copyBlockDev, 0, usLenPack);	// читаем пакет lenpack в copyBlockDev
                        //fin.Read(copyBlockDev, usLenPack, 5);

                        // формируем метку времени в copyBlockDev
                        Buffer.BlockCopy(BitConverter.GetBytes(m_uiTemp), 0, copyBlockDev, usLenPack, 4);
                        copyBlockDev[usLenPack + 4] = uiTemp2;

                        // и ставим в очередь на обработку
                        NetPackQ.Add(copyBlockDev);

                    } while (true);

                    // даем команду обработать накопивлиеся пакеты в очереди
                    NetPackQ.ParsePackInQueque();
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
            }
        }
                
        /// <summary>
        /// формирование пакетов для 
        /// определения связи с ФК
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tmrFCConnection_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            tmrFCConnection.Stop();

            try
            {

                for (int i = 0; i < slECUIDNumPakets.Count; i++)
                {
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter bw = new BinaryWriter(ms);

                    byte key = (byte)slECUIDNumPakets.GetKey(i);

                    // Ном фк
                    bw.Write(key);
                    //a7
                    bw.Write((byte)0xa7);
                    
                    UInt32 sec = 0; //количество секунд
                    bw.Write(sec);
                    byte secpart = 0;   // количество сотых долей от начала секунды
                    bw.Write(secpart);
                    byte numSeq = 0;    // номер последовательности
                    bw.Write(numSeq);
                    UInt16 numMes = 0; 	// номер сообщения
                    bw.Write(numMes);

                    UInt16 lendata = 8; // длина фрагмента данных - 2б длина + 2б устр + 2б ключ + 2б длина тега
                    bw.Write(lendata);
                    UInt16 numdev = 0;  // номер устройства - фк
                    bw.Write(numdev);
                    UInt16 addrfragm = (UInt16)60013;
                    bw.Write(addrfragm);
                    bw.Write((UInt16)slECUIDNumPakets[key]);

                    // пишем 0 в конец чтобы выйти из цикла
                    UInt16 end = 0;
                    bw.Write(end);

                    ms.Position = 0;
                    
                    //if ((UInt16)slECUIDNumPakets[key] != 0)
                        ProcessPakets(ms.ToArray(), new byte[]{});

                    // очицаем счетчик
                    slECUIDNumPakets[key] = (UInt16)0;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            tmrFCConnection.Start();
        }

		/// <summary>
		/// закрыть соединение
		/// </summary>
		/// <param name="tc"></param>
		private void CloseConnection(Socket tc)
		{
		}

		private void Send(Socket client, byte[] byteData)
		{
			try
			{
				//client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
			}
			catch (Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
				//CloseConnection(client);
				//tmrReconnectTCPClientToTCPServer.Start();
			}
		}

		//void arrForReceiveData_packetAppearance(byte[] pq)
		//{
		//    try
		//    {
		//        if (OnByteArrayPacketAppearance != null)
		//            OnByteArrayPacketAppearance(pq);
		//    }
		//    catch (Exception ex)
		//    {
		//        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
		//    }
		//}
		#endregion
	}
}
