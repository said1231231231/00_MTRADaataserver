/*#############################################################################
 *    Copyright (C) 2014 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: DeviceXDocument - класс для работы с файлом описания конфигурации проекта уровня предсталвения
 *                                                                             
 *	Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\IntermediateDescriptionConfiguration_Classes\DeviceXDocument.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.5                              
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : хх.хх.2014 
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace MTRADataServer.IntermediateDescriptionConfiguration_Classes
{
    public class DeviceXDocument
    {
        XDocument xdoc_dev = null;

        public DeviceXDocument(string path2Device_cfg)
        {
            
        }

        /// <summary>
        /// получить имя проекта
        /// </summary>
        /// <returns></returns>
        public string Get_NamePTK()
        {
            string nameptk = string.Empty;

            try
            {
                nameptk = xdoc_dev.Element("MTRA").Element("ProjectInfo").Element("NamePTK").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return nameptk;
        }
        /// <summary>
        /// получить имя проекта
        /// </summary>
        /// <returns></returns>
        public string Get_DSRouterServiceAddress()
        {
            string DSRouterServiceAddress = string.Empty;

            try
            {
                DSRouterServiceAddress = xdoc_dev.Element("MTRA").Element("ProjectInfo").Element("DSRouterServiceAddress").Value;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return DSRouterServiceAddress;
        }        
        /// <summary>
        /// получить список номеров DataServer'ов
        /// </summary>
        /// <returns></returns>
        public List<uint> Get_LstDataServersGUIDs()
        {
            List<uint> lstDataServersGUIDs = new List<uint>();

            try
            {
                var xe_dss = xdoc_dev.Element("MTRA").Element("Configuration").Elements("Object");

                foreach (var xe_ds in xe_dss)
                    lstDataServersGUIDs.Add(uint.Parse(xe_ds.Attribute("UniDS_GUID").Value));
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }

            return lstDataServersGUIDs;
        }

        #region старые функции
		public XElement GetSpecificDeviceValues()
        {
            XElement xe_SpecificDeviceValues_Section = null;

            if (xdoc_dev.Element("Device").Elements("SpecificDeviceValues").Count() != 0)
                //xe_SpecificDeviceValues_Section = xdoc_dev.Element("Device").Element("SpecificDeviceValues");
                xe_SpecificDeviceValues_Section = new XElement(xdoc_dev.Element("Device").Element("SpecificDeviceValues"));

            return xe_SpecificDeviceValues_Section;
        }
        public XElement GetGroupsSection()
        {
            XElement xe_GroupsSection = null;

            xe_GroupsSection = new XElement(xdoc_dev.Element("Device").Element("Groups"));

            return xe_GroupsSection;
        }
        public XElement GetDescriptInfoSection()
        {
            XElement xe_DescriptInfoSection = null;

            xe_DescriptInfoSection = new XElement(xdoc_dev.Element("Device").Element("DescriptInfo"));

            return xe_DescriptInfoSection;
        }
        public bool TagsSectionExist()
        {
            return xdoc_dev.Element("Device").Elements("Tags").Count() > 0 ? true : false;
        }
        public List<XElement> GetTagsList()
        {
            List<XElement> taglist = new List<XElement>();

			try
			{
                foreach (XElement xetagg in xdoc_dev.Element("Device").Element("Tags").Elements("Tag"))
                    taglist.Add(new XElement(xetagg));
			}
			catch(Exception ex)
			{
				TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex );
			}

            return taglist;
        }
    	#endregion
    }
}
