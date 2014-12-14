using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MTRADataServer
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static MTRADataServer.SDb storeDb = new MTRADataServer.SDb();

        public static List<LinksLib.LinksHT2NT.LinkHT2NTBase> LstLinksHT2NT = new List<LinksLib.LinksHT2NT.LinkHT2NTBase>();
        public static List<LinksLib.LinksNT2PT.LinkNT2PTBase> LstLinksNT2PT = new List<LinksLib.LinksNT2PT.LinkNT2PTBase>();

        public static MTRADataServer.SDb SDB
        {
            get { return storeDb; }
        }
    }
}
