using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace SplitCurves.Lib
{
    public static class Utility
    {
        /// <summary>
        /// Compute the signed distance from a plane and a point in the center of the curve bounding box.
        /// If the value is positive the curve is on the plane (side with positive normal),
        /// if negative is below the plane.
        /// </summary>
        /// <param name="plane">Plane to calculate the distance of.</param>
        /// <param name="crv">The curve to check.</param>
        /// <returns>The signed distance.</returns>
        public static double SignedDistance(Plane plane, Curve crv)
        {
            Point3d ptOnCrv = crv.GetBoundingBox(false).Center;
            Vector3d ptToOrigin = plane.Origin - ptOnCrv;
            return ptToOrigin * plane.Normal;
        }
    }
}
