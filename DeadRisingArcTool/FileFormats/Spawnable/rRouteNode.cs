using DeadRisingArcTool.FileFormats.Misc;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Spawnable
{
    [SerializableXmlStruct(0x2b93c4ad)]
    public struct rRouteNode
    {
        [XmlField("*mpRouteHead")]
        public RouteNodeHead RouteHead;
        [XmlField("mpRouteData")]
        public RouteNodeData[] RouteData;
    }

    [SerializableXmlStruct(0x354344d5)]
    public struct RouteNodeHead
    {
        [XmlField("version")]
        public byte[] Version;
        [XmlField("fileSize")]
        public uint FileSize;
        [XmlField("headSize")]
        public uint HeadSize;
        [XmlField("nodeNum")]
        public uint NodeNum;
        [XmlField("baseCode")]
        public uint BaseCode;
    }

    [SerializableXmlStruct(0x1270f9fe)]
    public struct RouteNodeData
    {
        [XmlField("minpos")]
        public Vector3 MinPos;
        [XmlField("maxpos")]
        public Vector3 MaxPos;
        [XmlField("hitFlag")]
        public byte HitFlag;
        [XmlField("myId")]
        public ushort MyId;
        [XmlField("linkNum")]
        public ushort LinkNum;
        [XmlField("attribute")]
        public uint Attribute;
        [XmlField("nodeLink")]
        public RouteNodeLink[] Links;
    }

    [SerializableXmlStruct(0x35dddca1)]
    public struct RouteNodeLink
    {
        [XmlField("linkId")]
        public ushort LinkId;
        [XmlField("fCost")]
        public float Cost;
    }
}
