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
using NativeConfigurationLib.NativeConfiguration;

namespace MTRADataServer.Fasilities
{
    abstract class ConfigurationFasility
    {
        public DataConfiguration GetConfiguration(string type)
        {
            DataConfiguration dataConfiguration = null;

            try
            {
                dataConfiguration = CreateConfiguration(type);

                //// настроим ссылки
                //foreach (LinksHT2NT.LinkHT2NTBase lht2nt in  LstLinksHT2NT)
                //    lht2nt.CreateLink();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return dataConfiguration;
        }

        protected abstract DataConfiguration CreateConfiguration(string type);
    }
}
