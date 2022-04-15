using System;
using System.Text.Json;

namespace wloutput
{
    public class Screen
    {
        public string Name;
        public Mode Mode;
        public Geometry Geometry;
        public Rectangle Position;
        public string Scale;
        public string ScaleFilter;
        public string Background;

        public Screen(string name, Mode mode, Geometry geometry, Rectangle position, string scale, string scaleFilter, string background)
        {
            Name = name;
            Mode = mode;
            Geometry = geometry;
            Position = position;
            Scale = scale;
            ScaleFilter = scaleFilter;
            Background = background;
        }
    }
}