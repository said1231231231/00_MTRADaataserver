/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: 02PresentationDataServer - класс представления конфигурации DS в интерфейсе пользователя
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentationTreeItems\02PresentationDataServer.cs
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

namespace MTRADataServer.PresentationTreeItems
{
    public class _02PresentationDataServer : INotifyPropertyChanged
    {
        /// <summary>
        /// уник номер DataServer
        /// </summary>
        private string uniDS_GUID;
        public string UniDS_GUID 
        {
            get { return uniDS_GUID; }
            set
            {
                uniDS_GUID = value;
                OnPropertyChanged(new PropertyChangedEventArgs("UniDS_GUID"));
            }
        }

        /// <summary>
        /// СПИСОК источников данных (согласно их природе)
        /// </summary>
        public List<PresentationTreeItems._03PresentatonDataSource> DATASOURCES = new List<PresentationTreeItems._03PresentatonDataSource>();

        /// <summary>
        /// СПИСОК устройств 
        /// </summary>
        public List<PresentationTreeItems._05PresentatonDevice> ListDevices4ThisDS = new List<PresentationTreeItems._05PresentatonDevice>();

        public _02PresentationDataServer()
        {
            DATASOURCES = new List<_03PresentatonDataSource>();
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
