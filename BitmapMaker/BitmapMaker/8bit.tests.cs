using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitmapMaker
{
    [TestClass]
    public class bitmap8tests
    {
        private const string IMAGE_FILE_NAME = "images/1206_104039.826.raw";

        [TestMethod]
        public void convertTo8bits()
        {


            var raw = new List<byte>();
            FileStream fs = File.Open(IMAGE_FILE_NAME, FileMode.Open);

            using (BinaryReader reader = new BinaryReader(fs))
            {
                try
                {
                    while (true)
                    {
                        ushort data16 = reader.ReadUInt16();
                        byte data8 = (byte)(data16 >> 8);
                        raw.Add(data8);
                        Trace.WriteLine(string.Format("0x{0:X} ", data8));
                    }
                }
                catch (System.IO.EndOfStreamException)
                {
                    Trace.WriteLine("All read complete");
                }
            }

            var height = 218;
            var width = 176;
            var bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            unsafe
            {
                byte* ptr = (byte*)data.Scan0.ToPointer();

                for (int i = 0; i < raw.Count; i++)
                {
                    *ptr = raw[i];
                    ptr++;
                }
            }

            bitmap.UnlockBits(data);
            SetGrayscaledPallete(bitmap);
            bitmap.Save("images/8.bmp");

        }

        public static void SetGrayscaledPallete(Bitmap image)
        {
            ColorPalette grayPalette = image.Palette;
            for (int i = 0; i < 256; i++)
            {
                grayPalette.Entries[i] = Color.FromArgb(i, i, i);
                image.Palette = grayPalette;
            }
        }
    }
}
