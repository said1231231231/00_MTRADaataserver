/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _04PresentatonController - класс представления контроллера источника данных DS в интерфейсе пользователя
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentationTreeItems\_04PresentatonController.cs
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

namespace MTRADataServer.PresentationTreeItems
{
    public class _04PresentatonController : INotifyPropertyChanged
    {
        /// <summary>
        /// guid контроллера
        /// </summary>
        private string objectGUID;
        public string ObjectGUID
        {
            get { return objectGUID; }
            set
            {
                objectGUID = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ObjectGUID"));
            }
        }
        /// <summary>
        /// номер контроллера
        /// </summary>
        private string controllerNumber;
        public string СontrollerNumber
        {
            get { return controllerNumber; }
            set
            {
                controllerNumber = value;
                OnPropertyChanged(new PropertyChangedEventArgs("СontrollerNumber"));
            }
        }

        /// <summary>
        /// список устройств контроллера
        /// </summary>
        public List<PresentationTreeItems._05PresentatonDevice> ListDevice4DataController { get; set; }

        public _04PresentatonController()
        {
            ListDevice4DataController = new List<_05PresentatonDevice>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
