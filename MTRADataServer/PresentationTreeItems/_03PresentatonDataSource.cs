/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _03PresentatonDataSource - класс представления источника DS в интерфейсе пользователя
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentationTreeItems\_03PresentatonDataSource.cs
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
    public class _03PresentatonDataSource : INotifyPropertyChanged
    {
        /// <summary>
        /// имя DataServer
        /// </summary>
        private string srcGuid;
        public string SrcGuid
        {
            get { return srcGuid; }
            set
            {
                srcGuid = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SrcGuid"));
            }
        }
        /// <summary>
        /// имя DataServer
        /// </summary>
        private string nameSourceDriver;
        public string NameSourceDriver
        {
            get { return nameSourceDriver; }
            set
            {
                nameSourceDriver = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NameSourceDriver"));
            }
        }

        /// <summary>
        /// список контролллеров источника
        /// </summary>
        public List<PresentationTreeItems._04PresentatonController> ListDataControllerHardware { get; set; }

        public _03PresentatonDataSource()
        {
            ListDataControllerHardware = new List<_04PresentatonController>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
