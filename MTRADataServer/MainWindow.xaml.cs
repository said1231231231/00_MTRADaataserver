/*#############################################################################
 *    Copyright (C) 2006-2011 Mehanotronika RA                            
 *    All rights reserved.                                                     
 *	~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *                                                                             
 *	Описание: Главное окно DataServer
 *                                                                             
 *	Файл                     : X:\Projects\00_MTRADataServer\MTRADataServer\MainWindow.xaml.cs
 *	Тип конечного файла      :                                         
 *	версия ПО для разработки : С#, Framework 4.0                                
 *	Разработчик              : Юров В.И.                                        
 *	Дата начала разработки   : 01.09.2011
 *	Дата посл. корр-ровки    : xx.хх.201х
 *	Дата (v1.0)              :                                                  
 ******************************************************************************
* Особенности реализации:
 * Используется ...
 *#############################################################################*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using MTRADataServer.Fasilities;
using HardwareConfigurationLib.HardwareConfiguration;
using NativeConfigurationLib.NativeConfiguration;
using PresentationConfigurationLib.PresentaionConfiguration;
namespace MTRADataServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region private
        /// <summary>
        /// мьютекс для определения единственности 
        /// запущенного экземпляра DS
        /// </summary>
        private static Mutex _syncObj;
        /// <summary>
        /// уник идентификатор для поддержания 
        /// механизма определения единственности
        /// экземпляра DS
        /// </summary>
        private const string _syncObjName = "{E663FA11-AE0D-480e-9FCA-4BE9B8CDB4E7}";
        //private ObservableCollection<Node> Nodes { get; set; }
        /// <summary>
        /// конфигурация
        /// </summary>
        DataConfiguration _dataConfiguration;
        /// <summary>
        /// Конфигурация уровня презентации
        /// </summary>
        _01Configuration PRESENTATIONCONFIGURATION;
        #endregion

        /// <summary>
        /// список соответсвия объектов дерева уник идентификаторам 
        /// для возможности идентификации узлов дерева
        /// </summary>
        private Dictionary<string, Tuple<Node, object>> DictionaryTreeViewObject = new Dictionary<string, Tuple<Node, object>>();
        /// <summary>
        /// Конфигурируемое устройство
        /// </summary>
        //ConvertDevice_Lib.Device dev = null;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //проверка записи одной копии программы
                TestExistDS();

                MainWindowInit();
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// инициализация конфигурации
        /// </summary>
        private void MainWindowInit()
        {
            try
            {
                StartTrace();

                ConfigurationFasility facility = new FileConfigurationFasility();

                // формируем конфигурацию
                _dataConfiguration = facility.GetConfiguration("OldXMLFilesConfiguration");

                // конфигурация уровня представления
                PresentationConfigurationFasility presentationfasility = new PresentationConfigurationFasility (_dataConfiguration);

                // фабрика для формирования конфигурации представления - из файла DSConfig или из базы
                PRESENTATIONCONFIGURATION = presentationfasility.GetConfiguration("DSConfigInFiles");

                // настроим ссылки для канала Hardware-Native
                foreach (LinksLib.LinksHT2NT.LinkHT2NTBase lht2nt in MTRADataServer.App.LstLinksHT2NT)
                    lht2nt.CreateLink();

                // настроим ссылки для канала Native-Presentation
                foreach (LinksLib.LinksNT2PT.LinkNT2PTBase lnt2pt in MTRADataServer.App.LstLinksNT2PT)
                    lnt2pt.CreateLink();    // здесь же настраиваем и формулы

                // запустить источники на обмен данными
                //_dataConfiguration._dataConfigurationHardware.DATASERVER.StartDataCommunicationExchange();

                // формируем деревья конфигураций hardware и native на основе описания конфигурации _dataConfiguration
                #region заполняем дерево hardware
                ObservableCollection<Node> Nodes = new ObservableCollection<Node>();
                FillingTree(Nodes);
                TVIMainHardware.ItemsSource = Nodes;
                //TVIMainHardware.Focus(); 
                #endregion
                #region заполняем дерево native
                ObservableCollection<Node> NodesNative = new ObservableCollection<Node>();
                FillingTreeNative(NodesNative);
                TVIMainNative.ItemsSource = NodesNative;
                #endregion
                #region заполняем дерево представления
                ObservableCollection<Node> NodesPresent = new ObservableCollection<Node>();
                FillingTreePresent(NodesPresent);
                TVIMainPresent.ItemsSource = NodesPresent;
                #endregion
 
                // настроим каналы
                foreach (LinksLib.LinksHT2NT.LinkHT2NTBase linkHT2NT in MTRADataServer.App.LstLinksHT2NT)
                    linkHT2NT.OnChangeTagNT += linkHT2NT.TAGND.linkHT2NT_OnChangeTagNT;
                foreach (LinksLib.LinksNT2PT.LinkNT2PTBase lnt2pt in MTRADataServer.App.LstLinksNT2PT)
                    lnt2pt.OnChangeTagPT += lnt2pt.TAGPL.linkNT2PT_OnChangeTagPT;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// заполнить дерево hardware
        /// </summary>
        private void FillingTree(ObservableCollection<Node> Nodes)
        {
            Nodes.Clear();

            ICollection<PresentationTreeItems._02PresentationDataServer> dscollection = App.SDB.Get_DS_Devs_Tags(_dataConfiguration._dataConfigurationHardware.DATASERVER);

            if (dscollection == null)
                throw new Exception(string.Format(@"(146) ...\MTRADataServer\MainWindow.xaml.cs: FillingTree() : Не удалось инициализировать элемент TreeView."));

            try
            {
                foreach (PresentationTreeItems._02PresentationDataServer dsh in dscollection)
                {
                    var level_1_items = new Node() { Text = dsh.UniDS_GUID };   //.NameDS_GUID
                    DictionaryTreeViewObject.Add(level_1_items.Id, new Tuple<Node, object>(level_1_items, dsh));

                    foreach (PresentationTreeItems._03PresentatonDataSource dssh in dsh.DATASOURCES)
                    {
                        var level_2_items = new Node() { Text = dssh.SrcGuid.ToString() + "#" + dssh.NameSourceDriver };
                        DictionaryTreeViewObject.Add(level_2_items.Id, new Tuple<Node, object>(level_2_items, dssh));

                        level_2_items.Parent.Add(level_1_items);
                        level_1_items.Children.Add(level_2_items);

                        // рекурсивно добавляем контроллеры и устройства
                        foreach (PresentationTreeItems._04PresentatonController dch in dssh.ListDataControllerHardware)
                        {
                            var level_3_items = new Node() { Text = dch.ObjectGUID + "#" + dch.СontrollerNumber };
                            DictionaryTreeViewObject.Add(level_3_items.Id, new Tuple<Node, object>(level_3_items, dch));

                            level_3_items.Parent.Add(level_2_items);
                            level_2_items.Children.Add(level_3_items);

                            foreach (PresentationTreeItems._05PresentatonDevice dh in dch.ListDevice4DataController)
                            {
                                var level_4_items = new Node() { Text = dh.DevGUID.ToString() + "#" + dh.DeviceType };
                                DictionaryTreeViewObject.Add(level_4_items.Id, new Tuple<Node, object>(level_4_items, dh));

                                level_4_items.Parent.Add(level_3_items);
                                level_3_items.Children.Add(level_4_items);
                            }
                        }
                    }
                    Nodes.Add(level_1_items);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// заполнить дерево Native
        /// </summary>
        private void FillingTreeNative(ObservableCollection<Node> Nodes)
        {
            Nodes.Clear();

            ICollection<PresentationTreeItems._02PresentationDataServer> dscollection = App.SDB.Get_DS_Devs_Tags_Native(_dataConfiguration.DATASERVER);

            if (dscollection == null)
                throw new Exception(string.Format(@"(205) ...\MTRADataServer\MainWindow.xaml.cs: FillingTreeNative() : Не удалось инициализировать элемент TreeView."));

            try
            {
                foreach (PresentationTreeItems._02PresentationDataServer dsh in dscollection)
                {
                    var level_1_items = new Node() { Text = dsh.UniDS_GUID };   //.NameDS_GUID
                    DictionaryTreeViewObject.Add(level_1_items.Id, new Tuple<Node, object>(level_1_items, dsh));

                    foreach (PresentationTreeItems._05PresentatonDevice dssh in dsh.ListDevices4ThisDS)
                    {
                        var level_2_items = new Node() { Text = dssh.DevGUID.ToString() + "#" + dssh.DeviceType};
                        DictionaryTreeViewObject.Add(level_2_items.Id, new Tuple<Node, object>(level_2_items, dssh));

                        level_2_items.Parent.Add(level_1_items);
                        level_1_items.Children.Add(level_2_items);

                        // рекурсивно добавляем группы в устройства
                        foreach (PresentationTreeItems._06PresentatonGroup dch in dssh.Groups)
                        {
                            var level_3_items = new Node() { Text = dch.GroupName};
                            DictionaryTreeViewObject.Add(level_3_items.Id, new Tuple<Node, object>(level_3_items, dch));

                            level_3_items.Parent.Add(level_2_items);
                            level_2_items.Children.Add(level_3_items);

                            if (dch.SubGroupList.Count > 0)
                                CreateSubGroupsNodes(dch, level_3_items);
                        }
                    }
                    Nodes.Add(level_1_items);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// заполнить дерево Present
        /// </summary>
        private void FillingTreePresent(ObservableCollection<Node> Nodes)
        {
            Nodes.Clear();

            ICollection<PresentationTreeItems._02PresentationDataServer> dscollection = App.SDB.Get_DS_Devs_Tags_Present(PRESENTATIONCONFIGURATION);

            if (dscollection == null)
                throw new Exception(string.Format(@"(205) ...\MTRADataServer\MainWindow.xaml.cs: FillingTreePresent() : Не удалось инициализировать элемент TreeView."));

            try
            {
                foreach (PresentationTreeItems._02PresentationDataServer dsh in dscollection)
                {
                    var level_1_items = new Node() { Text = dsh.UniDS_GUID };   //.NameDS_GUID
                    DictionaryTreeViewObject.Add(level_1_items.Id, new Tuple<Node, object>(level_1_items, dsh));

                    foreach (PresentationTreeItems._05PresentatonDevice dssh in dsh.ListDevices4ThisDS)
                    {
                        var level_2_items = new Node() { Text = dssh.DevGUID.ToString() + "#" + dssh.DeviceType };
                        DictionaryTreeViewObject.Add(level_2_items.Id, new Tuple<Node, object>(level_2_items, dssh));

                        level_2_items.Parent.Add(level_1_items);
                        level_1_items.Children.Add(level_2_items);

                        // рекурсивно добавляем группы в устройства
                        foreach (PresentationTreeItems._06PresentatonGroup dch in dssh.Groups)
                        {
                            var level_3_items = new Node() { Text = dch.GroupName };
                            DictionaryTreeViewObject.Add(level_3_items.Id, new Tuple<Node, object>(level_3_items, dch));

                            level_3_items.Parent.Add(level_2_items);
                            level_2_items.Children.Add(level_3_items);

                            if (dch.SubGroupList.Count > 0)
                                CreateSubGroupsNodes(dch, level_3_items);
                        }
                    }
                    Nodes.Add(level_1_items);
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
        /// <param name="dch"></param>
        /// <param name="level_3_items"></param>
        private void CreateSubGroupsNodes(PresentationTreeItems._06PresentatonGroup dch, Node level_3_items)
        {
            try
            {
                // добавляем подгруппы
                foreach (PresentationTreeItems._06PresentatonGroup grn in dch.SubGroupList)
                {
                    var level_4_items = new Node() { Text = grn.GroupName};
                    DictionaryTreeViewObject.Add(level_4_items.Id, new Tuple<Node, object>(level_4_items, grn));

                    level_4_items.Parent.Add(level_3_items);
                    level_3_items.Children.Add(level_4_items);

                    if (grn.SubGroupList.Count > 0)
                        CreateSubGroupsNodes(grn, level_4_items);
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        /// <summary>
        /// проверка работы одной копии программы
        /// </summary>
        private void TestExistDS()
        {
            try
            {
                /*
                      * Для определения того, запущена ли еще одна копия программы
                      * используется объект System.Threading.Mutex. 
                      * Этот объект является оболочкой для системного объекта синхронизации mutex,
                      * который может существовать только в единственном экземпляре. 
                      * Поэтому, если при его создании мы обнаружили, что объект уже был создан, 
                      * значит, его создала предыдущая копия нашего приложения, которое еще не завершилось.
                      */
                bool createNew;
                _syncObj = new Mutex(true, _syncObjName, out createNew);
                if (!createNew)
                {
                    Console.WriteLine("DataServer уже запущен.");
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        #region меню
        /// <summary>
        /// единый обработчик меню
        /// первого уровня
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch ((sender as MenuItem).Name)
                {
                    case "miActionExit":    // выход
                        Close();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        
        #endregion
        #region старт процесса трассировки приложения
        /// <summary>
        ///старт процесса трассировки приложения
        /// </summary>
        private void StartTrace()
        {
            try
            {
                TraceSourceLib.TraceSourceDiagMes.StartTrace("AppDiagnosticLog", 30000);
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }
        #endregion

        private void TextBlock_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            // список тегов для вывода в DataGrid
            List<PresentationTreeItems._07PresentatonTag> TagsRequest = new List<PresentationTreeItems._07PresentatonTag>();
 
            try
            {
                TextBlock tb = (TextBlock)sender;              
                string id =   tb.Uid;

                if (!DictionaryTreeViewObject.ContainsKey(id))
                {
                    throw new Exception("Нет соответсвия устройства идентификатору узла");
                }
                DGTags.ItemsSource = null;

                TagsRequest.Clear();
                TagsRequest.Clear();

                //HardwareConfiguration.DeviceHardware dh = DictionaryTreeViewObject[id].Item2 as HardwareConfiguration.DeviceHardware;

                PresentationTreeItems._05PresentatonDevice dh = DictionaryTreeViewObject[id].Item2 as PresentationTreeItems._05PresentatonDevice;

                if (dh == null)
                {
                        PresentationTreeItems._06PresentatonGroup dg = DictionaryTreeViewObject[id].Item2 as PresentationTreeItems._06PresentatonGroup;
                        if (dg.TagList == null)
                            return;

                        if (dg.ThisDevice.DeviceNativeLink != null)
                        {
                            foreach (PresentationTreeItems._07PresentatonTag ptag in dg.TagList)
                            {
                                Tag tg = dg.ThisDevice.DeviceNativeLink.dictTags4Parse[ptag.TagGuid];

                                tg.OnChangeTagNT += ptag.tg_OnChangeTagHT;

                                TagsRequest.Add(ptag);
                            }
                        }
                        else if (dg.ThisDevice.DevicePresentLink != null)
                        {
                            foreach (PresentationTreeItems._07PresentatonTag ptag in dg.TagList)
                            {
                                _05Tag tg = dg.ThisDevice.DevicePresentLink.dictTags4Parse[ptag.TagGuid];

                                ptag.TagGuid = tg.TagGUID;
                                //ptag.TagName = tg.ta.TagName;
                                ptag.TagQuality = tg.TagQuality;
                                ptag.TagType = tg.TagType;
                                ptag.TimeStamp = tg.TimeStamp;

                                if (tg.TagValue != null)
                                    ptag.TagValue = tg.TagValue;
                                else
                                {
                                }

                                tg.OnChangeTagPT += ptag.tg_OnChangeTagHT;

                                TagsRequest.Add(ptag);
                            }
                        }
                }
                else if (dh.DeviceHardwareLink != null)
                {
                    foreach ( TagHardware tg in dh.DeviceHardwareLink.LstTags )
                    {
                        PresentationTreeItems._07PresentatonTag pt = new PresentationTreeItems._07PresentatonTag();

                        pt.TagGuid = tg.TagGuid;
                        pt.TagName = tg.TagName;
                        pt.TagQuality = tg.TagQuality;
                        pt.TagType = tg.TagType;
                        pt.TimeStamp = tg.TimeStamp;

                        if (tg.TagValue != null)
                            pt.TagValue = tg.TagValue;

                        tg.OnChangeTagHT += pt.tg_OnChangeTagHT;
                        TagsRequest.Add(pt);
                    }
                }

                if (TagsRequest.Count > 0)
                    DGTags.ItemsSource = TagsRequest;
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        /// <summary>
        /// Take Id from CheckBox Uid and transfer value to CheckBoxId struct
        /// </summary>
        /// <param name="sender">The CheckBox clicked.</param>
        /// <param name="e">Parameters associated to the mouse event.</param>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckBox currentCheckBox = (CheckBox)sender;
            CheckBoxId.checkBoxId = currentCheckBox.Uid;
        }
    }
}
