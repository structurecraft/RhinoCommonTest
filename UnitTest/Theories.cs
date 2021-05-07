using System.Collections.Generic;
using Xunit;

namespace SplitCurves.Testing
{
    public static class Theories
    {
        public static TheoryData<KeyValuePair<double, int>> AngleAndCount = new TheoryData<KeyValuePair<double, int>>()
        {
            new KeyValuePair<double, int>(0, 4),
            new KeyValuePair<double, int>(45, 4),
            new KeyValuePair<double, int>(120, 3),
            new KeyValuePair<double, int>(120, 4),
            new KeyValuePair<double, int>(285, 10),
            new KeyValuePair<double, int>(78541209, 20),
        };
    }
}
