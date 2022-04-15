using System;
using System.Text.Json;

namespace wloutput
{
    public struct Point
    {
        public int X, Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}