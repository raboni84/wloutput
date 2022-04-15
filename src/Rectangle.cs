using System;
using System.Text.Json;

namespace wloutput
{
    public struct Rectangle
    {
        public int X, Y, W, H;

        public Rectangle(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public Rectangle(JsonElement json)
        {
            try
            {
                X = json.GetProperty("x").GetInt32();
                Y = json.GetProperty("y").GetInt32();
                W = json.GetProperty("width").GetInt32();
                H = json.GetProperty("height").GetInt32();
            }
            catch (Exception)
            {
                X = 0;
                Y = 0;
                W = 0;
                H = 0;
            }
        }
    }
}