/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: IProviderConfiguration4Sources - реализация интерфейса конфигурирования источника
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\InterfaceLibrary\IProviderConfiguration4Sources.cs
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
using HardwareConfigurationLib.HardwareConfiguration;

namespace InterfaceLibrary
{
    public interface IProviderConfiguration4HardwareSources
    {
        /// <summary>
        /// Создать источник
        /// </summary>
        /// <param name="name">имя источника</param>
        /// <returns></returns>
        DataSourceHardware CreateDataSourceHardware(string name);
        /// <summary>
        /// инициализация контроллеров
        /// </summary>
        /// <param name="dsh"></param>
        void SetDataSourceController(DataSourceHardware dsh);
        /// <summary>
        /// Инициализация устройств
        /// </summary>
        /// <param name="dch"></param>
        void SetDevices4DataController(DataControllerHardware dch);
        /// <summary>
        /// инициализация тегов устройств
        /// </summary>
        /// <param name="dev"></param>
        void CreateDeviceTags(DeviceHardware dev);
    }
}
