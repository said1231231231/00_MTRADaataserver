﻿/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DataControllerHardware - класс представления контроллера источника данных DS уровня hardware
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DataControllerHardware.cs
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
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HardwareConfigurationLib.HardwareConfiguration
{
    public class DataControllerHardware
    {
        /// <summary>
        /// guid контроллера
        /// </summary>        
        public string ObjectGUID {get;set;}
        /// <summary>
        /// номер контроллера
        /// </summary>
        public string СontrollerNumber {get;set;}
        /// <summary>
        /// список устройств контроллера
        /// </summary>
        public List<DeviceHardware> ListDevice4DataController {get;set;}
        /// <summary>
        /// источник которому принадлежит контроллер
        /// </summary>
        public DataSourceHardware DataSourceParent { get; set; }

        public DataControllerHardware()
        {
            ListDevice4DataController = new List<DeviceHardware>();
        }
    }
}
