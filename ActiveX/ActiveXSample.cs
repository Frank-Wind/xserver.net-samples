﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Ptv.XServer.Controls.Map;
using Ptv.XServer.Controls.Map.Layers.Shapes;
using Ptv.XServer.Controls.Map.Symbols;
using System.Windows;
using System.Windows.Media;

namespace Ptv.XServer.Controls
{
    /// <summary>
    /// This is the main control that embeds PTV xServer .NET's map and exposes this map to COM as an ActiveX control. 
    /// The control defines the necessary class attributes for COM to be supported, implements IMapControl as its main 
    /// interfaces and exposes some events through its source interface IMapEvents.
    /// 
    /// Please refer to the sample documentation for further details.
    /// </summary>
    [ComVisible(true)]
    [ProgId("PtvXServerControls.ActiveXSample")]
    [Guid("C8845F38-C2A7-4401-9980-C3B3390D5975")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IMapEvents))]
    public partial class ActiveXSample : UserControl, IMapControl
    {
        /// <summary> Initializes the control. </summary>
        public ActiveXSample()
        {
            InitializeComponent();

            // WORKAROUND: In some cases (e.g. Delphi 2006) WPF controls are rendered with a black background. This is no 
            // WPF map specific problem. The following lines force the background to System.Drawing.SystemColors.Control.
            var bkColor = System.Drawing.SystemColors.Control;
            formsMap.WrappedMap.Background = new SolidColorBrush(Color.FromRgb(bkColor.R, bkColor.G, bkColor.B));

            // construct and store the shape implementation
            Shapes = new Shapes(this, formsMap);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // test initialization
            XMapUrl = "https://api-test.cloud.ptvgroup.com/xmap/ws/XMap";
            XMapCredentials = "Insert your xToken here"; 
            Shapes.AddMarker(42, 8.3, 49, 30, "#f00", "Pin", "Hello");

            SetMapLocation(8.3, 49, 10);
        }

        /// <inheritdoc/>
        public string XMapUrl
        {
            get
            {
                return formsMap.XMapUrl;
            }
            set
            {
                formsMap.XMapUrl = value;
            }
        }

        /// <inheritdoc cref="IMapControl" />
        public string XMapCredentials
        {
            get
            {
                return formsMap.XMapCredentials;
            }
            set
            {
                formsMap.XMapCredentials = value;
            }
        }

        /// <inheritdoc/>
        public string XMapCopyright
        {
            get
            {
                return formsMap.XMapCopyright;
            }
            set
            {
                formsMap.XMapCopyright = value;
            }
        }

        /// <inheritdoc/>
        public void SetMapLocation(double x, double y, double zoom)
        {
            formsMap.SetMapLocation(new Point(x, y), zoom);
        }

        /// <inheritdoc/>
        public IShapes Shapes { get; }
        

        /// <summary>
        /// Fires a <see cref="OnShapeClicked"/> event.
        /// </summary>
        /// <param name="id">ID of the shape that has been clicked.</param>
        [ComVisible(false)]
        public void FireShapeClicked(int id)
        {
            if (OnShapeClicked == null) return;

            try { OnShapeClicked(id); }
            catch { /* ignored */ }
        }

        /// <summary> The delegate type used for the <see cref="OnShapeClicked"/> event. </summary>
        /// <param name="id">ID of the shape that was clicked.</param>
        /// <remarks>Can be marked [ComVisible(false)] by convention. As a COM event delegate this delegate has 
        /// (to have) the same signature as the corresponding event method defined through <see cref="IMapEvents"/>.</remarks>
        [ComVisible(false)]
        public delegate void OnShapeClickDelegate(int id);

        /// <summary>
        /// Fired, when the user clicks on a shape. This is (sort of) the event implementation of the corresponding 
        /// event method defined through the COM source interface <see cref="IMapEvents"/>.
        /// </summary>
        public event OnShapeClickDelegate OnShapeClicked;


        /// <summary>
        /// Hook that writes additional information to the Windows' registry allowing this control 
        /// to be used as an ActiveX control.
        /// </summary>
        /// <param name="key">The key of the control to be registered.</param>
        /// <remarks>Please refer to the sample documentation for further information, see also <see cref="Registrar"/>.</remarks>
        [ComRegisterFunction]
        public static void RegisterClass(string key)
        {
            typeof(ActiveXSample).RegisterClass(key);
        }

        /// <summary>
        /// Hook that removes additional information written by <see cref="RegisterClass"/> 
        /// from to the Windows' registry, fully unregistering this control as an ActiveX control.
        /// </summary>
        /// <param name="key">The key of the control to be registered.</param>
        /// <remarks>Please refer to the sample documentation for further information. 
        /// See also <see cref="Registrar"/>.</remarks>
        [ComUnregisterFunction]
        public static void UnregisterClass(string key)
        {
            typeof(ActiveXSample).UnregisterClass(key);
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IShapes"/>, as returned by <see cref="IMapControl"/>.
    /// </summary>
    /// <remarks>Class can be marked [ComVisible(false)], as it is solely 
    /// used through its main interface, <see cref="IShapes"/>.</remarks>
    [ComVisible(false)]
    internal class Shapes : IShapes
    {
        /// <summary>
        /// A list to map IDs to shapes.
        /// </summary>
        private readonly SortedList<int, FrameworkElement> idMap = new SortedList<int, FrameworkElement>();

        /// <summary>
        /// The shape layer itself.
        /// </summary>
        private readonly ShapeLayer shapeLayer = new ShapeLayer("Shapes");

        /// <summary>
        /// The owning control.
        /// </summary>
        private readonly ActiveXSample parent;

        /// <summary>
        /// The map in which to display shapes.
        /// </summary>
        private readonly FormsMap formsMap;
        
        /// <summary>
        /// Initializes a Shapes instance.
        /// </summary>
        /// <param name="parent">The control owning the Shapes' instances.</param>
        /// <param name="formsMap">The map in which to display shapes.</param>
        public Shapes(ActiveXSample parent, FormsMap formsMap)
        {
            this.parent = parent;
            this.formsMap = formsMap;
        }


        /// <inheritdoc/>
        public int Count
        {
            get
            {
                return shapeLayer.Shapes.Count;
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (!formsMap.Layers.Contains(shapeLayer)) return;

            shapeLayer.Shapes.Clear();
            formsMap.Layers.Remove(shapeLayer);
        }

        /// <summary>
        /// Used to create a symbol, given a symbol name.
        /// </summary>
        /// <param name="Symbol">Name of the symbol to create.</param>
        /// <returns>The symbol instance, returned as a FrameworkElement. 
        /// The default symbol, used on any error, is a Pin.</returns>
        /// <remarks>See remarks on <see cref="AddMarker"/> for further information 
        /// and the valid values for the Symbol parameter.</remarks>
        private FrameworkElement CreateSymbol(string Symbol)
        {
            try 
            {
                Symbol = Symbol.ToLower();

                // try to locate the type matching the given symbol name
                var pins = (from type in typeof(FormsMap).Assembly.GetTypes()
                        where type.Namespace != null && type.Namespace.Equals(typeof(Pin).Namespace) && type.Name.ToLower().Equals(Symbol)
                        select type).ToList();

                // if there's exactly one hit, create symbol instance
                if (pins.Count == 1)
                    return (FrameworkElement) Activator.CreateInstance(pins.First());
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.GetType().Name + ":\n" + ex.Message + "\n\n" + ex.StackTrace);
            }

            // Pin is the default
            return new Pin();
        }

        /// <summary>
        /// Used to create and initialize a symbol, given a symbol name.
        /// </summary>
        /// <param name="size">The size of the symbol, in pixels.</param>
        /// <param name="argbColor">The color of the symbol, specified as string.</param>
        /// <returns>The symbol instance, returned as a FrameworkElement. 
        /// The default symbol, used on any error, is a Pin.</returns>
        /// <remarks>See remarks on <see cref="AddMarker"/> for further information.</remarks>
        private static FrameworkElement CreateSymbol(int size, string argbColor)
        {
            // create the symbol
            var elem = new Pin();

            // set the color
            SetSymbolColor(elem, argbColor);

            // set the size
            elem.Width = elem.Height = size;

            // we need modify the anchor
            ShapeCanvas.SetAnchor(elem, LocationAnchor.RightBottom);

            // done
            return elem;
        }

        /// <summary>
        /// Tries to apply the specified color to the given symbol.
        /// </summary>
        /// <param name="symbol">Symbol to set the color for.</param>
        /// <param name="argbColor">Color, specified as AARRGGBB.</param>
        private static void SetSymbolColor(FrameworkElement symbol, string argbColor)
        {
            if (symbol.GetType().GetProperty("Color") == null)
                return;

            // dynamically try to get the "Color" property, then set the converted color.
            symbol.GetType().GetProperty("Color").SetValue(symbol, ColorConverter.ConvertFromString(argbColor), null);
        }

        /// <inheritdoc/>
        public bool AddMarker(int id, double x, double y, int size, string color, string Symbol, string toolTip)
        {
            if (idMap.ContainsKey(id))
                return false;

            // create shape
            var frameworkElement = CreateSymbol(size, color);

            // set location
            ShapeCanvas.SetLocation(frameworkElement, new Point(x, y));

            // add shape
            InsertShape(frameworkElement, id, toolTip);

            return true;
        }

        /// <summary> Adds a shape to the shape layer, making it visible. </summary>
        /// <param name="shape">Shape to add.</param>
        /// <param name="id">ID of the shape to add.</param>
        /// <param name="toolTip">Optional tool tip to be assigned with the shape.</param>
        private void InsertShape(FrameworkElement shape, int id, string toolTip)
        {
            // if tool tip is valid, set it.
            if (!string.IsNullOrEmpty(toolTip))
                shape.ToolTip = toolTip;

            // register MouseDown handler, redirect event
            shape.MouseDown += (clickedShape, args) => parent.FireShapeClicked((int)((FrameworkElement) clickedShape).Tag);

            // store id
            shape.Tag = id;

            // add the shape
            shapeLayer.Shapes.Add(shape);
            idMap.Add(id, shape);

            // make layer visible if it's not
            if (!formsMap.Layers.Contains(shapeLayer))
                formsMap.Layers.Add(shapeLayer);
        }

        /// <summary>
        /// Checks if a given object possibly contains a co-ordinate array.
        /// </summary>
        /// <param name="coordinates">Object to check.</param>
        /// <returns>True, if coordinate evaluation can continue. 
        /// False, if the given object is definitely invalid.</returns>
        /// <remarks>It is necessary to support coordinates being specified through an object (or 
        /// object array) as the type support may be very limited in COM. Plain variant arrays are 
        /// the preferred way e.g. in VBA, always resulting in unstructured and somewhat untyped 
        /// objects on the .NET side.</remarks>
        private static bool CoordinatesAreValid(object coordinates)
        {
            return coordinates != null && coordinates.GetType().IsArray && ((Array) coordinates).Length >= 4;
        }

        /// <summary>
        /// Tries to fill in a <see cref="PointCollection"/> given some object array.
        /// </summary>
        /// <param name="coordinates">The object array to convert.</param>
        /// <returns>Points read from the coordinates array, returned in a <see cref="PointCollection"/>.</returns>
        /// <remarks>It is necessary to support coordinates being specified through an object (or 
        /// object array) as the type support may be very limited in COM. Plain variant arrays are 
        /// the preferred way e.g. in VBA, always resulting in unstructured and somewhat untyped 
        /// objects on the .NET side.</remarks>
        private static PointCollection ConvertCoordinates(Array coordinates)
        {
            var points = new PointCollection();

            for (var i = 0; i < coordinates.Length / 2; ++i)
            {
                points.Add(new Point(
                    Convert.ToDouble(coordinates.GetValue(2 * i + 0)),
                    Convert.ToDouble(coordinates.GetValue(2 * i + 1))
                ));
            }

            return points;
        }

        /// <inheritdoc/>
        public bool AddLine(int id, object coordinates, int size, string color, string toolTip)
        {
            if (idMap.ContainsKey(id) || !CoordinatesAreValid(coordinates))
                return false;

            // create and initialize the polyline
            var mapPolyline = new MapPolyline 
            {
                MapStrokeThickness = size,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                Points = ConvertCoordinates(coordinates as Array),
                Tag = id
            };

            // insert the shape
            InsertShape(mapPolyline, id, toolTip);

            return true;
        }

        /// <inheritdoc/>
        public bool Remove(int id)
        {
            // done, if no such element exists
            if (!idMap.ContainsKey(id))
                return false;

            // get element
            var frameworkElement = idMap[id];

            // remove element
            shapeLayer.Shapes.Remove(frameworkElement);
            idMap.Remove(id);

            // also remove layer if there are not elements left
            if (shapeLayer.Shapes.Count < 1)
                formsMap.Layers.Remove(shapeLayer);

            return true;
        }
    }
}
