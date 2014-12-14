/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: FileDataConfiguration - класс представления конфигурации DS формируемый из 
 *                                  файлов (старое описание конфигурации)
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\Configuration\FileDataConfiguration.cs
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
//using MTRADataServer.PartsFactory;
//using MTRADataServer.PartsFactoryHardware;
using NativeConfigurationLib.NativeConfiguration;

namespace MTRADataServer.PartsFactory
{
    public class FileDataConfiguration : DataConfiguration
    {
        private ConfigurationPartsFactory _partsFactory;

        public FileDataConfiguration(ConfigurationPartsFactory partsFactory)
        { 
            try
            {
                _partsFactory = partsFactory;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// шаблон сборки конфигурации
        /// </summary>
        public override void Configure()
        {
            try
            {
                /*
                 * общий сценарий сборки конфигурации предполагается таким:
                 * Нужно собрать три связанных между собой конфигурации, каждая из который выполняет задачу своего уровня ответственности:
                 * HardwareConfiguration - задачи:
                 *                          представить данные устройств в терминах их поступления от контроллера - регистры modbus, типы мэк, типы opc ...
                 *                          локализовать логику общения с контроллером и заполнения тегов
                 *                          реализовать, если нужно, сохранение исторических данных в базу (журналы событий, осциллограммы, уставки, аварии и т.д.)
                 *                          запись уставок
                 * NativeConfiguration - назначение этого уровня - представить все теги (полную конфигурациив) в типах c#. Это делает возможным построить дерево тегов, которое будет служить основой 
                 *                              для инструментов уровня представления
                 * PresentaionConfiguration - уровень представления - строится на основе разбора DSConfig.cfg. На этом уровне осущ. поддержка расч тегов, функций, событий, тревог, сценариев, встроенного ЯВУ.
                 */

                // создаем корень HardwareConfiguration - конфигурацию проекта
                _partsFactory.CreateDataConfiguration(this);
                // имя проекта
                _partsFactory.SetNamePTK(this);
                // создаем DS
                _partsFactory.CreateDataserver(this);
                // имя и уник номер DS
                _partsFactory.SetDSName(this.DATASERVER);
                _partsFactory.SetDSGuid(this.DATASERVER);
                // источники DS
                _partsFactory.CreateDataSource(this.DATASERVER);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
