/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *Описание: DeviceHardware_MOA_ECU_BMRZ_100_DescrMOA - класс представления устройства БМРЗ-100 в контроллере МОА
 *                                                                             
 *Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DeviceHardware_MOA_ECU_BMRZ_100_DescrMOA.cs
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
    public class DeviceHardware_MOA_ECU_BMRZ_100_DescrMOA : DeviceHardware_MOA_ECU
    {
        #region создание тега с вычислением длины
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
                    if (xe_tag.Attribute("TagGUID").Value == "1966081")
                    { 
                    }
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

                    /*
                     *  длина тега в байтах - 
                     *  для БМРЗ-100, сама длина указана 
                     *  в size секции Device_level_Describe, 
                     *  поэтому сначала пытаемся честно 
                     *  оттуда прочитать длину:
                     *  длина в файле описания в size секции Device_level_Describe
                     *  указана в БАЙТАХ - это означает что и файл устройства при разборе пакетов тоже 
                     *  должен это учитывать
                     */

                    if (!dictsections.ContainsKey("size"))
                        (th as TagHardware).VendorLengthTag = GetLengthAsHandleValue(th);
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(dictsections["size"]))
                            (th as TagHardware).VendorLengthTag = int.Parse(dictsections["size"]);  // в БАЙТАХ
                        else // пытаемся руками
                            (th as TagHardware).VendorLengthTag = GetLengthAsHandleValue(th);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="th"></param>
        /// <returns>для БМРЗ 100 - возрвращаемое значение - в БАЙТАХ</returns>
        private int GetLengthAsHandleValue(TagHardware th)
        {
            // для БМРЗ 100 - возрвращаемое значение - в БАЙТАХ
            int length = 0; 
            try
            {
                switch (th.TagType)
                {
                    case "byte":
                        (th as TagHardware).VendorLengthTag = 1;
                        break;
                    case "BitField":
                    case "UInt":
                        (th as TagHardware).VendorLengthTag = 2;
                        break;
                    case "UDInt":
                    case "u32_data1970":
                    case "u32_ipV4":
                        (th as TagHardware).VendorLengthTag = 4;
                        break;
                    default:
                        // строка?
                        if (th.TagType.Contains("text:"))
                            (th as TagHardware).VendorLengthTag = int.Parse(th.TagType.Split(new char[] { ':' })[1]); // в файле длина в байтах
                        //else if (th.TagType.Contains("string_wopairwise:"))
                        //    (th as TagHardware_MOA_ECU).VendorLengthTag = int.Parse(th.TagType.Split(new char[] { ':' })[1]); // в файле длина в байтах
                        else                        
                            throw new Exception(string.Format(@"(163) X:\Projects\00_MTRADataServer\MTRADataServer\HardwareConfiguration\DeviceHardware_MOA_ECU_BMRZ_100_DescrMOA.cs: GetLengthAsHandleValue() : не удалось определить размер тега TagGuid = {0}.", th.TagGuid));

                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                throw ex;
            }
            return length;
        } 
	    #endregion    
        /// <summary>
        /// разобрать пакет с данными 
        /// от устройства с учетом специфики
        /// разбор идет по байтам
        /// </summary>
        public override void ParsePacketRawData(byte[] packetbin)
        {
            uint key = 0;
            uint keyOff = 0;
            Int16 lenpack = 0;
            Int16 numdev = 0;
            byte[] arrval;
            UInt32 cntb4read;
            byte[] tmpb;
            /*
             * последние 5 байт пакета - метка времени
             */
            const int lenTimeStamp = 5;

            try
            {
                MemoryStream ms = new MemoryStream(packetbin);
                BinaryReader binReader = new BinaryReader(ms);

                tmpb = new byte[binReader.BaseStream.Length - lenTimeStamp];  // lenTimeStamp - метка времени
                binReader.BaseStream.Read(tmpb, 0, (int)binReader.BaseStream.Length - lenTimeStamp);

                // прочитали метку времени данных пакета (из конца пакета)
                UInt32 timemarker = binReader.ReadUInt32();
                byte splitsecond100 = binReader.ReadByte();

                DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                timestamp = timestamp.AddSeconds(timemarker);
                DateTime timestampms = timestamp.AddMilliseconds(splitsecond100);

                binReader.BaseStream.Position = 0;

                lenpack = (short)binReader.ReadInt16();
                numdev = binReader.ReadInt16();

                key = (uint)binReader.ReadUInt16();	//читаем адрес первого блока    					

                // запомним пакет
                PACKETSTORAGE.AddPacket(Convert.ToUInt16(key), tmpb);

                //key = key * 2;
                keyOff = key;

                do
                {
                    if (!dictTags4Parse.ContainsKey(key))//this.varDev.Contains(key)
                    {
                        /*
                         * если блока с адресом key нет в таблице raw-тегов
                         * то пропускаем 1 байт, 
                         * увеличиваем key на 1 и опять пытаемся сопоставить 
                         * адрес и содержимое конфигурации сетевого уровня
                         */
                        //TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(TraceEventType.Error, 196, string.Format("{0} : {1} : {2} : Несуществующий адрес ModBus = {0}",
                        //    DateTime.Now.ToString(), "RTU_MOA.cs", "ParsePacketRawData()", key.ToString()));
                        key++;	// key меняется кратно размерности регистров modbus
                        binReader.ReadBytes(1);	// пропускаем 1 байт
                        continue;
                    }
                    /*
                     * если регистр есть в списке то 
                     * нужно прочитать из потока количество байт
                     * соответсвующих типу тега и сформировать байтовый массив значения 
                     * время и качество
                     */

                    #region старый код
                    ///*
                    // * если адрес есть, то 
                    // * ассоциируем его с соответсвующим тегом
                    // * и передаем содержимое байтового массива 
                    // * из потока на обработку классу соотв тега
                    // */

                    //lsttag = (varDev[key] as Tuple<UInt32, List<ITag>>).Item2;

                    //cntb4read = (varDev[key] as Tuple<UInt32, List<ITag>>).Item1;//* 2

                    //arrval = binReader.ReadBytes((int)cntb4read);
                    //if (arrval.Length == 0)
                    //    break;

                    //if (key == 864)
                    //    key = 864;

                    //foreach (ITag tag in lsttag)
                    //{
                    //    VarQuality vq = DeviceStateConnection ? VarQuality.vqGood : VarQuality.vqUndefined;
                    //    tag.SetTagValue(arrval, timestampms, vq); // обработка байтового массива значения тега из потока
                    //}

                    //// для презантации определяем
                    //if (key == 120026 && !HMI_MT_Settings.HMI_Settings.PTKMode.IsDemoVersion)
                    //    TestDevConnection();

                    //key += (int)(varDev[key] as Tuple<UInt32, List<ITag>>).Item1; 
                    #endregion

                } while (((key - keyOff)) < (lenpack - 6/*6*/));//  * 2 длина пакета без учета полей длины пакета, номера устройства и первого адреса поля (((key - keyOff) * 2) < (lenpack - 6))
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
    }
}
