/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: IntermediaDescriptionConfiguration - класс доступа к промежуточной конфигурации представления
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\IntermediateDescriptionConfiguration_Classes\IntermediaDescriptionConfiguration.cs
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
using InterfaceLibrary;

namespace MTRADataServer.IntermediateDescriptionConfiguration_Classes
{
    public class IntermediaDescriptionConfiguration
    {
        /// <summary>
        /// создать промежуточное представление 
        /// конфигурации
        /// </summary>
        /// <param name="typecfgsrc">тип источника с конфиг информацией</param>
        public IIntermediaDescription Create_IntermediaDescriptionConfiguration(string typecfgsrc)
        {
            IIntermediaDescription iidescr = null;

            try
            {
                switch (typecfgsrc)
                {
                    case "DSConfigInFiles": // создание промежуточной конфигурации по файлу DSConfig.cfg
                        iidescr = new PresentationConfiguration_DSConfigFile();
                        break;
                    default:
                        return null;
                }

                // инициализация конфигурации представления
                iidescr.InitPresentationConfig();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            return iidescr;
        }
    }
}
