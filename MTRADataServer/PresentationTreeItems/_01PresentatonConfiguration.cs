/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: _01PresentatonConfiguration - класс представления конфигурации в интерфейсе пользователя
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PresentationTreeItems\_01PresentatonConfiguration.cs
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

namespace MTRADataServer.PresentationTreeItems
{
    public class _01PresentatonConfiguration : INotifyPropertyChanged
    {
        /// <summary>
        /// имя конфигурации проекта
        /// </summary>
        public string NamePTK 
        {
            get { return namePTK; }
            set
            {
                namePTK = value;
                OnPropertyChanged(new PropertyChangedEventArgs("NamePTK"));
            }
        }
        private string namePTK;

        /// <summary>
        /// DataServer
        /// </summary>
        public PresentationTreeItems._02PresentationDataServer DATASERVER;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
