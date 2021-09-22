using System.Collections.Generic;
using System.Drawing;
using System;

using Rhino.Geometry;
using Rhino;

using Xunit;

using SplitCurves.Lib;


namespace SplitCurves.Testing
{

	[Collection("Rhino Collection")]
	public class CurveSplittingTest
	{

		// Please improve this unit test ?
		[Fact]
		public void SplittingTest()
		{
			Rectangle3d rect = new Rectangle3d(Plane.WorldXY, 5000, 1000);
			Curve Boundary = rect.ToNurbsCurve();

			List<Plane> splitPlanes = new List<Plane>();

			var ex = Assert.Throws<ArgumentException>(() => Curves.DivideCurve(Boundary, splitPlanes));
			Assert.Equal("At least one plane must be provided to divide the curve.", ex.Message);

			for (int i = 1; i < 5; i++)
			{
				Point3d planeOrigin = new Point3d(i * 500, 0, 0);
				Plane splitPlane = new Plane(planeOrigin, Vector3d.XAxis);
				splitPlanes.Add(splitPlane);
			}

			List<Curve> splitCurves = Curves.DivideCurve(Boundary, splitPlanes);

			Assert.Equal<int>(5, splitCurves.Count);
			Assert.All<Curve>(splitCurves, result => Assert.True(result.IsClosed));

			double boundaryArea = AreaMassProperties.Compute(Boundary).Area;

			double splitAreas = 0;
			foreach(Curve crv in splitCurves)
			{
				double crvArea = AreaMassProperties.Compute(crv).Area;
				splitAreas += crvArea;
			}

			Assert.Equal<double>(Math.Round(boundaryArea, 0), Math.Round(splitAreas, 0));
		}
	}

}