/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DeviceHardware_MOA_ECU_BMRZDescrMOA - класс представления устройства БМРЗ в контроллере МОА
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DeviceHardware_MOA_ECU_BMRZDescrMOA.cs
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
using System.Xml.Linq;
using System.IO;
using HardwareConfigurationLib.HardwareConfiguration;

namespace uvs_MOA.HardwareConfiguration_MOA_ECU
{
    public class DeviceHardware_MOA_ECU_BMRZDescrMOA : DeviceHardware_MOA_ECU
    {
        #region создать тег с вычислением его длины
        public override TagHardware CreateTagHardware(XElement xe_tag)
        {
            TagHardware th = null;
            try
            {
                /*
                 * у расчетных тегов нет секции Device_level_Describe
                 * поэтому их нужно игнорировать
                 */
                if (xe_tag.Elements("Device_level_Describe").Count() != 0)
                {
                    th = new TagHardware();

                    Dictionary<string, string> dictsections = new Dictionary<string, string>();

                    var xesections = xe_tag.Element("Device_level_Describe").Elements();

                    foreach (var xesection in xesections)
                        dictsections.Add(xesection.Name.ToString(), xesection.Value);

                    //// формируем идентификатор - перебирая возможные варианты имен секций
                    //if (dictsections.ContainsKey("adr"))
                    //    th.TagGuid = uint.Parse(dictsections["adr"]);
                    //else if (dictsections.ContainsKey("regadr"))
                    //    th.TagGuid = uint.Parse(dictsections["regadr"]);
                    //else if (dictsections.ContainsKey("mtudp"))
                    //    th.TagGuid = uint.Parse(dictsections["mtudp"]);
                    //else
                    //    throw new Exception(string.Format(@"(243) ...\FileConfigurationPartsFactoryHardware.cs: CreateTagHardware() : секция adr не найдена."));

                    th.TagGuid = uint.Parse(xe_tag.Element("DataServer_level_Describe").Element("Address").Value);

                    // формируем тип - перебирая возможные варианты имен секций
                    if (dictsections.ContainsKey("bmrztype"))
                        th.TagType = dictsections["bmrztype"];
                    else if (dictsections.ContainsKey("regtype"))
                        th.TagType = dictsections["regtype"];
                    else if (dictsections.ContainsKey("type"))
                        th.TagType = dictsections["type"];
                    else
                        throw new Exception(string.Format(@"(243) ...\FileConfigurationPartsFactoryHardware.cs: CreateTagHardware() : секция bmrztype не найдена."));

                    // формируем имя - перебирая возможные варианты имен секций
                    if (dictsections.ContainsKey("tagname"))
                        th.TagName = dictsections["tagname"];
                    else if (dictsections.ContainsKey("tagcaption"))
                        th.TagName = dictsections["tagcaption"];
                    else if (dictsections.ContainsKey("name"))
                        th.TagName = dictsections["name"];
                    else
                        throw new Exception(string.Format(@"(243) ...\FileConfigurationPartsFactoryHardware.cs: CreateTagHardware() : секция tagname не найдена."));

                    // длина тега в регистрах modbus
                    // длина тега в байтах - т.к. длина в байтах задана в конфигурационных файлах для БМРЗ
                    switch (th.TagType)
                    {
                        //case "byte":
                        case "Bit_FieldMT":
                        case "BitField":
                            (th as TagHardware).VendorLengthTag = 1;
                            break;
                        case "Int":
                        case "UInt":
                        case "UDInt":
                        case "BCDPack":
                        case "BCD":
                        case "BCD_FieldMT":
                        case "UInt_FieldMT":

                        case "Byte_FieldMT":
                        case "BCDPack_FieldMT":
                            //case "u16":
                            (th as TagHardware).VendorLengthTag = 2;
                            break;
                        case "Real":
                        case "u32_data1970":
                        case "u32_data1970_reverse":
                        case "u32_ipV4":
                            //case "i32":
                            //case "u32":
                            (th as TagHardware).VendorLengthTag = 4;
                            break;
                        case "DateTime4_FieldMT":
                            (th as TagHardware).VendorLengthTag = 8;
                            break;
                        case "Stringz_FieldMT":
                            (th as TagHardware).VendorLengthTag = int.Parse(dictsections["length"]) * 2; // надо ли умножать на 2 ?
                            break;
                        default:
                            // строка?
                            if (th.TagType.Contains("text:"))
                                (th as TagHardware).VendorLengthTag = int.Parse(th.TagType.Split(new char[] { ':' })[1]); // / 2 в файле длина в байтах поэтому делим на 2
                            else if (th.TagType.Contains("string_wopairwise:"))
                                (th as TagHardware).VendorLengthTag = int.Parse(th.TagType.Split(new char[] { ':' })[1]); // / 2 в файле длина в байтах поэтому делим на 2
                            else
                            {
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                th = null;
            }
            return th;
        }        
        #endregion
    }
}
