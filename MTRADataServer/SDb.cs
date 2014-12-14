using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using NativeConfigurationLib.NativeConfiguration;
using HardwareConfigurationLib.HardwareConfiguration;
using CommonClassesLib.CommonClasses;
using MTRADataServer.PresentationTreeItems;
using PresentationConfigurationLib.PresentaionConfiguration;

namespace MTRADataServer
{
    public class SDb
    {
        #region сборка конфигурации hardware
        /// <summary>
        /// сборка конфигурации hardware
        /// </summary>
        /// <param name="dsh"></param>
        /// <returns></returns>
        public ICollection<PresentationTreeItems._02PresentationDataServer> Get_DS_Devs_Tags(DataServerHardware dsh)
        {
            ObservableCollection<PresentationTreeItems._02PresentationDataServer> dataservers = new ObservableCollection<PresentationTreeItems._02PresentationDataServer>();

            try
            {
                PresentationTreeItems._02PresentationDataServer pds = new PresentationTreeItems._02PresentationDataServer();
                pds.UniDS_GUID = dsh.UniDS_GUID;

                foreach (DataSourceHardware dssh in dsh.DATASOURCES)
                {
                    PresentationTreeItems._03PresentatonDataSource pdss = new PresentationTreeItems._03PresentatonDataSource();
                    pdss.NameSourceDriver = dssh.NameSourceDriver;
                    pdss.SrcGuid = dssh.SrcGuid;

                    foreach (DataControllerHardware dch in dssh.ListDataControllerHardware)
                    {
                        PresentationTreeItems._04PresentatonController pc = new PresentationTreeItems._04PresentatonController();
                        pc.ObjectGUID = dch.ObjectGUID;
                        pc.СontrollerNumber = dch.СontrollerNumber;

                        foreach (DeviceHardware devh in dch.ListDevice4DataController)
                        {
                            PresentationTreeItems._05PresentatonDevice pd = new PresentationTreeItems._05PresentatonDevice();
                            pd.DevGUID = devh.DevGUID;
                            pd.DeviceType = devh.DeviceType;
                            pd.Enable = devh.Enable;
                            pd.DeviceHardwareLink = devh;

                            pc.ListDevice4DataController.Add(pd);
                        }

                        pdss.ListDataControllerHardware.Add(pc);
                    }

                    pds.DATASOURCES.Add(pdss);
                }

                dataservers.Add(pds);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                dataservers = null;
            }

            return dataservers;
        }        
        #endregion

        #region сборка конфигурации native
        /// <summary>
        /// сборка конфигурации native
        /// </summary>
        /// <param name="dsh"></param>
        /// <returns></returns>
        public ICollection<PresentationTreeItems._02PresentationDataServer> Get_DS_Devs_Tags_Native(DataServer dsn)
        {
            ObservableCollection<PresentationTreeItems._02PresentationDataServer> dataservers = new ObservableCollection<PresentationTreeItems._02PresentationDataServer>();

            try
            {
                PresentationTreeItems._02PresentationDataServer pds = new PresentationTreeItems._02PresentationDataServer();
                pds.UniDS_GUID = dsn.UniDS_GUID;

                foreach (Device devn in dsn.ListDevice4DS)
                {
                        PresentationTreeItems._05PresentatonDevice pd = new PresentationTreeItems._05PresentatonDevice();
                        pd.DevGUID = devn.DevGUID;
                        pd.DeviceType = devn.DeviceType;
                        pd.Enable = devn.Enable;
                        pd.DeviceNativeLink = devn;

                        // добавляем группы
                        foreach (Group grn in devn.Groups)
                        {
                            PresentationTreeItems._06PresentatonGroup pg = new PresentationTreeItems._06PresentatonGroup();
                            pg.Enable = grn.Enable;
                            pg.GroupCategory = grn.GroupCategory;
                            pg.GroupGUID = grn.GroupGUID;
                            pg.GroupName = grn.GroupName;
                            pg.ThisDevice = pd;

                            if (grn.SubGroupList.Count > 0)
                                CreateSubGroups(pg, grn.SubGroupList);

                            pg.TagList = new ObservableCollection<PresentationTreeItems._07PresentatonTag>();
                            CreateTagListInNativeGroup(grn, pg);

                            pd.Groups.Add(pg);
                        }

                        pds.ListDevices4ThisDS.Add(pd);
                }
                dataservers.Add(pds);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                dataservers = null;
            }

            return dataservers;
        }
        /// <summary>
        /// добваить теги в группу
        /// </summary>
        /// <param name="grn"></param>
        /// <param name="pg"></param>
        private void CreateTagListInNativeGroup(Group grn, PresentationTreeItems._06PresentatonGroup pg)
        {
            try
            {
                foreach( Tag tg in grn.TagList )
                {
                    PresentationTreeItems._07PresentatonTag ptag = new PresentationTreeItems._07PresentatonTag();
                    ptag.TagGuid = tg.TagGUID;
                    ptag.TagName = tg.TagName;
                    ptag.TagQuality = tg.TagQuality;
                    ptag.TagType = tg.TypeTag;
                    if (tg.TagValue != null)
                        ptag.TagValue = tg.TagValue;
                    ptag.TimeStamp = tg.TimeStamp;

                    pg.TagList.Add(ptag);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// создать подгруппы
        /// </summary>
        /// <param name="pg"></param>
        /// <param name="subgr"></param>
        private void CreateSubGroups(PresentationTreeItems._06PresentatonGroup pg, List<Group> list)
        {
            try
            {
                // добавляем группы
                foreach (Group grn in list)
                {
                    PresentationTreeItems._06PresentatonGroup spg = new PresentationTreeItems._06PresentatonGroup();
                    spg.Enable = grn.Enable;
                    spg.GroupCategory = grn.GroupCategory;
                    spg.GroupGUID = grn.GroupGUID;
                    spg.GroupName = grn.GroupName;
                    spg.ThisDevice = pg.ThisDevice;

                    if (grn.SubGroupList.Count > 0)
                        CreateSubGroups(spg, grn.SubGroupList);

                    pg.TagList = new ObservableCollection<PresentationTreeItems._07PresentatonTag>();
                    CreateTagListInNativeGroup(grn, spg);

                    pg.SubGroupList.Add(spg);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        } 
	    #endregion

        #region сборка конфигурации представления DS
        /// <summary>
        /// сборка конфигурации native
        /// </summary>
        /// <param name="dsh"></param>
        /// <returns></returns>
        public ICollection<PresentationTreeItems._02PresentationDataServer> Get_DS_Devs_Tags_Present(_01Configuration PC)
        {
            ObservableCollection<PresentationTreeItems._02PresentationDataServer> dataservers = new ObservableCollection<PresentationTreeItems._02PresentationDataServer>();

            try
            {
                foreach( _02DataServer dsn in PC.LstDataServers )
                {
                    PresentationTreeItems._02PresentationDataServer pds = new PresentationTreeItems._02PresentationDataServer();

                    pds.UniDS_GUID = dsn.UniDS_GUID.ToString();

                    foreach (_03Device devn in dsn.LstDevice)
                    {
                        PresentationTreeItems._05PresentatonDevice pd = new PresentationTreeItems._05PresentatonDevice();
                        pd.DevGUID = devn.ObjectGUID;
                        pd.DeviceType = devn.DescriptInfo_DeviceType;
                        pd.Enable = devn.Enable;
                        pd.DevicePresentLink = devn;

                        // добавляем группы
                        foreach (_04Group grn in devn.LstGroups)
                        {
                            PresentationTreeItems._06PresentatonGroup pg = new PresentationTreeItems._06PresentatonGroup();
                            pg.Enable = grn.Enable;
                            pg.GroupCategory = grn.Category.ToString();
                            pg.GroupGUID = grn.GroupGUID;
                            pg.GroupName = grn.Name;
                            pg.ThisDevice = pd;

                            if (grn.LstSubGroups.Count > 0)
                                CreateSubGroupsPresent(pg, grn.LstSubGroups);

                            pg.TagList = new ObservableCollection<PresentationTreeItems._07PresentatonTag>();
                            CreateTagListInPresentGroup(grn, pg);

                            pd.Groups.Add(pg);
                        }

                        pds.ListDevices4ThisDS.Add(pd);
                    }
                    dataservers.Add(pds);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
                dataservers = null;
            }

            return dataservers;
        }
        /// <summary>
        /// добваить теги в группу
        /// </summary>
        /// <param name="grn"></param>
        /// <param name="pg"></param>
        private void CreateTagListInPresentGroup(_04Group grn, PresentationTreeItems._06PresentatonGroup pg)
        {
            try
            {
                foreach (_05Tag tg in grn.LstTags)
                {
                    PresentationTreeItems._07PresentatonTag ptag = new PresentationTreeItems._07PresentatonTag();
                    ptag.TagGuid = tg.TagGUID;
                    ptag.TagName = tg.Name;
                    ptag.TagQuality = tg.TagQuality;
                    ptag.TagType = tg.TagType;
                    if (tg.TagValue != null)
                        ptag.TagValue = tg.TagValue;
                    ptag.TimeStamp = tg.TimeStamp;

                    pg.TagList.Add(ptag);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// создать подгруппы
        /// </summary>
        /// <param name="pg"></param>
        /// <param name="subgr"></param>
        private void CreateSubGroupsPresent(PresentationTreeItems._06PresentatonGroup pg, List<_04Group> list)
        {
            try
            {
                // добавляем группы
                foreach (_04Group grn in list)
                {
                    PresentationTreeItems._06PresentatonGroup spg = new PresentationTreeItems._06PresentatonGroup();
                    spg.Enable = grn.Enable;
                    spg.GroupCategory = grn.Category.ToString();
                    spg.GroupGUID = grn.GroupGUID;
                    spg.GroupName = grn.Name;
                    spg.ThisDevice = pg.ThisDevice;

                    if (grn.LstSubGroups.Count > 0)
                        CreateSubGroupsPresent(spg, grn.LstSubGroups);

                    pg.TagList = new ObservableCollection<PresentationTreeItems._07PresentatonTag>();
                    CreateTagListInPresentGroup(grn, spg);

                    pg.SubGroupList.Add(spg);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

    }
}
