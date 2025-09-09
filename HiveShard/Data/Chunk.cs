using System.Collections.Generic;

namespace HiveShard.Data
{
    public class Chunk
    {
        public int XCoord { get; }
        public int YCoord { get; }

        public const int Size = 128;
        public const int MaxChunks = MaxRow * MaxRow;
        public const int MaxRow = 3;

        public Chunk(int xCoord, int yCoord)
        {
            XCoord = xCoord;
            YCoord = yCoord;
        }

        public string Topic => $"{XCoord}-{YCoord}";

        public IEnumerable<Chunk> GetNeighbours()
        {
            List<Chunk> chunks = new List<Chunk>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Chunk chunk = new Chunk(XCoord + i, YCoord + j);
                    if(chunk.XCoord < 0 ||chunk.YCoord < 0 || chunk.XCoord >= Chunk.MaxRow || chunk.YCoord >= Chunk.MaxRow)
                        continue;
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }

        protected bool Equals(Chunk other)
        {
            return XCoord == other.XCoord && YCoord == other.YCoord;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Chunk)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (XCoord * 397) ^ YCoord;
            }
        }

        public int ToPartition()
        {
            return XCoord * MaxRow + YCoord;
        }

        public override string ToString()
        {
            return Topic;
        }
    }
}