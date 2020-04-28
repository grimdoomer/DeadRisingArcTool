using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IO
{
    public class EndianReader : BinaryReader
    {
        #region Fields

        public Endianness Endian { get; set; }

        #endregion

        #region Constructors

        public EndianReader(Stream stream) : base(stream)
        {
            // Default to little endian.
            this.Endian = Endianness.Little;
        }

        public EndianReader(Endianness Endian, Stream Input)
            : base(Input)
        {
            this.Endian = Endian;
        }

        public EndianReader(Endianness Endian, Stream input, Encoding encoding)
            : base(input, encoding)
        {
            this.Endian = Endian;
        }

        #endregion

        #region Methods

        public override double ReadDouble()
        {
            return ReadDouble(this.Endian);
        }

        public double ReadDouble(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(8);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToDouble(Data, 0);
        }

        public override short ReadInt16()
        {
            return ReadInt16(this.Endian);
        }

        public short ReadInt16(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(2);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToInt16(Data, 0);
        }

        public override int ReadInt32()
        {
            return ReadInt32(this.Endian);
        }

        public int ReadInt32(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(4);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToInt32(Data, 0);
        }

        public override long ReadInt64()
        {
            return ReadInt64(this.Endian);
        }

        public long ReadInt64(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(8);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToInt64(Data, 0);
        }

        public override float ReadSingle()
        {
            return ReadSingle(this.Endian);
        }

        public float ReadSingle(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(4);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToSingle(Data, 0);
        }

        public string ReadUnicodeString(int Length)
        {
            return ReadUnicodeString(this.Endian, Length);
        }

        public string ReadUnicodeString(Endianness Endian, int Length)
        {
            // Get Bytes
            string val = "";

            // Convert To String
            for (int i = 0; i < Length; i++)
            {
                // Get Char
                byte c = 0;

                // Endianness
                if (Endian == Endianness.Little)
                {
                    c = base.ReadByte();
                    base.BaseStream.Position++;
                }
                else
                {
                    base.BaseStream.Position++;
                    c = base.ReadByte();
                }

                // Convert To Char
                val += Convert.ToChar(c);
            }

            // Return
            return val;
        }

        public string ReadNullTerminatingString()
        {
            // Temp Place Holders
            string temp = "";
            byte c;

            // Read String
            while ((c = base.ReadByte()) != 0x00)
                temp += Convert.ToChar(c);

            // Return 
            return temp;
        }

        public override ushort ReadUInt16()
        {
            return ReadUInt16(this.Endian);
        }

        public ushort ReadUInt16(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(2);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToUInt16(Data, 0);
        }

        public override uint ReadUInt32()
        {
            return ReadUInt32(this.Endian);
        }

        public uint ReadUInt32(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(4);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToUInt32(Data, 0);
        }

        public override ulong ReadUInt64()
        {
            return ReadUInt64(this.Endian);
        }

        public ulong ReadUInt64(Endianness Endian)
        {
            // Get Bytes
            byte[] Data = base.ReadBytes(8);

            // Reverse
            if (Endian == Endianness.Big)
                Array.Reverse(Data);

            // Return
            return BitConverter.ToUInt64(Data, 0);
        }

        #endregion
    }
}
