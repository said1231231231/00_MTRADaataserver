using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HardwareConfigurationLib.HardwareConfiguration;
using Opc;
using Opc.Da;
using OpcCom;
using Server = Opc.Da.Server;

namespace uvs_OPC.HardwareConfiguration
{
    internal class OpcControllerHardware : DataControllerHardware
    {
        #region Public properties

        /// <summary>
        /// Url-адрес OPC-сервера
        /// </summary>
        public string OpcServerUrl { get; set; }

        /// <summary>
        /// Период получения обновлений от сервера (мс.)
        /// </summary>
        public int UpdateRate { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// 
        /// </summary>
        private Opc.Da.Server _daServer;

        /// <summary>
        /// Словарь всех тегов данного контроллера
        /// </summary>
        private Dictionary<string, OpcTagHardware> _tags;

        #endregion

        #region Public metods

        public void StartSubscribbe()
        {
            try
            {
                #region Подготавливаем словарь всех тегов всех устройств данного контроллера для удобства работы
                var tags = new List<TagHardware>();
                foreach (var deviceHardware in ListDevice4DataController)
                {
                    tags.AddRange(deviceHardware.dictTags4Parse.Values);
                }
                _tags = tags.ToDictionary(hardware => (hardware as OpcTagHardware).Path,
                    hardware => hardware as OpcTagHardware);
                #endregion

                _daServer = new Server(new OpcCom.Factory(), new URL(OpcServerUrl));
                _daServer.Connect();

                SubscriptionState subscriptionState = new SubscriptionState();
                subscriptionState.UpdateRate = UpdateRate;
                subscriptionState.Active = true;

                var subscription = _daServer.CreateSubscription(subscriptionState);
                subscription.AddItems(_tags.Values.Select(tag => tag.Item).ToArray());
                subscription.DataChanged += (handle, requestHandle, values) =>
                {
                    foreach (var itemValue in values)
                    {
                        if (!_tags.ContainsKey(itemValue.ItemName))
                            continue;

                        var opcTag = _tags[itemValue.ItemName];
                        opcTag.SetTagValue(itemValue.Value, itemValue.Quality);
                    }
                };
            }
            catch (Exception ex)
            {
                TraceSourceLib.TraceSourceDiagMes.WriteDiagnosticMSG(ex);
            }
        }

        #endregion
    }
}
