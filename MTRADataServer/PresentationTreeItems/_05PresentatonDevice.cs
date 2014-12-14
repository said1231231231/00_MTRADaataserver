/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DeviceHardware - класс представления устройства в конфигурации DS (интерфейс пользователя)
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DeviceHardware.cs
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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using PresentationConfigurationLib;

namespace MTRADataServer.PresentationTreeItems
{
    public class _05PresentatonDevice : INotifyPropertyChanged
    {
        /// <summary>
        /// доступность устройства для работы
        /// </summary>
        private bool enable;
        public bool Enable
        {
            get { return enable; }
            set
            {
                enable = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Enable"));
            }
        }
        /// <summary>
        /// уник номер устройства
        /// </summary>
        private uint devGUID;
        public uint DevGUID
        {
            get { return devGUID; }
            set
            {
                devGUID = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DevGUID"));
            }
        }
        /// <summary>
        /// имя типа устройства
        /// </summary>
        private string deviceType;
        public string DeviceType
        {
            get { return deviceType; }
            set
            {
                deviceType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DeviceType"));
            }
        }
        /// <summary>
        /// группы устройства
        /// </summary>
        public List<PresentationTreeItems._06PresentatonGroup> Groups { get; set; }
        /// <summary>
        /// ссылка на устройство уровня hardware
        /// </summary>
        public DeviceHardware DeviceHardwareLink = null;

        /// <summary>
        /// ссылка на устройство уровня Native
        /// </summary>
        public Device DeviceNativeLink = null;

        /// <summary>
        /// ссылка на устройство уровня Present
        /// </summary>
        public PresentationConfigurationLib.PresentaionConfiguration._03Device DevicePresentLink = null;

        public _05PresentatonDevice()
        {
            Groups = new List<_06PresentatonGroup>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
