using DeadRisingArcTool.FileFormats.Misc;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Spawnable
{
    [SerializableXmlStruct(0x2A51D160)]
    public struct rEnemyLayout
    {
        [XmlField("mVersion")]
        public uint mVersion;
        [XmlField("mAllSetCnt")]
        public uint mAllSetCnt;
        [XmlField("mTotalRatio")]
        public uint mTotalRatio;
        [XmlField("mSpeedyNum")]
        public uint mSpeedyNum;
        [XmlField("mExcludeEnable")]
        public bool mExcludeEnable;
        [XmlField("mpLayoutInfoList")]
        public rEnemyLayoutInfo[] mpLayoutInfoList;
    }

    [SerializableXmlStruct(0x1DA93AA8)]
    public struct rEnemyLayoutInfo
    {
        [XmlField("mSpace")]
        public EnemySpace[] mSpace;
    }

    [SerializableXmlStruct(0x1CAE3803)]
    public struct EnemySpace
    {
        [XmlField("mSpaceNo")]
        public int mSpaceNo;
        [XmlField("mShape")]
        public byte mShape;
        [XmlField("mRatio")]
        public byte mRatio;
        [XmlField("mRectX")]
        public float mRectX;
        [XmlField("mRectZ")]
        public float mRectZ;
        [XmlField("mRadius")]
        public float mRadius;
        [XmlField("mVertA")]
        public Vector3 mVertA;
        [XmlField("mVertB")]
        public Vector3 mVertB;
        [XmlField("mVertC")]
        public Vector3 mVertC;
        [XmlField("mVertD")]
        public Vector3 mVertD;
        [XmlField("mCenter")]
        public Vector3 mCenter;
        [XmlField("mCenterYFixEnable")]
        public bool mCenterYFixEnable;
        [XmlField("mCenterYFix")]
        public float mCenterYFix;
        [XmlField("mSpaceAngleY")]
        public short mSpaceAngleY;
        [XmlField("mSlopeAngle")]
        public float mSlopeAngle;
        [XmlField("mEnemyAngleY")]
        public short mEnemyAngleY;
        [XmlField("mEnemyAngleYRange")]
        public short mEnemyAngleYRange;
        [XmlField("mPatternNo")]
        public byte mPatternNo;
        [XmlField("mClip")]
        public bool mClip;
        [XmlField("mClipCenter")]
        public Vector3 mClipCenter;
        [XmlField("mClipRadius")]
        public float mClipRadius;
        [XmlField("mClipA")]
        public Vector3 mClipA;
        [XmlField("mClipB")]
        public Vector3 mClipB;
        [XmlField("mClipC")]
        public Vector3 mClipC;
        [XmlField("mClipD")]
        public Vector3 mClipD;
        [XmlField("mDefaultSetMax")]
        public short mDefaultSetMax;
        [XmlField("mCustomSetMax")]
        public short mCustomSetMax;
        [XmlField("mExclude")]
        public bool[] mExclude;
    }
}
