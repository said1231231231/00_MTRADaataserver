/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DeviceHardware_MOA_ECU - класс представления конфигурации DS
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DeviceHardware_MOA_ECU.cs
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
using System.IO;
using System.Xml.Linq;
using HardwareConfigurationLib.HardwareConfiguration;
using CommonClassesLib.CommonClasses;
using uvs_MOA.MOA_ECU_SOURCE;

namespace uvs_MOA.HardwareConfiguration_MOA_ECU
{
    public class DeviceHardware_MOA_ECU : HardwareConfigurationLib.HardwareConfiguration.DeviceHardware
    {
        /// <summary>
        /// последние полученные пакеты
        /// </summary>
        public PacketStorage PACKETSTORAGE
        {
            get { return packetstorage; }
        }
        PacketStorage packetstorage = new PacketStorage();

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

                    // формируем идентификатор - перебирая возможные варианты имен секций
                    if (dictsections.ContainsKey("adr"))
                        th.TagGuid = uint.Parse(dictsections["adr"]);
                    else if (dictsections.ContainsKey("regadr"))
                        th.TagGuid = uint.Parse(dictsections["regadr"]);
                    else if (dictsections.ContainsKey("mtudp"))
                        th.TagGuid = uint.Parse(dictsections["mtudp"]);
                    else
                        throw new Exception(string.Format(@"(243) ...\FileConfigurationPartsFactoryHardware.cs: CreateTagHardware() : секция adr не найдена."));

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
                    switch (th.TagType)
                    {
                        case "UInt_FieldMT":
                            (th as TagHardware).VendorLengthTag = 1;
                            break;
                        case "IPAdress_FieldMT":
                            (th as TagHardware).VendorLengthTag = 2;
                            break;
                        default:
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

        /// <summary>
        /// разобрать пакет с данными 
        /// от устройства с учетом специфики
        /// разбор идет по байтам
        /// </summary>
        public virtual void ParsePacketRawData(byte[] packetbin)
        {
            uint key = 0;
            uint keyOff = 0;
            Int16 lenpack = 0;
            Int16 numdev = 0;
            byte[] tmpb;
            /*
             * последние 5 байт пакета - метка времени
             */
            const int lenTimeStamp = 5;

            try
            {
                MemoryStream ms = new MemoryStream(packetbin);
                BinaryReader binReader = new BinaryReader(ms);
                UInt32 timemarker;
                byte splitsecond100;
                DateTime timestamp;
                
                    tmpb = new byte[binReader.BaseStream.Length - lenTimeStamp];  // lenTimeStamp - метка времени
                    binReader.BaseStream.Read(tmpb, 0, (int)binReader.BaseStream.Length - lenTimeStamp);

                    // прочитали метку времени данных пакета (из конца пакета)
                    timemarker = binReader.ReadUInt32();
                    splitsecond100 = binReader.ReadByte();

                    timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    timestamp = timestamp.AddSeconds(timemarker);
                    DateTime timestampms = timestamp.AddMilliseconds(splitsecond100);

                    binReader.BaseStream.Position = 0;

                    lenpack = (short)binReader.ReadInt16();
                    numdev = binReader.ReadInt16();

                    key = (uint)binReader.ReadUInt16();	//читаем адрес первого блока    					

                    // запомним пакет
                    PACKETSTORAGE.AddPacket(Convert.ToUInt16(key), tmpb);

                    keyOff = key * 2;
                    if (key == 60000)
                    { 
                    }
                while (binReader.BaseStream.Position < binReader.BaseStream.Length)
                {
                    if (key == 60013)
                    {
                        byte cntpack = binReader.ReadByte();
                        if (cntpack == 0)
                        { 
                            //сброс всех устройств контролллера
                            DataControllerHardware dch =  this.DataControllerHardwareParent;
                            foreach (DeviceHardware dh in dch.ListDevice4DataController)
                                dh.ResetAllTagsToUndefinedStatus();
                        }
                    }
                    if (!dictTags4Parse.ContainsKey(keyOff))//this.varDev.Contains(key)
                    {
                        /*
                         * если блока с адресом key нет в таблице raw-тегов
                         * то пропускаем 1 байт, 
                         * увеличиваем key на 1 и опять пытаемся сопоставить 
                         * адрес и содержимое конфигурации сетевого уровня
                         */
                        keyOff++;	// key меняется кратно размерности регистров modbus
                         binReader.ReadBytes(1);	// пропускаем 1 байт - не регистр модбас
                         continue;
                    }
                    /*
                     * если регистр есть в списке то 
                     * нужно прочитать из потока количество байт
                     * соответсвующих типу тега и сформировать байтовый массив значения 
                     * время и качество
                     */

                    byte[] memx = new byte[this.dictTags4Parse[keyOff].VendorLengthTag]; // * 2

                    binReader.Read(memx, 0, this.dictTags4Parse[keyOff].VendorLengthTag); // * 2

                    this.dictTags4Parse[keyOff].SetTagValue(memx, ProjectCommonData.VarQuality.vqGood);

                    //this.dictTags4Parse[keyOff].TagValue = memx;
                    //    // качество
                    //this.dictTags4Parse[keyOff].TagQuality = ProjectCommonData.VarQuality.vqGood;
                    //    // метка времени
                    //this.dictTags4Parse[keyOff].TimeStamp = DateTime.Now;//timestampms;

                        keyOff += (uint)this.dictTags4Parse[keyOff].VendorLengthTag; // * 2
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
