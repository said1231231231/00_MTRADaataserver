/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Controller - класс представления контроллера в конфигурации
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\Controller.cs
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
using System.ComponentModel;
//using MTRADataServer.NativeConfiguration;
using System.Collections.ObjectModel;


namespace NativeConfigurationLib.NativeConfiguration
{
    public class DataController
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
        public List<Device> ListDevice4DataController { get; set; }
        /// <summary>
        /// имя источника для данного контроллера
        /// </summary>
        public string DataSourceName4ThisController { get; set; }

        /// <summary>
        /// DataServer для этого устройства
        /// </summary>
        public DataServer DataServer4ThisDevice { get; set; }

        public DataController()
        {
            ListDevice4DataController = new List<Device>();
        }
    }
}
