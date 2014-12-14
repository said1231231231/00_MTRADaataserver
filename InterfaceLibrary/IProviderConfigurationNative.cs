/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: IProviderConfigurationNative - реализация интерфейса конфигурирования DS уровня
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\InterfaceLibrary\IProviderConfigurationNative.cs
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

namespace InterfaceLibrary
{
    public interface IProviderConfigurationNative
    {
        /// <summary>
        /// создать инициализировать 
        /// провайдер
        /// </summary>
        void CreateProvider();
        /// <summary>
        /// имя птк от провайдера
        /// </summary>
        string GetNamePTK();
        /// <summary>
        /// имя DS от провайдера
        /// </summary>
        /// <returns></returns>
        string Get_NameDS_GUID();
        /// <summary>
        /// уник номер DS
        /// </summary>
        /// <returns></returns>
        string GetDSGuid();
        /// <summary>
        /// инициализировать описание DataServer
        /// </summary>
        /// <returns></returns>
        void InitDataServerDescription(NativeConfigurationLib.NativeConfiguration.DataServer ds);
    }
}
