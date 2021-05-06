using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SplitCurves.Lib
{
    public static class DividerCreator
    {
        /// <summary>
        /// Creates divider planes according to desired angle and division area.<br></br>
        /// Workflow of this method; <br></br>
        /// 1. Creates an inner exteded curve to boundaries.
        /// 2. Gets t parameters of extended curve according to division area count.
        /// 3. Creates perpendicular planes on t parameters.
        /// </summary>
        /// <param name="boundary"> Boundary curve to extend curve of planes' path.</param>
        /// <param name="azimuthAngle"> Angle between north in order to create dividers with this angle.</param>
        /// <param name="divisionAreaCount"> Count of how many pieces we want to divide, it helps to decide span length and plane count.</param>
        /// <returns></returns>
        public static IEnumerable<Plane> FromAzimuth(Curve boundary, double azimuthAngle, int divisionAreaCount)
        {
            // Get plane path line which is perpendicular to azimuth.
            Curve planePath = GetAzimuthLine(boundary.GetBoundingBox(true).Center, azimuthAngle + 90);

            // Collect boundary to collection.
            List<GeometryBase> geometries = new List<GeometryBase>() { boundary };

            // Extend plane path line to boundary with both sides.
            Curve extended = planePath.Extend(CurveEnd.Both, CurveExtensionStyle.Line, geometries);

            // Get curve parameters to create planes with those.
            IEnumerable<double> curveParameters = GetCurveParameters(extended.Domain.Length, extended.Domain.Min, divisionAreaCount);

            // Return perpendicular frames on the extended line.
            return extended.GetPerpendicularFrames(curveParameters);
        }

        /// <summary>
        /// Gets parameters of curve in order to get perpendicular planes on plane path.
        /// </summary>
        /// <param name="domainLength"> Length of the domain which equals to difference between Domain.Max and Domain.Min.</param>
        /// <param name="domainMinimum"> Minimum parameter of the domain in order to add on it.</param>
        /// <param name="divisionAreaCount"> Count of division area that want to achieve.</param>
        /// <returns> Collection of parameters.</returns>
        public static IEnumerable<double> GetCurveParameters(double domainLength, double domainMinimum, int divisionAreaCount)
        {
            double equalSpans = domainLength / divisionAreaCount;

            for (int i = 1; i < divisionAreaCount; i++)
            {
                yield return Math.Round(domainMinimum + (equalSpans * i), 2, MidpointRounding.ToEven);
            }
        }

        /// <summary>
        /// Creates a unit line to extend boundary.
        /// </summary>
        /// <param name="point"> First point of the line.</param>
        /// <param name="azimuthAngle"> Angle between north that line will be parallel on it.</param>
        /// <returns> Curve through azimuth angle.</returns>
        private static Curve GetAzimuthLine(Point3d point, double azimuthAngle)
        {
            // Get 360 modulo of azimuth in case of user enters more than 360.
            double moduloOfAzimuth = azimuthAngle % 360;
            double radianOfAzimuth = (Math.PI / 180) * moduloOfAzimuth;

            // Direction vector of divider. It's slice!
            Vector3d azimuthVector = new Vector3d(Math.Sin(radianOfAzimuth), Math.Cos(radianOfAzimuth), 0);
            azimuthVector.Unitize();

            // Initialize transfrom
            Transform moveToVectorEnd = Transform.Translation(azimuthVector);

            // Move curve to end of the vector
            Point3d vectorEndPoint = new Point3d(point);
            vectorEndPoint.Transform(moveToVectorEnd);

            return new LineCurve(point, vectorEndPoint);
        }
    }
}
