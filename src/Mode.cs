using System;
using System.Text.Json;

namespace wloutput
{
    public struct Mode
    {
        public int W, H, R;

        public Mode(int w, int h, int r)
        {
            W = w;
            H = h;
            R = r;
        }

        public Mode(JsonElement json)
        {
            try
            {
                W = json.GetProperty("width").GetInt32();
                H = json.GetProperty("height").GetInt32();
                R = json.GetProperty("refresh").GetInt32();
            }
            catch (Exception)
            {
                W = 0;
                H = 0;
                R = 0;
            }
        }
    }
}