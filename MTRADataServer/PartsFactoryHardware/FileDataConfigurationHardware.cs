using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
//using MTRADataServer.PartsFactoryHardware;
using System.IO;
using HardwareConfigurationLib.HardwareConfiguration;

namespace MTRADataServer.PartsFactoryHardware
{
    public class FileDataConfigurationHardware : DataConfigurationHardware
    {
        private PartsFactoryHardware.ConfigurationPartsFactoryHardware _partsFactoryHardware;

        public FileDataConfigurationHardware(PartsFactoryHardware.ConfigurationPartsFactoryHardware partsFactoryHardware)
        { 
            try
            {
                _partsFactoryHardware = partsFactoryHardware;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// шаблон сборки конфигурации полевого уровня
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
                _partsFactoryHardware.CreateDataConfiguration(this);
                // создаем DS
                _partsFactoryHardware.CreateDataserver(this);
                // уник номер DS
                _partsFactoryHardware.SetDSGuid(this.DATASERVER);
                // источники DS
                _partsFactoryHardware.CreateDataSource(this.DATASERVER);

                DATASERVER.StartDataCommunicationExchange();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
