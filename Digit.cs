using System.Numerics;

namespace PaperIO;

partial class Program { 
    public class Digit
    {
        private static Dictionary<int, bool[,]> _pixels = new()
        {
            { 0, new[,] {
                {false, true, true, false}, 
                {true, false, false, true},
                {true, false, false, true},
                {true, false, false, true},
                {true, false, false, true},
                {true, false, false, true},
                {true, false, false, true},
                {false, true, true, false}
            }},
            { 1, new[,] {
                {false, false, true, false}, 
                {false, true, true, false},
                {true, false, true, false},
                {false, false, true, false},
                {false, false, true, false},
                {false, false, true, false},
                {false, false, true, false},
                {true, true, true, true}
            }},
            { 2, new[,] {
                {false, true, true, false}, 
                {true, false, false, true},
                {true, false, false, true},
                {false, false, false, true},
                {false, false, true, false},
                {false, true, false, false},
                {true, false, false, false},
                {true, true, true, true}
            }},
            { 3, new[,] {
                {false, true, true, false}, 
                {true, false, false, true},
                {false, false, false, true},
                {false, false, true, false},
                {false, false, false, true},
                {false, false, false, true},
                {true, false, false, true},
                {false, true, true, false}
            }},
            { 4, new[,] {
                {true, false, false, true}, 
                {true, false, false, true},
                {true, false, false, true},
                {true, true, true, true},
                {false, false, false, true},
                {false, false, false, true},
                {false, false, false, true},
                {false, false, false, true}
            }},
            { 5, new[,] {
                {true, true, true, true}, 
                {true, false, false, false},
                {true, false, false, false},
                {true, true, true, false},
                {false, false, false, true},
                {false, false, false, true},
                {false, false, false, true},
                {true, true, true, false}
            }},
            { 6, new[,] {
                {false, true, true, false}, 
                {true, false, false, true},
                {true, false, false, false},
                {true, false, false, false},
                {true, true, true, false},
                {true, false, false, true},
                {true, false, false, true},
                {false, true, true, false}
            }},
            { 7, new[,] {
                {true, true, true, true}, 
                {true, false, false, true},
                {false, false, false, true},
                {false, false, false, true},
                {false, false, true, false},
                {false, true, false, false},
                {false, true, false, false},
                {false, true, false, false}
            }},
            { 8, new[,] {
                {false, true, true, false}, 
                {true, false, false, true},
                {true, false, false, true},
                {true, false, false, true},
                {false, true, true, false},
                {true, false, false, true},
                {true, false, false, true},
                {false, true, true, false}
            }},
            { 9, new[,] {
                {false, true, true, false}, 
                {true, false, false, true},
                {true, false, false, true},
                {true, false, false, true},
                {false, true, true, true},
                {false, false, false, true},
                {true, false, false, true},
                {false, true, true, false}
            }},
        };
        
        public Vector2 anchor;
        public int digit;

        public Digit(int digit, Vector2 anchor)
        {
            this.digit = digit;
            this.anchor = anchor;
        }

        public List<Vector2> GetPixels()
        {
            bool[,] buffer = _pixels[digit];
            List<Vector2> output = new();

            for (int by = 0; by < 16; by++)
            {
                for (int bx = 0; bx < 8; bx++)
                {
                    if (buffer[by / 2, bx / 2])
                        output.Add(new Vector2(anchor.X + bx, anchor.Y + by));
                }
            }

            return output;
        }

    }
}