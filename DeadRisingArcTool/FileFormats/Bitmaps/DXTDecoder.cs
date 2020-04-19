using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeadRisingArcTool.FileFormats.Bitmaps
{
    public class DXTDecoder
    {
        public static byte[] DecodeDXT1(int Height, int Width, byte[] sourceData)
        {
            byte[] destData = new byte[Height * Width * 4];
            Color.ColorRGBA[] color = new Color.ColorRGBA[4];
            int dptr = 0;
            Color.ColorRGBA CColor;
            int CData;
            int ChunksPerHLine = Width / 4;
            bool trans;
            Color.ColorRGBA zeroColor = new Color.ColorRGBA();
            uint c1;
            uint c2;
            if (ChunksPerHLine == 0)
            {
                ChunksPerHLine += 1;
            }
            for (int ptr = 0; ptr < (Width * Height); ptr += 16)
            {
                c1 = (Convert.ToUInt32(sourceData[dptr + 1]) << 8) | (sourceData[dptr]);
                c2 = (Convert.ToUInt32(sourceData[dptr + 3]) << 8) | (sourceData[dptr + 2]);
                if (c1 > c2)
                {
                    trans = false;
                }
                else
                {
                    trans = true;
                }
                color[0] = Color.IntToRGBA(c1);
                color[1] = Color.IntToRGBA(c2);
                if (!(trans))
                {
                    color[2] = Color.GradientColors(color[0], color[1]);
                    color[3] = Color.GradientColors(color[1], color[0]);
                }
                else
                {
                    color[2] = Color.GradientColorsHalf(color[0], color[1]);
                    color[3] = zeroColor;
                }
                CData = (Convert.ToInt32(sourceData[dptr + 4]) << 0) | (Convert.ToInt32(sourceData[dptr + 5]) << 8) | (Convert.ToInt32(sourceData[dptr + 6]) << 16) | (Convert.ToInt32(sourceData[dptr + 7]) << 24);
                int ChunkNum = ptr / 16;
                long XPos = ChunkNum % ChunksPerHLine;
                long YPos = (ChunkNum - XPos) / ChunksPerHLine;
                long ttmp;
                int sizeh = Height < 4 ? Height : 4;
                int sizew = Width < 4 ? Width : 4;
                for (int x = 0; x <= sizeh - 1; x++)
                {
                    for (int y = 0; y <= sizew - 1; y++)
                    {
                        CColor = color[CData & 3];
                        CData >>= 2;
                        ttmp = ((YPos * 4 + x) * Width + XPos * 4 + y) * 4;
                        destData[ttmp] = CColor.b;
                        destData[ttmp + 1] = CColor.g;
                        destData[ttmp + 2] = CColor.r;
                        destData[ttmp + 3] = CColor.a;
                    }
                }
                dptr += 8;
            }
            return destData;
        }

        public static byte[] DecodeDXT23(int Height, int Width, byte[] sourceData)
        {
            byte[] destData = new byte[Height * Width * 4];
            Color.ColorRGBA[] color = new Color.ColorRGBA[4];
            Color.ColorRGBA CColor;
            int CData;
            int ChunksPerHLine = Width / 4;

            if (ChunksPerHLine == 0)
            {
                ChunksPerHLine += 1;
            }

            for (int ptr = 0; ptr <= (Width * Height) - 1; ptr += 16)
            {
                color[0] = Color.IntToRGBA(Convert.ToUInt32(sourceData[ptr + 8]) | Convert.ToUInt32(sourceData[ptr + 9]) << 8);
                color[1] = Color.IntToRGBA(Convert.ToUInt32(sourceData[ptr + 10]) | Convert.ToUInt32(sourceData[ptr + 11]) << 8);
                color[2] = Color.GradientColors(color[0], color[1]);
                color[3] = Color.GradientColors(color[1], color[0]);
                CData = (Convert.ToInt32(sourceData[ptr + 12]) << 0) | (Convert.ToInt32(sourceData[ptr + 13]) << 8) | (Convert.ToInt32(sourceData[ptr + 14]) << 16) | (Convert.ToInt32(sourceData[ptr + 15]) << 24);
                int ChunkNum = ptr / 16;
                long XPos = ChunkNum % ChunksPerHLine;
                long YPos = (ChunkNum - XPos) / ChunksPerHLine;
                long ttmp;
                int alpha;
                int sizeh = Height < 4 ? Height : 4;
                int sizew = Width < 4 ? Width : 4;

                for (int x = 0; x <= sizeh - 1; x++)
                {
                    alpha = sourceData[ptr + (2 * x)] | Convert.ToInt32(sourceData[ptr + (2 * x) + 1]) << 8;
                    for (int y = 0; y <= sizew - 1; y++)
                    {
                        CColor = color[CData & 3];
                        CData >>= 2;
                        CColor.a = (byte)((alpha & 15) * 16);
                        alpha >>= 4;
                        ttmp = ((YPos * 4 + x) * Width + XPos * 4 + y) * 4;
                        destData[ttmp] = CColor.b;
                        destData[ttmp + 1] = CColor.g;
                        destData[ttmp + 2] = CColor.r;
                        destData[ttmp + 3] = CColor.a;
                    }
                }
            }
            return destData;
        }

        public static byte[] DecodeDXT45(int Height, int Width, byte[] sourceData)
        {
            byte[] destData = new byte[Height * Width * 4];
            Color.ColorRGBA[] color = new Color.ColorRGBA[4];
            Color.ColorRGBA CColor;
            int CData;
            int ChunksPerHLine = Width / 4;
            if (ChunksPerHLine == 0)
            {
                ChunksPerHLine += 1;
            }
            for (int ptr = 0; ptr <= (Width * Height) - 1; ptr += 16)
            {
                color[0] = Color.IntToRGBA(Convert.ToUInt32(sourceData[ptr + 8]) | Convert.ToUInt32(sourceData[ptr + 9]) << 8);
                color[1] = Color.IntToRGBA(Convert.ToUInt32(sourceData[ptr + 10]) | Convert.ToUInt32(sourceData[ptr + 11]) << 8);
                color[2] = Color.GradientColors(color[0], color[1]);
                color[3] = Color.GradientColors(color[1], color[0]);
                CData = (Convert.ToInt32(sourceData[ptr + 12]) << 0) | (Convert.ToInt32(sourceData[ptr + 13]) << 8) | (Convert.ToInt32(sourceData[ptr + 14]) << 16) | (Convert.ToInt32(sourceData[ptr + 15]) << 24);
                byte[] alpha = new byte[8];
                alpha[0] = sourceData[ptr];
                alpha[1] = sourceData[ptr + 1];
                if ((alpha[0] > alpha[1]))
                {
                    alpha[2] = (byte)((6 * alpha[0] + 1 * alpha[1] + 3) / 7);
                    alpha[3] = (byte)((5 * alpha[0] + 2 * alpha[1] + 3) / 7);
                    alpha[4] = (byte)((4 * alpha[0] + 3 * alpha[1] + 3) / 7);
                    alpha[5] = (byte)((3 * alpha[0] + 4 * alpha[1] + 3) / 7);
                    alpha[6] = (byte)((2 * alpha[0] + 5 * alpha[1] + 3) / 7);
                    alpha[7] = (byte)((1 * alpha[0] + 6 * alpha[1] + 3) / 7);
                }
                else
                {
                    alpha[2] = (byte)((4 * alpha[0] + 1 * alpha[1] + 2) / 5);
                    alpha[3] = (byte)((3 * alpha[0] + 2 * alpha[1] + 2) / 5);
                    alpha[4] = (byte)((2 * alpha[0] + 3 * alpha[1] + 2) / 5);
                    alpha[5] = (byte)((1 * alpha[0] + 4 * alpha[1] + 2) / 5);
                    alpha[6] = 0;
                    alpha[7] = 255;
                }
                long tmpdword;
                int tmpword;
                long alphaDat;
                tmpword = sourceData[ptr + 2] | (Convert.ToInt32(sourceData[ptr + 3]) << 8);
                tmpdword = sourceData[ptr + 4] | (Convert.ToInt32(sourceData[ptr + 5]) << 8) | (sourceData[ptr + 6] << 16) | (Convert.ToInt32(sourceData[ptr + 7]) << 24);
                alphaDat = tmpword | (tmpdword << 16);
                int ChunkNum = ptr / 16;
                long XPos = ChunkNum % ChunksPerHLine;
                long YPos = (ChunkNum - XPos) / ChunksPerHLine;
                long ttmp;
                int sizeh = Height < 4 ? Height : 4;
                int sizew = Width < 4 ? Width : 4;
                for (int x = 0; x <= sizeh - 1; x++)
                {
                    for (int y = 0; y <= sizew - 1; y++)
                    {
                        CColor = color[CData & 3];
                        CData >>= 2;
                        CColor.a = alpha[alphaDat & 7];
                        alphaDat >>= 3;
                        ttmp = ((YPos * 4 + x) * Width + XPos * 4 + y) * 4;

                        destData[ttmp] = CColor.b;
                        destData[ttmp + 1] = CColor.g;
                        destData[ttmp + 2] = CColor.r;
                        destData[ttmp + 3] = CColor.a;
                    }
                }
            }
            return destData;
        }

        public static byte[] DecodeDXT1Texture(int Width, int Height, byte[] SourceData)
        {
            Bitmaps.Color.ColorRGBA[] Color = new Bitmaps.Color.ColorRGBA[5];

            Bitmaps.Color.ColorRGBA CColor;
            Bitmaps.Color.ColorRGBA zeroColor;

            int CData;
            int c1;
            int c2;
            int dptr = 0;

            bool trans;
            byte[] DestData = new byte[(Width * Height) * 4];

            int ChunksPerHLine = Width / 4;
            if (ChunksPerHLine == 0) ChunksPerHLine = 1;

            for (int ptr = 0; ptr < (Width * Height); ptr += 16)
            {
                c1 = (SourceData[dptr + 1] << 8) | (SourceData[dptr]);
                c2 = (SourceData[dptr + 3] << 8) | (SourceData[dptr + 2]);

                trans = (!(c1 > c2));

                Color[0] = Bitmaps.Color.ShortToColor(c1);
                Color[1] = Bitmaps.Color.ShortToColor(c2);

                if (!trans)
                {
                    Color[2] = Bitmaps.Color.GradientColors(Color[0], Color[1]);
                    Color[3] = Bitmaps.Color.GradientColors(Color[1], Color[0]);
                }
                else
                {
                    zeroColor = Color[0];
                    Color[2] = Bitmaps.Color.GradientColorsHalf(Color[0], Color[1]);
                    Color[3] = zeroColor;
                }

                CData = (SourceData[dptr + 4] << 0) | (SourceData[dptr + 5] << 8) |
                    (SourceData[dptr + 6] << 16) | (SourceData[dptr + 7] << 24);

                int ChunkNum = ptr / 16;
                long XPos = ChunkNum % ChunksPerHLine;
                long YPos = (ChunkNum - XPos) / ChunksPerHLine;

                long tmp1, tmp2;

                int sizeh = Height < 4 ? Height : 4;
                int sizew = Width < 4 ? Width : 4;

                int x, y;
                for (x = 0; x < sizeh; x++)
                {
                    for (y = 0; y < sizew; y++)
                    {
                        CColor = Color[CData & 3];
                        CData >>= 2;
                        tmp1 = ((YPos * 4 + x) * Width + XPos * 4 + y) * 4;
                        tmp2 = Bitmaps.Color.ColorToInt(CColor);
                        DestData[tmp1] = CColor.b;
                        DestData[tmp1 + 1] = CColor.g;
                        DestData[tmp1 + 2] = CColor.r;
                        DestData[tmp1 + 3] = CColor.a;
                    }
                }
                dptr += 8;
            }
            return DestData;
        }

        public static byte[] DecodeDXT23Texture(int Width, int Height, byte[] SourceData)
        {
            Bitmaps.Color.ColorRGBA[] Color = new Bitmaps.Color.ColorRGBA[5];

            Bitmaps.Color.ColorRGBA CColor;
            Bitmaps.Color.ColorRGBA c1, c2, c3, c4;
            int CData;
            byte[] DestData = new byte[(Width * Height) * 4];

            int ChunksPerHLine = Width / 4;
            if (ChunksPerHLine == 0) ChunksPerHLine = 1;

            for (int ptr = 0; ptr < (Width * Height); ptr += 16)
            {
                c1 = Bitmaps.Color.ShortToColor((SourceData[ptr + 8]) | (SourceData[ptr + 9] << 8));
                c2 = Bitmaps.Color.ShortToColor((SourceData[ptr + 10]) | (SourceData[ptr + 11] << 8));
                c3 = Bitmaps.Color.GradientColors(Color[0], Color[1]);
                c4 = Bitmaps.Color.GradientColors(Color[1], Color[0]);
                Color[0] = c1;
                Color[1] = c2;
                Color[2] = c3;
                Color[3] = c4;

                CData = (SourceData[ptr + 12] << 0) | (SourceData[ptr + 13] << 8) | (SourceData[ptr + 14] << 16) | (SourceData[ptr + 15] << 24);

                int ChunkNum = ptr / 16;
                long XPos = ChunkNum % ChunksPerHLine;
                long YPos = (ChunkNum - XPos) / ChunksPerHLine;

                long ttmp;

                int alpha;
                int sizeh = Height < 4 ? Height : 4;
                int sizew = Width < 4 ? Width : 4;
                int x, y;

                for (x = 0; x < sizeh; x++)
                {
                    alpha = SourceData[ptr + (2 * x)] | (SourceData[ptr + (2 * x) + 1]) << 8;
                    for (y = 0; y < sizew; y++)
                    {
                        CColor = Color[CData & 3];
                        CData >>= 2;
                        CColor.a = (byte)((alpha & 15) * 16);
                        alpha >>= 4;
                        ttmp = ((YPos * 4 + x) * Width + XPos * 4 + y) * 4;

                        DestData[ttmp] = CColor.b;
                        DestData[ttmp + 1] = CColor.g;
                        DestData[ttmp + 2] = CColor.r;
                        DestData[ttmp + 3] = CColor.a;
                    }
                }
            }
            return DestData;
        }

        public static byte[] DecodeDXT45Texture(int Width, int Height, byte[] SourceData, bool bigEndian)
        {
            Bitmaps.Color.ColorRGBA[] Color = new Bitmaps.Color.ColorRGBA[4];
            Bitmaps.Color.ColorRGBA CColor;

            int CData;
            byte[] DestData = new byte[(Width * Height) * 4];

            int ChunksPerHLine = Width / 4;
            if (ChunksPerHLine == 0) ChunksPerHLine = 1;

            int xBlocks = Width / 4;
            int yBlocks = Height / 4;

            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    byte[] Alpha = new byte[8];

                    int ptr = ((y * xBlocks) + x) * 16;


                    if (bigEndian == false)
                    {
                        Color[0] = Bitmaps.Color.ShortToColor(SourceData[ptr + 8] | (SourceData[ptr + 9] << 8));
                        Color[1] = Bitmaps.Color.ShortToColor(SourceData[ptr + 10] | (SourceData[ptr + 11] << 8));
                        Color[2] = Bitmaps.Color.GradientColors(Color[0], Color[1]);
                        Color[3] = Bitmaps.Color.GradientColors(Color[1], Color[0]);

                        CData = (SourceData[ptr + 12] << 0) | (SourceData[ptr + 13] << 8) | (SourceData[ptr + 14] << 16) | (SourceData[ptr + 15] << 24);

                        Alpha[0] = SourceData[ptr];
                        Alpha[1] = SourceData[ptr + 1];
                    }
                    else
                    {
                        Color[0] = Bitmaps.Color.ShortToColor(SourceData[ptr + 9] | (SourceData[ptr + 8] << 8));
                        Color[1] = Bitmaps.Color.ShortToColor(SourceData[ptr + 11] | (SourceData[ptr + 10] << 8));
                        Color[2] = Bitmaps.Color.GradientColors(Color[0], Color[1]);
                        Color[3] = Bitmaps.Color.GradientColors(Color[1], Color[0]);

                        CData = (SourceData[ptr + 15]) | (SourceData[ptr + 14] << 8) | (SourceData[ptr + 13] << 16) | (SourceData[ptr + 12] << 24);

                        Alpha[0] = SourceData[ptr + 1];
                        Alpha[1] = SourceData[ptr];
                    }

                    //Do the alphas
                    if (Alpha[0] > Alpha[1])
                    {
                        // 8-alpha block:  derive the other six alphas.
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.

                        Alpha[2] = (byte)((6 * Alpha[0] + 1 * Alpha[1] + 3) / 7); // bit code 010
                        Alpha[3] = (byte)((5 * Alpha[0] + 2 * Alpha[1] + 3) / 7); // bit code 011
                        Alpha[4] = (byte)((4 * Alpha[0] + 3 * Alpha[1] + 3) / 7); // bit code 100
                        Alpha[5] = (byte)((3 * Alpha[0] + 4 * Alpha[1] + 3) / 7); // bit code 101
                        Alpha[6] = (byte)((2 * Alpha[0] + 5 * Alpha[1] + 3) / 7); // bit code 110
                        Alpha[7] = (byte)((1 * Alpha[0] + 6 * Alpha[1] + 3) / 7); // bit code 111
                    }
                    else
                    {
                        // 6-alpha block.
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        Alpha[2] = (byte)((4 * Alpha[0] + 1 * Alpha[1] + 2) / 5); // Bit code 010
                        Alpha[3] = (byte)((3 * Alpha[0] + 2 * Alpha[1] + 2) / 5); // Bit code 011
                        Alpha[4] = (byte)((2 * Alpha[0] + 3 * Alpha[1] + 2) / 5); // Bit code 100
                        Alpha[5] = (byte)((1 * Alpha[0] + 4 * Alpha[1] + 2) / 5); // Bit code 101
                        Alpha[6] = 0;            // Bit code 110
                        Alpha[7] = 255;          // Bit code 111
                    }

                    // Byte	Alpha
                    // 0	Alpha_0
                    // 1	Alpha_1 
                    // 2	(0)(2) (2 LSBs), (0)(1), (0)(0)
                    // 3	(1)(1) (1 LSB), (1)(0), (0)(3), (0)(2) (1 MSB)
                    // 4	(1)(3), (1)(2), (1)(1) (2 MSBs)
                    // 5	(2)(2) (2 LSBs), (2)(1), (2)(0)
                    // 6	(3)(1) (1 LSB), (3)(0), (2)(3), (2)(2) (1 MSB)
                    // 7	(3)(3), (3)(2), (3)(1) (2 MSBs)
                    // (0

                    // Read an int and a short
                    //int tmpword = SourceData[ptr + 2] | (SourceData[ptr + 3] << 8);
                    //long tmpdword = SourceData[ptr + 4] | (SourceData[ptr + 5] << 8) | (SourceData[ptr + 6] << 16) | (SourceData[ptr + 7] << 24);

                    //long alphaDat = tmpword | (tmpdword << 16);

                    // Build the alpha mask.
                    long alphaMask = 0;
                    if (bigEndian == false)
                    {
                        alphaMask = SourceData[ptr + 7];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 6];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 5];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 4];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 3];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 2];
                    }
                    else
                    {
                        alphaMask = SourceData[ptr + 6];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 7];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 4];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 5];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 2];
                        alphaMask <<= 8;
                        alphaMask |= SourceData[ptr + 3];
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            int pixelDataStart = (Width * (y * 4 + i) * 4) + ((x * 4 + j) * 4);

                            CColor = Color[CData & 3];
                            CData >>= 2;

                            CColor.a = Alpha[alphaMask & 7];
                            alphaMask >>= 3;
                            
                            DestData[pixelDataStart] = CColor.b;
                            DestData[pixelDataStart + 1] = CColor.g;
                            DestData[pixelDataStart + 2] = CColor.r;
                            DestData[pixelDataStart + 3] = CColor.a;
                        }
                    }

                    //int ChunkNum = ptr / 16;
                    //long XPos = ChunkNum % ChunksPerHLine;
                    //long YPos = (ChunkNum - XPos) / ChunksPerHLine;
                    //long ttmp;
                    //int sizeh = Height < 4 ? Height : 4;
                    //int sizew = Width < 4 ? Width : 4;
                    //int x, y;
                    //for (x = 0; x < sizeh; x++)
                    //{
                    //    for (y = 0; y < sizew; y++)
                    //    {
                    //        CColor = Color[CData & 3];
                    //        CData >>= 2;
                    //        CColor.a = Alpha[alphaMask & 7];
                    //        alphaMask >>= 3;
                    //        ttmp = ((YPos * 4 + (x)) * Width) + ((XPos * 4 + y) * 4);
                    //        DestData[ttmp] = CColor.b;
                    //        DestData[ttmp + 1] = CColor.g;
                    //        DestData[ttmp + 2] = CColor.r;
                    //        DestData[ttmp + 3] = CColor.a;
                    //    }
                    //}
                }
            }
            return DestData;
        }

        public static byte[] DecodeR8G8(float Width, float Height, byte[] SourceData)
        {
            // Allocate a new buffer to fit ARGB colors.
            byte[] decodedData = new byte[SourceData.Length * 2];

            // Loop through all of the pixels and convert to ARGB.
            for (int ptr = 0; ptr < SourceData.Length; ptr += 2)
            {
                decodedData[(ptr * 2)] = 255;
                decodedData[(ptr * 2) + 1] = CalculateU8V8Color(SourceData[ptr]);
                decodedData[(ptr * 2) + 2] = CalculateU8V8Color(SourceData[ptr + 1]);
                decodedData[(ptr * 2) + 3] = 0;
            }

            // Return the new buffer.
            return decodedData;

        }

        public static byte CalculateU8V8Color(byte Color)
        {
            if (Color == 0)
            {
                Color = 127;
                return Color;
            }
            else
            {
                Color += 127;
                return (byte)Color;
            }
        }
















        public static byte[] DecodeDXT5_DEBUG(byte[] data, int width, int height)
        {
            byte[] pixData = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;
            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 16;
                    uint[] alphas = new uint[8];
                    ulong alphaMask = 0;

                    alphas[0] = data[blockDataStart + 1];
                    alphas[1] = data[blockDataStart + 0];

                    alphaMask |= data[blockDataStart + 6];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 7];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 4];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 5];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 2];
                    alphaMask <<= 8;
                    alphaMask |= data[blockDataStart + 3];

                    // 8-alpha or 6-alpha block
                    if (alphas[0] > alphas[1])
                    {
                        // 8-alpha block: derive the other 6
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);    // bit code 010
                        alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);    // bit code 011
                        alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);    // bit code 100
                        alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);    // bit code 101
                        alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);    // bit code 110
                        alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);    // bit code 111
                    }
                    else
                    {
                        // 6-alpha block.
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);    // Bit code 010
                        alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);    // Bit code 011
                        alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);    // Bit code 100
                        alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);    // Bit code 101
                        alphas[6] = 0x00;                                               // Bit code 110
                        alphas[7] = 0xFF;                                               // Bit code 111
                    }

                    byte[,] alpha = new byte[4, 4];

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            alpha[j, i] = (byte)alphas[alphaMask & 7];
                            alphaMask >>= 3;
                        }
                    }

                    ushort color0 = (ushort)((data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                    ushort color1 = (ushort)((data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);

                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;
                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j = k ^ 1;
                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            pixData[pixDataStart + 3] = alpha[i, j];

                            switch (codeDec)
                            {
                                case 0:
                                    pixData[pixDataStart + 0] = (byte)r0;
                                    pixData[pixDataStart + 1] = (byte)g0;
                                    pixData[pixDataStart + 2] = (byte)b0;
                                    break;
                                case 1:
                                    pixData[pixDataStart + 0] = (byte)r1;
                                    pixData[pixDataStart + 1] = (byte)g1;
                                    pixData[pixDataStart + 2] = (byte)b1;
                                    break;
                                case 2:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        pixData[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        pixData[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = 0;
                                        pixData[pixDataStart + 1] = 0;
                                        pixData[pixDataStart + 2] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }
                }
            }
            return pixData;
        }
    }
}
