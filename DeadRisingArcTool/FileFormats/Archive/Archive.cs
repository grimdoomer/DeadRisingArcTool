﻿using DeadRisingArcTool.FileFormats.Bitmaps;
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
    public struct ArchiveHeader
    {
        public const int kSizeOf = 8;
        public const int kHeaderMagic = 0x00435241;
        public const int kVersion = 4;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public short Version;
        /* 0x06 */ public short NumberOfFiles;
    }

    public class ArchiveFileEntry
    {
        public const int kSizeOf = 80;

        /* 0x00 */ public string FileName { get; set; }
        /* 0x40 */ public ResourceType FileType { get; set; }
        /* 0x44 */ public int CompressedSize { get; set; }
        /* 0x48 */ public int DecompressedSize { get; set;  }
        /* 0x4C */ public int DataOffset { get; set; }
    }

    public class Archive
    {
        // File streams for read/write access.
        private FileStream fileStream = null;
        private Endianness endian = Endianness.Little;
        private EndianReader reader = null;
        private EndianWriter writer = null;

        /// <summary>
        /// Full file path of the archive.
        /// </summary>
        public string FileName { get; private set;  }
        /// <summary>
        /// Index of the archive in the global <see cref="ArchiveCollection"/> collection
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Endiannes of the archive.
        /// </summary>
        public Endianness Endian { get { return this.endian; } }

        private List<ArchiveFileEntry> fileEntries = new List<ArchiveFileEntry>();
        /// <summary>
        /// Gets a list of file entries in the archive.
        /// </summary>
        public ArchiveFileEntry[] FileEntries { get { return fileEntries.ToArray(); } }

        /// <summary>
        /// Indicates if this archive was loaded from the mods directory or not.
        /// </summary>
        public bool IsPatchFile { get; private set; }

        public Archive(string fileName, bool isPatchFile = false)
        {
            // Initialize fields.
            this.FileName = fileName;
            this.IsPatchFile = isPatchFile;
        }

        /// <summary>
        /// Opens the archive for reading and writing
        /// </summary>
        /// <param name="forWrite">True if the file should be opened for writing, otherwise it will be read-only</param>
        /// <returns>True if the file was successfully opened, false otherwise</returns>
        private bool OpenArchive(bool forWrite)
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
                {
                    // Check if the backup file exists and if not create a backup file.
                    if (File.Exists(this.FileName + "_bak") == false)
                    {
                        // Create a backup file.
                        File.Copy(this.FileName, this.FileName + "_bak");
                    }

                    // Open the file for writing.
                    this.writer = new EndianWriter(this.endian, this.fileStream);
                }
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
        /// Closes all file streams on the archive
        /// </summary>
        private void CloseArchive()
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

            // Open the archive for reading.
            if (OpenArchive(false) == false)
            {
                // Failed to open the archive.
                return false;
            }

            // Make sure the file is large enough to have the header.
            if (this.fileStream.Length < ArchiveHeader.kSizeOf)
            {
                // File is too small to be a valid archive.
                goto Cleanup;
            }

            // Read the archive header.
            ArchiveHeader header = new ArchiveHeader();
            header.Magic = reader.ReadInt32();
            header.Version = reader.ReadInt16();
            header.NumberOfFiles = reader.ReadInt16();

            // Verify the header magic.
            if (header.Magic != ArchiveHeader.kHeaderMagic)
            {
                // Check if the magic is in big endian.
                if (EndianUtilities.ByteFlip32(header.Magic) == ArchiveHeader.kHeaderMagic)
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
                    // archive header has invalid magic.
                    goto Cleanup;
                }
            }

            // Verify the file version.
            if (header.Version != ArchiveHeader.kVersion)
            {
                // archive is invalid version.
                goto Cleanup;
            }

            // Loop for the number of files and read each file entry.
            for (int i = 0; i < header.NumberOfFiles; i++)
            {
                ArchiveFileEntry fileEntry = new ArchiveFileEntry();

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

            // Successfully parsed the archive.
            Result = true;

        Cleanup:
            // Close the archive.
            CloseArchive();

            // Return the result.
            return Result;
        }

        /// <summary>
        /// Searches for a file with matching name and returns the index of the file in the <see cref="FileEntries"/> array
        /// </summary>
        /// <param name="fileName">Name of the file to search for</param>
        /// <param name="matchFileExtension">True if the file name should have a matching file extension, or false to ignore the file extension</param>
        /// <returns>The index of the file in the <see cref="FileEntries"/> array, or -1 if a matching file name was not found</returns>
        public int FindFileFromName(string fileName, bool matchFileExtension = false)
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
        public T GetFileAsResource<T>(string fileName, bool matchFileExtension = false) where T : GameResource
        {
            // Search for file entry with matching file name.
            int index = FindFileFromName(fileName, matchFileExtension);
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
            return (T)GameResource.FromGameResource(decompressedData, this.fileEntries[index].FileName, 
                new DatumIndex((short)this.Index, (short)index), this.fileEntries[index].FileType, this.endian == Endianness.Big);
        }

        /// <summary>
        ///  Parses the resource data for the file at the specified index
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameResource"/> that will be returned</typeparam>
        /// <param name="fileIndex">Index of the file to parse</param>
        /// <returns>The parsed game resource object, or default(T) otherwise</returns>
        public T GetFileAsResource<T>(int fileIndex) where T : GameResource
        {
            // Decompress the file data.
            byte[] decompressedData = DecompressFileEntry(fileIndex);
            if (decompressedData == null)
            {
                // Failed to decompress the file data.
                return default(T);
            }

            // Parse the resource game and return the object.
            return (T)GameResource.FromGameResource(decompressedData, this.fileEntries[fileIndex].FileName,
                new DatumIndex((short)this.Index, (short)fileIndex), this.fileEntries[fileIndex].FileType, this.endian == Endianness.Big);
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
            // Open the archive for reading.
            if (OpenArchive(false) == false)
            {
                // Failed to open the archive.
                return null;
            }

            // Seek to the start of the file's compressed data
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Read the compressed data.
            byte[] compressedData = this.reader.ReadBytes(this.fileEntries[fileIndex].CompressedSize);

            // Decompress data.
            byte[] decompressedData = ZlibStream.UncompressBuffer(compressedData);

            // Close the archive and return.
            CloseArchive();
            return decompressedData;
        }

        #region File Manipulation

        /// <summary>
        /// Extracts the specified file to disk
        /// </summary>
        /// <param name="fileIndex">Index of the file to decompress</param>
        /// <param name="outputFileName">File path to save the decompressed data to</param>
        /// <returns>True if the data was successfully decompressed and written to file, false otherwise</returns>
        public bool ExtractFile(int fileIndex, string outputFileName)
        {
            // Open the archive for reading.
            if (OpenArchive(false) == false)
            {
                // Failed to open the archive.
                return false;
            }

            // Seek to the start of the file's compressed data
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Read the compressed data.
            byte[] compressedData = this.reader.ReadBytes(this.fileEntries[fileIndex].CompressedSize);
            
            // Decompress data and write to the output file.
            byte[] decompressedData = ZlibStream.UncompressBuffer(compressedData);
            File.WriteAllBytes(outputFileName, decompressedData);

            // Close the archive and return.
            CloseArchive();
            return true;
        }

        /// <summary>
        /// Extracts all files to the specified directory while maintaining file hierarchy
        /// </summary>
        /// <param name="outputFolder">Output folder to saves files to</param>
        /// <returns>True if the files were successfully extracted, false otherwise</returns>
        public bool ExtractAllFiles(string outputFolder)
        {
            // Open the archive for reading.
            if (OpenArchive(false) == false)
            {
                // Failed to open the archive.
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

            // Close the archive and return.
            CloseArchive();
            return true;
        }

        /// <summary>
        /// Replaces the contents of <paramref name="fileIndex"/> with the file contents of <paramref name="newFilePath"/>
        /// </summary>
        /// <param name="fileIndex">Index of the file to replace</param>
        /// <param name="newFilePath">File path of the new file contants</param>
        /// <returns>True if the data was successfully compressed and written to the archive, false otherwise</returns>
        public bool InjectFile(int fileIndex, string newFilePath)
        {
            byte[] decompressedData = null;

            try
            {
                // Read the contents of the new file.
                decompressedData = File.ReadAllBytes(newFilePath);
            }
            catch (Exception e)
            {
                // Failed to read the file contents.
                return false;
            }

            // Compress and inject the file data.
            return InjectFile(fileIndex, decompressedData);
        }

        /// <summary>
        /// Replaces the contents of <paramref name="fileIndex"/> with the specified buffer
        /// </summary>
        /// <param name="fileIndex">Index of the file to replace</param>
        /// <param name="data">New file contents to write</param>
        /// <returns>True if the data was successfully compressed and written to the archive, false otherwise</returns>
        public bool InjectFile(int fileIndex, byte[] data)
        {
            // Open the archive for writing.
            if (OpenArchive(true) == false)
            {
                // Failed to open the archive.
                return false;
            }

            // Compress the data.
            byte[] compressedData = ZlibUtilities.CompressData(data);

            // Seek to the end of the this file's data.
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset + this.fileEntries[fileIndex].CompressedSize;

            // Read all the remaining data in the file.
            byte[] remainingData = this.reader.ReadBytes((int)(this.reader.BaseStream.Length - this.reader.BaseStream.Position));

            // Seek to the start of the old file's data.
            this.writer.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Calculate the offset shift amount and write the new compressed data buffer.
            int offsetShift = compressedData.Length - this.fileEntries[fileIndex].CompressedSize;
            this.writer.Write(compressedData);

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
                    this.writer.BaseStream.Position = ArchiveHeader.kSizeOf + (ArchiveFileEntry.kSizeOf * i) + 76;
                    this.writer.Write(this.fileEntries[i].DataOffset);
                }
                else if (i == fileIndex)
                {
                    // Update the data sizes for this file entry.
                    this.fileEntries[i].CompressedSize = compressedData.Length;
                    this.fileEntries[i].DecompressedSize = data.Length;

                    // Write the new data sizes to file.
                    this.writer.BaseStream.Position = ArchiveHeader.kSizeOf + (ArchiveFileEntry.kSizeOf * i) + 64 + 4;
                    this.writer.Write(this.fileEntries[i].CompressedSize);
                    this.writer.Write(this.fileEntries[i].DecompressedSize);
                }
            }

            // Close the archive and return.
            CloseArchive();
            return true;
        }

        #endregion
    }
}