using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Endian
{
    public class EndianUtilities
    {
        public static short ByteFlip16(short value)
        {
            return (short)((value & 0xFF00) >> 8 | (value & 0xFF) << 8);
        }

        public static int ByteFlip32(int value)
        {
            return (int)((value & 0xFF000000) >> 24 | (value & 0xFF0000) >> 8 | (value & 0xFF00) << 8 | (value & 0xFF) << 24);
        }
    }
}
