using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Misc
{
    [SerializableXmlStruct(0x44BB32E)]
    public struct rUBCell
    {
        [XmlField("mCellInfoList")]
        public CellInfo[] mCellInfoList;
    }

    [SerializableXmlStruct(0x2DF13299)]
    public struct CellInfo
    {
        [XmlField("mId")]
        public uint mId;
        [XmlField("mMembershipX")]
        public uint mMembershipX;
        [XmlField("mMembershipY")]
        public uint mMembershipY;
        [XmlField("mMembershipZ")]
        public uint mMembershipZ;
        [XmlField("mMembershipW")]
        public uint mMembershipW;
        [XmlField("mCoefficientX")]
        public float mCoefficientX;
        [XmlField("mCoefficientY")]
        public float mCoefficientY;
        [XmlField("mCoefficientZ")]
        public float mCoefficientZ;
        [XmlField("mCoefficientW")]
        public float mCoefficientW;
        [XmlField("mConstant")]
        public float mConstant;
        [XmlField("mConclusion")]
        public uint mConclusion;
    }
}
