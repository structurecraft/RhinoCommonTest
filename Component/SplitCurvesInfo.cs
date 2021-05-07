using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace SplitCurves.Component
{
	public class SplitCurvesInfo : GH_AssemblyInfo
	{
		public override string Name=>  "SplitCurves";

		public override Bitmap Icon => null;

		public override string Description => "Splits a curve with planes.";		

		public override Guid Id	=> new Guid("11e9f5c7-3a6c-4c62-b02d-b923e6abdff4");

		public override string AuthorName => "Arjun Sharma";

		public override string AuthorContact => "arjun.sharma.arch@gmail.com";

	}
}
