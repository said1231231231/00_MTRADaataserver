/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _04Group - класс представления группы на уровне Presentation
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentaionConfiguration\_04Group.cs
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
    public class _04Group
    {
        public bool Enable { get; set; }
        public uint GroupGUID { get; set; }
        public string Name { get; set; }
        public uint Category { get; set; }
        /// <summary>
        /// список подгрупп
        /// </summary>
        public List<_04Group> LstSubGroups { get; set; }
        /// <summary>
        /// список тегов группы
        /// </summary>
        public List<_05Tag> LstTags { get; set; }
        /// <summary>
        /// ссылка на утройство для этой группы
        /// </summary>
        public _03Device Thisdevice = null;

        public _04Group()
        {
            LstSubGroups = new List<_04Group>();
            LstTags = new List<_05Tag>();
        }
    }
}
