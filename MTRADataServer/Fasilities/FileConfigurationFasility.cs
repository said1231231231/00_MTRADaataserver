/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: FileConfigurationFasility - класс конфигурировния из файлов
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Fasilities\FileConfigurationFasility.cs
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
using MTRADataServer.PartsFactory;
using MTRADataServer.PartsFactoryHardware;
using HardwareConfigurationLib.HardwareConfiguration;

namespace MTRADataServer.Fasilities
{
    class FileConfigurationFasility : ConfigurationFasility
    {
        protected override DataConfiguration CreateConfiguration(string type)
        {
            try
            {
                if (type == "OldXMLFilesConfiguration")
                {
                    InterfaceLibrary.IProviderConfigurationHardware PROVIDERCONFIGURATIONHARDWARE = new ProviderConfiguration.OldXMLJile_MOA.ProviderConfigurationHardware_OldXMLFile_MOA();

                    // для формирования уровня Hardware
                    ConfigurationPartsFactoryHardware partsFactoryHardware = new FileConfigurationPartsFactoryHardware(PROVIDERCONFIGURATIONHARDWARE);
                    DataConfigurationHardware dcHardwareConfiguration = new FileDataConfigurationHardware(partsFactoryHardware);
                    dcHardwareConfiguration.Configure();

                    // конфигурирование уровня native
                    InterfaceLibrary.IProviderConfigurationNative PROVIDERCONFIGURATIONVATIVE = new ProviderConfiguration.OldXMLJile_MOA.ProviderConfigurationNative_OldXMLFile_MOA();
                    ConfigurationPartsFactory partsFactory = new FileConfigurationPartsFactory(PROVIDERCONFIGURATIONVATIVE);                    
                    DataConfiguration dcNativeConfiguration = new FileDataConfiguration(partsFactory);    

                    // связка конфигураций hardware и native
                    dcNativeConfiguration._dataConfigurationHardware = dcHardwareConfiguration;
                    dcNativeConfiguration.Configure();
                    // * и возвратить конфигурацию Native
                    // */
                    return dcNativeConfiguration;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return null;
        }
    }
}
