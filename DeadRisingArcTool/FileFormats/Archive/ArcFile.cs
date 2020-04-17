using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.Utilities;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool
{
    public enum ResourceType
    {
        Invalid,
        Texture,
        Model,
    }

    public struct ArcFileHeader
    {
        public const int kSizeOf = 8;
        public const int kHeaderMagic = 0x00435241;
        public const int kVersion = 4;

        public int Magic;
        public short Version;
        public short NumberOfFiles;
    }

    public struct ArcFileEntry
    {
        public string FileName { get; set; }
        public int FileType { get; set; }
        public int CompressedSize { get; set; }
        public int DecompressedSize { get; set;  }
        public int DataOffset { get; set; }
    }

    public class ArcFile
    {
        // File streams for read/write access.
        private FileStream fileStream = null;
        private BinaryReader reader = null;
        private BinaryWriter writer = null;

        /// <summary>
        /// Full file path of the arc file.
        /// </summary>
        public string FileName { get; private set;  }

        private List<ArcFileEntry> fileEntries = new List<ArcFileEntry>();
        /// <summary>
        /// Gets a list of file entries in the arc file.
        /// </summary>
        public ArcFileEntry[] FileEntries { get { return fileEntries.ToArray(); } }

        public ArcFile(string fileName)
        {
            // Initialize fields.
            this.FileName = fileName;
        }

        private bool OpenArcFile(bool forWrite)
        {
            // Determine if we are opening for R or RW access.
            FileAccess fileAccess = forWrite == true ? FileAccess.ReadWrite : FileAccess.Read;

            try
            {
                // Open the file for reading.
                this.fileStream = new FileStream(this.FileName, FileMode.Open, fileAccess, FileShare.Read);
                this.reader = new BinaryReader(this.fileStream);

                // If we want write access then open it for writing as well.
                if (forWrite == true)
                    this.writer = new BinaryWriter(this.fileStream);
            }
            catch (Exception e)
            {
                // Failed to open the file.
                return false;
            }

            // Successfully opened the file.
            return true;
        }

        private void CloseArcFile()
        {
            // Close all streams.
            if (this.reader != null)
                this.reader.Close();
            if (this.writer != null)
                this.writer.Close();
            if (this.fileStream != null)
                this.fileStream.Close();
        }

        public bool OpenAndRead()
        {
            bool Result = false;

            // Open the arc file for reading.
            if (OpenArcFile(false) == false)
            {
                // Failed to open the arc file.
                return false;
            }

            // Make sure the file is large enough to have the header.
            if (this.fileStream.Length < ArcFileHeader.kSizeOf)
            {
                // File is too small to be a valid arc file.
                goto Cleanup;
            }

            // Read the arc file header.
            ArcFileHeader header = new ArcFileHeader();
            header.Magic = reader.ReadInt32();
            header.Version = reader.ReadInt16();
            header.NumberOfFiles = reader.ReadInt16();

            // Verify the header magic.
            if (header.Magic != ArcFileHeader.kHeaderMagic)
            {
                // Arc file header has invalid magic.
                goto Cleanup;
            }

            // Verify the file version.
            if (header.Version != ArcFileHeader.kVersion)
            {
                // Arc file is invalid version.
                goto Cleanup;
            }

            // Loop for the number of files and read each file entry.
            for (int i = 0; i < header.NumberOfFiles; i++)
            {
                ArcFileEntry fileEntry = new ArcFileEntry();

                // Save the current position and read the file name.
                long offset = this.reader.BaseStream.Position;
                fileEntry.FileName = this.reader.ReadNullTerminatedString();

                // Advance to the end of the file name and read the rest of the file entry structure.
                this.reader.BaseStream.Position = offset + 64;
                fileEntry.FileType = this.reader.ReadInt32();
                fileEntry.CompressedSize = this.reader.ReadInt32();
                fileEntry.DecompressedSize = this.reader.ReadInt32();
                fileEntry.DataOffset = this.reader.ReadInt32();

                // Add the file entry to the list of files.
                this.fileEntries.Add(fileEntry);
            }

            // Successfully parsed the arc file.
            Result = true;

        Cleanup:
            // Close the arc file.
            CloseArcFile();

            // Return the result.
            return Result;
        }

        public byte[] DecompressFileEntry(string fileName)
        {
            // Loop through all of the files until we find one that matches.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Check if the file name matches.
                if (this.fileEntries[i].FileName == fileName)
                {
                    // Decompress the file entry.
                    return DecompressFileEntry(i);
                }
            }

            // A file with matching name was not found.
            return null;
        }

        public byte[] DecompressFileEntry(int fileIndex)
        {
            // Open the arc file for reading.
            if (OpenArcFile(false) == false)
            {
                // Failed to open the arc file.
                return null;
            }

            // Seek to the start of the file's compressed data
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Read the compressed data.
            byte[] compressedData = this.reader.ReadBytes(this.fileEntries[fileIndex].CompressedSize);

            // Decompress data.
            byte[] decompressedData = ZlibStream.UncompressBuffer(compressedData);

            // Close the arc file and return.
            CloseArcFile();
            return decompressedData;
        }

        public bool ExtractFile(int fileIndex, string outputFileName)
        {
            // Open the arc file for reading.
            if (OpenArcFile(false) == false)
            {
                // Failed to open the arc file.
                return false;
            }

            // Seek to the start of the file's compressed data
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Read the compressed data.
            byte[] compressedData = this.reader.ReadBytes(this.fileEntries[fileIndex].CompressedSize);
            
            // Decompress data and write to the output file.
            byte[] decompressedData = ZlibStream.UncompressBuffer(compressedData);
            File.WriteAllBytes(outputFileName, decompressedData);

            // Close the arc file and return.
            CloseArcFile();
            return true;
        }

        #region Utilities

        public static ResourceType DetermineResouceTypeFromBuffer(byte[] buffer)
        {
            // Get the magic ID from the file buffer.
            int magic = BitConverter.ToInt32(buffer, 0);

            // Check the magic and determine the resource type.
            switch (magic)
            {
                case rTextureHeader.kMagic: return ResourceType.Texture;
                case rModelHeader.kMagic: return ResourceType.Model;
                default: return ResourceType.Invalid;
            }
        }

        #endregion
    }
}
