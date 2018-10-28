﻿//--------------------------------------------------------------
// Copyright (c) PTV Group
// 
// For license details, please refer to the file COPYING, which 
// should have been provided with this distribution.
//--------------------------------------------------------------

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Ptv.XServer.Controls.Map.Tools;


namespace Ptv.XServer.Net.ExtensibilityTest
{
    /// <summary>  </summary>
    public class MapPolygon : MapShape
    {
        #region public variables
        /// <summary>  </summary>
        public PointCollection Points { get; set; }
        #endregion

        #region constructor
        /// <summary>  </summary>
        public MapPolygon()
        {
            Points = new PointCollection();
        }
        #endregion

        #region protected methods
        /// <inheritdoc/>
        protected override Geometry DefiningGeometry
        {
            get
            {
                var streamGeometry = new StreamGeometry();
                using (var streamGeometryContext = streamGeometry.Open())
                {
                    for (int i = 0; i < Points.Count; i++)
                    {
                        var wgsPoint = new Point(Points[i].X, -Points[i].Y);
                        var mercatorPoint = GeoTransform.WGSToPtvMercator(wgsPoint);
                        mercatorPoint.X = mercatorPoint.X / 20015087.0 * 180 + 180;
                        mercatorPoint.Y = mercatorPoint.Y / 20015087.0 * 180 + 180;

                        if (i == 0)
                            streamGeometryContext.BeginFigure(new Point(mercatorPoint.X, mercatorPoint.Y), true, true);
                        else
                            streamGeometryContext.LineTo(new Point(mercatorPoint.X, mercatorPoint.Y), true, true);
                    }
                }

                return streamGeometry;
            }
        }
        #endregion
    }

    /// <summary> TODO: Comment. </summary>
    public abstract class MapShape : Shape
    {
        #region dependency property
        /// <summary>  </summary>
        public double InvariantStrokeThickness
        {
            get { return (double)GetValue(InvariantStrokeThicknessProperty); }
            set
            {
                SetValue(InvariantStrokeThicknessProperty, value);
                StrokeThickness = value;
            }
        }

        /// <summary>  </summary>
        public static DependencyProperty InvariantStrokeThicknessProperty =
            DependencyProperty.Register("InvariantStrokeThickness", typeof(double), typeof(MapShape), null);
        #endregion

        #region constructor
        /// <summary>  </summary>
        protected MapShape()
        {
            InvariantStrokeThickness = 1;
        }
        #endregion
    }
}

    //[Serializable]
    //public class MapPoint
    //{
    //    public MapPoint()
    //    {
    //    }

    //    public MapPoint(double latitude, double longitude)
    //    {
    //        this.Latitude = latitude;
    //        this.Longitude = longitude;
    //    }

    //    public double Latitude { get; set; }
    //    public double Longitude { get; set; }
    //}