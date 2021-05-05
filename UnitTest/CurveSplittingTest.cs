using Rhino;
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

        private static List<Plane> SplitPlanes
        {
            get
            {
                List<Plane> planes = new List<Plane>();

                for (int i = 1; i < 5; i++)
                {
                    Point3d planeOrigin = new Point3d(i * 3, 0, 0);
                    Plane splitPlane = new Plane(planeOrigin, Vector3d.XAxis);
                    planes.Add(splitPlane);
                }

                return planes;
            }
        }

        [Theory]
        [MemberData(nameof(CurveCollection.Boundaries), MemberType = typeof(CurveCollection))]
        public void Cuts_A_Closed_Boundary_Curve_Into_A_Set_Of_Closed_Curves(Curve boundary)
        {
            // Arrange
            int numberOfCuts = 5;
            double boundaryArea = AreaMassProperties.Compute(boundary).Area;

            // Act
            List<Curve> splitCurves = Curves.DivideCurve(boundary, SplitPlanes);
            List<Curve> splitCurves2 = Curves.DivideCurve2(boundary, SplitPlanes);

            // Assert
            // Using Fluent Assertions these assertions can be chained.
            Assert.Equal<int>(numberOfCuts, splitCurves.Count);
            Assert.Equal<int>(numberOfCuts, splitCurves2.Count);
            Assert.All<Curve>(splitCurves, result => Assert.True(result.IsClosed));
            Assert.All<Curve>(splitCurves2, result => Assert.True(result.IsClosed));

            double splitAreas = 0;
            foreach (Curve crv in splitCurves)
            {
                double crvArea = AreaMassProperties.Compute(crv).Area;
                splitAreas += crvArea;
            }

            Assert.Equal<double>(Math.Round(boundaryArea, 0), Math.Round(splitAreas, 0));
        }

        [Fact]
        public void DivideCurve_Throw_An_Exception_If_Input_Planes_Is_Null_Or_Empty()
        {
            // Arrange
            List<Plane> emptyPlanes = new List<Plane>();
            List<Plane> nullPlanes = null;

            // Assert
            Exception ex = Assert.Throws<Exception>(() => Curves.DivideCurve(CurveCollection.CircleBoundary, emptyPlanes));
            Assert.Equal("Input planes is null or empty!", ex.Message);
            Assert.Throws<Exception>(() => Curves.DivideCurve(CurveCollection.CircleBoundary, nullPlanes));
        }

        [Fact]
        public void DivideCurve_Throw_An_Exception_If_Input_Planes_Are_Not_Parallel()
        {
            // Arrange
            List<Plane> planes = new List<Plane>(SplitPlanes);
            Plane p = planes[2];
            p.Rotate(RhinoMath.ToRadians(35), Vector3d.ZAxis, planes[2].Origin);
            planes[2] = p;

            // Assert
            Exception ex = Assert.Throws<Exception>(() => Curves.DivideCurve(CurveCollection.CircleBoundary, planes));
            Assert.Equal("Planes should be parallel!", ex.Message);
        }

        [Fact]
        public void DivideCurve_Throw_An_Exception_If_The_Boundary_Is_Open()
        {
            // Assert
            Exception ex = Assert.Throws<Exception>(() => Curves.DivideCurve(CurveCollection.OpenBoundary, SplitPlanes));
            Assert.Equal("Boundary should be closed!", ex.Message);
        }

        [Fact]
        public void DivideCurve2_Throw_An_Exception_If_The_Boundary_Is_Not_Planar()
        {
            // Assert
            Exception ex = Assert.Throws<Exception>(() => Curves.DivideCurve2(CurveCollection.NonPlanarBoundary, SplitPlanes));
            Assert.Equal("Boundary should be Planar!", ex.Message);
        }
    }
}