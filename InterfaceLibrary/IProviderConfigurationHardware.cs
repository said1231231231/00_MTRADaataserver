/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: IProviderConfiguration - итерфейс абстрагирования данных конфигурации
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\IProviderConfiguration.cs
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

namespace InterfaceLibrary
{
    public interface IProviderConfigurationHardware
    {
        /// <summary>
        /// создать инициализировать 
        /// провайдер
        /// </summary>
        void CreateProvider();
        /// <summary>
        /// идентификатор DS
        /// </summary>
        /// <returns></returns>
        string Get_UniDS_GUID();
        /// <summary>
        /// Создать источники
        /// </summary>
        /// <returns></returns>
        List<HardwareConfigurationLib.HardwareConfiguration.DataSourceHardware> GetDataSources(HardwareConfigurationLib.HardwareConfiguration.DataServerHardware ds);
    }
}
