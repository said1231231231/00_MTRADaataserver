/*#############################################################################
 *    Copyright (C) 2006-2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _06PresentatonGroup - класс представления группы устройства в интерфейсе пользователя
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentationTreeItems\_06PresentatonGroup.cs
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

namespace MTRADataServer.PresentationTreeItems
{
    public class _06PresentatonGroup : INotifyPropertyChanged
    {
        private bool enable;
        /// <summary>
        /// включенность группы
        /// </summary>
        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Enable"));
            }
        }
        private uint groupGUID;
        /// <summary>
        /// уник номер группы
        /// </summary>
        public uint GroupGUID
        {
            get { return groupGUID; }
            set
            {
                groupGUID = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GroupGUID"));
            }
        }
        private string groupName;
        /// <summary>
        /// имя группы
        /// </summary>
        public string GroupName
        {
            get { return groupName; }
            set
            {
                groupName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GroupName"));
            }
        }
        private string groupCategory;
        /// <summary>
        /// категория группы
        /// </summary>
        public string GroupCategory
        {
            get { return groupCategory; }
            set
            {
                groupCategory = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GroupCategory"));
            }
        }

        private ObservableCollection<PresentationTreeItems._06PresentatonGroup> subGroupList;
        /// <summary>
        /// список подгрупп
        /// </summary>
        public ObservableCollection<PresentationTreeItems._06PresentatonGroup> SubGroupList
        {
            get { return subGroupList; }
            set
            {
                subGroupList = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SubGroupList"));
            }
        }
        private ObservableCollection<PresentationTreeItems._07PresentatonTag> tagList;
        /// <summary>
        /// список тегов группы для запроса
        /// </summary>
        public ObservableCollection<PresentationTreeItems._07PresentatonTag> TagList
        {
            get { return tagList; }
            set
            {
                tagList = value;
                OnPropertyChanged(new PropertyChangedEventArgs("TagList"));
            }
        }

        /// <summary>
        /// устройство которому принадлежит эта группа
        /// </summary>
        public PresentationTreeItems._05PresentatonDevice ThisDevice { get; set; }

        /// <summary>
        /// конструктор
        /// </summary>
        public _06PresentatonGroup()
        {
            SubGroupList = new ObservableCollection<PresentationTreeItems._06PresentatonGroup>();
            TagList = new ObservableCollection<_07PresentatonTag>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public struct CheckBoxId
        {
            public static string checkBoxId;
        }
    }
}
