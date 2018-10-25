﻿//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Media;
using Ptv.XServer.Controls.Map.Layers;
using Ptv.XServer.Controls.Map.TileProviders;
using Ptv.XServer.Controls.Map.Canvases;
using Ptv.XServer.Controls.Map.Layers.Untiled;
using Ptv.XServer.Controls.Map.Layers.Tiled;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary> Interaction logic for MainWindow.xaml </summary>
    public partial class MainWindow
    {
        #region constructor
        /// <summary> Constructor of the main window. </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region event handling
        /// <summary> Event handler for a successful loading of the window. </summary>
        /// <param name="sender"> Sender of this event. </param>
        /// <param name="e"> Event parameters. </param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // add xmap base layers
            mapControl1.XMapUrl = "https://xmap-eu-n-test.cloud.ptvgroup.com/xmap/ws/XMap";
            mapControl1.XMapCredentials = "xtok:EBB3ABF6-C1FD-4B01-9D69-349332944AD9";

            // goto KA
            mapControl1.SetMapLocation(new Point(8.4277, 49.0136), 12.6);

            // add osm layer
            //mapControl1.Layers.Add(new TileLayer("OSM")
            //{
            //    TileServer = new RemoteTileServer{
            //        RequestBuilderDelegate = (x, y, level) =>
            //        string.Format("http://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png", "abc"[(x ^ y) % 3], level, x, y)},
            //    Copyright = "© OpenStreetMap contributors, CC-BY-SA",
            //});

            // remote tile layer - insert before grid Labels
            mapControl1.Layers.Insert(
                mapControl1.Layers.IndexOf(mapControl1.Layers["Labels"]),
                new TiledLayer("RemoteLayer")
                {
                    IsTransparentLayer = true,
                    TiledProvider = new RemoteTiledProvider{RequestBuilderDelegate = (x, y, level) =>
                        string.Format("http://176.95.37.29/ShapeTiles/ShapeTileHandler.ashx?x={0}&y={1}&z={2}&layer=someLayer&style=someStyle",
                        x, y, level)}
                });

            // client-side tile Layer - insert before Labels   
            mapControl1.Layers.Insert(
                mapControl1.Layers.IndexOf(mapControl1.Layers["Labels"]),
                new TiledLayer("TiledOverlay")
                {
                    IsTransparentLayer = true,
                    TiledProvider = new MyTileRenderer()
                });

            // overlay Layer
            mapControl1.Layers.Add(
                new UntiledLayer("UntiledOverlay")
                {
                    UntiledProvider = new MyOverlayRenderer { Locations = MyOverlayRenderer.CreateTestLocations(10000), SymbolColor = Colors.Purple }
                });

            // hybrid canvas Layer
            mapControl1.Layers.Add(
                new BaseLayer("HybridCanvas")
                {
                    CanvasCategories = new[] { CanvasCategory.Content },
                    CanvasFactories = new BaseLayer.CanvasFactoryDelegate[]{
                        mapControl => new MyHybridLayer(mapControl) { Locations = MyOverlayRenderer.CreateTestLocations(10000), SymbolColor = Colors.Green }}
                });

            // (dynamic) vector layer defined by code
            mapControl1.Layers.Add(
                new BaseLayer("VectorLayer")
                {
                    CanvasCategories = new[] { CanvasCategory.Content },
                    CanvasFactories = new BaseLayer.CanvasFactoryDelegate[] { mapControl => new MyVectorCanvas(mapControl) }
                });

            // (static) vector layer defined in Xaml
            mapControl1.Layers.Add(
                new ShapeLayer("XamlLayer")
                {
                    VectorLayerType = typeof(MapXaml)
                });

            // basic canvas layer
            mapControl1.Layers.Add(
                new BaseLayer("CanvasLayer")
                {
                    CanvasCategories = new[] { CanvasCategory.Content },
                    CanvasFactories = new BaseLayer.CanvasFactoryDelegate[] { mapControl => new MyCanvasLayer(mapControl) { Locations = MyOverlayRenderer.CreateTestLocations(100), SymbolColor = Colors.Yellow } }
                });

            // canvas painter layer
            mapControl1.Layers.Add(
                new BaseLayer("MyScreenCanvasLayer")
                {
                    CanvasCategories = new[] { CanvasCategory.Content },
                    CanvasFactories = new BaseLayer.CanvasFactoryDelegate[] { mapControl => new MyScreenCanvasLayer(mapControl) { Locations = MyOverlayRenderer.CreateTestLocations(100), SymbolColor = Colors.Yellow } }
                });
            //mapControl1.Layers.Add(
            //    new CanvasPainterLayer("MultiCanvas")
            //    {
            //        CanvasPainter = new MyPainter(locations)
            //    });
        }
        #endregion
    }
}
