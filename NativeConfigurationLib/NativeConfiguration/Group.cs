/*#############################################################################
 *    Copyright (C) 2006-2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Group - класс представления группы устройства
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\Group.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С#, Framework 4.0                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : 09.06.2014
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
    public class Group
    {
        /// <summary>
        /// включенность группы
        /// </summary>
        public bool Enable {get;set;}
        /// <summary>
        /// уник номер группы
        /// </summary>
        public uint GroupGUID {get;set;}
        /// <summary>
        /// имя группы
        /// </summary>
        public string GroupName {get;set;}
        /// <summary>
        /// категория группы
        /// </summary>
        public string GroupCategory {get;set;}
        /// <summary>
        /// список подгрупп
        /// </summary>
        public List<Group> SubGroupList { get; set; }        
        /// <summary>
        /// список тегов группы для запроса
        /// </summary>
        public List<Tag> TagList {get;set;}

        /// <summary>
        /// конструктор
        /// </summary>
        public Group()
        {
            SubGroupList = new List<Group>();
        }
    }
}
