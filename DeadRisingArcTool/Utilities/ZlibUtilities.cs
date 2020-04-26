using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.Utilities
{
    // This class exists because there is a bug in Ionic.ZlibStream.Compress() that causes an
    // exception to be thrown when decompressing the buffer. This alternate CompressData()
    // method compresses the data in blocks and seems to resolve the issue. I'm not sure if
    // we need to do the same thing with decompress or not. It's seemed to work just fine this far.

    public class ZlibUtilities
    {
        /// <summary>
        /// Decompresses the Zlib input buffer
        /// </summary>
        /// <param name="compressedData">Data to be decompressed</param>
        /// <returns>The decompressed data</returns>
        public static byte[] DecompressData(byte[] compressedData)
        {
            return ZlibStream.UncompressBuffer(compressedData);
        }

        /// <summary>
        /// Compresses the input buffer using Zlib
        /// </summary>
        /// <param name="decompressedData">Data to be compressed</param>
        /// <returns>Zlib compressed buffer</returns>
        public static byte[] CompressData(byte[] decompressedData)
        {
            // Create a new memory stream for the input dat.
            using (MemoryStream input = new MemoryStream(decompressedData))
            {
                // Create a new memory stream for the output data.
                using (var raw = new MemoryStream())
                {
                    // Create the zlib compressor.
                    using (Stream compressor = new ZlibStream(raw, Ionic.Zlib.CompressionMode.Compress))
                    {
                        byte[] buffer = new byte[4096];
                        int n;

                        // Loop and compress all of the data in blocks.
                        while ((n = input.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            compressor.Write(buffer, 0, n);
                        }
                    }

                    // Return the compressed data as a byte array.
                    return raw.ToArray();
                }
            }
        }
    }
}
