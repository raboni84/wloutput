using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using toolbelt;

namespace wloutput
{
    static class Program
    {
        public static void Main(string[] args)
        {
            string background = FindBackgroundImage();
            Console.Error.WriteLine($"background image: {background}");

            List<Screen> setup = new List<Screen>();
            FindBestSetupForAllScreens(setup);
            int width, height;
            FindBestScaleAndPositionForAllScreens(setup, out width, out height);
            CropBackgroundImageForAllScreens(background, width, height, setup);

            foreach (var elem in setup)
            {
                Console.Error.WriteLine($"display \"{elem.Name}\" geometry {elem.Geometry.Wcm}cm {elem.Geometry.Hcm}cm {elem.Geometry.Ppcm}ppcm");
                string cmd = $"output \"{elem.Name}\" mode {elem.Mode.W}x{elem.Mode.H}@{elem.Mode.R / 1000}Hz pos {elem.Position.X} {elem.Position.Y} scale {elem.Scale} scale_filter {elem.ScaleFilter} bg \"{elem.Background}\" fill";
                Console.Error.WriteLine(cmd);
                ShellUtils.RunShellAsync("swaymsg", cmd, Console.OpenStandardError()).Await();
            }
        }

        private static void CropBackgroundImageForAllScreens(string background, int width, int height, List<Screen> setup)
        {
            DirectoryInfo di = Directory.CreateDirectory("/tmp/wloutput");
            using (Bitmap img = new Bitmap(background))
            {
                int newWidth = width;
                int newHeight = (int)(img.Height * ((double)width / img.Width));
                if (newHeight < height)
                {
                    newWidth = (int)(img.Width * ((double)height / img.Height));
                    newHeight = height;
                }
                using (Bitmap resized = new Bitmap(img, newWidth, newHeight))
                {
                    int xshift = (resized.Width / 2) - (width / 2);
                    int yshift = (resized.Height / 2) - (height / 2);
                    for (int i = 0; i < setup.Count; i++)
                    {
                        string filepath = di.FullName + "/" + Path.GetRandomFileName() + ".jpg";
                        Bitmap crop = resized.Clone(
                            new System.Drawing.Rectangle(
                                setup[i].Position.X + xshift,
                                setup[i].Position.Y + yshift,
                                setup[i].Position.W,
                                setup[i].Position.H), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        crop.Save(filepath);
                        setup[i].Background = filepath;
                    }
                }
            }
        }

        private static void FindBestScaleAndPositionForAllScreens(List<Screen> setup, out int width, out int height)
        {
            int minppcm = setup.Min(x => x.Geometry.Ppcm);
            int maxvert = setup.Max(x => x.Mode.H);
            int toppos = 0, rightpos = 0;
            Console.Error.WriteLine($"smallest diagonal: {minppcm}ppcm");
            for (int i = 0; i < setup.Count; i++)
            {
                decimal zoom = Math.Round((decimal)setup[i].Geometry.Ppcm / minppcm, 2);
                string zoomstr = zoom.ToString(CultureInfo.InvariantCulture);
                string filter = "nearest";
                if (zoomstr.Contains('.'))
                    filter = "linear";

                int screenWidth = (int)Math.Ceiling((decimal)setup[i].Mode.W / zoom);
                int screenHeight = (int)Math.Ceiling((decimal)setup[i].Mode.H / zoom);
                toppos = (maxvert / 2) - (screenHeight / 2);
                setup[i].Position = new Rectangle(rightpos, toppos, screenWidth, screenHeight);
                setup[i].Scale = zoomstr;
                setup[i].ScaleFilter = filter;
                rightpos += screenWidth;
            }
            width = rightpos;
            height = maxvert;
        }

        private static string FindBackgroundImage()
        {
            var files = Directory.EnumerateFiles("/usr/share/backgrounds", "*", new EnumerationOptions { RecurseSubdirectories = true });
            var pics = files.Where(x => x.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || x.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
            var notall = pics.Where(x => !x.Contains("/sway/"));
            string background = Shuffle(notall).First();
            return background;
        }

        private static void FindBestSetupForAllScreens(List<Screen> setup)
        {
            string outputs = ShellUtils.RunShellTextAsync("swaymsg", "-t get_outputs -r").Await();
            using (JsonDocument jdoc = JsonDocument.Parse(outputs))
            {
                foreach (var output in jdoc.RootElement.EnumerateArray())
                {
                    string name = GetOutputName(output);
                    //Rectangle rect = GetOutputDimensions(output);
                    Mode[] modes = GetOutputModes(output);
                    Mode matched = modes
                        .OrderByDescending(x => x.W)
                        .ThenByDescending(x => (decimal)x.W / (decimal)x.H)
                        .ThenByDescending(x => x.R)
                        .FirstOrDefault();
                    Geometry geom = GetOutputGeometry(name, matched);
                    Screen screen = new Screen(name, matched, geom, new Rectangle(), null, null, null);
                    setup.Add(screen);
                }
            }
        }

        private static Geometry GetOutputGeometry(string name, Mode mode)
        {
            string path = Directory.EnumerateDirectories("/sys/class/drm", $"card*-{name}*").FirstOrDefault();
            if (path != null)
            {
                byte[] data = File.ReadAllBytes($"{path}/edid");
                byte horizontal = data[21];
                byte vertical = data[22];
                return new Geometry(horizontal, vertical, mode);
            }
            return new Geometry(0, 0, 0);
        }

        private static Mode[] GetOutputModes(JsonElement output)
        {
            List<Mode> modes = new List<Mode>();
            JsonElement elem;
            if (!output.TryGetProperty("modes", out elem))
                throw new InvalidDataException("modes");
            foreach (var iter in elem.EnumerateArray())
            {
                modes.Add(new Mode(iter));
            }
            return modes.ToArray();
        }

        private static Rectangle GetOutputDimensions(JsonElement output)
        {
            JsonElement elem;
            if (!output.TryGetProperty("rect", out elem))
                throw new InvalidDataException("rect");
            return new Rectangle(elem);
        }

        private static string GetOutputName(JsonElement output)
        {
            JsonElement elem;
            if (!output.TryGetProperty("name", out elem))
                throw new InvalidDataException("name");
            return elem.GetString();
        }

        private static IEnumerable<T> Shuffle<T>(IEnumerable<T> list)
        {
            var r = new Random();
            var shuffledList =
                list.Select(x => new { Number = r.Next(), Item = x })
                    .OrderBy(x => x.Number)
                    .Select(x => x.Item);
            return shuffledList.ToList();
        }
    }
}