using DeadRisingArcTool.FileFormats.Misc;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Sound
{
    [SerializableXmlStruct(0x2E47C723)]
    public struct rSoundSeg
    {
        [XmlField("mpLayoutInfoList")]
        public rSoundSegLayoutInfo[] mpLayoutInfoList;
    }

    [SerializableXmlStruct(0x1E7A21E4)]
    public struct rSoundSegLayoutInfo
    {
        [XmlField("SHAPE")]
        public uint SHAPE;
        [XmlField("p->mSetPosC")]
        public Vector3 mSetPosC;
	    [XmlField("p->mSetPos0")]
        public Vector3 mSetPos0;
	    [XmlField("p->mSetPos1")]
        public Vector3 mSetPos1;
	    [XmlField("p->mSetPos2")]
        public Vector3 mSetPos2;
	    [XmlField("p->mSetPos3")]
        public Vector3 mSetPos3;
	    [XmlField("p->mPosY")]
        public float mPosY;
	    [XmlField("p->mHeight")]
        public float mHeight;
	    [XmlField("p->mRadius")]
        public float mRadius;
	    [XmlField("CHECK")]
        public uint CHECK;
        [XmlField("CHECK TARGET ANGLE")]
        public float CHECK_TARGET_ANGLE;
        [XmlField("CHECK RANGE  ANGLE")]
        public float CHECK_RANGE__ANGLE;
        [XmlField("CHECK CONDITIONAL NO")]
        public uint CHECK_CONDITIONAL_NO;
        [XmlField("SUCCESS")]
        public uint SUCCESS;
        [XmlField("SUCCESS SE REQUEST SND FILE NO")]
        public uint SUCCESS_SE_REQUEST_SND_FILE_NO;
        [XmlField("SUCCESS SE REQUEST NO")]
        public uint SUCCESS_SE_REQUEST_NO;
        [XmlField("SUCCESS STREAM FILE NO")]
        public uint SUCCESS_STREAM_FILE_NO;
        [XmlField("SUCCESS SEQ REQUEST SND FILE NO")]
        public uint SUCCESS_SEQ_REQUEST_SND_FILE_NO;
        [XmlField("SUCCESS SEQ REQUEST SEQ FILE NO")]
        public uint SUCCESS_SEQ_REQUEST_SEQ_FILE_NO;
        [XmlField("SUCCESS SEQ REQUEST LINE NO")]
        public uint SUCCESS_SEQ_REQUEST_LINE_NO;
        [XmlField("SUCCESS VOLUME CURV FILE NO")]
        public uint SUCCESS_VOLUME_CURV_FILE_NO;
        [XmlField("SUCCESS REVERB CURV FILE NO")]
        public uint SUCCESS_REVERB_CURV_FILE_NO;
        [XmlField("SUCCESS LFE CURV FILE NO")]
        public uint SUCCESS_LFE_CURV_FILE_NO;
        [XmlField("SUCCESS ADSR FILE NO")]
        public uint SUCCESS_ADSR_FILE_NO;
        [XmlField("SUCCESS ADSR INDEX NO")]
        public uint SUCCESS_ADSR_INDEX_NO;
        [XmlField("SUCCESS EQLZ FILE NO")]
        public uint SUCCESS_EQLZ_FILE_NO;
        [XmlField("SUCCESS EQLZ INDEX NO")]
        public uint SUCCESS_EQLZ_INDEX_NO;
        [XmlField("SUCCESS REVERB FILE NO")]
        public uint SUCCESS_REVERB_FILE_NO;
        [XmlField("SUCCESS REVERB INDEX NO")]
        public uint SUCCESS_REVERB_INDEX_NO;
        [XmlField("SUCCESS SE REQUEST SND FILE ID")]
        public ulong SUCCESS_SE_REQUEST_SND_FILE_ID;
        [XmlField("SUCCESS STREAM FILE ID")]
        public ulong SUCCESS_STREAM_FILE_ID;
        [XmlField("SUCCESS SEQ REQUEST SND FILE ID")]
        public ulong SUCCESS_SEQ_REQUEST_SND_FILE_ID;
        [XmlField("SUCCESS SEQ REQUEST SEQ FILE ID")]
        public ulong SUCCESS_SEQ_REQUEST_SEQ_FILE_ID;
        [XmlField("SUCCESS VOLUME CURV FILE ID")]
        public ulong SUCCESS_VOLUME_CURV_FILE_ID;
        [XmlField("SUCCESS REVERB CURV FILE ID")]
        public ulong SUCCESS_REVERB_CURV_FILE_ID;
        [XmlField("SUCCESS LFE CURV FILE ID")]
        public ulong SUCCESS_LFE_CURV_FILE_ID;
        [XmlField("SUCCESS ADSR FILE ID")]
        public ulong SUCCESS_ADSR_FILE_ID;
        [XmlField("SUCCESS EQLZ FILE ID")]
        public ulong SUCCESS_EQLZ_FILE_ID;
        [XmlField("SUCCESS REVERB FILE ID")]
        public ulong SUCCESS_REVERB_FILE_ID;
        [XmlField("SUCCESS VOLUME CURV FILE OLD")]
        public uint SUCCESS_VOLUME_CURV_FILE_OLD;
        [XmlField("SUCCESS REVERB CURV FILE OLD")]
        public uint SUCCESS_REVERB_CURV_FILE_OLD;
        [XmlField("SUCCESS LFE CURV FILE OLD")]
        public uint SUCCESS_LFE_CURV_FILE_OLD;
        [XmlField("SUCCESS ADSR FILE OLD")]
        public uint SUCCESS_ADSR_FILE_OLD;
        [XmlField("SUCCESS EQLZ FILE OLD")]
        public uint SUCCESS_EQLZ_FILE_OLD;
        [XmlField("SUCCESS REVERB FILE OLD")]
        public uint SUCCESS_REVERB_FILE_OLD;
        [XmlField("SUCCESS ADSR INDEX OLD")]
        public uint SUCCESS_ADSR_INDEX_OLD;
        [XmlField("SUCCESS EQLZ INDEX OLD")]
        public uint SUCCESS_EQLZ_INDEX_OLD;
        [XmlField("SUCCESS REVERB INDEX OLD")]
        public uint SUCCESS_REVERB_INDEX_OLD;
        [XmlField("SUCCESS GENERATOR NO")]
        public uint SUCCESS_GENERATOR_NO;
        [XmlField("SUCCESS SHUTOUT VOLUME")]
        public uint SUCCESS_SHUTOUT_VOLUME;
        [XmlField("SUCCESS TEMP0")]
        public uint SUCCESS_TEMP0;
        [XmlField("SUCCESS TEMP1")]
        public uint SUCCESS_TEMP1;
        [XmlField("SUCCESS TEMP2")]
        public uint SUCCESS_TEMP2;
        [XmlField("SUCCESS TEMP3")]
        public uint SUCCESS_TEMP3;
    }
}
