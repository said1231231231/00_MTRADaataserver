/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: класс поддержки работы wcf-точки подключения
 *                                                                             
 *	Файл                     : X:\Projects\38_DS4BlockingPrg\DataServerWPF\EntryPointLib\WcfDataServer.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 07.09.2011 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using CommonClasses;
using CommonClassesLib;
using DispatcherMessages;
using HMI_MT_Settings;
using InterfaceLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace WcfDataServer_Lib
{
    /* ServiceBehavior - особенности поведения сервиса:
 * ConfigurationName – задает имя соответствующей секции behavior в конфигурационном файле; 
 * ConcurrencyMode – устанавливает режим потоковой работы сервиса.
 *      В зависимости от значения данного свойства сервис может выполнять операции в однопоточном или многопоточном режимах.
 *      Свойство может принимать следующие значения из перечисления System.ServiceModel.ConcurrencyMode: 
 *          Single – значение по умолчанию, экземпляр сервиса работает в однопоточном режиме и не допускает обратных вызовов;
 *          Reentrant – сервис работает в однопоточном режиме, но допускает повторные вызовы;
 *          Multiple – сервис работает в многопоточном режиме, при этом механизмы WCF не утруждают себя синхронизацией,
 *                      поэтому работая в этом режиме не забывайте, при необходимости, программно обеспечивать синхронизацию потоков. 
 *  InstanceContextMode – определяет режим создания новых экземпляров сервиса. 
 *      В конечном счете, сервис представляет собой нестатический класс, который реализует контракты операций сервиса. 
 *      И, соответственно, перед тем как вызывать его методы, необходимо создать экземпляр данного класса. 
 *      Данное свойство определяет каким образом механизм WCF будет создавать новые экземпляры класса-сервиса,
 *      свойство может принимать следующие значения из перечисления System.ServiceModel.InstanceContextMode:
 *          PerSession – режим по умолчанию – для каждого сеанса(сессии) создается свой экземпляр сервиса
 *                      (когда клиент открывает соединение с сервисом), который используется для всех вызовов во время данного сеанса
 *                      и удаляется по его завершению; 
            PerCall – для каждого обращения клиента к сервису создается новый экземпляр класса-сервиса, который удаляется по завершению вызова;
 *          Single – все время работы сервиса будет существовать только один экземпляр класса-сервиса.
 *                  При первом обращении любого клиента создается новый экземпляр сервиса, 
 *                  который будет обрабатывать все последующие запросы и не удалиться никогда.
 *  IncludeExceptionDetailInFaults – логическое свойство, которое определяет,
 *     должны ли ошибки возникающие на стороне сервиса преобразовываться к типу System.ServiceModel.FaultException 
 *     и передаваться в ответном сообщении клиенту, который инициировал запрос. 
 *     По умолчанию данная функция отключена, ее следует включать в целях отладки сервиса;
 *  AutomaticSessionShutdown – логическое свойство, которое определяет должен ли сервис автоматически завершать сеанс,
 *      когда клиент закрывает соединение с сервисом.
 *      По умолчанию установлено значение true и сеанс сервиса существует, пока открыто соединение с клиентом.
 *      Если установить свойство AutomaticSessionShutdown в false – то автоматическое прерывание отключается 
 *      и можно контролировать время жизни сеанса в коде приложения сервиса; 
 */
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple/*.Single*/)]
    public class WcfDataServer : IWcfDataServer, IClientObserver
    {
        #region События
        #endregion

        #region Свойства
        #endregion

        #region public
        #endregion

        #region private
        /// <summary>
        /// объект блокировки
        /// </summary>
        static object lockKey = new object();
        /// <summary>
        /// словарь клиентов
        /// </summary>
        static Dictionary<string, WCFSession> clientsDictionary = new Dictionary<string, WCFSession>();
        /// <summary>
        /// идентификатор клиента
        /// </summary>
        uint clientID = 0xffffffff;
        /// <summary>
        /// идентификатор сеанса WCF
        /// </summary>
        string SessionId = string.Empty;
        StringBuilder sb = new StringBuilder();
        /// <summary>
        /// очередь вх запросов
        /// </summary>
        MessageQueque inputQueque;
        /// <summary>
        /// очередь выходных запросов (результатов)
        /// </summary>
        MessageQueque outputQueque;
        /// <summary>
        ///  поток для формирования результата
        /// </summary>
        MemoryStream msout = new MemoryStream();
        /// <summary>
        /// поток для формирования запроса из частей
        /// большого пакета
        /// </summary>
        MemoryStream msPartial = new MemoryStream();
        /// <summary>
        /// поток для запуска задания на получение тегов от DataServer
        /// </summary>
        BackgroundWorker bgw = new BackgroundWorker();
        /// <summary>
        /// примитив синхронизации
        /// </summary>
        ManualResetEvent mre = new ManualResetEvent(false);  
        /// <summary>
        /// список ошибок слоя 
        /// WCF-службы
        /// </summary> 
        CommonClasses.ListError listError;     
        /// <summary>
        /// объект для обратной 
        /// связи с клиентом
        /// </summary>
        IWcfDataServerCallback callbackEvent;        
        /// <summary>
        /// текущий контекст обмена
        /// </summary>
        OperationContext CurrentСontext;
        #endregion

        #region конструктор(ы) и инициализация
        public WcfDataServer()
        {
            try
            {
                //HMI_MT_Settings.HMI_Settings.DataServer = DataServer.DataServer.Iinstance;
                
                Debug.WriteLine("WcfDataServer()");

                if (HMI_MT_Settings.HMI_Settings.DataServer == null)
                    HMI_MT_Settings.HMI_Settings.DataServer = new DataServer.DataServer();

                clientID = HMI_MT_Settings.HMI_Settings.GetClientID();

                #region фиксация сессии
                //Get client callback channel
                CurrentСontext = OperationContext.Current;

                var sessionID = CurrentСontext.SessionId;
                //Utilities.LogConsole(" - Создание экземпляра службы.");
                var currClient = CurrentСontext.GetCallbackChannel<IWcfDataServerCallback>();

                //=============================================================================================
                WCFSession session;
                if (!clientsDictionary.TryGetValue(sessionID, out session))
                    lock (lockKey)
                    {
                        clientsDictionary[sessionID] = new WCFSession(currClient);                         
                        CurrentСontext.Channel.Faulted += Disconnect;
                        CurrentСontext.Channel.Closed += Disconnect;
                    }
                #endregion

                inputQueque = HMI_MT_Settings.HMI_Settings.InputMesSched.GetQueque(clientID);
                inputQueque.OnNewMesAppearance += HMI_MT_Settings.HMI_Settings.InputMesSched.inputQueque_OnNewMesAppearance;
                outputQueque = HMI_MT_Settings.HMI_Settings.outMesSched.GetQueque(clientID);
                outputQueque.OnNewMesAppearance += new NewMesAppearance(outputQueque_OnNewMesAppearance);

                bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);

                // инициализировали список ошибок
                listError = new ListError();
                listError.OnNewError += new NewError(listError_OnNewError);
                
                // регистрируем подписчика на изменение тегов
                HMI_MT_Settings.HMI_Settings.SubscribeObservable.RegisterObserve(CurrentСontext.SessionId, this);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// Разъединение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnect(object sender, EventArgs e)
        {
            lock (lockKey)
            {
                try
                {
                    var Context = CurrentСontext;
                    if (Context != null)
                    {
                        var sessionID = Context.SessionId;
                        clientsDictionary.Remove(sessionID);
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 184, string.Format("{0} : {1} : {2} : Клиент отсоединился.", DateTime.Now.ToString(), @"X:\Projects\00_DataServer\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs", "Disconnect()()"));
                    }
                    else
                    {
                        //Utilities.LogTrace(" - OperationContext.Current=null Видимо уже убили...");
                    }

                    // отписываем подписчика на изменение тегов
                    HMI_MT_Settings.HMI_Settings.SubscribeObservable.RemoveObserve(CurrentСontext.SessionId);
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 184, string.Format("{0} : {1} : {2} : ОШИБКА: {3}", DateTime.Now.ToString(), @"X:\Projects\00_DataServer\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs", "Disconnect()()", ex.Message));
                    //Utilities.LogTrace(" - Ошибка в Disconnect: " + ex.Message);
                }
            }
        }
        #endregion

        #region public-методы
        #endregion

        #region public-методы реализации интерфейса IClientObserver
        /// <summary>
        /// вызывается когда теги были изменены
        /// теперь их значения нужно передать DSR, который передаст их соответсвующему
        /// hmi-клиенту(ам)
        /// </summary>
        /// <param name="lsttags4update"></param>
        public void UpdateSubscribeTags(List<TagValue> lsttags4update)
        {
            DSTagValue dstv;
            Dictionary<string, DSTagValue> lstdstagvalue = new Dictionary<string,DSTagValue>();

            try
            {                
                foreach( TagValue tv in lsttags4update )
                {
                    dstv = new DSTagValue(tv.VarQuality, tv.VarValueAsObject);

                    if (dstv != null)
                        lstdstagvalue.Add(tv.IdTag, dstv);

                    if (tv.IdTag == "0.769.51")
                    {
                    }
                }

                if (lstdstagvalue.Count() > 0)
                    callbackEvent.NotifyChangedTags(lstdstagvalue);
                /*
                 * здесь недоработанный момент для случая когда DSR'ов несколько !!!!!!!!!!
                 */
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

        #region public-методы реализации интерфейса IWcfDataServer

        #region ручной ввод данных
        /// <summary>
        /// Установить значение тега 
        /// с уровня HMI через тип object
        /// (качество тега vqHandled)
        /// </summary>
        /// <param name="arrTagValue"></param>
        public void SetTagValueFromHMI(string idTag, object valinobject)
        {
            try
            {
                UInt16 UniDsGuid = 0;
                UInt32 LocObjectGuid = 0;
                UInt32 tagguid = 0;

                string[] strreq = idTag.Split(new char[]{'.'});
                // 2 байта - ds
                UniDsGuid = UInt16.Parse(strreq[0]);
                // 4 байта - LocObjectGuid
                LocObjectGuid = UInt32.Parse(strreq[1]);
                // TagGuid тега
                tagguid = UInt32.Parse(strreq[2]);

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    ITag tag = dev.GetTag(tagguid);
                    if (tag != null)
                        tag.SetTagValueFromHMI(valinobject);
                    else
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 299, string.Format("{0} : {1} : Попытка установки значения несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "SetTagValueFromHMI()"));
                }
                else
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 298, string.Format("{0} : {1} : Попытка установки тега несуществующего устройства .", @"\WcfDataServer_Lib\WcfDataServer.cs", "SetTagValueFromHMI()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// восстановить процесс естесвенного обновления тега
        /// (качество тега vqGood или по факту)
        /// </summary>        
        public void ReSetTagValueFromHMI(string idTag)
        {
            try
            {
                UInt16 UniDsGuid = 0;
                UInt32 LocObjectGuid = 0;
                UInt32 tagguid = 0;

                string[] strreq = idTag.Split(new char[] { '.' });
                // 2 байта - ds
                UniDsGuid = UInt16.Parse(strreq[0]);
                // 4 байта - LocObjectGuid
                LocObjectGuid = UInt32.Parse(strreq[1]);
                // TagGuid тега
                tagguid = UInt32.Parse(strreq[2]);

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    ITag tag = dev.GetTag(tagguid);
                    if (tag != null)
                        tag.ReSetTagValueFromHMI();
                    else
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 336, string.Format("{0} : {1} : Попытка сброса значения несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "SetTagValueFromHMI()"));
                }
                else
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 339, string.Format("{0} : {1} : Попытка сброса значения тега несуществующего устройства .", @"\WcfDataServer_Lib\WcfDataServer.cs", "SetTagValueFromHMI()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

        #region поддержка функционирования старого арма - обмен пакетами
        /// <summary>
        /// функция для принятия больших запросов
        /// с возможностью их соединения после разбиения на 
        /// стороне клиента
        /// </summary>
        /// <param name="arr">запрос</param>
        /// <param name="currentNumberPacket">номер текущего сегмента общего пакета</param>
        /// <param name="packetCount">число сегментов большого пакета</param>
        /// <returns></returns>
        public MemoryStream GetDSValueAsPartialByteBuffer(byte[] arr, int currentNumberPacket, int packetCount)
        {
            MemoryStream rez = new MemoryStream();
             
            try
            {
                if (currentNumberPacket == 1)
                    msPartial = new MemoryStream();

                // дописали
                msPartial.Write(arr, 0, arr.Length);

                if (currentNumberPacket == packetCount)
                    rez = GetDSValueAsByteBuffer(msPartial.ToArray());                
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// запрос данных в виде пакета - 
        /// это функция двойного назначения -
        /// она исп и для поддержания старого арма и 
        /// для работы нового списка обмена значениями
        /// в виде словаря
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public MemoryStream GetDSValueAsByteBuffer(byte[] arr)
        {

            msout = new MemoryStream();
            uint clientid = clientID;

            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 217, string.Format("{0} : Получен запрос {1} байт.", DateTime.Now.ToString(), arr.Length.ToString()));

            // расширяем пакет запроса
            byte[] arrWithLen = new byte[arr.Length + 4];

            Buffer.BlockCopy(BitConverter.GetBytes(clientID), 0, arrWithLen, 0, 4);
            Buffer.BlockCopy(arr, 0, arrWithLen, 4, arr.Length);

            try
            {
                // в отд потоке ставим сообщение во входную очередь данной точки доступа
                if (!bgw.IsBusy)
                    bgw.RunWorkerAsync(arrWithLen);
                else
                    return msout;

                mre.WaitOne();
                if (outputQueque.Count != 0)
                {
                    // убрать код клиента
                    MemoryStream mst = new MemoryStream(outputQueque.Dequeue());
                    byte[] arrmst = new byte[mst.Length - 4];
                    mst.Position = 0;

                    Buffer.BlockCopy(mst.ToArray(), 4, arrmst, 0, arrmst.Length);

                    msout = new MemoryStream(arrmst);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            mre.Reset();

            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 251, string.Format("{0} : Отправлено по запросу {1} байт.", DateTime.Now.ToString(),msout.Length.ToString()));

            return msout;
        }
        void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            uint clientid = clientID;

            try
            {
                inputQueque.Add(e.Argument as byte[]);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        void outputQueque_OnNewMesAppearance()
        {
            try
            {
                mre.Set();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// запрос осциллограммы - содедржимое
        /// запроса в структуре, скрытой в массиве байт
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        public MemoryStream GetDSOscByIdInBD(byte[] pq)
        {
            // потоки для ввода
            MemoryStream msin = new MemoryStream(pq);
            BinaryReader br = new BinaryReader(msin);
            // потоки для формирования результата
            MemoryStream rez = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(rez);
            byte codeError = 0;
            byte reqtype = 0;
            ushort id_correlation = 0;
            ushort UniDsGuid = 0;
            try
            {
                #region запрос осциллограммы (по номерам записей в БД)
                UInt32 idbl = 0;
                byte[] blockOscArchivData = new byte[] { };
                try
                {
                    try
                    {
                        reqtype = br.ReadByte();	//тип запроса
                        id_correlation = br.ReadUInt16();

                        // запрос к локальному DS
                        UniDsGuid = br.ReadUInt16();

                        // идентификатор блока с осциллограммой в БД
                        idbl = br.ReadUInt32();

                        // читаем строку подключения к БД
                        UInt16 len_str_cnt = br.ReadUInt16();
                        byte[] bstr_cnt = br.ReadBytes(len_str_cnt);

                        string str_cnt = Encoding.UTF8.GetString(bstr_cnt);
                        // вызов хранимой процедуры для получения блока архивных данных с заданным id_block
                        blockOscArchivData = DataBaseLib.DataBaseReq.GetBlockData(str_cnt, (int)idbl);

                        codeError = (byte)PacketParse_CodeError.CodeError_Ok;    //0;

                        if (blockOscArchivData.Length == 0)
                            throw new Exception("(353) : MessageHandler.cs : PacketParse() : Пакет с осциллограммой имеет нулевую длину");
                    }
                    catch (Exception ex)
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                        TraceSourceLib.TraceSourceDiagMes.WriteDump(TraceEventType.Error, 539, string.Format("ОШИБКА ОБМЕНА, тип пакета = {0}", reqtype.ToString()), msin);
                        codeError = (byte)PacketParse_CodeError.CodeError_NoTagGUID;    //1;
                    }

                    #region формируем обратный пакет с телом осциллограммы
                    // код клиента
                    //bw.Write(clientID);
                    // тип пакета 
                    bw.Write(reqtype);
                    // код ошибки
                    bw.Write(codeError);
                    // идентификатор коррелящии
                    bw.Write(id_correlation);
                    // уник номер DS
                    bw.Write(UniDsGuid);
                    // идентификатор блока
                    bw.Write(idbl);
                    // длина осциллограммы
                    bw.Write(blockOscArchivData.Length);
                    bw.Write(blockOscArchivData);
                    // результат в единую выходную очередь пакетов
                    rez.Position = 0;
                    #endregion
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                    throw ex;
                }
                #endregion
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            Console.WriteLine(string.Format("сформирован пакет с осциллограммой длиной {0} байт", rez.Length));
            return rez;
        }
        /// <summary>
        /// запрос на отображение архивной информации -
        /// запрос архивных данных (по номерам записей в БД) -
        /// уставки, аварии
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        public void SetReq2ArhivInfo (byte[] pq)            
        {
            // потоки для ввода
            MemoryStream msin = new MemoryStream(pq);
            BinaryReader br = new BinaryReader(msin);
            
            byte reqtype = 0;
            ushort id_correlation = 0;
            ushort UniDsGuid = 0;
            UInt32 LocObjectGuid = 0;

            try
            {
                byte[] blockOscArchivData = new byte[] { };
                        
                //тип запроса
                reqtype = br.ReadByte();

                id_correlation = br.ReadUInt16();

                // запрос к локальному DS
                UniDsGuid = br.ReadUInt16();

                // уник номер объекта в пределах конкретного DataServer
                LocObjectGuid = br.ReadUInt32();

                // номер группы или 0xffff(запрос на архивную запись) 
                UInt16 numgr = br.ReadUInt16();

                //if (numgr != 0xffff)
                //    throw new Exception(string.Format("(301) : MessageHandler.cs : PacketParse() : Некорректный формат запроса архивных данных numgr = {0}", numgr.ToString()));

                UInt32 id_block = br.ReadUInt32();
                // читаем строку подключения к БД
                UInt16 len_str_cnt = br.ReadUInt16();
                byte[] bstr_cnt = br.ReadBytes(len_str_cnt);

                string str_cnt = Encoding.UTF8.GetString(bstr_cnt);
                // вызов хранимой процедуры для получения блока архивных данных с заданным id_block
                byte[] blockArchivData = DataBaseLib.DataBaseReq.GetBlockData(str_cnt, (int)id_block);

                // отдаем устройству на разбор
                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    if (numgr == 0xffff || numgr == 0xfffe || numgr == 0xfffd) // запрашиваем у устройства номер группы с авар инф или уставками
                        numgr = dev.GetNumGroup4ReqNumber(numgr);

                    br.BaseStream.Position = 0;

                    dev.ParsePacketRawDataArchiv(clientID, id_block, blockArchivData);
                }
           }
           catch (Exception ex)
           {
               TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
               TraceSourceLib.TraceSourceDiagMes.WriteDump(TraceEventType.Error, 539, string.Format("ОШИБКА ОБМЕНА, тип пакета = {0}", reqtype.ToString()), msin);
               //codeError = (byte)PacketParse_CodeError.CodeError_NoTagGUID;    //1;
           }
        }
        /// <summary>
        /// выполнить команду
        /// </summary>
        /// <param name="pq"></param>
        /// <returns></returns>
        public MemoryStream RunCMDMOA(byte[] pq)
        {
            MemoryStream rez = new MemoryStream();

            // потоки для ввода
            MemoryStream msin = new MemoryStream(pq);
            BinaryReader br = new BinaryReader(msin);

            // потоки для формирования результата
            MemoryStream msout = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(msout);

            // для сбора инф по знач тегов
            MemoryStream mstagrez = new MemoryStream();
            BinaryWriter bwtagrez = new BinaryWriter(mstagrez);

            byte reqtype = 0;
            ushort id_correlation = 0;
            ushort UniDsGuid = 0;
            UInt32 LocObjectGuid = 0;
            // код ошибки
            byte codeError = (byte)PacketParse_CodeError.CodeError_Ok;    //0;

            try
            {
                #region запрос на выполнение команды
                string cmdName = string.Empty;
                try
                {
                    //тип запроса - формальность
                    reqtype = br.ReadByte();

                    id_correlation = br.ReadUInt16();

                    // запрос к локальному DS
                    UniDsGuid = br.ReadUInt16();

                    // уник номер объекта в пределах конкретного DataServer
                    LocObjectGuid = br.ReadUInt32();

                    // длина кода (имени) команды
                    UInt16 lenCmdCode = br.ReadUInt16();

                    // восстановим код (имя) команды в виде строки
                    byte[] arrInfo = br.ReadBytes(lenCmdCode);
                    System.Text.UTF8Encoding utf = new System.Text.UTF8Encoding();
                    cmdName = utf.GetString(arrInfo);

                    // длина доп параметров
                    UInt16 lenArrParams = br.ReadUInt16();

                    // доп параметры
                    byte[] arrParams = new byte[] { };
                    if (lenArrParams > 0)
                        arrParams = br.ReadBytes(lenArrParams);

                    // команда всем ECU или отдельному устройству
                    if (UniDsGuid == 0 && LocObjectGuid == 0)
                        RunCMD4AllECU(cmdName, arrParams);
                    else
                        // определяем устройство которому предназначена команда и передаем ее на выполнение
                        if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                        {
                            IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                            // создаем команду
                            CommandLib.FactoryCommand fc = new CommandLib.FactoryCommand();
                            ICommand cmd = fc.CreateCommand("ver_1");
                            ArrayList arrThr = new ArrayList();
                            arrThr.Add(dev);
                            arrThr.Add(UniDsGuid);
                            arrThr.Add(LocObjectGuid);
                            arrThr.Add(cmdName);
                            arrThr.Add(arrParams);
                            arrThr.Add(clientID);

                            cmd.Init(arrThr);
                            cmd.OnCmdExecuted += new CmdExecuted(cmd_OnCmdExecuted);

                            dev.AlgoritmCmdExecute.RunCMD(cmd);
                        }
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                    TraceSourceLib.TraceSourceDiagMes.WriteDump(TraceEventType.Error, 539, string.Format("ОШИБКА ОБМЕНА, тип пакета = {0}", reqtype.ToString()), msin);
                    codeError = (byte)CommandResult._1_FAIL_TRIGGERING;
                }

                #region готовим результат по выполнению команды
                if (codeError != (byte)CommandResult._0_SUCCESS_TRIGGERING)
                {
                    #region готовим результирующий пакет клиенту
                    try
                    {
                        // код клиента
                        bw.Write(clientID);
                        reqtype = 2;
                        bw.Write(reqtype);			// тип пакета 
                        codeError = Convert.ToByte(CommandResult._1_FAIL_TRIGGERING);
                        bw.Write(codeError);		// код ошибки
                        id_correlation = 0;
                        bw.Write(id_correlation);	// идентификатор коррелящии

                        // тело с рез-том команды
                        bwtagrez.Write(UniDsGuid);
                        bwtagrez.Write(LocObjectGuid);

                        // пакуем имя команды
                        byte[] bnamecmd = Encoding.UTF8.GetBytes(cmdName);
                        bwtagrez.Write(BitConverter.GetBytes(Convert.ToUInt16(bnamecmd.Length)));
                        bwtagrez.Write(bnamecmd);

                        mstagrez.Position = 0;
                        // результат
                        bw.Write(Convert.ToUInt16(bwtagrez.BaseStream.Length));
                        bw.Write(mstagrez.ToArray());

                        // результат в единую выходную очередь пакетов
                        msout.Position = 0;
                        //mainOutputMesQue.Add(msout.ToArray());
                    }
                    catch (Exception ex)
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                    }
                    #endregion
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return msout;
        }
        /// <summary>
        /// выдать команду всем ECU
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="arrParams"></param>
        private void RunCMD4AllECU(string cmdName, byte[] arrParams)
        {
            try
            {
                #region для всех фк выдаем команду TKD
                foreach (ISrcCfgManager iscm in HMI_MT_Settings.HMI_Settings.DataServer.SRC_mtdex)
                {
                    IConfiguration config = iscm.GetSourceConfiguration();
                    foreach (KeyValuePair<uint, IController> kvp in config.SlObjectConfiguration)
                    {
                        IController cntr = kvp.Value;
                        foreach (IDevice dev in cntr.GetEcuRTUs())
                            if (dev is uvs_MOA.ECU_MOA)
                            {
                                // создаем команду
                                CommandLib.FactoryCommand fc = new CommandLib.FactoryCommand();
                                InterfaceLibrary.ICommand cmd = (InterfaceLibrary.ICommand)fc.CreateCommand("ver_1");
                                ArrayList arrThr = new ArrayList();
                                arrThr.Add(dev);
                                arrThr.Add((UInt16)0);
                                arrThr.Add(dev.ObjectGUID);
                                arrThr.Add(cmdName);
                                arrThr.Add(arrParams);
                                arrThr.Add((uint)0xffffffff);   // tcpclient отсутствует

                                cmd.Init(arrThr);
                                cmd.OnCmdExecuted += new CmdExecuted(cmd_OnCmdExecuted);

                                dev.AlgoritmCmdExecute.RunCMD(cmd);
                            }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// реакция на завершение команды
        /// </summary>
        void cmd_OnCmdExecuted(ICommand cmd)
        {
            // потоки для формирования результата
            MemoryStream msout = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(msout);
            // для формирования тела с ответом на команду
            MemoryStream mstagrez = new MemoryStream();
            BinaryWriter bwtagrez = new BinaryWriter(mstagrez);

            /*
             * готовим пакет с результатом выполнения команды - 
             * вообще-то это нужно делать на уровне устройства 
             * или того компонента, который владее инф о процессе выполнения команды
             * Пока в качестве заглушки формируем пакет здесь
             */
            #region готовим результирующий пакет клиенту
            try
            {
                #region формирование результат для команд с АСУ ЛАЙТ
                if (HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.ContainsKey(cmd.idDSRouterSession))
                {
                    switch (cmd.ResultTriggering)
                    {
                        case CommandResult._4_FAIL_TIMEOUT:
                            HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[cmd.idDSRouterSession] = EnumerationCommandStates.cmdCancelAtDataServerByTimer;
                            break;
                        case CommandResult._1_FAIL_TRIGGERING:
                            HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[cmd.idDSRouterSession] = EnumerationCommandStates.cmdDiscardByDataServer;
                            break;
                        case CommandResult._0_SUCCESS_TRIGGERING:
                            HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[cmd.idDSRouterSession] = EnumerationCommandStates.complete;
                            break;
                        default:
                            break;
                    }
                    // удаляем команду из словаря-очереди и ставим статус выполнения
                    RemoveCMDFromQueue(cmd.idDSRouterSession);
                }
                #endregion

                // код клиента
                //bw.Write(cmd.TCPClientID);
                byte reqtype = 2;
                bw.Write(reqtype);			// тип пакета 
                byte codeError = Convert.ToByte((int)cmd.ResultTriggering);
                bw.Write(codeError);		// код ошибки
                UInt16 id_correlation = 0;
                bw.Write(id_correlation);	// идентификатор коррелящии

                // тело с рез-том команды
                bwtagrez.Write(cmd.DS);
                bwtagrez.Write(cmd.ObjUni);

                // пакуем имя команды
                byte[] bnamecmd = Encoding.UTF8.GetBytes(cmd.CmdName);
                bwtagrez.Write(BitConverter.GetBytes(Convert.ToUInt16(bnamecmd.Length)));
                bwtagrez.Write(bnamecmd);
                bwtagrez.Write(BitConverter.GetBytes(Convert.ToUInt16(cmd.OutParams.Length)));
                bwtagrez.Write(cmd.OutParams);

                mstagrez.Position = 0;
                // результат
                bw.Write(Convert.ToUInt16(bwtagrez.BaseStream.Length));	// длина рез-та выборки значений тегов
                bw.Write(mstagrez.ToArray());

                // результат в единую выходную очередь пакетов
                msout.Position = 0;
                callbackEvent.NotifyCMDExecuted(msout.ToArray());
                //mainOutputMesQue.Add(msout.ToArray());
                //mainOutputMesQue.Add(msout.ToArray());
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            #endregion
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <param name="numksdu">ds</param>
        /// <param name="numvtu">rtuguid</param>
        /// <param name="tagguid">tagguid команды</param>
        /// <param name="arr">опция - массив параметров</param>
        /// <returns>успешность запуска команды на выполнение</returns>
        public void RunCMD(UInt16 numksdu, uint numvtu, uint tagguid, byte[] arr, string idDSRouterSession)
        {
            try
            {
                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey(numvtu))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[numvtu];
                    // создаем команду
                    CommandLib.FactoryCommand fc = new CommandLib.FactoryCommand();
                    ICommand cmd = fc.CreateCommand("ver_1");
                    ArrayList arrThr = new ArrayList();
                    arrThr.Add(dev);
                    arrThr.Add(numksdu);
                    arrThr.Add(numvtu);
                    arrThr.Add(tagguid);//cmdName
                    arrThr.Add(arr);
                    arrThr.Add(clientID);

                    cmd.Init(arrThr);
                    cmd.idDSRouterSession = idDSRouterSession;
                    cmd.OnCmdExecuted += new CmdExecuted(cmd_OnCmdExecuted);

                    // обновим состояние команды как активное
                    if (HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.ContainsKey(idDSRouterSession))
                        HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[idDSRouterSession] = EnumerationCommandStates.cmdactive;

                    dev.AlgoritmCmdExecute.RunCMD(cmd);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                // ставим статус
                if (HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.ContainsKey(idDSRouterSession))
                    HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[idDSRouterSession] = EnumerationCommandStates.cmdDiscardByDataServer;
                // и удляем из очереди
                if (dictCMDQueue.ContainsKey(idDSRouterSession))
                    dictCMDQueue.Remove(idDSRouterSession);
            }
        }
        #endregion

        #region информация об DataServer
        /// <summary>
        /// уникальный идентификатор DataServer'a DSGuid
        /// </summary>
        /// <returns></returns>
        public string GetDSGUID()
        {
            //#region фиксация сессии
            ////Get client callback channel
            //var context = OperationContext.Current;
            //var sessionID = context.SessionId;
            ////Utilities.LogConsole(" - Создание экземпляра службы.");
            //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

            //context.Channel.Faulted += Disconnect;
            //context.Channel.Closed += Disconnect;
            ////=============================================================================================

            ////=============================================================================================
            //WCFSession session;
            //if (!clientsDictionary.TryGetValue(sessionID, out session))
            //    lock (lockKey)
            //        clientsDictionary[sessionID] = new WCFSession(currClient);
            //#endregion

            /*
             * пока DS один возвращаем 0 - 
             * признак локальности
             */
            string dsguid = string.Empty;
            try
            {
                dsguid = HMI_MT_Settings.HMI_Settings.DataServer.DSGuid;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                listError.CreateError(1002);
            }
            return dsguid;
        }
        /// <summary>
        /// информация об имени DataServer конкретного DataServer: 
        /// возвращает имя проекта, которое извлекается из файла
        /// …\Project\Project.cfg xml-секция Project|NamePTK
        /// </summary>
        /// <returns></returns>
        public string GetDSINFO()
        {            
            string dsname = string.Empty;
            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                dsname = HMI_MT_Settings.HMI_Settings.DataServer.DataServerName;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                listError.CreateError(1003);
            }
            return dsname;
        }
        /// <summary>
        /// информация о конфигурации DataServer
        /// в виде файла
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <returns></returns>
        public Stream GetDSConfigFile()
        {
            Stream fs = null;
            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                CommonClasses.DataServerConfigFile dscfg = new CommonClasses.DataServerConfigFile();

                fs = dscfg.GetConfigurationFileAsStream();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return fs;
        }
        #endregion

        #region информация об источниках конкретного DataServer
        /// <summary>
        /// список идентификаторов источников 
        /// указанного DataServer
        /// в формате SrcGuid; SrcGuid; SrcGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <returns></returns>
        public string GetSourceGUIDs()
        {
            /*
             * пока DS один возвращаем 
             * источники текущего DS
             */

            StringBuilder sbrez = new StringBuilder();
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                List<ISrcCfgManager> lstsrc = HMI_MT_Settings.HMI_Settings.DataServer.SRC_mtdex;

                foreach (ISrcCfgManager src in lstsrc)
                    sbrez.Append(src.SrcId + ';');
                // удалим последний символ
                rez = sbrez.ToString();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez.Remove(rez.Count() - 1);
        }
        /// <summary>
        /// возвращает имя источника SrcGuid
        /// DataServer DSGuid
        /// </summary>
        /// <param name="SrcGuid"></param>
        /// <returns></returns>
        public string GetSourceName(UInt16 SrcGuid)
        {
            /*
             * пока DS один возвращаем 
             * имя требуемого источники 
             * в текущем DS
             */
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                ISrcCfgManager src = GetSrcBySrcGuid(SrcGuid);

                if (src == null)
                    throw new Exception(string.Format(@"(475) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetSourceName() : источник не найден SrcGuid = {0}.", SrcGuid));

                rez = src.GetSrcName().Trim();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        #endregion

        #region информация о контроллерах конкретного источника конкретного DataServer 
        /// <summary>
        /// список идентификаторов контроллеров источника SrcGuid 
        /// DataServer DSGuid в формате EcuGuid; EcuGuid; EcuGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <returns></returns>
        public string GetECUGUIDs(UInt16 SrcGuid)
        {
            string rez = string.Empty;
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                ISrcCfgManager src = GetSrcBySrcGuid(SrcGuid);

                if (src == null)
                    throw new Exception(string.Format(@"(503) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetECUGUIDs() : источник не найден SrcGuid = {0}.", SrcGuid));

                foreach (KeyValuePair<uint,IController> item in src.GetSourceConfiguration().SlObjectConfiguration)
                    sbrez.Append(item.Value.GetECUNumber().ToString().Trim() + ';');

                // удалим последний символ
                rez = sbrez.ToString();
                rez = rez.Remove(rez.Count() - 1);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// возвращает имя контрлллера EcuGuid 
        /// источника SrcGuid для  DataServer DSGuid.
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <returns></returns> 
        public string GetECUName(UInt16 SrcGuid, UInt16 EcuGuid)
        {
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IController ecu = GetECUByECUGuid(SrcGuid,EcuGuid);

                if (ecu == null)
                    throw new Exception(string.Format(@"(538) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetSourceName() : источник не найден SrcGuid = {0}; EcuGUID = {1}.",SrcGuid, EcuGuid));

                rez = (ecu as IDevice).GetDeviceDescription().Trim();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        } 
	    #endregion

        #region информация об устройствах контроллера ECU источника SrcGuid DataServer DSGuid
        /// <summary>
        /// список идентификаторов устройств 
        /// контроллера EcuGuid источника SrcGuid 
        /// DataServer DSGuid 
        /// в формате RtuGuid; RtuGuid; RtuGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <returns></returns>
        public string GetSrcEcuRTUGUIDs(UInt16 SrcGuid, UInt16 EcuGuid)
        {
            string rez = string.Empty;
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IController ecu = GetECUByECUGuid(SrcGuid, EcuGuid);

                if (ecu == null)
                    throw new Exception(string.Format(@"(567) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUGUIDs() : контрлооер не найден SrcGuid = {0}; EcuGuid = {1}.", SrcGuid, EcuGuid));

                foreach (IDevice item in ecu.GetEcuRTUs())
                    sbrez.Append(item.ObjectGUID.ToString().Trim() + ';');

                // удалим последний символ
                rez = sbrez.ToString();
                rez = rez.Remove(rez.Count() - 1);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// список идентификаторов устройств 
        /// контроллера EcuGuid источника SrcGuid 
        /// DataServer DSGuid 
        /// в формате RtuGuid; RtuGuid; RtuGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <returns></returns>
        public string GetRTUGUIDs()
        {
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                List<ISrcCfgManager> lstsrc = HMI_MT_Settings.HMI_Settings.DataServer.SRC_mtdex;

                foreach (ISrcCfgManager src in lstsrc)
                {
                    IConfiguration srcconfig = src.GetSourceConfiguration();
                    foreach (KeyValuePair<uint, IController> kvp in srcconfig.SlObjectConfiguration)
                    {
                        foreach (IDevice item in kvp.Value.GetEcuRTUs())
                            sbrez.Append(item.ObjectGUID.ToString().Trim() + ';');
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            // удалим последний символ
            return sbrez.ToString().Remove(sbrez.Length - 1);

        }
        /// <summary>
        /// имя типа устройства
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        public string GetRTUTypeName(UInt32 RtuGuid)
        {
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                if (rtu == null)
                    throw new Exception(string.Format(@"(599) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUTypeName() : устройство не найдено RtuGuid = {0}.", RtuGuid));

                rez = rtu.GetDeviceTypeName().Trim();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// строка описания устройства
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        public string GetRTUDescription(UInt32 RtuGuid)
        {
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                if (rtu == null)
                    throw new Exception(string.Format(@"(599) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUTypeName() : устройство не найдено RtuGuid = {0}.", RtuGuid));

                rez = rtu.GetDeviceDescription().Trim();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// признак доступности устройства для обработки
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        public bool IsRTUEnable(UInt32 RtuGuid)
        {
            bool rez = false;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                if (rtu == null)
                    throw new Exception(string.Format(@"(599) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUTypeName() : устройство не найдено RtuGuid = {0}.", RtuGuid));

                rez = rtu.IsDeviceEnable();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        #endregion

        #region информация о группах устройства RtuGuid контроллера ECU источника SrcGuid DataServer DSGuid
        /// <summary>
        /// список групп первого уровня в формате
        /// GroupGuid; GroupGuid; GroupGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        public string GetGroupGUIDs(UInt32 RtuGuid)
        {
            string rez = string.Empty;
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                if (rtu == null)
                    throw new Exception(string.Format(@"(657) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetGroupGUIDs() : устройство не найдено RtuGuid = {0}.",  RtuGuid));

                foreach (IGroup item in rtu.GetGroupHierarchy())
                    sbrez.Append(item.GroupGUID.ToString() + ';');

                // удалим последний символ
                rez = sbrez.ToString();
                rez = rez.Remove(rez.Count() - 1);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// список подгрупп группы(подгруппы) (Sub)GroupGuid
        /// в формате SubGroupGuid; SubGroupGuid; SubGroupGuid…
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
        public string GetSubGroupGUIDsInGroup(UInt32 RtuGuid, UInt32 GroupGuid)
        {
            string rez = string.Empty;
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IGroup sgr = GetGroupByGroupGuid(RtuGuid,GroupGuid);

                foreach (IGroup item in sgr.SubGroupsList)
                    sbrez.Append(item.GroupGUID.ToString() + ';');

                if (sbrez.Length > 0)
                {
                    // удалим последний символ
                    rez = sbrez.ToString();
                    rez = rez.Remove(rez.Count() - 1);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// имя группы GroupGuid устройства RtuGuid
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
        public string GetRTUGroupName(UInt32 RtuGuid, UInt32 GroupGuid)
        {
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IGroup group = GetGroupByGroupGuid(RtuGuid, GroupGuid);

                if (group == null)
                    throw new Exception(string.Format(@"(725) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUGroupName() : групп не найдена RtuGuid = {0}; GroupGuid = {1}.", RtuGuid, GroupGuid));

                rez = group.NameGroup.Trim();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// признак доступности группы для обработки
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        public bool IsGroupEnable(UInt32 RtuGuid, UInt32 GroupGuid) 
        {
            bool rez = false;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IGroup group = GetGroupByGroupGuid(RtuGuid, GroupGuid);

                if (group == null)
                    throw new Exception(string.Format(@"(779) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: IsGroupEnable() : группа устройства не найдена RtuGuid = {0}; group = {1}.", RtuGuid, group));

                rez = group.GroupEnable;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        #endregion

        #region информация о тегах устройства RtuGuid контроллера ECU источника SrcGuid DataServer DSGuid
        /// <summary>
        /// список всех тегов устройства в формате TagGuid; TagGuid; TagGuid… 
        /// Список может быть очень большим, поэтому доступ к тегам можно получить
        /// через иерархию групп с помощью следующей функции:
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        public string GetRtuTagGUIDs(UInt32 RtuGuid)
        {
            string rez = string.Empty;
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                if (rtu == null)
                    throw new Exception(string.Format(@"(657) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetGroupGUIDs() : устройство не найдено RtuGuid = {0}.", RtuGuid));

                foreach (ITag item in rtu.GetRtuTags())
                    sbrez.Append(item.TagGUIDVar.ToString() + ';');

                // удалим последний символ
                rez = sbrez.ToString();
                rez = rez.Remove(rez.Count() - 1);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// список тегов группы GroupGuid устройства
        /// RtuGuid в формате TagGuid; TagGuid; TagGuid… 
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <returns></returns>
        public string GetRtuGroupTagGUIDs(UInt32 RtuGuid, UInt32 GroupGuid)
        {
            string rez = string.Empty;
            StringBuilder sbrez = new StringBuilder();

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                IGroup sgr = GetGroupByGroupGuid(RtuGuid, GroupGuid);

                foreach (string item in sgr.SubGroupTagsList)
                {
                    sbrez.Append(item.ToString() + ';');
                }

                if (sbrez.Length > 0)
                {
                    // удалим последний символ
                    rez = sbrez.ToString();
                    rez = rez.Remove(rez.Count() - 1);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// информация о имени и типе тега в формате
        /// имя_тега;тип_тега
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <param name="GroupGuid"></param>
        /// <param name="TagGUID"></param>
        /// <returns></returns>
        public string GetRTUTagName(UInt32 RtuGuid, UInt32 TagGUID)
        {
            string rez = string.Empty;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                ITag tag = GetTagByTagGuid(RtuGuid, TagGUID);

                if (tag == null)
                    throw new Exception(string.Format(@"(883) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUTagName() : тег не найден RtuGuid = {0}; TagGUID = {1}.",RtuGuid, TagGUID));
                Type t = tag.GetType();
                rez = string.Format("{0};{1}", tag.NameVar, t.Name);   // tag.TypeVar);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        public byte[] GetRTUTagsValue (UInt32 RtuGuid, byte[]request)
        {
            byte[] rez = new byte[]{};

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                /*
                 * Формируем пакет для разборки в X:\Projects\00_DataServer\DataServer\MessageHandler.cs - 
                 * на входе у нас послед TagGUID'ов, на выходе пакет в формате предыдущего варианта DS.
                 * 
                 * Функция GetDSValueAsByteBuffer форрмирует ответный пакет на запрос значений тегов:
                 * 1 байт – тип формата ответа. Для массива с ответом на запрос тегов этот байт должен иметь значение 1.
                 * 1 байт – общий код ошибки ответа 
                 * 2 байта – резерв
                 * 2 байта – общая длина ответа (может есть смысл перейти на 4 байта чтобы передавать пакеты более 65 Кб?). Убирать
                 *      это поле не стоит чтобы сохранить единый механизм обработки пакетов для разных протоколов обмена
                 *      
                 * Далее для каждого тега:
                 * 2 байта (UInt16)   уник номер DataServer от которого получен тег
                 * 4 байта (UInt32) - уник номер объекта(устройства) в пределах конкретного DataServer
                 * 4 байта (UInt32) - TagGUID;
                 * 2 байта – резерв;
                 * 2 байта (UInt16) - длина байтового массива отдельного тега. Тоже можно обойтись, но целесообразно оставить 
                 *      для совместимости и доп. контроля целостности формата пакета.
                 *      N байт – значение тега, длина значения тега тега определяется по его типу:
                 *      TagAnalog – 4 байта
                 *      TagDiscret – 1 байт
                 *      TagDateTime – 8 байт
                 *      TagEnum – 4 байта (Single)
                 *      TagString –строка в виде массива байт. длина строки массива байт указывается в предществующем коде.
                 * 1 байт - качество_тега
                 * 8 байт (Int64) – метка времени
                 */
                MemoryStream mstagpacket = new MemoryStream();
                BinaryWriter bwmstagpacket = new BinaryWriter(mstagpacket);

                MemoryStream msout = new MemoryStream();
                BinaryWriter bwout = new BinaryWriter(msout);

                MemoryStream ms = new MemoryStream(request);
                BinaryReader br = new BinaryReader(ms);

                UInt16 UniDsGuid = 0;
                UInt32 LocObjectGuid = 0;
                UInt16 grGuid = 0;
                UInt32 tagguid = 0;

                // 1 байт - byte тип пакета - 1
                byte packettype = 1;
                bwout.Write(packettype);
                // 2 байта - корреляция
                UInt16 id_correlation = 0;
                bwout.Write(id_correlation);

                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    // 2 байта - ds
                    UniDsGuid = 0;  // 0 - локальный DS
                    bwmstagpacket.Write(UniDsGuid);
                    // 4 байта - LocObjectGuid
                    LocObjectGuid = RtuGuid;
                    bwmstagpacket.Write(LocObjectGuid);
                    // 2 байта - UInt16 - grGuid
                    bwmstagpacket.Write(grGuid);
                    // TagGuid тега
                    tagguid = br.ReadUInt32();
                    bwmstagpacket.Write(tagguid);
                }

                // длина запросов послед тегов
                UInt16 reqlen = Convert.ToUInt16(bwmstagpacket.BaseStream.Length);
                bwout.Write(reqlen);
                mstagpacket.Position = 0;
                bwout.Write(mstagpacket.ToArray());

                rez = GetDSValueAsByteBuffer(msout.ToArray()).ToArray();
                                
                //inputQueque.Add(ms.ToArray());

                //ITag tag = GetTagByTagGuid(DSGuid, SrcGuid, EcuGuid, RtuGuid, TagGUID);

                //if (tag == null)
                //    throw new Exception(string.Format(@"(883) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUTagName() : тег не найден DSGuid = {0}; SrcGuid = {1}; EcuGuid = {2}; RtuGuid = {3}; TagGUID = {4}.", DSGuid, SrcGuid, EcuGuid, RtuGuid, TagGUID));
                //Type t = tag.GetType();
                //rez = string.Format("{0}({1})", tag.NameVar, t.Name);   // tag.TypeVar);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        ///// <summary>
        ///// список тегов группы GroupGuid устройства
        ///// RtuGuid в формате TagGuid; TagGuid; TagGuid… 
        ///// </summary>
        ///// <param name="DSGuid"></param>
        ///// <param name="SrcGuid"></param>
        ///// <param name="EcuGuid"></param>
        ///// <param name="RtuGuid"></param>
        ///// <param name="GroupGuid"></param>
        ///// <param name="TagGuid"></param>
        ///// <returns></returns>
        //public bool IsTagEnable(string DSGuid, string SrcGuid, string EcuGuid, string RtuGuid, string GroupGuid, string TagGUID)
        //{
        //    bool rez = false;

        //    try
        //    {
        //        ITag tag = GetTagByTagGuid(DSGuid, SrcGuid, EcuGuid, RtuGuid, TagGUID);

        //        if (tag == null)
        //            throw new Exception(string.Format(@"(883) ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs: GetRTUTagName() : тег не найден DSGuid = {0}; SrcGuid = {1}; EcuGuid = {2}; RtuGuid = {3}; TagGUID = {4}.", DSGuid, SrcGuid, EcuGuid, RtuGuid, TagGUID));

        //        rez = tag.e;
        //    }
        //    catch (Exception ex)
        //    {
        //        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
        //    }
        //    return rez;

        //}
        #endregion

        #region Dictionary<string, DSTagValue> GetTagsValue() + подписка отписка

        /// <summary>
        /// вернуть значения тегов в виде словаря 
        /// </summary>
        public Dictionary<string, DSTagValue> GetTagsValue(List<string> request)//Dictionary<string, object> request
        {
            Dictionary<string, DSTagValue> rez = new Dictionary<string, DSTagValue>();
            byte[] rez_prelim = new byte[] { };

            try
            {
                #region - старый код - фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                #endregion

                /*
                 * Формируем результат запроса тегов от клиента по следующему принципу:
                 * на вход поступает список в виде ds.dev.tagguid,ds.dev.tagguid,...
                 * этот список преобразуется в пакет для разборки в X:\Projects\00_DataServer\DataServer\MessageHandler.cs - 
                 * на выходе пакет в формате предыдущего варианта DS.
                 * 
                 * Функция GetDSValueAsByteBuffer форрмирует ответный пакет на запрос значений тегов:
                 * 1 байт – тип формата ответа. Для массива с ответом на запрос тегов этот байт должен иметь значение 1.
                 * 1 байт – общий код ошибки ответа 
                 * 2 байта – резерв
                 * 2 байта – общая длина ответа (может есть смысл перейти на 4 байта чтобы передавать пакеты более 65 Кб?). Убирать
                 *      это поле не стоит чтобы сохранить единый механизм обработки пакетов для разных протоколов обмена
                 *      
                 * Далее для каждого тега:
                 * 2 байта (UInt16)   уник номер DataServer от которого получен тег
                 * 4 байта (UInt32) - уник номер объекта(устройства) в пределах конкретного DataServer
                 * 4 байта (UInt32) - TagGUID;
                 * 2 байта – резерв или 2 байта (UInt16) - длина байтового массива отдельного тега. без нее можно 
                 *              обойтись, но целесообразно оставить для совместимости и доп. контроля целостности формата пакета.
                 * N байт – значение тега, длина значения тега тега определяется по его типу:
                 *      TagAnalog – 4 байта
                 *      TagDiscret – 1 байт
                 *      TagDateTime – 8 байт
                 *      TagEnum – 4 байта (Single)
                 *      TagString –строка в виде массива байт. длина строки массива байт указывается в предществующем коде.
                 * 1 байт - качество_тега
                 * 8 байт (Int64) – метка времени
                 */

                #region Альтернативная реализация (???)

                //var result = new Dictionary<string, DSTagValue>();
                //foreach (var reqTag in request)
                //{
                //    try
                //    {
                //        var c = reqTag.Split('.');

                //        var devGuid = UInt32.Parse(c[1]);
                //        var tagGuid = UInt32.Parse(c[2]);

                //        // получаем ссылку на устройство
                //        var device = HMI_Settings.DataServer.GetRTUById(devGuid);
                //        if (device == null)
                //        {
                //            result.Add(reqTag, new DSTagValue { VarQuality = (uint)VarQuality.vqNonExistDevice, VarValueAsObject = null});
                //            continue;
                //        }

                //        // Получаем ссылку на тег
                //        var tag = device.GetTag(tagGuid);
                //        if (tag == null)
                //        {
                //            result.Add(reqTag, new DSTagValue { VarQuality = (uint)VarQuality.vqUknownTag, VarValueAsObject = null });
                //            continue;
                //        }

                //        result.Add(reqTag, new DSTagValue { VarQuality = (uint)tag.DaataQualityAsEnum, VarValueAsObject = tag.ValueAsObject });
                //    }
                //    catch (FormatException ex)
                //    {
                //        result.Add(reqTag, new DSTagValue { VarQuality = (uint)VarQuality.vqUknownError, VarValueAsObject = null });
                //    }
                //    catch(Exception ex)
                //    {
                //        // ??
                //    }
                //}

                //return result;

                #endregion

                MemoryStream mstagpacket = new MemoryStream();
                BinaryWriter bwmstagpacket = new BinaryWriter(mstagpacket);

                MemoryStream msout = new MemoryStream();
                BinaryWriter bwout = new BinaryWriter(msout);

                // 1 байт - byte тип пакета - 1
                byte packettype = 1;
                bwout.Write(packettype);
                // 2 байта - корреляция
                UInt16 id_correlation = 0;
                bwout.Write(id_correlation);                                

                foreach (var reqtag in request)
                {
                    // проверим строку идентификации на корректность
                    if (!Regex.IsMatch(reqtag, @"\A[0-9]+\.[0-9]+\.[0-9]+\Z"))
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 2042, DateTime.Now.ToString() + @" (2041 (=== ОШИБКА ===) ): ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs : GetTagsValue() : Некорректнаая строка запроса тега : " + reqtag);
                        continue;
                    }

                    string[] strreq = reqtag.Split(new char[]{'.'});

                    if (strreq.Length != 3)
                    {
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 1101, DateTime.Now.ToString() + @" (1101 (=== ОШИБКА ===) ): ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs : GetTagsValue() : Некорректнаая строка запроса тега : " + reqtag);
                        continue;
                    }

                    #region часть заглушки
                    //// формируем элемент выходного словаря
                    //DSTagValue dstv = new DSTagValue();
                    //dstv.VarQuality = 1;
                    //dstv.VarValueAsObject = 1;

                    //rez.Add(reqtag, dstv); 
                    #endregion

                    UInt16 UniDsGuid = 0;
                    UInt32 LocObjectGuid = 0;
                    UInt16 grGuid = 0;
                    UInt32 tagguid = 0;

                    // 2 байта - ds
                    UniDsGuid = UInt16.Parse(strreq[0]);
                    bwmstagpacket.Write(UniDsGuid);
                    // 4 байта - LocObjectGuid
                    LocObjectGuid = UInt32.Parse(strreq[1]);
                    bwmstagpacket.Write(LocObjectGuid);
                    // 2 байта - UInt16 - grGuid
                    bwmstagpacket.Write(grGuid);
                    // TagGuid тега
                    tagguid = UInt32.Parse(strreq[2]);
                    bwmstagpacket.Write(tagguid);
                }

                // длина запросов послед тегов
                UInt16 reqlen = Convert.ToUInt16(bwmstagpacket.BaseStream.Length);
                bwout.Write(reqlen);
                mstagpacket.Position = 0;
                bwout.Write(mstagpacket.ToArray());

                rez_prelim = GetDSValueAsByteBuffer(msout.ToArray()).ToArray();

                ///*
                // * разбираем пакет с результатами запроса тегов - 
                // * формируем словарь
                // */
                rez = CreateDictionaryFromBytePaket(rez_prelim);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 1930, DateTime.Now.ToString() + @" : ...\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs : GetTagsValue() : Отправлено по разовому запросу клиента {0} тегов: " + rez.Count());
            
            return rez;
        }

        /// <summary>
        /// создать словарь на основе двоичного 
        /// пакета с результатами выполнения запроса
        /// значений тегов
        /// </summary>
        private Dictionary<string, DSTagValue> CreateDictionaryFromBytePaket(byte[] rez_prelim)
        {
            MemoryStream msrez = new MemoryStream(rez_prelim);
            BinaryReader br = new BinaryReader(msrez);
            Dictionary<string, DSTagValue> dictList = new Dictionary<string, DSTagValue>();
            object  val_object = null;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                byte type_response = br.ReadByte();
                byte codeerror_gentral = br.ReadByte();
                UInt16 reserve_01 = br.ReadUInt16();
                UInt16 length_response = br.ReadUInt16();
                StringBuilder sbkey = new StringBuilder();

                while( br.BaseStream.Position < br.BaseStream.Length )
                {
                    UInt16 unids = br.ReadUInt16();
                    UInt32 unidev = br.ReadUInt32();
                    // уник номер группы-подгруппы
                    UInt16 grGuid = br.ReadUInt16();
                    UInt32 unitag = br.ReadUInt32();
                    UInt16 lenTag = br.ReadUInt16();
                    // значение тега
                    byte[] tagInBytes = br.ReadBytes(lenTag);
                    byte[] timestmp = br.ReadBytes(8);
                    byte quality = br.ReadByte();

                    sbkey.Clear();
                    sbkey.Append(string.Format("{0}.{1}.{2}", unids,unidev,unitag));
                    if (unidev == 542 && unitag == 500)
                    { 
                    }
                    string tagtype = "uknown";  // для несуществующего тега
                    VarQuality vggg = (VarQuality) Enum.Parse(typeof(VarQuality), quality.ToString());
                    /*
                     * по качесву тега смотрим особые случай
                     */
                    /*
                     * из файла InterfaceLibraryCFG\Enums_Delegats_Structs.cs
                        /// <summary>
                        ///	Признак качества переменной 
                        /// </summary> 
                        public enum VarQuality
                        {
                            vqUndefined = 0,        // Не определено (не производилось ни одного чтения, нет связи)
                            vqGood = 1,             // Хорошее качество
                            vqArhiv = 2,            // архивная переменная (из БД)
                            vqRangeError = 3,       // Выход за пределы диапазона
                            vqHandled = 4,          // Ручной ввод данных
                            vqUknownTag = 5,           // несуществующий тег (? что значит не существующий тег - м.б. это может исп. в ответах на запросы когда запрашивается тег кот. нет, тогда возвращ его ид и это знач качества)
                            vqErrorConverted = 6,   // ошибка преобразования в целевой тип
                            vqNonExistDevice = 7,   // несуществующее устройство
                            vqTagLengthIs0 = 8,      // длина запрашиваемого тега нулевая
                            vqUknownError = 9       // неизвестная ошибка при попытке получения значения тега
                        };
                     */
                    Single val_analog = 0;
                    switch (vggg)
                    {
                        case VarQuality.vqUndefined:
                            if (HMI_MT_Settings.HMI_Settings.slGlobalListTagsType.ContainsKey(sbkey.ToString())) 
                                tagtype = HMI_MT_Settings.HMI_Settings.slGlobalListTagsType[sbkey.ToString()];

                            switch (tagtype)
                            {
                                case "Analog":
                                        val_analog = 0;
                                        val_object = val_analog;
                                   break;
                                case "Discret":
                                    bool val_discret = false;
                                    val_object = val_discret;
                                    break;
                                case "Enum":
                                    val_analog = 0;
                                    val_object = val_analog;
                                    break;
                                case "DateTime":
                                    DateTime  val_dt = DateTime.MinValue;
                                    val_object = val_dt;
                                    break;
                                case "String":
                                    string val_st = string.Empty;
                                    val_object = val_st;
                                    break;
                                default:
                                    val_object = null;
                                    break;
                            }
                            break;
                        case VarQuality.vqArhiv:
                        case VarQuality.vqHandled:
                        case VarQuality.vqGood:
                            if (HMI_MT_Settings.HMI_Settings.slGlobalListTagsType.ContainsKey(sbkey.ToString())) 
                                tagtype = HMI_MT_Settings.HMI_Settings.slGlobalListTagsType[sbkey.ToString()];

                            switch (tagtype)
                            {
                                case "Analog":
                                    try
                                    {
                                        val_analog = BitConverter.ToSingle(tagInBytes,0);
                                        val_object = val_analog;
                                     }
                                    catch (Exception ex)
                                    {
                                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                                    }
                                   break;
                                case "Discret":
                                    bool val_discret = BitConverter.ToBoolean(tagInBytes, 0);
                                    val_object = val_discret;
                                    break;
                                case "DateTime":
                                    try
                                    {
                                        long lng = BitConverter.ToInt64(tagInBytes, 0);
                                        DateTime val_dt = DateTime.FromBinary(lng);
                                        //val_analog = BitConverter.ToSingle(tagInBytes,0);
                                        val_object = val_dt;
                                     }
                                    catch (Exception ex)
                                    {
                                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                                    }
                                    break;
                                case "Enum":
                                    try
                                    {
                                        val_analog = BitConverter.ToSingle(tagInBytes,0);
                                        val_object = val_analog;
                                     }
                                    catch (Exception ex)
                                    {
                                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                                    }
                                    break;
                                case "String":
                                    System.Text.UTF8Encoding utf = new UTF8Encoding( );
                                    string str = utf.GetString(tagInBytes);
                                    val_object = str;
                                    break;
                                default:
                                    val_object = null;
                                    break;
                            }
                            break;
                        case VarQuality.vqRangeError:
                            break;
                        case VarQuality.vqUknownTag:
                            break;
                        case VarQuality.vqErrorConverted:
                            break;
                        case VarQuality.vqNonExistDevice:
                            break;
                        case VarQuality.vqTagLengthIs0:
                            break;
                        case VarQuality.vqUknownError:
                            break;
                        default:
                            break;
                    }

                    //byte[] timestmp = br.ReadBytes(8);
                    //byte quality = br.ReadByte();

                    // формируем элемент выходного словаря
                    DSTagValue dstv = new DSTagValue();
                    dstv.VarQuality = quality;
                    dstv.VarValueAsObject = val_object;

                    dictList.Add(string.Format("{0}.{1}.{2}", unids, unidev, unitag), dstv);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dictList;
        }

        /// <summary>
        /// подписаться на теги
        /// </summary>
        /// <param name="request">список строк: ds.rtu.tagguid, ds.rtu.tagguid</param>
        public void SubscribeRTUTags(List<string> request)
        {
            try
            {
                /*
                 * по соглашению с ПР
                 * при каждой подписке
                 * очищаем предшестующуюподписку, если она была.
                 * Вариант с добавлением тегов тоже работает, просто
                 * не нужно производить очистку функцией ClearObservableTags
                 */
                HMI_MT_Settings.HMI_Settings.SubscribeObservable.ClearObservableTags(CurrentСontext.SessionId);

                HMI_MT_Settings.HMI_Settings.SubscribeObservable.AddObservableTags(CurrentСontext.SessionId, request);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// отписаться от обновления тегов
        /// </summary>
        /// <param name="request"></param>
        public void UnscribeRTUTags(List<string> request)
        {
            try
            {

                HMI_MT_Settings.HMI_Settings.SubscribeObservable.RemoveObservableTags(CurrentСontext.SessionId, request);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }       
        #endregion

        #region функции общего назначения
        #region код последней ошибки
        /// <summary>
        /// получить коды последних ошибок
        /// (стек ошибок) при обмене с DS
        /// </summary>
        /// <returns></returns>
        public LstError GetDSLastErrorsGUID()
        {
            LstError rez = null;

            try
            {
                //#region фиксация сессии
                ////Get client callback channel
                //var context = OperationContext.Current;
                //var sessionID = context.SessionId;
                ////Utilities.LogConsole(" - Создание экземпляра службы.");
                //var currClient = context.GetCallbackChannel<IWcfDataServerCallback>();

                //context.Channel.Faulted += Disconnect;
                //context.Channel.Closed += Disconnect;
                ////=============================================================================================

                ////=============================================================================================
                //WCFSession session;
                //if (!clientsDictionary.TryGetValue(sessionID, out session))
                //    lock (lockKey)
                //        clientsDictionary[sessionID] = new WCFSession(currClient);
                //#endregion

                LstError le = new LstError();
                le.lstError = listError.GetStkListError();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// получить код последней ошибки при обмене с DS
        /// </summary>
        /// <returns></returns>
        public string GetDSLastErrorGUID()
        {
            string rez = string.Empty;

            try
            {
                rez = listError.GetDSLastError();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }

        /// <summary>
        /// получить текст ошибки 
        /// при обмене с DS по ее коду
        /// </summary>
        /// <param name="lastErrorGUID"></param>
        /// <returns></returns>
        public string GetDSErrorTextByErrorGUID(string errorGUID)
        {
            string rez = string.Empty;
            try
            {
                rez = listError.GetDSErrorTextByErrorGUID(errorGUID);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// квитировать (очистить)
        /// стек ошибок
        /// </summary>
        public void AcknowledgementOfErrors()
        {
            listError.AcknowledgementOfErrors();
        }
        /// <summary>
        /// регистрация клиента для 
        /// процесса оповещения о новой ошибке
        /// </summary>
        /// <param name="keyticker"></param>
        public void RegisterForErrorEvent(string keyticker)
        {
            try
            {
                callbackEvent = OperationContext.Current.GetCallbackChannel<IWcfDataServerCallback>();            
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// реакция на событие появления новой ошибки
        /// </summary>
        /// <param name="strerror"></param>
        void listError_OnNewError(string strerror)
        {
            try
            {
                callbackEvent.NewErrorEvent(strerror);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion
        #region ping-pong
        /// <summary>
        /// Пустой запрос для поддержания активности
        /// </summary>
        public void Ping()
        {
            try
            {
                lock (lockKey)
                {
                    var currClient = CurrentСontext.GetCallbackChannel<IWcfDataServerCallback>();
                    currClient.Pong();
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 1684, string.Format("{0} : {1} : {2} : Ping.", DateTime.Now.ToString(), @"X:\Projects\00_DataServer\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs", "Ping()()"));
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 1688, string.Format("{0} : {1} : {2} : ОШИБКА: {3}", DateTime.Now.ToString(), @"X:\Projects\00_DataServer\WcfDataServer_Lib\WcfDataServer_Lib\WcfDataServer.cs", "Ping()()", ex.Message));
                //Utilities.LogTrace(" - Ошибка обмена для поддержания активности (Ping-Pong): " + ex.Message);
            }
        }
        #endregion
        #endregion

        enum UserAction
        {
            UserLogin = 1,
            UserLogout = 2,

        }

        #region Новые методы

        #region Работа с пользователем

        DSAuthResult IWcfDataServer.Authorization(string userName, string userPassword, bool isFirstAuthorization, DSUserSessionInfo userSessionInfo)
        {
            try
            {
                var authResult = HMI_Settings.DataBaseProvider.Authorization(userName, userPassword);

                if (authResult != null && isFirstAuthorization && authResult.AuthResult == AuthResult.Ok)
                    HMI_Settings.DataBaseProvider.WriteUserEvent(authResult.DSUser.UserID, 1, 0, userSessionInfo.UserIpAddress, userSessionInfo.UserMacAddress);

                return authResult;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return new DSAuthResult {AuthResult = AuthResult.NoConnectionToDb};
        }

        List<DSUser> IWcfDataServer.GetUsersList()
        {
            return HMI_Settings.DataBaseProvider.GetUsersList();
        }

        List<DSUserGroup> IWcfDataServer.GetUserGroupsList()
        {
            try
            {
                return HMI_Settings.DataBaseProvider.GetUserGroupsList();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        bool IWcfDataServer.CreateUserGroup(string groupName, string groupComment, string groupRight, DSUserSessionInfo userSessionInfo)
        {
            try
            {
                if (HMI_Settings.DataBaseProvider.CreateUserGroup(groupName, groupComment, groupRight) != null)
                {
                    HMI_Settings.DataBaseProvider.WriteUserEvent(userSessionInfo.UserId, 27, 0, userSessionInfo.UserIpAddress, userSessionInfo.UserMacAddress);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return false;
        }

        bool IWcfDataServer.CreateUser(string userName, string userPassword, string userComment, int userGroupID, DSUserSessionInfo userSessionInfo)
        {
            try
            {
                if (HMI_Settings.DataBaseProvider.CreateUser(userName, userPassword, userComment, userGroupID) != null)
                {
                    HMI_Settings.DataBaseProvider.WriteUserEvent(userSessionInfo.UserId, 12, 0, userSessionInfo.UserIpAddress, userSessionInfo.UserMacAddress);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return false;
        }

        #endregion

        #region Работа с событиями

        #region Запрос событий

        /// <summary>
        /// Получение событий
        /// </summary>
        List<DSEventValue> IWcfDataServer.GetEvents(DateTime dateTimeFrom, DateTime dateTimeTo, bool needSystemEvents, bool needUserEvents, bool needTerminalEvents, List<uint> devicesList)
        {
            try
            {
                return HMI_Settings.DataBaseProvider.GetEvents(dateTimeFrom, dateTimeTo, needSystemEvents, needUserEvents, needTerminalEvents, devicesList);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        #endregion

        #region Работа с данными

        /// <summary>
        /// Получить содержимое осциллограммы по её номеру
        /// </summary>
        DSOscillogram IWcfDataServer.GetOscillogramByID(Int32 eventDataID)
        {
            try
            {
                DSEventData eventData = HMI_Settings.DataBaseProvider.GetEventDataByID(eventDataID);

                DSOscillogram dsOscillogram = new DSOscillogram();
                dsOscillogram.Date = eventData.Date;
                dsOscillogram.OscillogramType = eventData.DataType;
                dsOscillogram.SourceName = eventData.SourceName;
                dsOscillogram.SourceComment = eventData.SourceComment;

                if (eventData.Data is List<byte[]>)
                    dsOscillogram.Content = eventData.Data as List<byte[]>;
                else
                    dsOscillogram.Content = new List<byte[]> { eventData.Data as byte[] };

                return dsOscillogram;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Получить архивную информацию (аварии, уставки и т.д.) как словарь значений
        /// </summary>
        Dictionary<string, DSTagValue> IWcfDataServer.GetHistoricalDataByID(Int32 dataID)
        {
            var result = new Dictionary<string, DSTagValue>();
            var rawresult = new Dictionary<uint, ITag>();

            try
            {
                DSEventData arhivData = HMI_Settings.DataBaseProvider.GetEventDataByID(dataID);

                if (arhivData == null || arhivData.DataType != DSEventDataType.Alarm)
                    return result;

                /*
                 * локализуем целевой блок и 
                 * отдаем устройству на разбор,
                 * ждем списка тегов
                 */
                uint LocObjectGuid = (uint)arhivData.DeviceGuid;

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey(LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[LocObjectGuid];

                    rawresult = dev.ParsePacketRawDataArchivToList((uint)arhivData.DeviceGuid, (byte [])arhivData.Data);

                    foreach (KeyValuePair<uint, ITag> kvp in rawresult)
                    {
                        DSTagValue dst = new DSTagValue();
                        /*
                         * enum в число
                        
                        return (int)Enum.Parse(type, value.ToString()); 
                         */ 
                        var type = kvp.Value.DataQualityAsEnum.GetType();
                        string tmp = kvp.Value.DataQualityAsEnum.ToString();

                        dst.VarQuality = Convert.ToUInt32(Enum.Parse(type, tmp));
                        dst.VarValueAsObject = kvp.Value.ValueAsObject;

                        result.Add(kvp.Value.StrTagId, dst);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return result;
        }

        #endregion

        #region Работа с квитированием

        /// <summary>
        /// Проверяет есть ли не кватированные аварийтные сообщения
        /// </summary>
        bool IWcfDataServer.IsNotReceiptedEventsExist()
        {
            try
            {
                return HMI_Settings.DataBaseProvider.IsNotReceiptedEventsExist();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return false;
        }

        /// <summary>
        /// Получить не квитированные аварийные сообщения
        /// </summary>
        List<DSEventValue> IWcfDataServer.GetNotReceiptedEvents()
        {
            try
            {
                return HMI_Settings.DataBaseProvider.GetNotReceiptedEvents();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Квитировать сообщения
        /// </summary>
        void IWcfDataServer.ReceiptEvents(List<Int32> eventValuesId, Int32 userID, string receiptComment, DSUserSessionInfo userSessionInfo)
        {
            try
            {
                HMI_Settings.DataBaseProvider.ReceiptEvents(eventValuesId, userID, receiptComment);

                HMI_Settings.DataBaseProvider.WriteUserEvent(userSessionInfo.UserId, 5, 0, userSessionInfo.UserIpAddress, userSessionInfo.UserMacAddress);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// Квитировать все сообщения
        /// </summary>
        void IWcfDataServer.ReceiptAllEvents(Int32 userID, string receiptComment, DSUserSessionInfo userSessionInfo)
        {
            try
            {
                HMI_Settings.DataBaseProvider.ReceiptAllEvents(userID, receiptComment);

                HMI_Settings.DataBaseProvider.WriteUserEvent(userSessionInfo.UserId, 5, 0, userSessionInfo.UserIpAddress, userSessionInfo.UserMacAddress);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        #endregion

        #endregion

        #region Уставки
        List<DSSettingsSet> IWcfDataServer.GetSettingsSetsList(uint devGuid)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return null;
        }

        Dictionary<string, DSTagValue> IWcfDataServer.GetValuesFromSettingsSet(int dataID)
        {
            /*
             * не закончено т.к. не определились с 
             * конечной концепцией работы с уставками
             */
            throw new NotImplementedException();

            var result = new Dictionary<string, DSTagValue>();
            var rawresult = new Dictionary<uint, ITag>();

            try
            {
                DSEventData arhivData = HMI_Settings.DataBaseProvider.GetEventDataByID(dataID);

                if (arhivData == null || arhivData.DataType != DSEventDataType.Ustavki)
                    return result;

                /*
                 * локализуем целевой блок и 
                 * отдаем устройству на разбор,
                 * ждем списка тегов
                 */
                uint LocObjectGuid = (uint)arhivData.DeviceGuid;

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey(LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[LocObjectGuid];

                    rawresult = dev.ParsePacketRawDataArchivToList((uint)arhivData.DeviceGuid, (byte [])arhivData.Data);

                    foreach (KeyValuePair<uint, ITag> kvp in rawresult)
                    {
                        DSTagValue dst = new DSTagValue();
                        /*
                         * enum в число
                        
                        return (int)Enum.Parse(type, value.ToString()); 
                         */
                        var type = kvp.Value.DataQualityAsEnum.GetType();
                        string tmp = kvp.Value.DataQualityAsEnum.ToString();

                        dst.VarQuality = Convert.ToUInt32(Enum.Parse(type, tmp));
                        dst.VarValueAsObject = kvp.Value.ValueAsObject;

                        result.Add(kvp.Value.StrTagId, dst);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return result;
        }

        void IWcfDataServer.SaveSettingsToDevice(uint devGuid, Dictionary<string, DSTagValue> tagsValues)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Документы

        #region Load

        /// <summary>
        /// Получить список документов терминала
        /// </summary>
        List<DSDocumentDataValue> IWcfDataServer.GetDocumentsList(Int32 devGuid)
        {
            try
            {
                return HMI_Settings.DataBaseProvider.GetDocumentsList(devGuid);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Получить ссылку на документ
        /// </summary>
        DSDataFile IWcfDataServer.GetDocumentByID(Int32 documentId)
        {
            try
            {
                return HMI_Settings.DataBaseProvider.GetDocumentByID(documentId);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        #endregion

        #region Upload

        private List<byte[]> _uploadFileChunks = null;

        /// <summary>
        /// Загрузить кусочек файла
        /// </summary>
        bool IWcfDataServer.UploadFileChunk(byte[] fileChunk)
        {
            if (_uploadFileChunks == null)
                _uploadFileChunks = new List<byte[]>();

            _uploadFileChunks.Add(fileChunk);

            return true;
        }

        /// <summary>
        /// Сохранить файл
        /// </summary>
        bool IWcfDataServer.SaveUploadedFile(Int32 devGuid, Int32 userId, string fileName, string fileComment)
        {
            if (_uploadFileChunks == null || _uploadFileChunks.Count == 0)
                return false;

            try
            {
                var filedata = new List<byte>();
                foreach (var fileChunk in _uploadFileChunks)
                    filedata.AddRange(fileChunk);

                _uploadFileChunks = null;

                HMI_Settings.DataBaseProvider.AddDocumentToTerminal(devGuid, userId, fileName, fileComment, filedata.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return false;
        }

        /// <summary>
        /// Отменяет загрузку файлов
        /// </summary>
        void IWcfDataServer.TerminateUploadFileSession()
        {
            _uploadFileChunks = null;
        }

        #endregion

        #endregion

        #region Тренды

        /// <summary>
        /// Получить список тегов, у которых включена запись значений
        /// </summary>
        public List<string> GetTagsListWithEnabledTrendSave()
        {
            try
            {
                return HMI_Settings.TrendMamager.GetTagsListWithEnabledTrendSave();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Получить диапозоы времени, в которых доступны данные
        /// </summary>
        public List<Tuple<DateTime, DateTime>> GetTagTrendDateTimeRanges(uint devGuid, uint tagGuid)
        {
            try
            {
                return HMI_Settings.TrendMamager.GetTrendDateTimeRanges(devGuid, tagGuid);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Получить тренд единым списком
        /// </summary>
        public List<Tuple<DateTime, object>> GetTagTrend(uint devGuid, uint tagGuid, DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                return HMI_Settings.TrendMamager.GetTagArhivTrend(devGuid, tagGuid, startDateTime, endDateTime);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Получить список обособленных трендов
        /// </summary>
        public List<List<Tuple<DateTime, object>>> GetTagTrendsList(uint devGuid, uint tagGuid, DateTime startDateTime, DateTime endDateTime)
        {
            try
            {
                return HMI_Settings.TrendMamager.GetTagArhivTrendsList(devGuid, tagGuid, startDateTime, endDateTime);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Получить настройки режима работы записи тренда
        /// </summary>
        public DSTrendSettings GetTrendSettings(uint devGuid, uint tagGuid)
        {
            try
            {
                return HMI_Settings.TrendMamager.GetTagTrendSettings(devGuid, tagGuid);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }

        /// <summary>
        /// Установить настройки режима работы записи тренда
        /// </summary>
        public void SetTrendSettings(uint devGuid, uint tagGuid, DSTrendSettings trendSettings)
        {
            #region Получаем ссылку на Tag

            var device = HMI_Settings.DataServer.GetRTUById(devGuid);
            if (device == null)
                return;

            var tag = device.GetTag(tagGuid);
            if (tag == null)
                return;

            #endregion

            HMI_Settings.TrendMamager.SetTagTrendSettings(tag, trendSettings);

            // Сохраняем в конф. файл настройки
            CommonUtils.WriteTrendSettingsToDevCFGFile(tag.ThisDevice.PathToDeviceCFG, tag.TagGUIDVar, trendSettings);
        }

        #endregion

        #endregion

        #region Ручной ввод преобразовывающих коэффициентов
        /// <summary>
        /// Получить коэффициент преобразования для тега
        /// </summary>
        public Object GetTagAnalogTransformationRatio(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid)
        {
            object retval = null;

            try
            {
                UInt16 UniDsGuid = dsGuid;
                UInt32 LocObjectGuid = devGuid;
                UInt32 tagguid = tagGuid;

                //string[] strreq = idTag.Split(new char[] { '.' });
                //// 2 байта - ds
                //UniDsGuid = UInt16.Parse(strreq[0]);
                //// 4 байта - LocObjectGuid
                //LocObjectGuid = UInt32.Parse(strreq[1]);
                //// TagGuid тега
                //tagguid = UInt32.Parse(strreq[2]);

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    ITag tag = dev.GetTag(tagguid);
                    if (tag != null)
                        retval = (tag as ITransfomationRatioReCalculate).TransformationRatio;
                    else
                        throw new Exception(string.Format("{0} : {1} : Попытка получения значения пересчета несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "GetTagAnalogTransformationRatio()"));

                }
                else
                    throw new Exception(string.Format("{0} : {1} : Попытка получения значения пересчета несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "GetTagAnalogTransformationRatio()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                Single ret = 0;
                retval = ret;
            }
            return retval;
        }

        /// <summary>
        /// Установить коэффициент преобразования
        /// </summary>
        public void SetTagAnalogTransformationRatio(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid, Object transformationRatio)
        {
            try
            {
                UInt16 UniDsGuid = dsGuid;
                UInt32 LocObjectGuid = devGuid;
                UInt32 tagguid = tagGuid;

                //string[] strreq = idTag.Split(new char[] { '.' });
                //// 2 байта - ds
                //UniDsGuid = UInt16.Parse(strreq[0]);
                //// 4 байта - LocObjectGuid
                //LocObjectGuid = UInt32.Parse(strreq[1]);
                //// TagGuid тега
                //tagguid = UInt32.Parse(strreq[2]);

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    ITag tag = dev.GetTag(tagguid);
                    if (tag != null)
                        tag.SetTransformationRatioFromHMI(transformationRatio);
                    else
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 299, string.Format("{0} : {1} : Попытка установки значения несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "SetTagValueFromHMI()"));
                }
                else
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 298, string.Format("{0} : {1} : Попытка установки тега несуществующего устройства .", @"\WcfDataServer_Lib\WcfDataServer.cs", "SetTagValueFromHMI()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// Сбросить коэффициент преобразования
        /// </summary>
        public void ReSetTagAnalogTransformationRatio(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid)
        {
            try
            {
                UInt16 UniDsGuid = dsGuid;
                UInt32 LocObjectGuid = devGuid;
                UInt32 tagguid = tagGuid;

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    ITag tag = dev.GetTag(tagguid);
                    if (tag != null)
                        tag.ReSetTransformationRatioFromHMI();
                    else
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 2910, string.Format("{0} : {1} : Попытка сброса коэф пересчета значения несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "ReSetTagAnalogTransformationRatio()"));
                }
                else
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 2913, string.Format("{0} : {1} : Попытка сброса коэф пересчета значения тега несуществующего устройства .", @"\WcfDataServer_Lib\WcfDataServer.cs", "ReSetTagAnalogTransformationRatio()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// Возвращает true, если значение дискретного тега инверсируется
        /// </summary>
        public bool IsInverseDiscretTag(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid)
        {
            bool retval = false;

            try
            {
                UInt16 UniDsGuid = dsGuid;
                UInt32 LocObjectGuid = devGuid;
                UInt32 tagguid = tagGuid;

                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    ITag tag = dev.GetTag(tagguid);
                    if (tag != null)
                        retval = (tag as ITransfomationRatioReCalculate).IsInverseBoolValue;
                    else
                        throw new Exception(string.Format("{0} : {1} : Попытка получения значения пересчета несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "GetTagAnalogTransformationRatio()"));

                }
                else
                    throw new Exception(string.Format("{0} : {1} : Попытка получения значения пересчета несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "GetTagAnalogTransformationRatio()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                retval = false;
            }
            return retval;
        }

        /// <summary>
        /// Инверсирует значение дискретного тега
        /// </summary>
        public void InverseDiscretTag(UInt16 dsGuid, UInt32 devGuid, UInt32 tagGuid, bool newInverseProperty)
        {
            try
            {
                ITag tag = GetTagByTagId(dsGuid, devGuid, tagGuid);

                if (tag == null)
                    return;

                tag.SetTagReverseProperty(newInverseProperty);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// получить тег по его
        /// идентификационной информации
        /// </summary>
        /// <param name="dsGuid"></param>
        /// <param name="devGuid"></param>
        /// <param name="tagGuid"></param>
        /// <returns></returns>
        private ITag GetTagByTagId(ushort dsGuid, uint devGuid, uint tagGuid)
        {
            ITag tag = null;

            UInt16 UniDsGuid = dsGuid;
            UInt32 LocObjectGuid = devGuid;
            UInt32 tagguid = tagGuid;

            try
            {
                if (HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID.ContainsKey((uint)LocObjectGuid))
                {
                    IDevice dev = (IDevice)HMI_MT_Settings.HMI_Settings.DataServer.Sl4Access2TagsSetByObjectGUID[(uint)LocObjectGuid];
                    tag = dev.GetTag(tagguid);
                    if (tag == null)
                        TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 3033, string.Format("{0} : {1} : Попытка установки значения несуществующего тега .", @"\WcfDataServer_Lib\WcfDataServer.cs", "GetTagByTagId()"));
                }
                else
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 3036, string.Format("{0} : {1} : Попытка установки тега несуществующего устройства .", @"\WcfDataServer_Lib\WcfDataServer.cs", "GetTagByTagId()"));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return tag;
        }
        #endregion

        #region Комманды
        /// <summary>
        /// словарь-очередь для идентификации команд
        /// и управления ими
        /// </summary>
        Dictionary<string, CMDInfo> dictCMDQueue = new Dictionary<string, CMDInfo>();
        private object lockObjectForCommands = new object();

        /// <summary>
        /// анти-таймер на отслеживание состояния выполнения команды
        /// </summary>
        // private System.Timers.Timer tmierUpdateCommandState_anti;
        /// <summary>
        /// <summary>
        /// Запрос на запуск команды на устройстве (SAF IEC)
        /// </summary>
        /// <param name="ACommandID">ds.dev.cmdid</param>
        /// <param name="AParameters">массив параметров</param>
        /// <param name="idDSRouterSession">идентификатор сессии на DSRouter кот выдал команду</param>
        /// <returns></returns>
        public void CommandRun(string ACommandID, object[] AParameters, string idDSRouterSession)
        {
            try
            {
                string[] arrcmdid = ACommandID.Split(new char[] { '.' });
                UInt16 DS4CurrtntCMD = UInt16.Parse(arrcmdid[0]);
                uint dev = UInt32.Parse(arrcmdid[1]);
                uint tagguid = UInt32.Parse(arrcmdid[2]);

                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 1385, string.Format("{0} : Получен запрос на команду от HMI-клиента для {1}.", DateTime.Now.ToString(), ACommandID));

                /*
                 * если в очереди команд нет то -
                 * запуск команды в отдельном потоке
                 */
                if (dictCMDQueue.Count() == 0)
                {
                    // добавим команду в очередь
                    AddCMDInQueue(DS4CurrtntCMD, dev, tagguid, null, idDSRouterSession);

                    Thread t = new Thread(() => RunCMD(DS4CurrtntCMD, dev, tagguid, null, idDSRouterSession));
                    t.Start();
                }
                else
                {
                    // добавим команду в очередь
                    AddCMDInQueue(DS4CurrtntCMD, dev, tagguid, null, idDSRouterSession);
                }
            }
            catch (Exception ex)
            {   
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// добавить команду в очередь
        /// </summary>
        /// <param name="cmdinfo"></param>
        private void AddCMDInQueue(UInt16 DS4CurrtntCMD, uint dev, uint tagguid, object[] AParameters, string idDSRouterSession)
        {
            /*
             * если в очереди уже есть команда от конкретного 
             * клиента, то старая заменяется новой - за состоянием 
             * команд должен следить клиент и не выдавать новую команду пока
             * не поймет состояние старой
             */
            try
            {
                CMDInfo cmdinfo = new CMDInfo(DS4CurrtntCMD, dev, tagguid, null, idDSRouterSession);
                
                if (!dictCMDQueue.ContainsKey(idDSRouterSession))
                    dictCMDQueue.Add(idDSRouterSession, cmdinfo);
                else
                    dictCMDQueue[idDSRouterSession] = cmdinfo;

                if (!HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.ContainsKey(cmdinfo.idDSRouterSession))
                    HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.Add(cmdinfo.idDSRouterSession, EnumerationCommandStates.cmdWaitInDSQueue);
                else
                    HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[cmdinfo.idDSRouterSession] = EnumerationCommandStates.cmdWaitInDSQueue;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        }
        /// <summary>
        /// удалить команду из очереди команд
        /// </summary>
        /// <param name="cmdinfo"></param>
        private void RemoveCMDFromQueue(string idDSRouterSession)
        {
            /*
             * удаляем команду из очереди команд и ставляем 
             * статус команды для внешних запросов о состоянии команды
             * для конкретного клиента
             */
            try
            {
                if (dictCMDQueue.ContainsKey(idDSRouterSession))
                {
                    dictCMDQueue.Remove(idDSRouterSession);
                    // удаляем команду из словаря для отслеживания
                    //HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.Remove(idDSRouterSession);

                    // запускаем следующую команду - если она есть
                    if (dictCMDQueue.Count() > 0)
                        RunCMD(dictCMDQueue.ElementAt(0).Value.numksdu, dictCMDQueue.ElementAt(0).Value.numvtu, dictCMDQueue.ElementAt(0).Value.tagguid, dictCMDQueue.ElementAt(0).Value.arr, dictCMDQueue.ElementAt(0).Value.idDSRouterSession);
                }
                else
                { 
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        }
        /// <summary>
        /// проверка статуса выполнения команды
        /// для конкретного клиента
        /// </summary>
        /// <param name="ACommandID"></param>
        /// <returns></returns>
        public EnumerationCommandStates CommandStateCheck(string idDSRouterSession)
        {
            if (HMI_MT_Settings.HMI_Settings.dictCurrentCMDState.ContainsKey(idDSRouterSession))
            {
                Console.WriteLine(string.Format("Cостояние выполнения команды: {0}", HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[idDSRouterSession]));
                // если состояние команды - неизвестная команда, то нужно ее удалить
                if (HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[idDSRouterSession] == EnumerationCommandStates.cmdUnknown)
                    RemoveCMDFromQueue(idDSRouterSession);

                return HMI_MT_Settings.HMI_Settings.dictCurrentCMDState[idDSRouterSession];
            }
            return EnumerationCommandStates.cmdUnknown;
        }
        #endregion

        #region Работа с тревогами
        /// <summary>
        /// Получить список неквитированных 
        /// событий (заданное количество)
        /// </summary>
        public List<DSAlarmsInfo> GetLastNonConfirmMAlarms(Int32 count)
        {
            List<DSAlarmsInfo> lstLastAlarms = new List<DSAlarmsInfo>();
            try
            {
                foreach (Juornals.AlarmRecord ar in HMI_MT_Settings.HMI_Settings.ALARMJOURNAL.GetNLastNonConfirmAlarmRecord(count))
                {
                    DSAlarmsInfo dsai = new DSAlarmsInfo();
                    dsai.ALARMLEVEL = ar.ALARMLEVEL;
                    dsai.ALARMTEXTMESSAGE = ar.ALARMTEXTMESSAGE;
                    dsai.ALARMTIMESTAMP = ar.ALARMTIMESTAMP;
                    dsai.devguid = ar.devguid;
                    dsai.StrTagId = ar.StrTagId;
                    dsai.tagguid = ar.tagguid;
                    dsai.GuidAlarmRecord = ar.GuidAlarmRecord;
                    dsai.COMMENT = ar.COMMENT;

                    lstLastAlarms.Add(dsai);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return lstLastAlarms;
        }
        /// <summary>
        /// квиитрование
        /// </summary>
        /// <param name="EventGuid"></param>
        /// <param name="ECCComment"></param>
        public void ConfirmAlarm(string EventGuid, string ECCComment)
        {
            try
            {
                 HMI_MT_Settings.HMI_Settings.ALARMJOURNAL.ConfirmAlarm(EventGuid, ECCComment);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// запрос списка тревог из диапазона дат
        /// </summary>
        /// <param name="DTStart"></param>
        /// <param name="DTEnd"></param>
        /// <returns></returns>
        public List<DSAlarmsInfo> GetAlarmsInDatesRange(DateTime DTStart, DateTime DTEnd)
        {
            List<DSAlarmsInfo> lstLastAlarms = new List<DSAlarmsInfo>();
            try
            {
                foreach (Juornals.AlarmRecord ar in HMI_MT_Settings.HMI_Settings.ALARMJOURNAL.GetAlarmRecordsFromDatesRange(DTStart, DTEnd))
                {
                    DSAlarmsInfo dsai = new DSAlarmsInfo();
                    dsai.CONFIRM = ar.CONFIRM;
                    dsai.ALARMLEVEL = ar.ALARMLEVEL;
                    dsai.ALARMTEXTMESSAGE = ar.ALARMTEXTMESSAGE;
                    dsai.ALARMTIMESTAMP = ar.ALARMTIMESTAMP;
                    dsai.devguid = ar.devguid;
                    dsai.StrTagId = ar.StrTagId;
                    dsai.tagguid = ar.tagguid;
                    dsai.GuidAlarmRecord = ar.GuidAlarmRecord;
                    dsai.COMMENT = ar.COMMENT;

                    lstLastAlarms.Add(dsai);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return lstLastAlarms;
        }
        #endregion
        #endregion

        #region private-методы
        /// <summary>
        /// извлечь источник 
        /// по его идентификатору
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <returns></returns>
        private ISrcCfgManager GetSrcBySrcGuid(UInt16 SrcGuid)
        {
            ISrcCfgManager rez = null;
            
            try
            {
                List<ISrcCfgManager> lstsrc = HMI_MT_Settings.HMI_Settings.DataServer.SRC_mtdex;

                foreach (ISrcCfgManager src in lstsrc)
                {
                    if (src.SrcId.Trim() == SrcGuid.ToString())
                        rez = src;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        
        /// <summary>
        /// извлечь контроллер 
        /// по его идентификатору
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <returns></returns>
        private IController GetECUByECUGuid(UInt16 SrcGuid,UInt16 EcuGuid)
        {
            IController rez = null;
            
            try
            {
                ISrcCfgManager src = GetSrcBySrcGuid(SrcGuid);

                foreach (KeyValuePair<uint,IController> item in src.GetSourceConfiguration().SlObjectConfiguration)
                    if (item.Value.GetECUNumber() == EcuGuid)
                        rez = item.Value;                        
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// извлечь устройство 
        /// по его идентификатору 
        /// (способ 1)
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        IDevice GetRTUByRTUGuid(UInt16 SrcGuid,UInt16 EcuGuid,UInt32 RtuGuid)
        {
           IDevice rez = null;
            
            try
            {
                IController ecu = GetECUByECUGuid(SrcGuid, EcuGuid);

                foreach (IDevice item in ecu.GetEcuRTUs())
                    if (item.ObjectGUID == RtuGuid)
                        rez = item;                        
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// извлечь устройство 
        /// по его идентификатору 
        /// (способ 2)
        /// </summary>
        /// <param name="rtuguid"></param>
        /// <returns></returns>
        private IDevice GetRTUByRTUGuid(UInt32 rtuguid)
        {
            IDevice rez = null;

            try
            {
                List<ISrcCfgManager> lstsrc = HMI_MT_Settings.HMI_Settings.DataServer.SRC_mtdex;

                foreach (ISrcCfgManager src in lstsrc)
                {
                    IConfiguration srcconfig = src.GetSourceConfiguration();
                    foreach (KeyValuePair<uint, IController> kvp in srcconfig.SlObjectConfiguration)
                    {
                        foreach (IDevice item in kvp.Value.GetEcuRTUs())
                            if (item.ObjectGUID == rtuguid)
                            {
                                rez = item;
                                return rez;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return rez;
        }
        /// <summary>
        /// извлечь группу 
        /// по ее идентификатору
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        IGroup GetGroupByGroupGuid(UInt32 RtuGuid, UInt32 GroupGuid)
        {
            IGroup rez = null;

            try
            {
                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                foreach (IGroup item in rtu.GetGroupPlainHierarchy())
                    if (item.GroupGUID == GroupGuid.ToString())
                        rez = item;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        /// <summary>
        /// извлечь тег 
        /// по его идентификатору
        /// </summary>
        /// <param name="DSGuid"></param>
        /// <param name="SrcGuid"></param>
        /// <param name="EcuGuid"></param>
        /// <param name="RtuGuid"></param>
        /// <returns></returns>
        ITag GetTagByTagGuid(UInt32 RtuGuid, UInt32 TagGuid)
        {
            ITag rez = null;

            try
            {
                IDevice rtu = GetRTUByRTUGuid(RtuGuid);

                foreach (ITag item in rtu.GetRtuTags())
                    if (item.TagGUIDVar == TagGuid)
                        rez = item;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return rez;
        }
        #endregion
    }

    /// <summary>
    /// класс описания WCF-сессии
    /// </summary>
    public class WCFSession
    {
        #region Сессия работы клиента с WCF-сервисом
        private IWcfDataServerCallback client;
            public IWcfDataServerCallback Client
            {
                get { return client; }
                set { client = value; }
            }
            
            public WCFSession(IWcfDataServerCallback cclient)
            {
                try
                {
                    client = cclient;
                }
                catch (Exception ex)
                {
                    TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                }
            }
        #endregion
    }
}