using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats
{
    /// <summary>
    /// Resource types found in arc files
    /// </summary>
    public enum ResourceType : int
    {
        Unknown,
        cResource,
        r2Texture,
        rArchive,
        rAreaHitLayout,
        rCameraList,
        rCameraListXml,
        rCloth,
        rClothXml,
        rCollision,
        rEffectAnim,
        rEffectAnimation,
        rEffectList,
        rEffectSetData,
        rEffectSetDataList,
        rEffectStrip,
        rEnemyLayout,
        rEnemyLayoutXml,
        rEventTimeSchedule,
        rFacialAnimation,
        rFacialPattern,
        rFSMBrain,
        rFSMBrainXml,
        rGridEnvLight,
        rHavokConstraintLayout,
        rHavokLinkCollisionLayout,
        rHavokVehicleData,
        rHavokVertexLayout,
        rHkCollision,
        rHkMaterial,
        rItemLayout,
        rMapLink,
        rMarkerLayout,
        rMarkerLayoutXml,
        rMessage,
        rMobLayout,
        rModel,
        rModelInfoXml,
        rModelLayout,
        rModelLayoutXml,
        rModelMontage,
        rMotionList,
        rMovie,
        rMovieInterMediate,
        rNMMachine,
        rNMMachineXml,
        rNulls,
        rRenderTargetTexture,
        rRouteNode,
        rRouteNodeXml,
        rScheduler,
        rSchedulerXml,
        rScoopList,
        rShader,
        rSMAdd,
        rSoundAst,
        rSoundCdp,
        rSoundRrd,
        rSoundSeg,
        rSoundSegXml,
        rSoundSeq,
        rSoundSnd,
        rSoundWed,
        rSprAnm,
        rSprLayout,
        rTexture,
        rTextureDDS,
        rUBCell,
        rUBCellList,
        rUBCellXml,
        rUBMembershipSpaceList
    }

    public abstract class GameResource : IRenderable
    {
        #region KnownResourceTypes

        /// <summary>
        /// Dictionary of resource ids and their corresponding <see cref="ResourceType"/>
        /// </summary>
        public static readonly Dictionary<int, ResourceType> KnownResourceTypes = new Dictionary<int, ResourceType>()
        {
            { 0x004A352B, ResourceType.cResource },
            { 0x7470D7E9, ResourceType.r2Texture },
            { 0x21034C90, ResourceType.rArchive },
            { 0x54503672, ResourceType.rAreaHitLayout },
            { 0x5EF1FB52, ResourceType.rCameraList },
            { 0x3F1D8ECD, ResourceType.rCameraListXml },
            { 0x234D7104, ResourceType.rCloth },
            { 0x15BB3845, ResourceType.rClothXml },
            { 0x3900DAD0, ResourceType.rCollision },
            { 0x5E7D6A45, ResourceType.rEffectAnim },
            { 0x03FAE282, ResourceType.rEffectAnimation },
            { 0x294488A8, ResourceType.rEffectList },
            { 0x32231BD1, ResourceType.rEffectSetData },
            { 0x482B5B95, ResourceType.rEffectSetDataList },
            { 0x528770DF, ResourceType.rEffectStrip },
            { 0x2A51D160, ResourceType.rEnemyLayout },
            { 0x4BBDA4FF, ResourceType.rEnemyLayoutXml },
            { 0x124597FE, ResourceType.rEventTimeSchedule },
            { 0x264087B8, ResourceType.rFacialAnimation },
            { 0x4F6FFDDC, ResourceType.rFacialPattern },
            { 0x3E394A0E, ResourceType.rFSMBrain },
            { 0x4017D81C, ResourceType.rFSMBrainXml },
            { 0x5E3DC9F3, ResourceType.rGridEnvLight },
            { 0x57BAE388, ResourceType.rHavokConstraintLayout },
            { 0x6C3B4904, ResourceType.rHavokLinkCollisionLayout },
            { 0x26C299D0, ResourceType.rHavokVehicleData },
            { 0x1AEB54D1, ResourceType.rHavokVertexLayout },
            { 0x3C3D0C05, ResourceType.rHkCollision },
            { 0x5435D27B, ResourceType.rHkMaterial },
            { 0x33B68E3E, ResourceType.rItemLayout },
            { 0x5A45BA9C, ResourceType.rMapLink },
            { 0x543E41DE, ResourceType.rMarkerLayout },
            { 0x35D23441, ResourceType.rMarkerLayoutXml },
            { 0x4CDF60E9, ResourceType.rMessage },
            { 0x25007E1D, ResourceType.rMobLayout },
            { 0x1041BD9E, ResourceType.rModel },
            { 0x07D3088E, ResourceType.rModelInfoXml },
            { 0x08EF36C1, ResourceType.rModelLayout },
            { 0x6903435E, ResourceType.rModelLayoutXml },
            { 0x20208A05, ResourceType.rModelMontage },
            { 0x139EE51D, ResourceType.rMotionList },
            { 0x4D3EDF75, ResourceType.rMovie },
            { 0x335F4A1E, ResourceType.rMovieInterMediate },
            { 0x4126B31B, ResourceType.rNMMachine },
            { 0x6ACE1ACF, ResourceType.rNMMachineXml },
            { 0x5E4C723C, ResourceType.rNulls },
            { 0x27CE98F6, ResourceType.rRenderTargetTexture },
            { 0x2B93C4AD, ResourceType.rRouteNode },
            { 0x007B6D79, ResourceType.rRouteNodeXml },
            { 0x44E79B6E, ResourceType.rScheduler },
            { 0x11AFA688, ResourceType.rSchedulerXml },
            { 0x4A31FCD8, ResourceType.rScoopList },
            { 0x4E32817C, ResourceType.rShader },
            { 0x0B0B8495, ResourceType.rSMAdd },
            { 0x0532063F, ResourceType.rSoundAst },
            { 0x75D21272, ResourceType.rSoundCdp },
            { 0x5A3CED86, ResourceType.rSoundRrd },
            { 0x2E47C723, ResourceType.rSoundSeg },
            { 0x08FB2473, ResourceType.rSoundSegXml },
            { 0x48459606, ResourceType.rSoundSeq },
            { 0x586995B1, ResourceType.rSoundSnd },
            { 0x3D007115, ResourceType.rSoundWed },
            { 0x55A8FB34, ResourceType.rSprAnm },
            { 0x34A8C353, ResourceType.rSprLayout },
            { 0x3CAD8076, ResourceType.rTexture },
            { 0x17DC04AA, ResourceType.rTextureDDS },
            { 0x044BB32E, ResourceType.rUBCell },
            { 0x1189D435, ResourceType.rUBCellList },
            { 0x0596DDE7, ResourceType.rUBCellXml },
            { 0x42000343, ResourceType.rUBMembershipSpaceList }
        };

        /// <summary>
        /// Reverse dictionary of resource types and their type ids.
        /// </summary>
        public static readonly Dictionary<ResourceType, int> KnownResourceTypesReverse = new Dictionary<ResourceType, int>()
        {
            { ResourceType.cResource, 0x004A352B },
            { ResourceType.r2Texture, 0x7470D7E9 },
            { ResourceType.rArchive, 0x21034C90 },
            { ResourceType.rAreaHitLayout, 0x54503672 },
            { ResourceType.rCameraList, 0x5EF1FB52 },
            { ResourceType.rCameraListXml, 0x3F1D8ECD },
            { ResourceType.rCloth, 0x234D7104 },
            { ResourceType.rClothXml, 0x15BB3845 },
            { ResourceType.rCollision, 0x3900DAD0 },
            { ResourceType.rEffectAnim, 0x5E7D6A45 },
            { ResourceType.rEffectAnimation, 0x03FAE282 },
            { ResourceType.rEffectList, 0x294488A8 },
            { ResourceType.rEffectSetData, 0x32231BD1 },
            { ResourceType.rEffectSetDataList, 0x482B5B95 },
            { ResourceType.rEffectStrip, 0x528770DF },
            { ResourceType.rEnemyLayout, 0x2A51D160 },
            { ResourceType.rEnemyLayoutXml, 0x4BBDA4FF },
            { ResourceType.rEventTimeSchedule, 0x124597FE },
            { ResourceType.rFacialAnimation, 0x264087B8 },
            { ResourceType.rFacialPattern, 0x4F6FFDDC },
            { ResourceType.rFSMBrain, 0x3E394A0E },
            { ResourceType.rFSMBrainXml, 0x4017D81C },
            { ResourceType.rGridEnvLight, 0x5E3DC9F3 },
            { ResourceType.rHavokConstraintLayout, 0x57BAE388 },
            { ResourceType.rHavokLinkCollisionLayout, 0x6C3B4904 },
            { ResourceType.rHavokVehicleData, 0x26C299D0 },
            { ResourceType.rHavokVertexLayout, 0x1AEB54D1 },
            { ResourceType.rHkCollision, 0x3C3D0C05 },
            { ResourceType.rHkMaterial, 0x5435D27B },
            { ResourceType.rItemLayout, 0x33B68E3E },
            { ResourceType.rMapLink, 0x5A45BA9C },
            { ResourceType.rMarkerLayout, 0x543E41DE },
            { ResourceType.rMarkerLayoutXml, 0x35D23441 },
            { ResourceType.rMessage, 0x4CDF60E9 },
            { ResourceType.rMobLayout, 0x25007E1D },
            { ResourceType.rModel, 0x1041BD9E },
            { ResourceType.rModelInfoXml, 0x07D3088E },
            { ResourceType.rModelLayout, 0x08EF36C1 },
            { ResourceType.rModelLayoutXml, 0x6903435E },
            { ResourceType.rModelMontage, 0x20208A05 },
            { ResourceType.rMotionList, 0x139EE51D },
            { ResourceType.rMovie, 0x4D3EDF75 },
            { ResourceType.rMovieInterMediate, 0x335F4A1E },
            { ResourceType.rNMMachine, 0x4126B31B },
            { ResourceType.rNMMachineXml, 0x6ACE1ACF },
            { ResourceType.rNulls, 0x5E4C723C },
            { ResourceType.rRenderTargetTexture, 0x27CE98F6 },
            { ResourceType.rRouteNode, 0x2B93C4AD },
            { ResourceType.rRouteNodeXml, 0x007B6D79 },
            { ResourceType.rScheduler, 0x44E79B6E },
            { ResourceType.rSchedulerXml, 0x11AFA688 },
            { ResourceType.rScoopList, 0x4A31FCD8 },
            { ResourceType.rShader, 0x4E32817C },
            { ResourceType.rSMAdd, 0x0B0B8495 },
            { ResourceType.rSoundAst, 0x0532063F },
            { ResourceType.rSoundCdp, 0x75D21272 },
            { ResourceType.rSoundRrd, 0x5A3CED86 },
            { ResourceType.rSoundSeg, 0x2E47C723 },
            { ResourceType.rSoundSegXml, 0x08FB2473 },
            { ResourceType.rSoundSeq, 0x48459606 },
            { ResourceType.rSoundSnd, 0x586995B1 },
            { ResourceType.rSoundWed, 0x3D007115 },
            { ResourceType.rSprAnm, 0x55A8FB34 },
            { ResourceType.rSprLayout, 0x34A8C353 },
            { ResourceType.rTexture, 0x3CAD8076 },
            { ResourceType.rTextureDDS, 0x17DC04AA },
            { ResourceType.rUBCell, 0x044BB32E },
            { ResourceType.rUBCellList, 0x1189D435 },
            { ResourceType.rUBCellXml, 0x0596DDE7 },
            { ResourceType.rUBMembershipSpaceList, 0x42000343 }
        };

        #endregion

        /// <summary>
        /// File path of the game resource
        /// </summary>
        public string FileName { get; protected set; }
        /// <summary>
        /// Datum for tracking what arc file this resource is located in
        /// </summary>
        public DatumIndex Datum { get; set; }
        /// <summary>
        /// True if the byte order of the resource is big endian
        /// </summary>
        public bool IsBigEndian { get; protected set; }
        /// <summary>
        /// Type of game resource
        /// </summary>
        public ResourceType FileType { get; protected set; }

        private static Dictionary<ResourceType, Type> resourceParsers = null;
        /// <summary>
        /// Gets a dictionary where the key is the resource type and the value is the <see cref="System.Type"/> of the parser
        /// </summary>
        public static Dictionary<ResourceType, Type> ResourceParsers
        {
            get
            {
                // If the list is null, build it now.
                if (resourceParsers == null)
                    BuildResourceParsersDictionary();

                return resourceParsers;
            }
        }

        /// <summary>
        /// Creates a new game resource of the specified type
        /// </summary>
        /// <param name="fileName">Game resource file name</param>
        /// <param name="index">Datum index for locating the file</param>
        /// <param name="fileType">Resource file type</param>
        /// <param name="isBigEndian">True if the file is in big endian byte order</param>
        protected GameResource(string fileName, DatumIndex index, ResourceType fileType, bool isBigEndian)
        {
            // Initialize fields.
            this.FileName = fileName;
            this.Datum = index;
            this.FileType = fileType;
            this.IsBigEndian = isBigEndian;
        }

        /// <summary>
        /// Builds the list of resource parsers by finding all types in the assembly that have the <see cref="GameResourceParserAttribute"/> attribute
        /// </summary>
        private static void BuildResourceParsersDictionary()
        {
            // Initialize the list of resource parsers.
            resourceParsers = new Dictionary<ResourceType, Type>();

            // Get a list of types that have the GameResourceParser attribute.
            Type[] parserTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<GameResourceParserAttribute>() != null).ToArray();

            // Loop through all the parser types and build the parser dictionary.
            foreach (Type parser in parserTypes)
            {
                // Get the resource types this parser supports.
                ResourceType[] resourceTypes = parser.GetCustomAttribute<GameResourceParserAttribute>().ResourceTypes;
                for (int i = 0; i < resourceTypes.Length; i++)
                    resourceParsers.Add(resourceTypes[i], parser);
            }
        }

        /// <summary>
        /// Creates a new GameResource object using the game resource parser for that game object type
        /// </summary>
        /// <param name="buffer">Game resource buffer</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="datum">Datum index of the file</param>
        /// <param name="fileType">Type of file</param>
        /// <param name="isBigEndian">True if the file is in big endian byte order, false otherwise</param>
        /// <returns>The parsed GameResource instance</returns>
        public static GameResource FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Make sure we have a parser for the resource type.
            if (resourceParsers.ContainsKey(fileType) == false)
                return null;

            // Invoke the FromGameResource method on the parser type.
            return (GameResource)resourceParsers[fileType].GetMethod("FromGameResource").Invoke(null, new object[] { buffer, fileName, datum, fileType, isBigEndian });
        }

        /// <summary>
        /// Gets the full file name with file extension for the specified <see cref="ArchiveFileEntry"/>
        /// </summary>
        /// <param name="fileEntry"></param>
        /// <returns></returns>
        public static string GetFullResourceName(ArchiveFileEntry fileEntry)
        {
            return GetFullResourceName(fileEntry.FileName, fileEntry.FileType);
        }

        /// <summary>
        /// Gets the full file name with extension for the specified file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static string GetFullResourceName(string filePath, ResourceType fileType)
        {
            // Return the resource file name with file extension.
            return filePath + "." + fileType.ToString();
        }

        /// <summary>
        /// Writes all resource data into a byte array that can be written back to an arc file.
        /// </summary>
        /// <returns>Resource data as a byte array.</returns>
        public abstract byte[] ToBuffer();

        #region IRenderable

        public virtual bool InitializeGraphics(RenderManager manager)
        {
            return false;
        }

        public virtual bool DrawFrame(RenderManager manager)
        {
            return false;
        }

        public virtual void DrawObjectPropertiesUI(RenderManager manager)
        {

        }

        public virtual void CleanupGraphics(RenderManager manager)
        {

        }

        #endregion
    }
}
