﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Gadgets;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map;
using System.Printing;
using System.IO;
using System.Windows.Media.Animation;
using Ptv.XServer.Controls.Map.Layers.Tiled;
using Ptv.XServer.Controls.Map.Symbols;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Tools;

namespace Circles
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            GlobalOptions.InfiniteZoom = true;
            this.Map.Loaded += new RoutedEventHandler(Map_Loaded);
        }

        private void Map_Loaded(object sender, RoutedEventArgs e)
        {
            // http://ows.terrestris.de/dienste.html
            Map.Layers.Add(new TiledLayer("BASEMAP")
            {
                Caption = "Base Map",
                Icon = ResourceHelper.LoadBitmapFromResource("Ptv.XServer.Controls.Map;component/Resources/Background.png"),
                TiledProvider = new WmsProvider(
                    "http://ows.terrestris.de/osm/service?SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&FORMAT=image%2fpng&LAYERS=OSM-WMS&STYLES=,&SRS=EPSG%3a3857", 19),
                Copyright = "© terrestris",
                IsBaseMapLayer = true // overlay layers cannot be moved under a basemap layer
            });

            Map.Layers.Add(new UntiledLayer("BUSSTOPS") { 
                Caption = "Bus Stops",
                UntiledProvider = new WmsUntiledProvider(
                    "http://ows.terrestris.de/osm-haltestellen?service=WMS&request=GetMap&version=1.1.1&layers=OSM-Bushaltestellen&styles=&format=image%2Fpng&transparent=true&srs=EPSG%3A3857", 19),
                Copyright = "© terrestris"

            });
        }
    }
}
