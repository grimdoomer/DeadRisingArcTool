using DeadRisingArcTool.FileFormats.Archive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Misc
{
    [GameResourceParser(ResourceType.rCameraListXml,
        ResourceType.rClothXml,
        ResourceType.rEnemyLayoutXml,
        ResourceType.rFSMBrainXml,
        ResourceType.rMarkerLayoutXml,
        ResourceType.rModelInfoXml,
        ResourceType.rModelLayoutXml,
        ResourceType.rNMMachineXml,
        ResourceType.rRouteNodeXml,
        ResourceType.rSchedulerXml,
        ResourceType.rSoundSegXml,
        ResourceType.rUBCellXml,
        ResourceType.rEventTimeSchedule,
        ResourceType.rHavokVehicleData,
        ResourceType.rAreaHitLayout,
        ResourceType.rHavokConstraintLayout,
        ResourceType.rHavokLinkCollisionLayout,
        ResourceType.rHavokVertexLayout,
        ResourceType.rItemLayout,
        ResourceType.rMobLayout,
        ResourceType.rSprLayout,
        ResourceType.rSMAdd,
        ResourceType.rMapLink)]
    public class XmlFile : GameResource
    {
        /// <summary>
        /// Xml document data.
        /// </summary>
        public byte[] Buffer { get; set; }

        protected XmlFile(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
            : base(fileName, datum, fileType, isBigEndian)
        {
            // Initialize fields.
            this.Buffer = buffer;
        }

        public override byte[] ToBuffer()
        {
            return this.Buffer;
        }

        public static XmlFile FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Create a new XmlFile from the resource buffer.
            return new XmlFile(buffer, fileName, datum, fileType, isBigEndian);
        }
    }
}
