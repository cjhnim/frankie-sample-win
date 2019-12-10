using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BitmapMaker
{
    [TestClass]
    public class bitmap16tests
    {
        [TestMethod]
        public void gradientImage()
        {
            int width = 4095;
            int height = 1200;
            int bits = 12;

            byte[] wedge = Wedge(width, height, bits);

            Bitmap bmp = Convert(wedge, width, height, bits);

            string file = "images/gradient_16.png";

            bmp.Save(file);
        }

        [TestMethod]
        public void convertTo12bits()
        {
            FileStream fs = File.Open("images/1206_104039.826.raw", FileMode.Open);
            int bits = 12;

            Bitmap bmp = convertToBitmap(fs, bits);

            string file = "images/12.png";

            bmp.Save(file);
        }

        [TestMethod]
        public void convertTo10bits()
        {
            FileStream fs = File.Open("images/1206_104039.826.raw", FileMode.Open);
            int bits = 10;

            Bitmap bmp = convertToBitmap(fs, bits);

            string file = "images/10.png";

            bmp.Save(file);
        }

        private static Bitmap convertToBitmap(FileStream fs, int bits)
        {
            int shift = 16 - bits;
            var raw = new List<byte>();

            using (BinaryReader reader = new BinaryReader(fs))
            {
                try
                {
                    while (true)
                    {
                        byte data2 = reader.ReadByte();
                        raw.Add(data2);
                        Trace.WriteLine(string.Format("2:0x{0:X}", data2));

                        byte data1 = (byte)(reader.ReadByte() >> shift);
                        raw.Add(data1);
                        Trace.WriteLine(string.Format("1:0x{0:X}", data1));
                    }
                }
                catch (System.IO.EndOfStreamException)
                {
                    Trace.WriteLine("All read complete");
                }
            }

            Bitmap bmp = Convert(raw.ToArray(), 176, 218, bits);
            return bmp;
        }

        static Bitmap Convert(byte[] input, int width, int height, int bits)
        {
            // Convert byte buffer (2 bytes per pixel) to 32-bit ARGB bitmap

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var rect = new Rectangle(0, 0, width, height);

            var lut = CreateLut(bits);

            var bitmap_data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            ConvertCore(width, height, bits, input, bitmap_data, lut);

            bitmap.UnlockBits(bitmap_data);

            return bitmap;
        }

        static unsafe void ConvertCore(int width, int height, int bits, byte[] input, BitmapData output, uint[] lut)
        {
            // Copy pixels from input to output, applying LUT

            ushort mask = (ushort)((1 << bits) - 1);

            int in_stride = output.Stride;
            int out_stride = width * 2;

            byte* out_data = (byte*)output.Scan0;

            fixed (byte* in_data = input)
            {
                for (int y = 0; y < height; y++)
                {
                    uint* out_row = (uint*)(out_data + (y * in_stride));

                    ushort* in_row = (ushort*)(in_data + (y * out_stride));

                    for (int x = 0; x < width; x++)
                    {
                        ushort in_pixel = (ushort)(in_row[x] & mask);

                        out_row[x] = lut[in_pixel];
                    }
                }
            }
        }

        static uint[] CreateLut(int bits)
        {
            // Create a linear LUT to convert from grayscale to ARGB

            int max_input = 1 << bits;

            uint[] lut = new uint[max_input];

            for (int i = 0; i < max_input; i++)
            {
                // map input value to 8-bit range
                //
                byte intensity = (byte)((i * 0xFF) / max_input);

                // create ARGB output value A=255, R=G=B=intensity
                //
                lut[i] = (uint)(0xFF000000L | (intensity * 0x00010101L));
            }

            return lut;
        }

        static byte[] Wedge(int width, int height, int bits)
        {
            // horizontal wedge

            int max = 1 << bits;

            byte[] pixels = new byte[width * height * 2];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixel = x % max;

                    int addr = ((y * width) + x) * 2;

                    pixels[addr + 1] = (byte)((pixel & 0xFF00) >> 8);
                    pixels[addr + 0] = (byte)((pixel & 0x00FF));
                }
            }

            return pixels;
        }
    }
}
