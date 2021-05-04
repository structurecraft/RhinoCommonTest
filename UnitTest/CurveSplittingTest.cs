using Rhino.Geometry;
using SplitCurves.Lib;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;


namespace SplitCurves.Testing
{

    [Collection("Rhino Collection")]
    public class CurveSplittingTest
    {
        private readonly ITestOutputHelper _testOutput;

        public CurveSplittingTest(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        private static Curve Boundary => new Rectangle3d(Plane.WorldXY, 5000, 1000).ToNurbsCurve();

        [Fact]
        public void SplittingTest()
        {
            // Arrange
            int numberOfCuts = 5;
            double boundaryArea = AreaMassProperties.Compute(Boundary).Area;

            List<Plane> splitPlanes = new List<Plane>();

            for (int i = 1; i < numberOfCuts; i++)
            {
                Point3d planeOrigin = new Point3d(i * 500, 0, 0);
                Plane splitPlane = new Plane(planeOrigin, Vector3d.XAxis);
                splitPlanes.Add(splitPlane);
            }

            // Act
            List<Curve> splitCurves = Curves.DivideCurve(Boundary, splitPlanes);
            //List<Curve> splitCurves = Curves.DivideCurve2(Boundary, splitPlanes);

            // Assert
            Assert.Equal<int>(numberOfCuts, splitCurves.Count);
            Assert.All<Curve>(splitCurves, result => Assert.True(result.IsClosed));

            double splitAreas = 0;
            foreach (Curve crv in splitCurves)
            {
                double crvArea = AreaMassProperties.Compute(crv).Area;
                splitAreas += crvArea;
            }

            Assert.Equal<double>(Math.Round(boundaryArea, 0), Math.Round(splitAreas, 0));
        }

    }

}