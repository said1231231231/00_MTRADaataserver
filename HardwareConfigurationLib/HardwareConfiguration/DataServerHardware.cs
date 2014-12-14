/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DataServerHardware - класс представления конфигурации DS уровня hardware
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DataServerHardware.cs
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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HardwareConfigurationLib.HardwareConfiguration
{
    public class DataServerHardware
    {
        /// <summary>
        /// СПИСОК источников данных (согласно их природе)
        /// </summary>
        public List<DataSourceHardware> DATASOURCES = new List<DataSourceHardware>();

        /// <summary>
        /// имя DataServer
        /// </summary>
        //private string nameDS_GUID;

        /// <summary>
        /// уник номер DataServer
        /// </summary>
        public string UniDS_GUID { get; set; }

        /// <summary>
        /// список устройств DataServer (от всех источников)
        /// </summary>
        public List<DeviceHardware> ListDevice4DS {get;set;}

        public DataServerHardware()
        {
            ListDevice4DS = new List<DeviceHardware>();
        }

        /// <summary>
        /// инициировать обмен данными
        /// на источниках
        /// </summary>
        public void StartDataCommunicationExchange()
        {
            try
            {
                foreach (DataSourceHardware dsh in this.DATASOURCES)
                    dsh.StartDataCommunicationExchange();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
