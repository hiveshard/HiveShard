using System.Collections.Generic;

namespace DotChaser.Maps
{
    public class ElaborateMap: Map
    {
        public override int[] Grid { get; } =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 1, 1, 1, 0, 1, 1, 1, 0,
            0, 1, 0, 0, 0, 0, 0, 1, 0,
            0, 1, 0, 1, 0, 1, 0, 1, 0,
            0, 0, 0, 1, 0, 1, 0, 0, 0,
            1, 1, 1, 1, 0, 1, 1, 1, 1,
            0, 0, 0, 1, 0, 1, 0, 0, 0,
            0, 1, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 0, 0, 0, 0, 1, 0,
            0, 1, 1, 1, 0, 1, 1, 1, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0,
        };

        public override int Width => 9;
        public override int Height => 11;
    }
}