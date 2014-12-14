/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: IProviderConfigurationPresentation - интерфейс конфигурирования уровня предсталвения
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\InterfaceLibrary\IProviderConfigurationPresentation.cs
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
using NativeConfigurationLib.NativeConfiguration;

namespace InterfaceLibrary
{
    public interface IProviderConfigurationPresentation
    {
        /// <summary>
        /// создать инициализировать 
        /// провайдер
        /// </summary>
        void CreateProvider();
    }
}
