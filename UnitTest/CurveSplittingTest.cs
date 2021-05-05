using System.Collections.Generic;
using System;

using Rhino.Geometry;
using Rhino;

using Xunit;
using FluentAssertions;

using SplitCurves.Lib;
using SplitCurve.Lib;
using System.Linq;

namespace SplitCurves.Testing
{

	[Collection("Rhino Collection")]
	public class CurveSplittingTest : IDisposable
	{
		private List<Point3d> ControlPointsOfInterpolatedCurve;
		private List<Point3d> CornerPointsOfPolylineCurve;
		private Curve RectangularCurve;
		private Curve PolylineCurve;
		private Curve CircleCurve;
		private Curve InterpolatedCurve;

        public CurveSplittingTest()
        {
			// Initialize control points of interpolated nurbs curve.
			this.ControlPointsOfInterpolatedCurve = new List<Point3d>()
			{
				new Point3d(0,0,0),
				new Point3d(1000,0,0),
				new Point3d(1200,550,0),
				new Point3d(750,650,0),
				new Point3d(200,200,0),
			};

			// Initialize corner points of polyline curve.
			this.CornerPointsOfPolylineCurve = new List<Point3d>()
			{
				new Point3d(0,0,0),
				new Point3d(1000,0,0),
				new Point3d(1200,100,0),
				new Point3d(1200,400,0),
				new Point3d(0,400,0),
			};

			this.RectangularCurve = BoundaryCreator.RectangularCurve(Point3d.Origin, 5000, 1000);
			this.PolylineCurve = BoundaryCreator.PolylineCurve(this.CornerPointsOfPolylineCurve);
			this.CircleCurve = BoundaryCreator.CircleCurve(Point3d.Origin, 500);
			this.InterpolatedCurve = BoundaryCreator.InterpolatedCurve(this.ControlPointsOfInterpolatedCurve, 3);
		}

        public void Dispose()
        {
			this.RectangularCurve.Dispose();
			this.PolylineCurve.Dispose();
			this.CircleCurve.Dispose();
			this.InterpolatedCurve.Dispose();
        }

		[Theory]
		[MemberData(nameof(Theories.AngleAndCount), MemberType = typeof(Theories))]
		public void RectangularCurveSplittingTest(KeyValuePair<double, int> angleAndCount)
		{
			// Arrange
			double boundaryArea = AreaMassProperties.Compute(this.RectangularCurve).Area;
			IEnumerable<Plane> splitPlanes = DividerCreator.FromAzimuth(this.RectangularCurve, angleAndCount.Key, angleAndCount.Value);

			// Act
			IEnumerable<Curve> splitCurves = Curves.DivideCurve(this.RectangularCurve, splitPlanes);

			// Assertions
			splitCurves.Should().HaveCount(angleAndCount.Value, "because we divided our curve with [angleAndCount.Value - 1] planes.");
			splitCurves.All(c => c.IsClosed).Should().BeTrue("because DivideCurve have to serve them as closed.");
			splitCurves.Sum(c => AreaMassProperties.Compute(c).Area).Should().BeApproximately(boundaryArea, 10e2, "because area summation of divided curves should be equal to source curve.");
		}

		[Theory]
		[MemberData(nameof(Theories.AngleAndCount), MemberType = typeof(Theories))]
		public void PolylineCurveSplittingTest(KeyValuePair<double, int> angleAndCount)
		{
			// Arrange
			double boundaryArea = AreaMassProperties.Compute(this.PolylineCurve).Area;
			IEnumerable<Plane> splitPlanes = DividerCreator.FromAzimuth(this.PolylineCurve, angleAndCount.Key, angleAndCount.Value);

			// Act
			IEnumerable<Curve> splitCurves = Curves.DivideCurve(this.PolylineCurve, splitPlanes);

			// Assertions
			splitCurves.Should().HaveCount(angleAndCount.Value, "because we divided our curve with [angleAndCount.Value - 1] planes.");
			splitCurves.All(c => c.IsClosed).Should().BeTrue("because DivideCurve have to serve them as closed.");
			splitCurves.Sum(c => AreaMassProperties.Compute(c).Area).Should().BeApproximately(boundaryArea, 10e2, "because area summation of divided curves should be equal to source curve.");
		}

		[Theory]
		[MemberData(nameof(Theories.AngleAndCount), MemberType = typeof(Theories))]
		public void CircleCurveSplittingTest(KeyValuePair<double, int> angleAndCount)
		{
			// Arrange
			double boundaryArea = AreaMassProperties.Compute(this.CircleCurve).Area;
			IEnumerable<Plane> splitPlanes = DividerCreator.FromAzimuth(this.CircleCurve, angleAndCount.Key, angleAndCount.Value);

			// Act
			IEnumerable<Curve> splitCurves = Curves.DivideCurve(this.CircleCurve, splitPlanes);

			// Assertions
			splitCurves.Should().HaveCount(angleAndCount.Value, "because we divided our curve with [angleAndCount.Value - 1] planes.");
			splitCurves.All(c => c.IsClosed).Should().BeTrue("because DivideCurve have to serve them as closed.");
			splitCurves.Sum(c => AreaMassProperties.Compute(c).Area).Should().BeApproximately(boundaryArea, 10e2, "because area summation of divided curves should be equal to source curve.");
		}

		[Theory]
		[MemberData(nameof(Theories.AngleAndCount), MemberType = typeof(Theories))]
		public void InterpolatedCurveSplittingTest(KeyValuePair<double, int> angleAndCount)
		{
			// Arrange
			double boundaryArea = AreaMassProperties.Compute(this.InterpolatedCurve).Area;
			IEnumerable<Plane> splitPlanes = DividerCreator.FromAzimuth(this.InterpolatedCurve, angleAndCount.Key, angleAndCount.Value);

			// Act
			IEnumerable<Curve> splitCurves = Curves.DivideCurve(this.InterpolatedCurve, splitPlanes);

			// Assertions
			splitCurves.Should().HaveCount(angleAndCount.Value, "because we divided our curve with [angleAndCount.Value - 1] planes.");
			splitCurves.All(c => c.IsClosed).Should().BeTrue("because DivideCurve have to serve them as closed.");
			splitCurves.Sum(c => AreaMassProperties.Compute(c).Area).Should().BeApproximately(boundaryArea, 10e2, "because area summation of divided curves should be equal to source curve.");
		}
	}
}