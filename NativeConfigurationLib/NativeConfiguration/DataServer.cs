/*#############################################################################
 *    Copyright (C) 2006-2013 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DataServer - класс представления DataServer в конфигурации для запроса тегов
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\DataServer.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С#, Framework 4.0                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : 31.07.2013
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NativeConfigurationLib.NativeConfiguration
{
    public class DataServer
    {
        /// <summary>
        /// ссылка на корень конфигурации
        /// </summary>
        public DataConfiguration DATACONFIGURATION;
        /// <summary>
        /// СПИСОК имен источников данных (согласно их природе)
        /// для обращения к их конфигурациям
        /// </summary>
        public List<string> DATASOURCES;
        public List<DataController> DATACONTROLLER;
        /// <summary>
        /// имя DataServer
        /// </summary>
        public string NameDS_GUID{get;set;}
        /// <summary>
        /// уник номер DataServer
        /// </summary>
        public string UniDS_GUID {get;set;}
        /// <summary>
        /// список устройств DataServer (от всех источников)
        /// </summary>
        public List<Device> ListDevice4DS{get;set;}
        
        public DataServer()
        {
            DATASOURCES = new List<string>();
            DATACONTROLLER = new List<DataController>();
            ListDevice4DS = new List<Device>();
        }
    }
}
