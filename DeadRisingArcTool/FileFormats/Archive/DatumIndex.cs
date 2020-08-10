using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Archive
{
    /// <summary>
    /// Handle tracking for individual files in an archive.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct DatumIndex
    {
        /// <summary>
        /// Value used for resources that are not assigned to a file.
        /// </summary>
        public const long Unassigned = -1L;

        [FieldOffset(0)]
        private uint archiveId;
        /// <summary>
        /// Id of the archive in the ArcFileCollection.
        /// </summary>
        public uint ArchiveId { get { return this.archiveId; } set { this.archiveId = value; } }

        [FieldOffset(4)]
        private uint fileId;
        /// <summary>
        /// Id of the file in the Archive's file collection.
        /// </summary>
        public uint FileId { get { return this.fileId; } set { this.fileId = value; } }

        [FieldOffset(0)]
        private long datum;
        /// <summary>
        /// Unique datum used to uniquely identify this file.
        /// </summary>
        public long Datum { get { return this.datum; } set { this.datum = value; } }

        public DatumIndex(uint arcId, uint fileId)
        {
            this.datum = 0; // Satisfy the compiler
            this.archiveId = arcId;
            this.fileId = fileId;
        }

        public DatumIndex(long datum)
        {
            this.archiveId = 0;
            this.fileId = 0;
            this.datum = datum;
        }

        public static bool operator ==(DatumIndex left, DatumIndex right)
        {
            return left.Datum == right.Datum;
        }

        public static bool operator !=(DatumIndex left, DatumIndex right)
        {
            return left.Datum != right.Datum;
        }

        public static explicit operator DatumIndex(long datum)
        {
            return new DatumIndex(datum);
        }

        public override bool Equals(object obj)
        {
            // Make sure the other object is the same type.
            if (obj.GetType() != typeof(DatumIndex))
                return false;

            // Compare datums.
            return ((DatumIndex)obj).Datum == this.Datum;
        }

        public override int GetHashCode()
        {
            return (int)(this.ArchiveId ^ this.FileId);
        }
    }
}
