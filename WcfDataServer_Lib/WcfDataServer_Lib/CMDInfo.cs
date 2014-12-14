/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: CMDInfo - класс для хранения информации о команде для постановки ее в очередь команд
 *                      и последующего запуска
 *                                                                             
 *Файл                     : X:\Projects\00_DataServer\WcfDataServer_Lib\WcfDataServer_Lib\CMDInfo.cs
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

namespace WcfDataServer_Lib
{
    public class CMDInfo
    {
        public UInt16 numksdu{get;set;}
        public uint numvtu {get;set;}
        public uint tagguid {get;set;}
        public byte[] arr {get;set;}
        public string idDSRouterSession { get; set; }

        public CMDInfo(UInt16 numksdu, uint numvtu, uint tagguid, byte[] arr, string idDSRouterSession)
        { 
            try
            {
                this.numksdu = numksdu;
                this.numvtu = numvtu;
                this.tagguid = tagguid;
                this.arr = arr;
                this.idDSRouterSession = idDSRouterSession;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }      
        }
    }
}
