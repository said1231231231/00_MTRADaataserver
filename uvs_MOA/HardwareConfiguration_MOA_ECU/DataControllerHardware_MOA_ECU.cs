/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DataControllerHardware_MOA_ECU - класс представления контроллера МОА
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Fasilities\DataControllerHardware_MOA_ECU.cs
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
using HardwareConfigurationLib.HardwareConfiguration;

namespace uvs_MOA.HardwareConfiguration_MOA_ECU
{
    public class DataControllerHardware_MOA_ECU : DataControllerHardware
    {
        /// <summary>
        /// диспетчер обработки пакетов
        /// </summary>
        /// <param name="fcnum">номер контроллера</param>
        /// <param name="devnum">номер устройства с учетом контроллера</param>
        /// <param name="packetbin">пакет для разборки</param>
        public void PacketHandler(uint devnum, byte[] packetbin)
        {
            try
            {
                foreach (DeviceHardware dh in this.ListDevice4DataController)
                    if (dh.DevGUID == devnum)
                        (dh as DeviceHardware_MOA_ECU).ParsePacketRawData(packetbin);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
