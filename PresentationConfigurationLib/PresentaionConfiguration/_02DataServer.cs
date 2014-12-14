/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Configuration - класс представления DS  на уровне Presentation
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\Configuration.cs
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

namespace PresentationConfigurationLib.PresentaionConfiguration
{
    public class _02DataServer
    {
        public uint UniDS_GUID { get; set; }
        public string NameDataServer { get; set; }
        /// <summary>
        /// ссылка на конфигурацию представления
        /// </summary>
        public _01Configuration PresentConfiguration { get; set; }

        public List<_03Device> LstDevice { get; set; }

        public _02DataServer()
        {
            LstDevice = new List<_03Device>();
        }
    }
}
