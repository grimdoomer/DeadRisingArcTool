using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using DeadRisingArcTool.FileFormats.Misc;
using DeadRisingArcTool.Utilities;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BoundingBox = DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos.BoundingBox;

namespace DeadRisingArcTool.FileFormats.Spawnable
{
    [GameResourceParser(ResourceType.rAreaHitLayout)]
    public class rAreaHitLayout : GameResource
    {
        #region LayoutInfo

        public class LayoutInfo
        {
            [XmlField("SHAPE")]
            public byte Shape;
            [XmlField("p->mRadius")]
            public float Radius;
            [XmlField("p->mRectX")]
            public float RectX;
            [XmlField("p->mRectY")]
            public float RectY;
            [XmlField("CHECK")]
            public uint Check;
            [XmlField("SUCCESS")]
            public uint Success;
            [XmlField("TRRIGER")]
            public uint Trigger;
            [XmlField("TARGET ANGLE")]
            public Vector3 TargetAngle;
            [XmlField("RANGE  ANGLE")]
            public Vector3 RangeAngle;
            [XmlField("DAY    -")]
            public byte Day;
            [XmlField("Hour   -")]
            public byte Hour;
            [XmlField("Minute -")]
            public byte Minute;
            [XmlField("Second -")]
            public byte Second;
            [XmlField("DAY     ")]
            public byte Day1;
            [XmlField("Hour    ")]
            public byte Hour1;
            [XmlField("Minute  ")]
            public byte Minute1;
            [XmlField("Second  ")]
            public byte Second1;
            [XmlField("CHECK EVENT_SYSTEM")]
            public uint CheckEventSystem;
            [XmlField("CHECK EVENT_MAIN")]
            public uint CheckEventMain;
            [XmlField("CHECK EVENT_MAIN2")]
            public uint CheckEventMain2;
            [XmlField("CHECK EVENT_BOSS")]
            public uint CheckEventBoss;

            [XmlField("CHECK EVENT_EVS2")]
            public uint CheckEventEvs2;
            [XmlField("CHECK EVENT_MAIN5")]
            public uint CheckEventMain5;
            [XmlField("CHECK EVENT EM_SET00")]
            public uint CheckEventEm_Set00;
            [XmlField("CHECK EM_SET01")]
            public uint CheckEm_Set01;
            [XmlField("CHECK EVENT_DOOR")]
            public uint CheckEventDoor;
            [XmlField("CHECK EVENT_MESS")]
            public uint CheckEventMess;
            [XmlField("CHECK EVENT_TUDO")]
            public uint CheckEventTudo;
            [XmlField("CHECK EVENT_TUTO")]
            public uint CheckEventTutorial;
            [XmlField("CHECK EVENT_TUTO2")]
            public uint CheckEventTutorial2;
            [XmlField("CHECK EVENT_BROKE")]
            public uint CheckEventBroke;
            [XmlField("CHECK EVENT_PHOTO")]
            public uint CheckEventPhoto;
            [XmlField("CHECK EVENT_EM_APPEAR")]
            public uint CheckEventEmAppear;
            [XmlField("CHECK EVENT_DIE")]
            public uint CheckEventDie;
            [XmlField("CHECK EVENT_ETC")]
            public uint CheckEventEtc;
            [XmlField("CHECK EVENT_AREA")]
            public uint CheckEventArea;
            [XmlField("CHECK EVENT_WAITTING")]
            public uint CheckEventWaiting;
            [XmlField("CHECK EVENT_TREASURE")]
            public uint CheckEventTreasure;
            [XmlField("CHECK EVENT_TREASURE2")]
            public uint CheckEventTrasure2;
            [XmlField("CHECK CONTENTS_KEY")]
            public uint CheckContentsKey;
            [XmlField("CHECK EVENT_CASE")]
            public uint CheckEventCase;
            [XmlField("CHECK EVENT MESSAGE")]
            public uint CheckEventMessage;
            [XmlField("CHECK ACHIVEMENT")]
            public uint CheckAchievement;
            [XmlField("CHECK ACHIVEMENT2")]
            public uint CheckAchievement2;
            [XmlField("CHECK NOT EVENT_SYSTEM")]
            public uint CheckNotEventSystem;
            [XmlField("CHECK NOT EVENT_MAIN")]
            public uint CheckNotEventMain;
            [XmlField("CHECK NOT EVENT_MAIN2")]
            public uint CheckNotEventMain2;
            [XmlField("CHECK NOT EVENT_BOSS")]
            public uint CheckNotEventBoss;
            [XmlField("CHECK NOT EVENT_EVS")]
            public uint CheckNotEventEvs;
            [XmlField("CHECK NOT EVENT_MAIN5")]
            public uint CheckNotEventMain5;
            [XmlField("CHECK NOT EVENT EM_SET00")]
            public uint CheckNotEventEmSet00;
            [XmlField("CHECK NOT EM_SET01")]
            public uint CheckNotEmSet01;
            [XmlField("CHECK NOT EVENT_DOOR")]
            public uint CheckNotEventDoor;
            [XmlField("CHECK NOT EVENT_MESS")]
            public uint CheckNotEventMess;
            [XmlField("CHECK NOT EVENT_TUDO")]
            public uint CheckNotEventTudo;
            [XmlField("CHECK NOT EVENT_TUTO")]
            public uint CheckNotEventTutorial;
            [XmlField("CHECK NOT EVENT_TUTO2")]
            public uint CheckNotEventTutorial2;
            [XmlField("CHECK NOT EVENT_BROKE")]
            public uint CheckNotEventBroke;
            [XmlField("CHECK NOT EVENT_PHOTO")]
            public uint CheckNotEventPhoto;
            [XmlField("CHECK NOT EVENT_EM_APPEAR")]
            public uint CheckNotEventEmAppear;
            [XmlField("CHECK NOT EVENT_DIE")]
            public uint CheckNotEventDie;
            [XmlField("CHECK NOT EVENT_ETC")]
            public uint CheckNotEventEtc;
            [XmlField("CHECK NOT EVENT_AREA")]
            public uint CheckNotEventArea;
            [XmlField("CHECK NOT EVENT_WAITTING")]
            public uint CheckNotEventWaiting;
            [XmlField("CHECK NOT EVENT_TREASURE")]
            public uint CheckNotEventTreasure;
            [XmlField("CHECK NOT EVENT_TREASURE2")]
            public uint CheckNotEventTreasure2;
            [XmlField("CHECK NOT CONTENTS_KEY")]
            public uint CheckNotContentsKey;
            [XmlField("CHECK NOT EVENT_CASE")]
            public uint CheckNotEventCase;
            [XmlField("CHECK NOT EVENT MESSAGE")]
            public uint CheckNotEventMessage;
            [XmlField("CHECK NOT ACHIVEMENT")]
            public uint CheckNotAchievement;
            [XmlField("CHECK NOT ACHIVEMENT2")]
            public uint CheckNotAchievement2;
            [XmlField("SUCCESS EVENT_SYSTEM")]
            public uint SuccessEventSystem;
            [XmlField("SUCCESS EVENT_MAIN")]
            public uint SuccessEventMain;
            [XmlField("SUCCESS EVENT_MAIN2")]
            public uint SuccessEventMain2;
            [XmlField("SUCCESS EVENT_BOSS")]
            public uint SuccessEventBoss;
            [XmlField("SUCCESS EVENT_EVS")]
            public uint SuccessEventEvs;
            [XmlField("SUCCESS EVENT_MAIN5")]
            public uint SuccessEventMain5;
            [XmlField("SUCCESS SET_MAIN")]
            public uint SuccessSetMain;
            [XmlField("SUCCESS EM_SET00")]
            public uint SuccessEmSet00;
            [XmlField("SUCCESS EM_SET01")]
            public uint SuccessEmSet01;
            [XmlField("SUCCESS EVENT_DOOR")]
            public uint SuccessEventDoor;
            [XmlField("SUCCESS EVENT_MESS")]
            public uint SuccessEventMess;
            [XmlField("SUCCESS EVENT_TUDO")]
            public uint SuccessEventTudo;
            [XmlField("SUCCESS EVENT_TUTO")]
            public uint SuccessEventTutorial;
            [XmlField("SUCCESS EVENT_TUTO2")]
            public uint SuccessEventTutorial2;
            [XmlField("SUCCESS EVENT_BROKE")]
            public uint SuccessEventBroke;
            [XmlField("SUCCESS EVENT_PHOTO")]
            public uint SuccessEventPhoto;
            [XmlField("SUCCESS EVENT_EM_APPEAR")]
            public uint SuccessEventEmAppear;
            [XmlField("SUCCESS EVENT_DIE")]
            public uint SuccessEventDie;
            [XmlField("SUCCESS EVENT_ETC")]
            public uint SuccessEventEtc;
            [XmlField("SUCCESS EVENT_AREA")]
            public uint SuccessEventArea;
            [XmlField("SUCCESS EVENT_WAITTING")]
            public uint SuccessEventWaiting;
            [XmlField("SUCCESS EVENT_TREASURE")]
            public uint SuccessEventTreasure;
            [XmlField("SUCCESS EVENT_TREASURE2")]
            public uint SuccessEventTrasure2;
            [XmlField("SUCCESS CONTENTS_KEY")]
            public uint SuccessContentsKey;
            [XmlField("SUCCESS EVENT_CASE")]
            public uint SuccessEventCase;
            [XmlField("SUCCESS EVENT MESSAGE")]
            public uint SuccessEventMessage;
            [XmlField("SUCCESS ACHIVEMENT")]
            public uint SuccessAchievement;
            [XmlField("SUCCESS ACHIVEMENT2")]
            public uint SuccessAchievement2;
            [XmlField("MESSAGE_NO")]
            public ushort MessageNo;
            [XmlField("MESSAGE_TYPE")]
            public ushort MessageType;
            [XmlField("CHECK MESSAGE NO  ")]
            public ushort CheckMessageNo;
            [XmlField("CHECK MESSAGE TYPE")]
            public ushort CheckMessageType;
            [XmlField("CHECK MESSAGE POS ")]
            public Vector3 CheckMessagePos;
            [XmlField("ITEM")]
            public uint Item;
            [XmlField("EVENT NAME")]
            public string EventName;
            [XmlField("EVENT TIME NO")]
            public uint EventTimeNo;
            [XmlField("p->mCursorWorldPos")]
            public Vector3 CursorWorldPos;
            [XmlField("p->mFileName")]
            public string[] FileName;
            [XmlField("AREA JUMP NAME")]
            public string AreaJumpName;
            [XmlField("AREA JUMP POS  ")]
            public Vector3 AreaJumpPos;
            [XmlField("AREA JUMP ANGLE")]
            public Vector3 AreaJumpAngle;
            [XmlField("AREA DOOR NO")]
            public uint AreaDoorNo;
            [XmlField("AREA CHECK NAME")]
            public string AreaCheckName;
            [XmlField("SQOOP_NO")]
            public uint ScoopNo;
            [XmlField("CLOTH NO")]
            public int ClothNo;
            [XmlField("CLOTH ID")]
            public int ClothId;
            [XmlField("ACT TYPE")]
            public uint ActType;
            [XmlField("BANK   NO")]
            public uint BankNo;
            [XmlField("MOITON NO")]
            public uint MotionNo;
            [XmlField("ICON NO")]
            public uint IconNo;
        }

        #endregion

        public byte[] Buffer { get; private set; }

        private List<LayoutInfo> layoutInfoList = new List<LayoutInfo>();
        public LayoutInfo[] LayoutInfoList { get { return this.layoutInfoList.ToArray(); } }

        // Rendering resources.
        private BoundingBox[] areaHitPlacements;

        // UI resources.
        private List<string> areaJumpNames = null;
        private int selectedSpawnIndex = -1;
        private int hoveredSpawnIndex = -1;

        // Cached list of fields for the layout info struct.
        private FieldInfo[] layoutInfoFields = typeof(LayoutInfo).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public rAreaHitLayout(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian) :
            base(fileName, datum, fileType, isBigEndian)
        {
            // Initialize fields.
            this.Buffer = buffer;
        }

        public override byte[] ToBuffer()
        {
            throw new NotImplementedException();
        }

        #region FromGameResource

        public static rAreaHitLayout FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Create a new rAreaHitLayout object to populate.
            rAreaHitLayout layout = new rAreaHitLayout(buffer, fileName, datum, fileType, isBigEndian);

            // Cache a list of all fields in the LayoutInfo struct key'd by their xml field name.
            Dictionary<string, FieldInfo> layoutInfoFields = layout.layoutInfoFields.ToDictionary(
                (FieldInfo f) => { return ((XmlFieldAttribute)f.GetCustomAttribute(typeof(XmlFieldAttribute))).FieldName; },
                (FieldInfo f) => { return f; },
                StringComparer.InvariantCultureIgnoreCase);

            // Create a new xml reader on the file buffer.
            XmlReader reader = XmlTextReader.Create(new MemoryStream(buffer));

            // Read until we hit the class element.
            while (reader.Read() == true)
            {
                // Check the current node for the class element.
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "class")
                {
                    // Make sure the name and type are rAreaHitLayout.
                    if (reader.GetAttribute("name").Equals("rAreaHitLayout", StringComparison.InvariantCultureIgnoreCase) == false ||
                        reader.GetAttribute("type").Equals("rAreaHitLayout", StringComparison.InvariantCultureIgnoreCase) == false)
                    {
                        // Element has invalid attributes.
                        return null;
                    }

                    // Read the next element and make sure it is the layout info list.
                    while (reader.Read() == true)
                    {
                        if (reader.Name == "array" && reader.GetAttribute("name").Equals("mpLayoutInfoList", StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            // Get the layout info count.
                            int layoutCount = int.Parse(reader.GetAttribute("count"));

                            // Loop and read all the layout info structs.
                            for (int i = 0; i < layoutCount; i++)
                            {
                                // Read the next element and check for the classref element.
                                while (reader.Read() == true)
                                {
                                    if (reader.Name == "classref" && reader.GetAttribute("type").Equals("rAreaHitLayout::LayoutInfo", StringComparison.InvariantCultureIgnoreCase) == true)
                                    {
                                        // Create a new layout info struct.
                                        LayoutInfo info = new LayoutInfo();

                                        // Loop until we hit the classref end element.
                                        while (reader.Read() == true && reader.NodeType != XmlNodeType.EndElement && reader.Name != "classref")
                                        {
                                            // Check if the current element is a start element.
                                            if (reader.NodeType == XmlNodeType.Element)
                                            {
                                                // Check if the field name is in our cached field list.
                                                string fieldName = reader.GetAttribute("name");
                                                if (layoutInfoFields.ContainsKey(fieldName) == true)
                                                {
                                                    // Parse the field value.
                                                    object fieldValue = ParseFieldValue(reader);

                                                    // Set the field value.
                                                    layoutInfoFields[fieldName].SetValue(info, fieldValue);
                                                }
                                                else
                                                {
                                                    // TODO: Unsupported field.
                                                }
                                            }
                                        }

                                        // Add the layout to the list.
                                        layout.layoutInfoList.Add(info);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Close the xml reader and return the area hit layout object.
            reader.Close();
            return layout;
        }

        private static object ParseFieldValue(XmlReader reader)
        {
            // Get the field value based on the field type.
            object fieldValue = null;
            switch (reader.Name)
            {
                case "u8": fieldValue = byte.Parse(reader.GetAttribute("value")); break;
                case "u16": fieldValue = ushort.Parse(reader.GetAttribute("value")); break;
                case "s32": fieldValue = int.Parse(reader.GetAttribute("value")); break;
                case "u32": fieldValue = uint.Parse(reader.GetAttribute("value")); break;
                case "f32": fieldValue = float.Parse(reader.GetAttribute("value")); break;
                case "string": fieldValue = reader.GetAttribute("value"); break;
                case "vector3":
                    {
                        fieldValue = new Vector3(
                            float.Parse(reader.GetAttribute("x")),
                            float.Parse(reader.GetAttribute("y")),
                            float.Parse(reader.GetAttribute("z")));
                        break;
                    }
                case "array":
                    {
                        // Get the legnth of the array and allocate a temp one.
                        int arrayLength = int.Parse(reader.GetAttribute("count"));
                        string[] arrayValues = new string[arrayLength];

                        // Read the inner xml for the array values.
                        int i = 0;
                        while (reader.Read() == true && reader.NodeType != XmlNodeType.EndElement && reader.Name != "array")
                        {
                            // Make sure this is an element start node.
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                // Parse the current array entry.
                                arrayValues[i++] = (string)ParseFieldValue(reader);
                            }
                        }

                        // Set the field value.
                        fieldValue = arrayValues;
                        break;
                    }
                default:
                    {
                        // Unsupported node type.
                        throw new Exception("Unsupported node type");
                    }
            }

            // Return the parsed field value.
            return fieldValue;
        }

        #endregion

        #region IRenderable

        public override bool InitializeGraphics(RenderManager manager)
        {
            // Allocate arrays.
            this.areaHitPlacements = new BoundingBox[this.layoutInfoList.Count];
            this.areaJumpNames = new List<string>();

            // If there is at least one spawn set the initial spawn index.
            if (this.layoutInfoList.Count > 0)
                this.selectedSpawnIndex = 0;

            // Initialize the area jump placements array.
            for (int i = 0; i < this.areaHitPlacements.Length; i++)
            {
                // Setup the min/max vectors.
                Vector4 minBounds = new Vector4(-(this.layoutInfoList[i].RectX / 2.0f), 0.0f, 0.0f, 0.0f);
                Vector4 maxBounds = new Vector4(this.layoutInfoList[i].RectX / 2, this.layoutInfoList[i].RectY, 10.0f, 0.0f);

                // Initialize the placement box.
                this.areaHitPlacements[i] = new BoundingBox(minBounds, maxBounds, new Color4(0xFFFF0000));
                this.areaHitPlacements[i].Style = Geometry.DirectX.Gizmos.RenderStyle.Solid;
                this.areaHitPlacements[i].InitializeGraphics(manager);

                // Format the area name for the object properties window.
                this.areaJumpNames.Add("Jump " + i.ToString() + " - " + this.layoutInfoList[i].AreaJumpName);
            }

            // Initialized successfully.
            return true;
        }

        public override bool DrawFrame(RenderManager manager)
        {
            // Loop and draw each of the area jump boxes.
            for (int i = 0; i < this.layoutInfoList.Count; i++)
            {
                // Calculate the position and rotation matrices for the jump point.
                Matrix rotX = Matrix.RotationX(this.layoutInfoList[i].AreaJumpAngle.X);
                Matrix rotY = Matrix.RotationY(this.layoutInfoList[i].AreaJumpAngle.Y);
                Matrix rotZ = Matrix.RotationZ(this.layoutInfoList[i].AreaJumpAngle.Z);
                Quaternion rotation = Quaternion.RotationMatrix(rotX * rotY * rotZ);
                Vector3 rotPos = new Vector3(this.layoutInfoList[i].RectX / 2, 0.0f, 0.0f);
                Matrix world = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, rotation, this.layoutInfoList[i].CursorWorldPos);

                // Setup WVP matrix.
                manager.ShaderConstants.gXfViewProj = Matrix.Transpose(world * manager.Camera.ViewMatrix * manager.ProjectionMatrix);
                manager.UpdateShaderConstants();

                // Draw the bounding box for the jump trigger.
                this.areaHitPlacements[i].Color = new Color4(this.hoveredSpawnIndex == i ? 0x8000FF00 : 0x80FF0000);
                this.areaHitPlacements[i].DrawFrame(manager);
            }

            return true;
        }

        public override void DrawObjectPropertiesUI(RenderManager manager)
        {
            // Reset the hovered area jump index.
            this.hoveredSpawnIndex = -1;

            // Draw the combo box for area jump selection.
            string previewString = this.selectedSpawnIndex != -1 ? this.areaJumpNames[this.selectedSpawnIndex] : "";
            if (ImGui.BeginCombo("Area Jump", previewString) == true)
            {
                // Add option entries for each jump point.
                for (int i = 0; i < this.areaJumpNames.Count; i++)
                {
                    // Add an option for the jump point.
                    bool isSelected = this.selectedSpawnIndex == i;
                    if (ImGui.Selectable(this.areaJumpNames[i], isSelected) == true)
                        this.selectedSpawnIndex = i;

                    // Set focus on the selected item when first opening.
                    if (isSelected == true)
                        ImGui.SetItemDefaultFocus();

                    // If the item is hovered change the placement sphere color.
                    if (ImGui.IsItemHovered() == true)
                        this.hoveredSpawnIndex = i;
                }
                ImGui.EndCombo();
            }
            ImGui.Separator();

            // If the selected jump point index is set show the spawn properties.
            if (this.selectedSpawnIndex != -1)
            {
                // Display jump point properties.
                ImGui.InputScalarInt8("SHAPE", ref this.layoutInfoList[this.selectedSpawnIndex].Shape);
                ImGui.InputFloat("p->mRadius", ref this.layoutInfoList[this.selectedSpawnIndex].Radius);
                ImGui.InputFloat("p->mRectX", ref this.layoutInfoList[this.selectedSpawnIndex].RectX);
                ImGui.InputFloat("p->mRectY", ref this.layoutInfoList[this.selectedSpawnIndex].RectY);
                ImGui.InputScalarUInt32("CHECK", ref this.layoutInfoList[this.selectedSpawnIndex].Check);
            }
        }

        public override void CleanupGraphics(RenderManager manager)
        {
        }

        #endregion
    }
}
