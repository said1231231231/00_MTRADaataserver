using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeConfigurationLib.NativeConfiguration;
using LinksLib.LinksHT2NT;

namespace InterfaceLibrary
{
    public interface IProviderConfiguration4NativeSource
    {
        /// <summary>
        /// создать источник по его имени и добавить его описание 
        /// к DataServer
        /// </summary>
        /// <param name="name_src"></param>
        /// <returns></returns>
        void CreateDataSourceNative(DataServer ds, string name_src, List<LinkHT2NTBase> lstLinksHT2NT);
    }
}
