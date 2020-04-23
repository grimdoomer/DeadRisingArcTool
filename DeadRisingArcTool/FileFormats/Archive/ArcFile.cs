using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.Utilities;
using IO;
using IO.Endian;
using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Archive
{
    public struct ArcFileHeader
    {
        public const int kSizeOf = 8;
        public const int kHeaderMagic = 0x00435241;
        public const int kVersion = 4;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public short Version;
        /* 0x06 */ public short NumberOfFiles;
    }

    public class ArcFileEntry
    {
        public const int kSizeOf = 80;

        /* 0x00 */ public string FileName { get; set; }
        /* 0x40 */ public ResourceType FileType { get; set; }
        /* 0x44 */ public int CompressedSize { get; set; }
        /* 0x48 */ public int DecompressedSize { get; set;  }
        /* 0x4C */ public int DataOffset { get; set; }
    }

    public class ArcFile
    {
        // File streams for read/write access.
        private FileStream fileStream = null;
        private Endianness endian = Endianness.Little;
        private EndianReader reader = null;
        private EndianWriter writer = null;

        /// <summary>
        /// Full file path of the arc file.
        /// </summary>
        public string FileName { get; private set;  }

        /// <summary>
        /// Endiannes of the arc file.
        /// </summary>
        public Endianness Endian { get { return this.endian; } }

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

        /// <summary>
        /// Opens the arc file for reading and writing
        /// </summary>
        /// <param name="forWrite">True if the file should be opened for writing, otherwise it will be read-only</param>
        /// <returns>True if the file was successfully opened, false otherwise</returns>
        private bool OpenArcFile(bool forWrite)
        {
            // Determine if we are opening for R or RW access.
            FileAccess fileAccess = forWrite == true ? FileAccess.ReadWrite : FileAccess.Read;

            try
            {
                // Open the file for reading.
                this.fileStream = new FileStream(this.FileName, FileMode.Open, fileAccess, FileShare.Read);
                this.reader = new EndianReader(this.endian, this.fileStream);

                // If we want write access then open it for writing as well.
                if (forWrite == true)
                    this.writer = new EndianWriter(this.endian, this.fileStream);
            }
            catch (Exception e)
            {
                // Failed to open the file.
                return false;
            }

            // Successfully opened the file.
            return true;
        }

        /// <summary>
        /// Closes all file streams on the arc file
        /// </summary>
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
                // Check if the magic is in big endian.
                if (EndianUtilities.ByteFlip32(header.Magic) == ArcFileHeader.kHeaderMagic)
                {
                    // Set the endianness for future IO operations.
                    this.endian = Endianness.Big;
                    this.reader.Endian = Endianness.Big;

                    // Correct the header values we already read.
                    header.Magic = EndianUtilities.ByteFlip32(header.Magic);
                    header.Version = EndianUtilities.ByteFlip16(header.Version);
                    header.NumberOfFiles = EndianUtilities.ByteFlip16(header.NumberOfFiles);
                }
                else
                {
                    // Arc file header has invalid magic.
                    goto Cleanup;
                }
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
                int fileType = this.reader.ReadInt32();
                fileEntry.CompressedSize = this.reader.ReadInt32();
                fileEntry.DecompressedSize = this.reader.ReadInt32();
                fileEntry.DataOffset = this.reader.ReadInt32();

                // Check if the type of file is known.
                if (GameResource.KnownResourceTypes.ContainsKey(fileType) == true)
                {
                    // Set the file type and add it as a file extension to the file name.
                    fileEntry.FileType = GameResource.KnownResourceTypes[fileType];
                    fileEntry.FileName += "." + fileEntry.FileType.ToString();
                }
                else
                    fileEntry.FileType = ResourceType.Unknown;

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

        /// <summary>
        /// Searches for an arc file with matching name and returns the index of the arc file in the <see cref="FileEntries"/> array
        /// </summary>
        /// <param name="fileName">Name of the file to search for</param>
        /// <param name="matchFileExtension">True if the file name should have a matching file extension, or false to ignore the file extension</param>
        /// <returns>The index of the arc file in the <see cref="FileEntries"/> array, or -1 if a matching file name was not found</returns>
        public int FindArcFileFromName(string fileName, bool matchFileExtension = false)
        {
            // Loop through all of the files until we find one that matches.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Check if the file name matches.
                if ((matchFileExtension == true && this.fileEntries[i].FileName == fileName) ||
                    (matchFileExtension == false && CompareFileNamesNoExt(this.fileEntries[i].FileName, fileName) == true))
                {
                    // Decompress the file entry.
                    return i;
                }
            }

            // A file with matching name was not found.
            return -1;
        }

        /// <summary>
        /// Searches for a file entry with matching file name and parses the resource data for that file
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameResource"/> that will be returned</typeparam>
        /// <param name="fileName">Name of the file to search for</param>
        /// <param name="matchFileExtension">True if the file name should have a matching file extension, or false to ignore the file extension</param>
        /// <returns>The parsed game resource object, or default(T) otherwise</returns>
        public T GetArcFileAsResource<T>(string fileName, bool matchFileExtension = false) where T : GameResource
        {
            // Search for file entry with matching file name.
            int index = FindArcFileFromName(fileName, matchFileExtension);
            if (index == -1)
            {
                // A file entry with matching file name was not found.
                return default(T);
            }

            // Decompress the file data.
            byte[] decompressedData = DecompressFileEntry(index);
            if (decompressedData == null)
            {
                // Failed to decompress the file data.
                return default(T);
            }

            // Parse the resource game and return the object.
            return (T)GameResource.FromGameResource(decompressedData, this.fileEntries[index].FileName, this.fileEntries[index].FileType, this.endian == Endianness.Big);
        }

        /// <summary>
        ///  Parses the resource data for the file at the specified index
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameResource"/> that will be returned</typeparam>
        /// <param name="fileIndex">Index of the file to parse</param>
        /// <returns>The parsed game resource object, or default(T) otherwise</returns>
        public T GetArcFileAsResource<T>(int fileIndex) where T : GameResource
        {
            // Decompress the file data.
            byte[] decompressedData = DecompressFileEntry(fileIndex);
            if (decompressedData == null)
            {
                // Failed to decompress the file data.
                return default(T);
            }

            // Parse the resource game and return the object.
            return (T)GameResource.FromGameResource(decompressedData, this.fileEntries[fileIndex].FileName, this.fileEntries[fileIndex].FileType, this.endian == Endianness.Big);
        }

        public byte[] DecompressFileEntry(string fileName, bool matchFileExtension = false)
        {
            // Loop through all of the files until we find one that matches.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Check if the file name matches.
                if ((matchFileExtension == true && this.fileEntries[i].FileName == fileName) ||
                    (matchFileExtension == false && CompareFileNamesNoExt(this.fileEntries[i].FileName, fileName) == true))
                {
                    // Decompress the file entry.
                    return DecompressFileEntry(i);
                }
            }

            // A file with matching name was not found.
            return null;
        }

        private bool CompareFileNamesNoExt(string file1, string file2)
        {
            // Loop and compare the the file names.
            for (int i = 0; i < Math.Min(file1.Length, file2.Length); i++)
            {
                // If the characters don't match we fail.
                if (file1[i] != file2[i])
                    return false;
            }

            // Make sure the next character in file1 is a period for the file extension.
            if (file1.Length < file2.Length && file1[file2.Length] != '.')
                return false;

            // If we made it here it's good enough for what we need.
            return true;
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

        #region File Manipulation

        /// <summary>
        /// Extracts the specified arc file to disk
        /// </summary>
        /// <param name="fileIndex">Index of the file to decompress</param>
        /// <param name="outputFileName">File path to save the decompressed data to</param>
        /// <returns>True if the data was successfully decompressed and written to file, false otherwise</returns>
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

        /// <summary>
        /// Extracts all files to the specified directory while maintaining file hierarchy
        /// </summary>
        /// <param name="outputFolder">Output folder to saves files to</param>
        /// <returns>True if the files were successfully extracted, false otherwise</returns>
        public bool ExtractAllFiles(string outputFolder)
        {
            // Open the arc file for reading.
            if (OpenArcFile(false) == false)
            {
                // Failed to open the arc file.
                return false;
            }

            // Loop through all of the files and extract each one.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Format the folder name and check if it already exists.
                string fileName = this.FileEntries[i].FileName;
                string fileFolderPath = outputFolder + "\\" + fileName.Substring(0, fileName.LastIndexOf("\\"));
                if (Directory.Exists(fileFolderPath) == false)
                {
                    // Create the directory now.
                    Directory.CreateDirectory(fileFolderPath);
                }

                // Seek to the start of the file's compressed data.
                this.reader.BaseStream.Position = this.fileEntries[i].DataOffset;

                // Read the compressed data.
                byte[] compressedData = this.reader.ReadBytes(this.fileEntries[i].CompressedSize);

                // Decompress and write to file.
                byte[] decompressedData = ZlibStream.UncompressBuffer(compressedData);
                File.WriteAllBytes(string.Format("{0}\\{1}.{2}", fileFolderPath, fileName.Substring(fileName.LastIndexOf("\\") + 1), 
                    this.fileEntries[i].FileType.ToString()), decompressedData);
            }

            // Close the arc file and return.
            CloseArcFile();
            return true;
        }

        /// <summary>
        /// Replaces the contents of <paramref name="fileIndex"/> with the file contents of <paramref name="newFilePath"/>
        /// </summary>
        /// <param name="fileIndex">Index of the file to replace</param>
        /// <param name="newFilePath">File path of the new file contants</param>
        /// <returns>True if the data was successfully compressed and written to the arc file, false otherwise</returns>
        public bool InjectFile(int fileIndex, string newFilePath)
        {
            // Open the arc file for writing.
            if (OpenArcFile(true) == false)
            {
                // Failed to open the arc file.
                return false;
            }

            // Read the contents of the new file.
            byte[] decompressedData = File.ReadAllBytes(newFilePath);

            // Compress the data.
            byte[] compressedData = ZlibStream.CompressBuffer(decompressedData);

            // Seek to the end of the this file's data.
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset + this.fileEntries[fileIndex].CompressedSize;

            // Read all the remaining data in the file.
            byte[] remainingData = this.reader.ReadBytes((int)(this.reader.BaseStream.Length - this.reader.BaseStream.Position));

            // Seek to the start of the old file's data.
            this.writer.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Calculate the offset shift amount and write the new compressed data buffer.
            int offsetShift = compressedData.Length - this.fileEntries[fileIndex].CompressedSize;
            this.writer.Write(compressedData);

            // Align the remaining data to a 2 byte boundary.
            if (this.writer.BaseStream.Position % 2 != 0)
            {
                // Write 1 byte of padding, I don't think this matters but w/e...
                this.writer.Write(0xCD);
                offsetShift += 1;
            }

            // Write the remaining file data.
            this.writer.Write(remainingData);

            // Loop and update any file entries that need to accommodate for the file size change.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Check if this file's offset needs to be updated or if it is the file we just replaced.
                if (this.fileEntries[i].DataOffset > this.fileEntries[fileIndex].DataOffset)
                {
                    // Update this file entries data offset.
                    this.fileEntries[i].DataOffset += offsetShift;

                    // Write the new offset to file.
                    this.writer.BaseStream.Position = ArcFileHeader.kSizeOf + (ArcFileEntry.kSizeOf * i) + 64;
                    this.writer.Write(this.fileEntries[i].DataOffset);
                }
                else if (i == fileIndex)
                {
                    // Update the data sizes for this file entry.
                    this.fileEntries[i].CompressedSize = compressedData.Length;
                    this.fileEntries[i].DecompressedSize = decompressedData.Length;

                    // Write the new data sizes to file.
                    this.writer.BaseStream.Position = ArcFileHeader.kSizeOf + (ArcFileEntry.kSizeOf * i) + 64 + 4;
                    this.writer.Write(this.fileEntries[i].CompressedSize);
                    this.writer.Write(this.fileEntries[i].DecompressedSize);
                }
            }

            // Close the arc file and return.
            CloseArcFile();
            return true;
        }

        #endregion
    }
}
