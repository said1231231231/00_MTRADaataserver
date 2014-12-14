/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: Configuration - класс представления конфигурации на уровне Presentation
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

namespace PresentationConfigurationLib.PresentaionConfiguration
{
    public class _01Configuration
    {
        /// <summary>
        /// имя ПТК
        /// </summary>
        public string NamePTK { get; set; }
        /// <summary>
        /// DSRouterServiceAddress
        /// </summary>
        /// <returns></returns>
        public string DSRouterServiceAddress { get; set; }

        public List<_02DataServer> LstDataServers { get; set; }

        public _01Configuration()
        {
            LstDataServers = new List<_02DataServer>();
        }
        /// <summary>
        /// получить устройство Presentation
        /// </summary>
        /// <param name="devguid"></param>
        /// <returns></returns>
        public _03Device GetDeviceByGUID(uint devguid)
        {
            _03Device dh = null;
            try
            {
                foreach ( _02DataServer _02ds in this.LstDataServers )
                    foreach (_03Device dha in _02ds.LstDevice)
                        if (dha.ObjectGUID == devguid)
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
    }
}
