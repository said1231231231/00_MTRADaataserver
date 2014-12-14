/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DataSourceHardware_MOA_ECU - класс представления источника данных для МОА
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DataSourceHardware_MOA_ECU.cs
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
using uvs_MOA.MOA_ECU_SOURCE;
using HardwareConfigurationLib.HardwareConfiguration;
using uvs_MOA.MOA_ECU_SOURCE;

namespace uvs_MOA.HardwareConfiguration_MOA_ECU
{
    public class DataSourceHardware_MOA_ECU : DataSourceHardware
    {
        /// <summary>
        /// порт для получения данных от сервера
        /// по UDP
        /// </summary>
        public int udpserver_port { get; set; }

        /// <summary>
        /// инициировать обмен данными
        /// для устройств источника
        /// </summary>
        public override void StartDataCommunicationExchange()
        {
            try
            {
                ClientServerOn_MOA_UDPSockets moa_udp_client = new ClientServerOn_MOA_UDPSockets(udpserver_port, NameSourceDriver );//srcinfo
                // создаем экземпляры классов очереди и разборщика пакетов
                PacketQueque PQueque = new PacketQueque();
                moa_udp_client.NetPackQ = PQueque;
                PacketParser_udp_MOA PParser = new PacketParser_udp_MOA();
                PParser.Init(this);
                PQueque.packetAppearance += new PacketAppearance(PParser.byteQueque_packetAppearance);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// диспетчер обработки пакетов
        /// </summary>
        /// <param name="fcnum">номер контроллера</param>
        /// <param name="devnum">номер устройства с учетом контроллера</param>
        /// <param name="packetbin">пакет для разборки</param>
        public void PacketHandler(uint fcnum, uint devnum, byte[] packetbin)
        { 
            try
            {
                foreach (DataControllerHardware dch in this.ListDataControllerHardware)
                    if (dch.СontrollerNumber == fcnum.ToString())
                        (dch as DataControllerHardware_MOA_ECU).PacketHandler(devnum, packetbin);             
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
