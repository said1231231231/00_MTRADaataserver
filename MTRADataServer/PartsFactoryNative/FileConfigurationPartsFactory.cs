/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: FileConfigurationPartsFactory - класс поддерживающий функциональность сборки конфигурации Native
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\PartsFactory\FileConfigurationPartsFactory.cs
 *Тип конечного файла      :                                         
 *версия ПО для разработки : С# 5.0, Framework 4.5                                
 *Разработчик              : Юров В.И.                                        
 *Дата начала разработки   : 05.11.2014
 *Дата посл. корр-ровки    : xx.хх.201х
 *Дата (v1.0)              :                                                  
 ******************************************************************************
* Легенда:
* В этом классе собраны функции сборки конфигурации Native на основе конфигурации проекта в файлах
 * В Native конфигурации уровни дерева когфигурации иные, это:
 *  конфигурация - корень конфигурации
 *  DS - dataserver'а
 *  Контроллер - для управления группами устройств. Учет устройств параллелен в DS и контролллере
 *  устройства - нумерация устройств как и раньше привязана к контроллеру, 0-е устройство на каждой ветке - сам контроллер
 *  группы (иерархия)
 *  теги 
 *  команды
*#############################################################################*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using NativeConfigurationLib;
using NativeConfigurationLib.NativeConfiguration;
using CommonClassesLib;
using CommonClassesLib.CommonClasses;
using LinksLib;
using MTRADataServer;
using InterfaceLibrary;

namespace MTRADataServer.PartsFactory
{
    public class FileConfigurationPartsFactory : ConfigurationPartsFactory
    {
        #region private-поля
        IProviderConfigurationNative PROVIDERCONFIGURATION;

        /// <summary>
        /// путь к файлу проекта Project.cfg
        /// </summary>
        string PathToPrjFile = string.Empty;
        /// <summary>
        /// путь к файлу проекта Configuration.cfg
        /// </summary>
        string PathToConfigurationFile = string.Empty;
        #endregion

        #region конструктор
        public FileConfigurationPartsFactory(IProviderConfigurationNative provconf)
        {
            PROVIDERCONFIGURATION = provconf;
            try
            {
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }            
        } 
	    #endregion

        #region инициализация конфигурации DataConfiguration
        /// <summary>
        /// инициализация конфигурации
        /// </summary>
        /// <param name="dc"></param>
        public override void CreateDataConfiguration(DataConfiguration dc)
        {
            try
            {
                PROVIDERCONFIGURATION.CreateProvider();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                Process.GetCurrentProcess().Kill();
            }
        }
        /// <summary>
        /// имя птк
        /// </summary>
        /// <returns></returns>
        public override void SetNamePTK(DataConfiguration dc)
        {
            try
            {                
                dc.NamePTK = PROVIDERCONFIGURATION.GetNamePTK();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }        
        #endregion

        #region инициализация DataServer
        public override void CreateDataserver(DataConfiguration dc)
        {
            DataServer ds = new DataServer();

            try
            {
                ds.DATACONFIGURATION = dc;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
            dc.DATASERVER = ds;
        }
        public override void SetDSName(DataServer ds)
        { 
            try
            {
                ds.NameDS_GUID = PROVIDERCONFIGURATION.Get_NameDS_GUID();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        public override void SetDSGuid(DataServer ds)
        { 
            try
            {
                ds.UniDS_GUID = PROVIDERCONFIGURATION.GetDSGuid();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion
        
        #region Информация об источниках - иницмализация DS
        /// <summary>
        /// создать конфигуарцию Native
        /// источников
        /// </summary>
        /// <param name="ds"></param>
        public override void CreateDataSource(DataServer ds)
        {
            try
            {
                PROVIDERCONFIGURATION.InitDataServerDescription(ds);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }    
        }
        #endregion
    }
}
