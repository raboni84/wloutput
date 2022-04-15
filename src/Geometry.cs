using System;
using System.Text.Json;
using toolbelt;

namespace wloutput
{
    public struct Geometry
    {
        public int Wcm, Hcm, Ppcm;

        public Geometry(int wcm, int hcm, int ppcm)
        {
            Wcm = wcm;
            Hcm = hcm;
            Ppcm = ppcm;
        }

        public Geometry(int wcm, int hcm, Mode mode)
        {
            try
            {
                Wcm = wcm;
                Hcm = hcm;
                double diagonalScreen = Mathematics.FastSquareRoot(((double)wcm * (double)wcm) + ((double)hcm * (double)hcm));
                double diagonalPixel = Mathematics.FastSquareRoot(((double)mode.W * (double)mode.W) + ((double)mode.H * (double)mode.H));
                Ppcm = (int)Math.Ceiling(diagonalPixel / diagonalScreen);
            }
            catch (Exception)
            {
                Wcm = 0;
                Hcm = 0;
                Ppcm = 0;
            }
        }
    }
}