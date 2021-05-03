using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace SplitCurve.Lib
{
    public static class DividerCreator
    {
        /// <summary>
        /// Creates planes from azimuth angle.
        /// Sine value of the azimuth gives the x value of unit vector,
        /// Cosine vaue of the azimuth gives the y value of unit vector.
        /// </summary>
        /// <param name="boundary"> Source object.</param>
        /// <param name="azimuthDegree"> Clockwise angle from North.</param>
        /// <param name="count"> Count of desired sub curve loop.</param>
        /// <returns></returns>
        public static IEnumerable<Plane> FromAzimuth(Curve boundary, double azimuthDegree, int count)
        {
            // Get 360 modulo of azimuth in case of user enters more than 360.
            double moduloOfAzimuth = azimuthDegree % 360;
            double radianOfAzimuth = (Math.PI / 180) * moduloOfAzimuth;

            // Get points from 0 and 0.5 t parameter.
            // NOTE!: Here assumed that curve almost symmetric according to Y-axis.
            Point3d pt1 = boundary.PointAt(0);
            Point3d pt2 = boundary.PointAt(boundary.Domain.Mid);

            // Find distance between this two point.
            double dist = Math.Abs(pt1.X - pt2.X);

            // Find offset value between planes.
            double offsetValue = dist / count;

            // Direction vector of divider. It's slice!
            Vector3d azimuthVector = new Vector3d(Math.Sin(radianOfAzimuth) * offsetValue, Math.Cos(radianOfAzimuth) * offsetValue, 0);

            // Initialize transform objects in order to move points.
            Transform moveToAzimuth = Transform.Translation(azimuthVector);
            Transform moveToUp = Transform.Translation(new Vector3d(0, 0, 1));

            // Duplicate points before moving.
            Point3d azimuthPoint = new Point3d(pt1);
            Point3d upperPoint = new Point3d(pt1);
            
            // Move points to create Plane from 3 point
            azimuthPoint.Transform(moveToAzimuth);
            upperPoint.Transform(moveToUp);

            // Create first plane.
            Plane plane = new Plane(pt1, azimuthPoint, upperPoint);

            // Initialize offset vector, it will reverse below if needed
            Vector3d offsetVector = new Vector3d(azimuthVector);

            // Vector turns to west
            if (azimuthDegree < 270 && azimuthDegree >= 90)
            {
                // Vector should move to east
                if ((pt2.X - pt1.X) >= 0)
                {
                    offsetVector.Reverse();
                }
                offsetVector.Rotate(Math.PI / 2, new Vector3d(0, 0, 1));
            }
            // Vector turns to east
            else
            {
                // Vector should move to west
                if ((pt2.X - pt1.X) <= 0)
                {
                    offsetVector.Reverse();
                }
                offsetVector.Rotate(3 * (Math.PI / 2), new Vector3d(0, 0, 1));
            }
            
            return OffsetPlanes(plane, Transform.Translation(offsetVector), count);
        }

        private static IEnumerable<Plane> OffsetPlanes(Plane sourcePlane, Transform offsetTransform, int count)
        {
            yield return sourcePlane;
            Plane plane = new Plane(sourcePlane);
            for (int i = 0; i < count; i++)
            {
                Plane movedPlane = new Plane(plane);
                movedPlane.Transform(offsetTransform);
                plane.Transform(offsetTransform);
                yield return movedPlane;
            }
        }
    }
}
