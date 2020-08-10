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
    public struct ArchiveHeader
    {
        public const int kSizeOf = 8;
        public const int kHeaderMagic1 = 0x00435241;    // 'ARC'
        public const int kHeaderMagic2 = 0x53435241;    // 'ARCS'
        public const int kVersion = 4;

        /* 0x00 */ public int Magic;
        /* 0x04 */ public short Version;
        /* 0x06 */ public short NumberOfFiles;
    }

    public class ArchiveFileEntry
    {
        public const int kSizeOf = 80;
        public const int kMaxFileNameLength = 64;
        public const int kUsableFileNameLength = 64 - 1;

        /* 0x00 */ public string FileName { get; set; }
        /* 0x40 */ public ResourceType FileType { get; set; }
        /* 0x44 */ public int CompressedSize { get; set; }
        /* 0x48 */ public int DecompressedSize { get; set;  }
        /* 0x4C */ public int DataOffset { get; set; }

        public uint FileId { get; set; }

        /// <summary>
        /// Gets the file name with no file extension
        /// </summary>
        /// <returns></returns>
        public string GetFileNameNoExtension()
        {
            // Remove the file extension from the file name.
            int index = this.FileName.IndexOf('.');
            if (index != -1)
                return this.FileName.Substring(0, index);

            // No file extension, return as-is.
            return this.FileName;
        }
    }

    public class Archive
    {
        // File streams for read/write access.
        private FileStream fileStream = null;
        private EndianReader reader = null;
        private EndianWriter writer = null;

        /// <summary>
        /// Full file path of the archive.
        /// </summary>
        public string FileName { get; private set;  }
        /// <summary>
        /// Unique id for the archive.
        /// </summary>
        public uint ArchiveId { get; private set; }
        /// <summary>
        /// Endiannes of the archive.
        /// </summary>
        public Endianness Endian { get; private set; }

        private List<ArchiveFileEntry> fileEntries = new List<ArchiveFileEntry>();
        /// <summary>
        /// Gets a list of file entries in the archive.
        /// </summary>
        public ArchiveFileEntry[] FileEntries { get { return fileEntries.ToArray(); } }

        // Unique id counter for file entries in the lookup dictionary. I hate this but trying
        // to use hashcodes for the file name kept resulting in collisions.
        private uint nextFileId = 0x80000000;

        // Dictionary of file ids to position in the fileEntries list.
        private Dictionary<uint, int> fileEntryLookupDictionary = new Dictionary<uint, int>();

        /// <summary>
        /// Indicates if this archive was loaded from the mods directory or not.
        /// </summary>
        public bool IsPatchFile { get; private set; }

        public Archive(string fileName, uint archiveId, bool isPatchFile = false, Endianness endian = Endianness.Little)
        {
            // Initialize fields.
            this.FileName = fileName;
            this.ArchiveId = archiveId;
            this.IsPatchFile = isPatchFile;
            this.Endian = endian;
        }

        #region Archive open/close

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
                this.fileStream = new FileStream(this.FileName, FileMode.OpenOrCreate, fileAccess, FileShare.Read);
                this.reader = new EndianReader(this.Endian, this.fileStream);

                // If we want write access then open it for writing as well.
                if (forWrite == true)
                {
                    // Open the file for writing.
                    this.writer = new EndianWriter(this.Endian, this.fileStream);
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
            if (header.Magic != ArchiveHeader.kHeaderMagic1)
            {
                // Check if the magic is in big endian.
                if (EndianUtilities.ByteFlip32(header.Magic) == ArchiveHeader.kHeaderMagic1)
                {
                    // Set the endianness for future IO operations.
                    this.Endian = Endianness.Big;
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

                // Create a unique file id for the file.
                fileEntry.FileId = this.nextFileId++;

                // Check if the type of file is known.
                if (GameResource.KnownResourceTypes.ContainsKey(fileType) == true)
                {
                    // Set the file type and add it as a file extension to the file name.
                    fileEntry.FileType = GameResource.KnownResourceTypes[fileType];
                    fileEntry.FileName += "." + fileEntry.FileType.ToString();
                }
                else
                    fileEntry.FileType = ResourceType.Unknown;

                // Calculate the unique id for the file and add it to the file loopup dictionary.
                this.fileEntryLookupDictionary.Add(fileEntry.FileId, this.fileEntries.Count);

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

        #endregion

        #region File access

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
        /// Searches for a file with matching file id and returns the index of the file in the <see cref="FileEntries"/> array
        /// </summary>
        /// <param name="datum">File id of the file to find</param>
        /// <returns>The index of the file in the <see cref="FileEntries"/> array or -1 if the file id was not found</returns>
        public int FindFileFromDatum(DatumIndex datum)
        {
            // Make sure we have an entry for this file id in the lookup dictionary.
            if (this.fileEntryLookupDictionary.ContainsKey(datum.FileId) == false)
            {
                // No entry for this file id.
                return -1;
            }

            // Return the index of the file.
            return this.fileEntryLookupDictionary[datum.FileId];
        }

        /// <summary>
        /// Searches for a file entry with matching file name and parses the resource data for that file
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameResource"/> that will be returned</typeparam>
        /// <param name="fileName">Name of the file to search for</param>
        /// <param name="matchFileExtension">True if the file name should have a matching file extension, or false to ignore the file extension</param>
        /// <returns>The parsed game resource object, or null otherwise</returns>
        public T GetFileAsResource<T>(string fileName, bool matchFileExtension = false) where T : GameResource
        {
            // Search for file entry with matching file name.
            int index = FindFileFromName(fileName, matchFileExtension);
            if (index == -1)
            {
                // A file entry with matching file name was not found.
                return null;
            }

            // Decompress the file data.
            byte[] decompressedData = DecompressFileEntry(index);
            if (decompressedData == null)
            {
                // Failed to decompress the file data.
                return null;
            }

            // Parse the resource game and return the object.
            return (T)GameResource.FromGameResource(decompressedData, this.fileEntries[index].FileName, 
                new DatumIndex(this.ArchiveId, this.fileEntries[index].FileId), this.fileEntries[index].FileType, this.Endian == Endianness.Big);
        }

        /// <summary>
        ///  Parses the resource data for the file with the specified file id
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameResource"/> that will be returned</typeparam>
        /// <param name="fileId">File id of the file to parse</param>
        /// <returns>The parsed game resource object, or null otherwise</returns>
        public T GetFileAsResource<T>(uint fileId) where T : GameResource
        {
            // Make sure we have a file id entry in the dictionary.
            if (this.fileEntryLookupDictionary.ContainsKey(fileId) == false)
            {
                // No dictionary entry for this file id.
                return null;
            }

            // Get the index of the file from the file id.
            int fileIndex = this.fileEntryLookupDictionary[fileId];

            // Decompress the file data.
            byte[] decompressedData = DecompressFileEntry(fileIndex);
            if (decompressedData == null)
            {
                // Failed to decompress the file data.
                return null;
            }

            // Parse the resource game and return the object.
            return (T)GameResource.FromGameResource(decompressedData, this.fileEntries[fileIndex].FileName,
                new DatumIndex(this.ArchiveId, this.fileEntries[fileIndex].FileId), this.fileEntries[fileIndex].FileType, this.Endian == Endianness.Big);
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

        /// <summary>
        /// Gets the compressed file contents for the specified file
        /// </summary>
        /// <param name="fileId">File id if the file to retrieve file contents for</param>
        /// <returns>Compressed file contents for the specified file</returns>
        public byte[] GetCompressedFileContents(uint fileId)
        {
            // Make sure we have an entry for this file id in the reverse lookup dictionary.
            if (this.fileEntryLookupDictionary.ContainsKey(fileId) == false)
            {
                // No file entry for this file id.
                return null;
            }

            // Open the archive for reading.
            if (OpenArchive(false) == false)
            {
                // Failed to open the archive.
                return null;
            }

            // Seek to the start of the compressed data.
            int fileIndex = this.fileEntryLookupDictionary[fileId];
            this.reader.BaseStream.Position = this.fileEntries[fileIndex].DataOffset;

            // Read the compressed data.
            byte[] compressedData = this.reader.ReadBytes(this.fileEntries[fileIndex].CompressedSize);

            // Close the archive and return the buffer.
            CloseArchive();
            return compressedData;
        }

        #endregion

        #region Extraction/Injection/Add

        /// <summary>
        /// Adds the specified files to the archive
        /// </summary>
        /// <param name="datums">File ids for files to be added to the archive, files must be loaded in the <see cref="ArchiveCollection"/></param>
        /// <param name="fileNames">New file names for the files being added</param>
        /// <returns>True if the files were added successfully, false otherwise</returns>
        public bool AddFilesFromDatums(DatumIndex[] datums, string[] fileNames, out DatumIndex[] newDatums)
        {
            // Satisfy the compiler.
            newDatums = new DatumIndex[0];

            // Open the archive for reading only in case we are duplicating a file.
            if (OpenArchive(false) == false)
            {
                // Failed to open the archive for reading.
                return false;
            }

            // Create a new memory stream to hold compressed file data.
            List<int> fileOffsets = new List<int>();
            MemoryStream dataStream = new MemoryStream((int)this.reader.BaseStream.Length);

            // Loop through all the files in the archive and read each one into the memory stream.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Seek to the data offset.
                this.reader.BaseStream.Position = this.fileEntries[i].DataOffset;

                // Read the compressed file data.
                byte[] compressedData = this.reader.ReadBytes(this.fileEntries[i].CompressedSize);

                // Save the data offset and write the compressed data to the memory stream.
                fileOffsets.Add((int)dataStream.Position);
                dataStream.Write(compressedData, 0, compressedData.Length);
            }

            // Close the archive for now.
            CloseArchive();

            // Loop through all the files to be added to the archive.
            newDatums = new DatumIndex[datums.Length];
            for (int i = 0; i < datums.Length; i++)
            {
                // Get the file entry for this archive.
                ArchiveCollection.Instance.GetArchiveFileEntryFromDatum(datums[i], out Archive archive, out ArchiveFileEntry fileEntry);

                // Decompress the file contents.
                byte[] compressedData = archive.GetCompressedFileContents(fileEntry.FileId);

                // Create a new file entry for this file.
                ArchiveFileEntry newFileEntry = new ArchiveFileEntry();
                newFileEntry.FileName = fileNames[i] + "." + fileEntry.FileType.ToString();
                newFileEntry.CompressedSize = compressedData.Length;
                newFileEntry.DecompressedSize = fileEntry.DecompressedSize;
                newFileEntry.FileId = this.nextFileId++;
                newFileEntry.FileType = fileEntry.FileType;

                // Copy the new datum to the output list.
                newDatums[i] = new DatumIndex(this.ArchiveId, newFileEntry.FileId);

                // Add the file entry to the list and create a reverse lookup entry.
                this.fileEntryLookupDictionary.Add(newFileEntry.FileId, this.fileEntries.Count);
                this.fileEntries.Add(newFileEntry);

                // Add the data offset to the list and write the compressed data to the data stream.
                fileOffsets.Add((int)dataStream.Position);
                dataStream.Write(compressedData, 0, compressedData.Length);
            }

            // Reopen the archive for writing.
            if (OpenArchive(true) == false)
            {
                // Failed to open the archive for writing.
                return false;
            }

            // Update the header for the new file count.
            this.writer.BaseStream.Position = 0;
            this.writer.Write(ArchiveHeader.kHeaderMagic1);
            this.writer.Write((short)ArchiveHeader.kVersion);
            this.writer.Write((short)this.fileEntries.Count);

            // Calculate the data start offset based on the new number of files.
            int dataStart = ArchiveHeader.kSizeOf + (this.fileEntries.Count * ArchiveFileEntry.kSizeOf);
            if (dataStart % 0x8000 != 0)
                dataStart += 0x8000 - (dataStart % 0x8000);

            // Loop and write all the file entries.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Write the file entry.
                string fileName = this.fileEntries[i].GetFileNameNoExtension();
                this.writer.Write(fileName.ToCharArray());
                this.writer.Write(new byte[ArchiveFileEntry.kMaxFileNameLength - fileName.Length]);

                // If we know the file type for this file get the file type id.
                if (GameResource.KnownResourceTypesReverse.ContainsKey(this.fileEntries[i].FileType) == true)
                    this.writer.Write(GameResource.KnownResourceTypesReverse[this.fileEntries[i].FileType]);
                else
                    this.writer.Write((int)this.fileEntries[i].FileType);

                this.writer.Write(this.fileEntries[i].CompressedSize);
                this.writer.Write(this.fileEntries[i].DecompressedSize);
                this.writer.Write(dataStart + fileOffsets[i]);

                // Update the file offset.
                this.fileEntries[i].DataOffset = dataStart + fileOffsets[i];
            }

            // Fill the rest with padding.
            this.writer.Write(new byte[dataStart - (int)this.writer.BaseStream.Position]);

            // Write the data stream and calculate the new file size.
            this.writer.Write(dataStream.ToArray(), 0, (int)dataStream.Length);
            this.fileStream.SetLength(this.writer.BaseStream.Position);

            // Close the archive.
            CloseArchive();
            return true;
        }

        public bool AddFiles(string[] newFileNames, string[] filePaths, out DatumIndex[] newDatums)
        {
            // Satisfy the compiler.
            newDatums = new DatumIndex[0];

            // Open the archive for writing.
            if (OpenArchive(true) == false)
            {
                // Failed to open the archive for writing.
                return false;
            }

            // Create a new memory stream to hold compressed file data.
            List<int> fileOffsets = new List<int>();
            MemoryStream dataStream = new MemoryStream((int)this.reader.BaseStream.Length);

            // Loop through all the files in the archive and read each one into the memory stream.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Seek to the data offset.
                this.reader.BaseStream.Position = this.fileEntries[i].DataOffset;

                // Read the compressed file data.
                byte[] compressedData = this.reader.ReadBytes(this.fileEntries[i].CompressedSize);

                // Save the data offset and write the compressed data to the memory stream.
                fileOffsets.Add((int)dataStream.Position);
                dataStream.Write(compressedData, 0, compressedData.Length);
            }

            // Loop through all of the new files and create file entries for each one.
            newDatums = new DatumIndex[newFileNames.Length];
            for (int i = 0; i < newFileNames.Length; i++)
            {
                // Read the file contents and compress it.
                byte[] decompressedData = File.ReadAllBytes(filePaths[i]);
                byte[] compressedData = ZlibUtilities.CompressData(decompressedData);

                // Get the resource type from the file extension.
                ResourceType fileType = (ResourceType)Enum.Parse(typeof(ResourceType), filePaths[i].Substring(filePaths[i].LastIndexOf('.') + 1), true);

                // Create a new file entry for this file.
                ArchiveFileEntry newFileEntry = new ArchiveFileEntry();
                newFileEntry.FileName = newFileNames[i] + "." + fileType.ToString();
                newFileEntry.CompressedSize = compressedData.Length;
                newFileEntry.DecompressedSize = decompressedData.Length;
                newFileEntry.FileId = this.nextFileId++;
                newFileEntry.FileType = fileType;

                // Copy the new datum to the output list.
                newDatums[i] = new DatumIndex(this.ArchiveId, newFileEntry.FileId);

                // Add the file entry to the list and create a reverse lookup entry.
                this.fileEntryLookupDictionary.Add(newFileEntry.FileId, this.fileEntries.Count);
                this.fileEntries.Add(newFileEntry);

                // Add the data offset to the list and write the compressed data to the data stream.
                fileOffsets.Add((int)dataStream.Position);
                dataStream.Write(compressedData, 0, compressedData.Length);
            }

            // Update the header for the new file count.
            this.writer.BaseStream.Position = 0;
            this.writer.Write(ArchiveHeader.kHeaderMagic1);
            this.writer.Write((short)ArchiveHeader.kVersion);
            this.writer.Write((short)this.fileEntries.Count);

            // Calculate the data start offset based on the new number of files.
            int dataStart = ArchiveHeader.kSizeOf + (this.fileEntries.Count * ArchiveFileEntry.kSizeOf);
            if (dataStart % 0x8000 != 0)
                dataStart += 0x8000 - (dataStart % 0x8000);

            // Loop and write all the file entries.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Write the file entry.
                string fileName = this.fileEntries[i].GetFileNameNoExtension();
                this.writer.Write(fileName.ToCharArray());
                this.writer.Write(new byte[ArchiveFileEntry.kMaxFileNameLength - fileName.Length]);

                // If we know the file type for this file get the file type id.
                if (GameResource.KnownResourceTypesReverse.ContainsKey(this.fileEntries[i].FileType) == true)
                    this.writer.Write(GameResource.KnownResourceTypesReverse[this.fileEntries[i].FileType]);
                else
                    this.writer.Write((int)this.fileEntries[i].FileType);

                this.writer.Write(this.fileEntries[i].CompressedSize);
                this.writer.Write(this.fileEntries[i].DecompressedSize);
                this.writer.Write(dataStart + fileOffsets[i]);

                // Update the file offset.
                this.fileEntries[i].DataOffset = dataStart + fileOffsets[i];
            }

            // Fill the rest with padding.
            this.writer.Write(new byte[dataStart - (int)this.writer.BaseStream.Position]);

            // Write the data stream and calculate the new file size.
            this.writer.Write(dataStream.ToArray(), 0, (int)dataStream.Length);
            this.fileStream.SetLength(this.writer.BaseStream.Position);

            // Close the archive.
            CloseArchive();
            return true;
        }

        /// <summary>
        /// Extracts the specified file to disk
        /// </summary>
        /// <param name="fileId">File id of the file to decompress</param>
        /// <param name="outputFileName">File path to save the decompressed data to</param>
        /// <returns>True if the data was successfully decompressed and written to file, false otherwise</returns>
        public bool ExtractFile(uint fileId, string outputFileName)
        {
            // Make sure we have a file id entry in the dictionary.
            if (this.fileEntryLookupDictionary.ContainsKey(fileId) == false)
            {
                // No dictionary entry for this file id.
                return false;
            }

            // Get the index of the file from the file id.
            int fileIndex = this.fileEntryLookupDictionary[fileId];

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
        /// <param name="fileId">File id of the file to replace</param>
        /// <param name="newFilePath">File path of the new file contants</param>
        /// <returns>True if the data was successfully compressed and written to the archive, false otherwise</returns>
        public bool InjectFile(uint fileId, string newFilePath)
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
            return InjectFile(fileId, decompressedData);
        }

        /// <summary>
        /// Replaces the contents of <paramref name="fileIndex"/> with the specified buffer
        /// </summary>
        /// <param name="fileId">File id of the file to replace</param>
        /// <param name="data">New file contents to write</param>
        /// <returns>True if the data was successfully compressed and written to the archive, false otherwise</returns>
        public bool InjectFile(uint fileId, byte[] data)
        {
            // Make sure we have a file id entry in the dictionary.
            if (this.fileEntryLookupDictionary.ContainsKey(fileId) == false)
            {
                // No dictionary entry for this file id.
                return false;
            }

            // Get the index of the file from the file id.
            int fileIndex = this.fileEntryLookupDictionary[fileId];

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

        #region File manipulation

        /// <summary>
        /// Renames the specified file
        /// </summary>
        /// <param name="fileId">File id of the file to be renamed</param>
        /// <param name="newFileName">New file name</param>
        /// <returns>True if the file name was updated and changes written to archive, false otherwise</returns>
        public bool RenameFile(uint fileId, string newFileName)
        {
            // Make sure the file name length is not too long.
            if (newFileName.Length == 0 || newFileName.Length > ArchiveFileEntry.kUsableFileNameLength)
            {
                // File name is too long.
                return false;
            }

            // Make sure we have a file id entry in the dictionary.
            if (this.fileEntryLookupDictionary.ContainsKey(fileId) == false)
            {
                // No dictionary entry for this file id.
                return false;
            }

            // Get the index of the file from the file id.
            int fileIndex = this.fileEntryLookupDictionary[fileId];

            // Open the archive for writing.
            if (OpenArchive(true) == false)
            {
                // Failed to open the archive for writing.
                return false;
            }

            // Update the file entry.
            this.fileEntries[fileIndex].FileName = newFileName + "." + this.fileEntries[fileIndex].FileType.ToString();

            // Seek to the offset of the file entry.
            this.writer.BaseStream.Position = ArchiveHeader.kSizeOf + (fileIndex * ArchiveFileEntry.kSizeOf);

            // Write the new file name to file.
            this.writer.Write(newFileName.ToCharArray());
            this.writer.Write(new byte[ArchiveFileEntry.kMaxFileNameLength - newFileName.Length]);

            // Close the file stream.
            CloseArchive();
            return true;
        }

        /// <summary>
        /// Deletes all of the specified files from the archive
        /// </summary>
        /// <param name="fileIds">Array of file DatumIndexes to delete from the archive</param>
        /// <returns>True if the files were deleted successfully, false otherwise</returns>
        public bool DeleteFiles(DatumIndex[] fileIds)
        {
            // Open the archive for writing.
            if (OpenArchive(true) == false)
            {
                // Failed to open the archive for writing.
                return false;
            }

            // Create a new memory stream to store file data in.
            List<int> dataOffsets = new List<int>();
            MemoryStream dataStream = new MemoryStream((int)this.reader.BaseStream.Length);

            // Loop through all of the files and read the compressed data into the memory stream.
            List<int> fileIndices = new List<int>();
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Check if this is one of the files to be deleted.
                if (fileIds.Contains(new DatumIndex(this.ArchiveId, this.fileEntries[i].FileId)) == true)
                {
                    // Save the file id and continue.
                    fileIndices.Add(i);
                    continue;
                }

                // Seek to the compressed data and read it.
                this.reader.BaseStream.Position = this.fileEntries[i].DataOffset;
                byte[] data = this.reader.ReadBytes(this.fileEntries[i].CompressedSize);

                // Write the data into the memory stream.
                dataOffsets.Add((int)dataStream.Length);
                dataStream.Write(data, 0, data.Length);
            }

            // Sort the list of files indices to remove in descending order. This is to ensure
            // when we remove them the indices wont shift underneath us from the removals.
            fileIndices.Sort((x, y) => y.CompareTo(x));

            // Remove the files to be deleted from the file entry list.
            for (int i = 0; i < fileIndices.Count; i++)
            {
                // Remove references to the file.
                uint fileId = this.fileEntries[fileIndices[i]].FileId;
                this.fileEntries.RemoveAt(fileIndices[i]);
            }

            // Rebuild the reverse file lookup dictionary.
            this.fileEntryLookupDictionary.Clear();
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Add a new entry to the dictionary.
                this.fileEntryLookupDictionary.Add(this.fileEntries[i].FileId, i);
            }

            // Update the header for the new file count.
            this.writer.BaseStream.Position = 6;
            this.writer.Write((short)this.fileEntries.Count);

            // Calculate the data start offset based on the new number of files.
            int dataStart = ArchiveHeader.kSizeOf + (this.fileEntries.Count * ArchiveFileEntry.kSizeOf);
            if (dataStart % 0x8000 != 0)
                dataStart += 0x8000 - (dataStart % 0x8000);

            // Loop and write all the file entries.
            for (int i = 0; i < this.fileEntries.Count; i++)
            {
                // Write the file entry.
                string fileName = this.fileEntries[i].GetFileNameNoExtension();
                this.writer.Write(fileName.ToCharArray());
                this.writer.Write(new byte[ArchiveFileEntry.kMaxFileNameLength - fileName.Length]);

                // If we know the file type for this file get the file type id.
                if (GameResource.KnownResourceTypesReverse.ContainsKey(this.fileEntries[i].FileType) == true)
                    this.writer.Write(GameResource.KnownResourceTypesReverse[this.fileEntries[i].FileType]);
                else
                    this.writer.Write((int)this.fileEntries[i].FileType);

                this.writer.Write(this.fileEntries[i].CompressedSize);
                this.writer.Write(this.fileEntries[i].DecompressedSize);
                this.writer.Write(dataStart + dataOffsets[i]);

                // Update the file offset.
                this.fileEntries[i].DataOffset = dataStart + dataOffsets[i];
            }

            // Fill the rest with padding.
            this.writer.Write(new byte[dataStart - (int)this.writer.BaseStream.Position]);

            // Write the data stream and calculate the new file size.
            this.writer.Write(dataStream.ToArray(), 0, (int)dataStream.Length);
            this.fileStream.SetLength(this.writer.BaseStream.Position);

            // Close the archive.
            CloseArchive();
            return true;
        }

        #endregion

        #region Utilities

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

        #endregion
    }
}
