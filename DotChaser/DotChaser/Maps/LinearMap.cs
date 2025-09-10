using System.Collections.Generic;
using System.Numerics;

namespace DotChaser.Maps
{
    public class LinearMap: Map
    {
        public override int[] Grid { get; } =
        {
            0, 0, 0,
        };

        public override int Width => 3;
        public override int Height => 1;
    }
}