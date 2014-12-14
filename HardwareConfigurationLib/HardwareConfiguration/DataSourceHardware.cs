/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DataSourceHardware - класс представления источника DS уровня Hardware
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DataSourceHardware.cs
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
    public abstract class DataSourceHardware
    {   
        /// <summary>
        /// уник номер источника
        /// </summary>
        public string SrcGuid{get;set;}
        /// <summary>
        /// имя источника
        /// </summary>
        public string NameSourceDriver {get;set;}
        /// <summary>
        /// список контролллеров источника
        /// </summary>
        public List<DataControllerHardware> ListDataControllerHardware { get; set; }

        /// <summary>
        /// список устройств DataServer (от всех источников)
        /// </summary>
        public List<DeviceHardware> ListDevice4DataSource {get;set;}

        /// <summary>
        /// DataServer кот принадлежить источник
        /// </summary>
        public DataServerHardware DataServerParent { get; set; }

        public DataSourceHardware()
        {
            ListDataControllerHardware = new List<DataControllerHardware>();
            ListDevice4DataSource = new List<DeviceHardware>();
        }

        /// <summary>
        /// инициировать обмен данными
        /// для устройств источника
        /// </summary>
        public abstract void StartDataCommunicationExchange();
    }
}
