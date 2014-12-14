using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClassesLib.CommonClasses;
using HardwareConfigurationLib.HardwareConfiguration;
using Opc.Da;

namespace uvs_OPC.HardwareConfiguration
{
    public class OpcTagHardware : TagHardware
    {
        #region Public properties

        /// <summary>
        /// Путь тега. Нужен для запроса значений от OPC-сервера
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Идентификатор тега при подписке
        /// </summary>
        public Opc.Da.Item Item
        {
            get
            {
                if (_item == null)
                {
                    _item = new Item
                    {
                        Active = true,
                        ItemName = Path,
                        ClientHandle = Path
                    };
                }
                return _item;
            }
        }
        private Opc.Da.Item _item;

        #endregion

        #region Public metods

        /// <summary>
        /// Задать значение тега
        /// </summary>
        public void SetTagValue(object value, Quality quality)
        {
            base.SetTagValue(value, quality == Quality.Good ? ProjectCommonData.VarQuality.vqGood : ProjectCommonData.VarQuality.vqUndefined);
        }

        #endregion
    }
}
