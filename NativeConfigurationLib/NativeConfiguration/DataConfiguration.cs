/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Configuration - класс представления конфигурации DS
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\Configuration.cs
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
using HardwareConfigurationLib;

namespace NativeConfigurationLib.NativeConfiguration
{
    public abstract class DataConfiguration
    {
        /// <summary>
        /// имя конфигурации проекта
        /// </summary>
        public string NamePTK {get;set;}

        /// <summary>
        /// DataServer
        /// </summary>
        public DataServer DATASERVER;
        /// <summary>
        /// конфигурация уровня источников
        /// </summary>
        public HardwareConfigurationLib.HardwareConfiguration.DataConfigurationHardware _dataConfigurationHardware;

        public abstract void Configure();

        public DataConfiguration()
        {
            //LstLinksHT2NT = new List<LinksHT2NT.LinkHT2NTBase>();
        }

        /// <summary>
        /// получить устройство Hardware
        /// </summary>
        /// <param name="devguid"></param>
        /// <returns></returns>
        public Device GetDeviceByGUID(uint devguid)
        {
            Device dh = null;
            try
            {
                foreach (Device dha in this.DATASERVER.ListDevice4DS)
                    if (dha.DevGUID == devguid)
                    {
                        dh = dha;
                        break;
                    }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dh;
        }

        public struct CheckBoxId
        {
            public static string checkBoxId;
        }
    }
}
