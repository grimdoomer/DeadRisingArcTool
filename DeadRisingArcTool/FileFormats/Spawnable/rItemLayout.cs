using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.FileFormats.Geometry.DirectX;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Gizmos;
using DeadRisingArcTool.FileFormats.Geometry.DirectX.Misc;
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
using System.Windows.Forms;
using System.Xml;

namespace DeadRisingArcTool.FileFormats.Spawnable
{
    [GameResourceParser(ResourceType.rItemLayout)]
    public class rItemLayout : GameResource, IPickableObject
    {
        #region LayoutInfo

        public class LayoutInfo
        {
            [XmlField("SHAPE")]
            public byte Shape;
            [XmlField("p->mRectX")]
            public float RectX;
            [XmlField("p->mRectY")]
            public float RectY;
            [XmlField("p->mRadius")]
            public float Radius;
            [XmlField("p->mAngle")]
            public Vector3 Angle;
            [XmlField("p->mRangeAngle")]
            public Vector3 RangeAngle;
            [XmlField("p->mModelAngle")]
            public Vector3 ModelAngle;
            [XmlField("p->mCursorWorldPos")]
            public Vector3 CursorWorldPos;
            [XmlField("p->mCursorWorldPosOffs")]
            public Vector3 CursorWorldPosOffs;
            [XmlField("p->mMessNo")]
            public ushort MessageNo;
            [XmlField("p->mMessType")]
            public ushort MessageType;
            [XmlField("p->mMessOffs")]
            public Vector3 MessageOffset;
            [XmlField("CHECK")]
            public uint Check;
            [XmlField("ITEM NO")]
            public uint ItemNo;
            [XmlField("ITEM ID")]
            public uint ItemId;
            [XmlField("ITEM UNIQUE")]
            public uint ItemUnique;
            [XmlField("ITEM SET TYPE")]
            public uint ItemSetType;
            [XmlField("ITEM PHOTO ID")]
            public uint ItemPhotoId;
            [XmlField("CHANGE ITEM NO")]
            public uint ChangeItemNo;
            [XmlField("CHANGE ITEM ID")]
            public uint ChangeItemId;
            [XmlField("CHANGE ITEM SET TYPE")]
            public uint ChangeItemSetType;
            [XmlField("BROKEN_FLAG")]
            public uint BrokenFlag;
            [XmlField("BROKEN_CHECK_FLAG")]
            public uint BrokenCheckFlag;
            [XmlField("EVENT_SYSTEM")]
            public uint EventSystem;
            [XmlField("EVENT_MAIN")]
            public uint EventMain;
            [XmlField("EVENT_MAIN2")]
            public uint EventMain2;
            [XmlField("EVENT_SET")]
            public uint EventSet;
            [XmlField("EVENT_DOOR")]
            public uint EventDoor;
            [XmlField("EVENT_MESS")]
            public uint EventMessage;
            [XmlField("EVENT_TODO")]
            public uint EventTodo;
            [XmlField("EVENT_TUTO")]
            public uint EventTutorial;
            [XmlField("EVENT_TUTO2")]
            public uint EventTutorial2;
            [XmlField("EVENT_BROKE")]
            public uint EventBroke;
            [XmlField("EVENT_PHOTO")]
            public uint EventPhoto;
            [XmlField("EVENT_APPEAR")]
            public uint EventAppear;
            [XmlField("EVENT_DIE")]
            public uint EventDie;
            [XmlField("EVENT_WAITTING")]
            public uint EventWaiting;
            [XmlField("EVENT_TREASURE")]
            public uint EventTreasure;
            [XmlField("EVENT_CASE")]
            public uint EventCase;
            [XmlField("MESSAGE")]
            public uint Message;
            [XmlField("ONE_SET_FLAG0")]
            public uint OneSetFlag0;
            [XmlField("ONE_SET_FLAG1")]
            public uint OneSetFlag1;
            [XmlField("ONE_SET_FLAG2")]
            public uint OneSetFlag2;
            [XmlField("ONE_SET_FLAG3")]
            public uint OneSetFlag3;
            [XmlField("p->className")]
            public string ClassName;
            [XmlField("p->resourceName")]
            public string ResourceName;
            [XmlField("MOB SET TYPE")]
            public uint MobSetType;
            [XmlField("MOB SET X")]
            public uint MobSetX;
            [XmlField("MOB SET Y")]
            public uint MobSetY;
            [XmlField("MOB SET Z")]
            public uint MobSetZ;
            [XmlField("MOB ADJUST X")]
            public float MobAdjustX;
            [XmlField("MOB ADJUST Y")]
            public float MobAdjustY;
            [XmlField("MOB ADJUST Z")]
            public float MobAdjustZ;
            [XmlField("HOSEI JNT NO")]
            public uint HoseiJointNo;
            [XmlField("HOSEI OFFSET POS")]
            public Vector3 HoseiOffsetPos;
            [XmlField("HOSEI RADIUS")]
            public float HoseiRadius;
            [XmlField("HOSEI HEIGHT")]
            public float HoseiHeight;
            [XmlField("ACT TYPE")]
            public uint ActType;
            [XmlField("BANK   NO")]
            public uint BankNo;
            [XmlField("MOTION NO")]
            public uint MotionNo;
            [XmlField("ICON NO")]
            public uint IconNo;
            [XmlField("SCR NO1")]
            public uint ScrNo1;
            [XmlField("SCR NO2")]
            public uint ScrNo2;
        }

        #endregion

        public byte[] Buffer { get; private set; }

        private List<LayoutInfo> layoutInfoList = new List<LayoutInfo>();
        public LayoutInfo[] LayoutInfoList { get { return this.layoutInfoList.ToArray(); } }

        // Rendering resources.
        private ItemPlacementGizmo[] itemPlacements;
        private GameResource[] itemModels;

        // UI resources.
        private List<string> itemSpawnNames = null;
        private int selectedSpawnIndex = -1;
        private int hoveredSpawnIndex = -1;

        // Selected object data.
        private HashSet<int> selectedSpawnIndices = new HashSet<int>();

        // Cached list of fields for the layout info struct.
        private FieldInfo[] layoutInfoFields = typeof(LayoutInfo).GetFields(BindingFlags.Public | BindingFlags.Instance);

        public rItemLayout(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian) :
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

        public static rItemLayout FromGameResource(byte[] buffer, string fileName, DatumIndex datum, ResourceType fileType, bool isBigEndian)
        {
            // Create a new rItemLayout object to populate.
            rItemLayout layout = new rItemLayout(buffer, fileName, datum, fileType, isBigEndian);

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
                    // Make sure the name and type are rItemLayout.
                    if (reader.GetAttribute("name").Equals("rItemLayout", StringComparison.InvariantCultureIgnoreCase) == false ||
                        reader.GetAttribute("type").Equals("rItemLayout", StringComparison.InvariantCultureIgnoreCase) == false)
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
                                    if (reader.Name == "classref" && reader.GetAttribute("type").Equals("rItemLayout::LayoutInfo", StringComparison.InvariantCultureIgnoreCase) == true)
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
                                                    // Get the field value based on the field type.
                                                    object fieldValue = null;
                                                    switch (reader.Name)
                                                    {
                                                        case "u8": fieldValue = byte.Parse(reader.GetAttribute("value")); break;
                                                        case "u16": fieldValue = ushort.Parse(reader.GetAttribute("value")); break;
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
                                                        default:
                                                            {
                                                                // Unsupported node type.
                                                                throw new Exception("Unsupported node type");
                                                            }
                                                    }

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

            // Close the xml reader and return the item layout object.
            reader.Close();
            return layout;
        }

        #endregion

        #region IRenderable

        public override bool InitializeGraphics(RenderManager manager)
        {
            // Allocate arrays.
            this.itemPlacements = new ItemPlacementGizmo[this.layoutInfoList.Count];
            this.itemModels = new GameResource[this.layoutInfoList.Count];
            this.itemSpawnNames = new List<string>();

            // If there is at least one spawn set the initial spawn index.
            if (this.layoutInfoList.Count > 0)
                this.selectedSpawnIndex = 0;

            // Initialize the item placements array.
            for (int i = 0; i < this.itemPlacements.Length; i++)
            {
                // Check if there is a model for this item id we can render.
                if (this.layoutInfoList[i].ItemNo < GameItems.StockGameItems.Length && GameItems.StockGameItems[this.layoutInfoList[i].ItemNo].FilePath != "")
                {
                    // Get the model instance from the manager.
                    string fileName = GameItems.StockGameItems[this.layoutInfoList[i].ItemNo].FilePath.Replace("arc\\rom\\", "model\\") + ".rModel";
                    GameResource resource = manager.GetResourceFromFileName(fileName);
                    if (resource != null)
                    {
                        // Add the resource to the list.
                        this.itemModels[i] = resource;
                    }
                    else
                    {

                    }
                }
                else if (this.layoutInfoList[i].ResourceName != string.Empty && this.layoutInfoList[i].ResourceName != "NULL")
                {
                    // Try to load the model by file name.
                    GameResource resource = manager.GetResourceFromFileName(this.layoutInfoList[i].ResourceName + ".rModel");
                    if (resource != null)
                    {
                        // Add the resource to the list.
                        this.itemModels[i] = resource;
                    }
                    else
                    {

                    }
                }

                // Compute the placement rotation.
                Matrix rotX = Matrix.RotationX(this.layoutInfoList[i].ModelAngle.X);
                Matrix rotY = Matrix.RotationY(this.layoutInfoList[i].ModelAngle.Y);
                Matrix rotZ = Matrix.RotationZ(this.layoutInfoList[i].ModelAngle.Z);
                Quaternion rotation = Quaternion.RotationMatrix(rotX * rotY * rotZ);

                // Initialize the placement gizmo.
                this.itemPlacements[i] = new ItemPlacementGizmo(this.layoutInfoList[i].CursorWorldPos + this.layoutInfoList[i].CursorWorldPosOffs, 
                    this.layoutInfoList[i].ModelAngle, (rModel)this.itemModels[i]);
                this.itemPlacements[i].InitializeGraphics(manager);

                // Format the item name for the object properties window.
                string itemName = "Item " + i.ToString();
                if (this.layoutInfoList[i].ItemNo < GameItems.StockGameItems.Length)
                    itemName += " - " + GameItems.StockGameItems[this.layoutInfoList[i].ItemNo].DisplayName;

                this.itemSpawnNames.Add(itemName);
            }

            // Initialized successfully.
            return true;
        }

        public override bool DrawFrame(RenderManager manager)
        {
            // Loop and draw each of the item spawn spheres.
            for (int i = 0; i < this.layoutInfoList.Count; i++)
            {
                // If there is a game resource for this item render it, otherwise render the sphere.
                if (this.itemModels[i] != null)
                {
                    // Update the model position and rotation based on the placement info.
                    rModel model = (rModel)this.itemModels[i];
                    model.modelPosition = new Vector4(this.layoutInfoList[i].CursorWorldPos + this.layoutInfoList[i].CursorWorldPosOffs, 1.0f);

                    Matrix rotX = Matrix.RotationX(this.layoutInfoList[i].ModelAngle.X);
                    Matrix rotY = Matrix.RotationY(this.layoutInfoList[i].ModelAngle.Y);
                    Matrix rotZ = Matrix.RotationZ(this.layoutInfoList[i].ModelAngle.Z);
                    model.modelRotation = Quaternion.RotationMatrix(rotX * rotY * rotZ).ToVector4();

                    // TODO: Create a transformation matrix for the instance data.
                    Matrix itemTransform = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, model.modelRotation.ToQuaternion(), model.modelPosition.ToVector3());

                    //// Perform a clipping test to see if we should cull this item or now.
                    //if (manager.ViewFrustumBoundingBox.ClipTest(Vector4.Transform(model.header.BoundingBoxMin, itemTransform).ToVector3(), Vector4.Transform(model.header.BoundingBoxMax, itemTransform).ToVector3()) == false)
                    //    continue;

                    // Draw the model.
                    model.DrawFrame(manager);
                }

                // Draw a sphere for the item placement.
                //this.itemPlacements[i].IsFocused = this.hoveredSpawnIndex == i || this.pickedSpawnIndex == i;
                //this.itemPlacements[i].ShowRotationCircles = this.pickedSpawnIndex == i;
                this.itemPlacements[i].DrawFrame(manager);
            }

            return true;
        }

        public override void DrawObjectPropertiesUI(RenderManager manager)
        {
            // Reset the hovered spawn item index.
            this.hoveredSpawnIndex = -1;

            // Draw the combo box for item selection.
            string previewString = this.selectedSpawnIndex != -1 ? this.itemSpawnNames[this.selectedSpawnIndex] : "";
            if (ImGui.BeginCombo("Item Spawn", previewString) == true)
            {
                // Add option entries for each spawn.
                for (int i = 0; i < this.itemSpawnNames.Count; i++)
                {
                    // Add an option for the item spawn.
                    bool isSelected = this.selectedSpawnIndex == i;
                    if (ImGui.Selectable(this.itemSpawnNames[i], isSelected) == true)
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

            // If the selected spawn index is set show the spawn properties.
            if (this.selectedSpawnIndex != -1)
            {
                // Display item spawn properties.
                ImGui.InputScalarInt8("SHAPE", ref this.layoutInfoList[this.selectedSpawnIndex].Shape);
                ImGui.InputFloat("p->mRectX", ref this.layoutInfoList[this.selectedSpawnIndex].RectX);
                ImGui.InputFloat("p->mRectY", ref this.layoutInfoList[this.selectedSpawnIndex].RectY);
                ImGui.InputFloat("p->mRadius", ref this.layoutInfoList[this.selectedSpawnIndex].Radius);
                ImGui.InputFloat3("p->mAngle", ref this.layoutInfoList[this.selectedSpawnIndex].Angle);
                ImGui.InputFloat3("p->mRangeAngle", ref this.layoutInfoList[this.selectedSpawnIndex].RangeAngle);
                ImGui.InputFloat3("p->mModelAngle", ref this.layoutInfoList[this.selectedSpawnIndex].ModelAngle);
                ImGui.InputFloat3("p->mCursorWorldPos", ref this.layoutInfoList[this.selectedSpawnIndex].CursorWorldPos);
                ImGui.InputFloat3("p->mCursorWorldPosOffs", ref this.layoutInfoList[this.selectedSpawnIndex].CursorWorldPosOffs);
                ImGui.InputScalarUInt16("p->mMessNo", ref this.layoutInfoList[this.selectedSpawnIndex].MessageNo);
                ImGui.InputScalarUInt16("p->mMessType", ref this.layoutInfoList[this.selectedSpawnIndex].MessageType);
                ImGui.InputFloat3("p->mMessOffs", ref this.layoutInfoList[this.selectedSpawnIndex].MessageOffset);
                ImGui.InputScalarUInt32("CHECK", ref this.layoutInfoList[this.selectedSpawnIndex].Check);
                ImGui.InputScalarUInt32("Item No", ref this.layoutInfoList[this.selectedSpawnIndex].ItemNo);
                ImGui.InputScalarUInt32("Item Id", ref this.layoutInfoList[this.selectedSpawnIndex].ItemId);
                ImGui.InputScalarUInt32("Item Unique", ref this.layoutInfoList[this.selectedSpawnIndex].ItemUnique);
                ImGui.InputScalarUInt32("Item Set Type", ref this.layoutInfoList[this.selectedSpawnIndex].ItemSetType);
                ImGui.InputScalarUInt32("Item Photo Id", ref this.layoutInfoList[this.selectedSpawnIndex].ItemPhotoId);
                ImGui.InputScalarUInt32("Change Item No", ref this.layoutInfoList[this.selectedSpawnIndex].ChangeItemNo);
                ImGui.InputScalarUInt32("Change Item Id", ref this.layoutInfoList[this.selectedSpawnIndex].ChangeItemId);
                ImGui.InputScalarUInt32("Change Item Set Type", ref this.layoutInfoList[this.selectedSpawnIndex].ChangeItemSetType);
                ImGui.InputScalarUInt32("Broken Flag", ref this.layoutInfoList[this.selectedSpawnIndex].BrokenFlag);
                ImGui.InputScalarUInt32("Broken Check Flag", ref this.layoutInfoList[this.selectedSpawnIndex].BrokenCheckFlag);
                ImGui.InputScalarUInt32("Event System", ref this.layoutInfoList[this.selectedSpawnIndex].EventSystem);
                ImGui.InputScalarUInt32("Event Main", ref this.layoutInfoList[this.selectedSpawnIndex].EventMain);
                ImGui.InputScalarUInt32("Event Main 2", ref this.layoutInfoList[this.selectedSpawnIndex].EventMain2);
                ImGui.InputScalarUInt32("Event Set", ref this.layoutInfoList[this.selectedSpawnIndex].EventSet);
                ImGui.InputScalarUInt32("Event Door", ref this.layoutInfoList[this.selectedSpawnIndex].EventDoor);
                ImGui.InputScalarUInt32("Event Mess", ref this.layoutInfoList[this.selectedSpawnIndex].EventMessage);
                ImGui.InputScalarUInt32("Event Todo", ref this.layoutInfoList[this.selectedSpawnIndex].EventTodo);
                ImGui.InputScalarUInt32("Event Tutorial", ref this.layoutInfoList[this.selectedSpawnIndex].EventTutorial);
                ImGui.InputScalarUInt32("Event Tutorial 2", ref this.layoutInfoList[this.selectedSpawnIndex].EventTutorial2);
                ImGui.InputScalarUInt32("Event Broke", ref this.layoutInfoList[this.selectedSpawnIndex].EventBroke);
                ImGui.InputScalarUInt32("Event Photo", ref this.layoutInfoList[this.selectedSpawnIndex].EventPhoto);
                ImGui.InputScalarUInt32("Event Appear", ref this.layoutInfoList[this.selectedSpawnIndex].EventAppear);
                ImGui.InputScalarUInt32("Event Die", ref this.layoutInfoList[this.selectedSpawnIndex].EventDie);
                ImGui.InputScalarUInt32("Event Waiting", ref this.layoutInfoList[this.selectedSpawnIndex].EventWaiting);
                ImGui.InputScalarUInt32("Event Treasure", ref this.layoutInfoList[this.selectedSpawnIndex].EventTreasure);
                ImGui.InputScalarUInt32("Event Case", ref this.layoutInfoList[this.selectedSpawnIndex].EventCase);
                ImGui.InputScalarUInt32("Message", ref this.layoutInfoList[this.selectedSpawnIndex].Message);
                ImGui.InputScalarUInt32("One Set Flag 0", ref this.layoutInfoList[this.selectedSpawnIndex].OneSetFlag0);
                ImGui.InputScalarUInt32("One Set Flag 1", ref this.layoutInfoList[this.selectedSpawnIndex].OneSetFlag1);
                ImGui.InputScalarUInt32("One Set Flag 2", ref this.layoutInfoList[this.selectedSpawnIndex].OneSetFlag2);
                ImGui.InputScalarUInt32("One Set Flag 3", ref this.layoutInfoList[this.selectedSpawnIndex].OneSetFlag3);
                ImGui.InputText("p->className", ref this.layoutInfoList[this.selectedSpawnIndex].ClassName, 0);
                ImGui.InputText("p->resourceName", ref this.layoutInfoList[this.selectedSpawnIndex].ResourceName, 0);
                ImGui.InputScalarUInt32("Mob Set Type", ref this.layoutInfoList[this.selectedSpawnIndex].MobSetType);
                ImGui.InputScalarUInt32("Mob Set X", ref this.layoutInfoList[this.selectedSpawnIndex].MobSetX);
                ImGui.InputScalarUInt32("Mob Set Y", ref this.layoutInfoList[this.selectedSpawnIndex].MobSetY);
                ImGui.InputScalarUInt32("Mob Set Z", ref this.layoutInfoList[this.selectedSpawnIndex].MobSetZ);
                ImGui.InputFloat("Mob Adjust X", ref this.layoutInfoList[this.selectedSpawnIndex].MobAdjustX);
                ImGui.InputFloat("Mob Adjust Y", ref this.layoutInfoList[this.selectedSpawnIndex].MobAdjustY);
                ImGui.InputFloat("Mob Adjust Z", ref this.layoutInfoList[this.selectedSpawnIndex].MobAdjustZ);
                ImGui.InputScalarUInt32("Hosei Joint No", ref this.layoutInfoList[this.selectedSpawnIndex].HoseiJointNo);
                ImGui.InputFloat3("Hosei Offset Pos", ref this.layoutInfoList[this.selectedSpawnIndex].HoseiOffsetPos);
                ImGui.InputFloat("Hosei Radius", ref this.layoutInfoList[this.selectedSpawnIndex].HoseiRadius);
                ImGui.InputFloat("Hosei Height", ref this.layoutInfoList[this.selectedSpawnIndex].HoseiHeight);
                ImGui.InputScalarUInt32("Act Type", ref this.layoutInfoList[this.selectedSpawnIndex].ActType);
                ImGui.InputScalarUInt32("Bank No", ref this.layoutInfoList[this.selectedSpawnIndex].BankNo);
                ImGui.InputScalarUInt32("Motion No", ref this.layoutInfoList[this.selectedSpawnIndex].MotionNo);
                ImGui.InputScalarUInt32("Icon No", ref this.layoutInfoList[this.selectedSpawnIndex].IconNo);
                ImGui.InputScalarUInt32("Scr No 1", ref this.layoutInfoList[this.selectedSpawnIndex].ScrNo1);
                ImGui.InputScalarUInt32("Scr No 2", ref this.layoutInfoList[this.selectedSpawnIndex].ScrNo2);
            }
        }

        public override void CleanupGraphics(RenderManager manager)
        {
        }

        public override bool DoClippingTest(RenderManager manager, FastBoundingBox viewBox)
        {
            // Always return true since we we handle clipping in the DrawFrame function.
            return true;
        }

        #endregion

        #region IPickableObject

        public bool DoPickingTest(RenderManager manager, Ray pickingRay, out float distance, out object context)
        {
            bool result = false;
            int closestObjectIndex = -1;
            float objectDistance = float.MaxValue;

            // Loop through all of the items in the list.
            for (int i = 0; i < this.layoutInfoList.Count; i++)
            {
                // Make sure there is a game resource for this spawn.
                if (this.itemModels[i] == null)
                    continue;

                // Calculate the world matrix for the object.
                Vector3 itemPosition = this.layoutInfoList[i].CursorWorldPos + this.layoutInfoList[i].CursorWorldPosOffs;
                Matrix rotX = Matrix.RotationX(this.layoutInfoList[i].ModelAngle.X);
                Matrix rotY = Matrix.RotationY(this.layoutInfoList[i].ModelAngle.Y);
                Matrix rotZ = Matrix.RotationZ(this.layoutInfoList[i].ModelAngle.Z);
                Matrix itemTransform = Matrix.Transformation(Vector3.Zero, Quaternion.Zero, Vector3.One, Vector3.Zero, Quaternion.RotationMatrix(rotX * rotY * rotZ), itemPosition);

                // Get the model instance.
                rModel model = (rModel)this.itemModels[i];

                // Perform a clipping test to see if we should perform ray tracing.
                //if (manager.ViewFrustumBoundingBox.ClipTest(Vector4.Transform(model.header.BoundingBoxMin, itemTransform).ToVector3(), Vector4.Transform(model.header.BoundingBoxMax, itemTransform).ToVector3()) == false)
                //    continue;

                // Invert the item transformation matrix and convert the picking ray to world space units for the game model.
                itemTransform.Invert();
                Ray objectPickingRay = new Ray(Vector3.TransformCoordinate(pickingRay.Position, itemTransform), Vector3.TransformNormal(pickingRay.Direction, itemTransform));
                objectPickingRay.Direction.Normalize();

                // Check if the picking ray intersects the bounding box of the model.
                if (objectPickingRay.Intersects(new SharpDX.BoundingBox(model.header.BoundingBoxMin.ToVector3(), model.header.BoundingBoxMax.ToVector3())) == true)
                {
                    // Check if this model is closer than any model we found previously.
                    if (model.header.BoundingBoxMin.Z < objectDistance)
                    {
                        // Save the new closest model.
                        closestObjectIndex = i;
                        objectDistance = model.header.BoundingBoxMin.Z;
                    }

                    // Flag that we found at least one object.
                    result = true;
                }
            }

            // Return the picking result.
            distance = objectDistance;
            context = closestObjectIndex;
            return result;
        }

        public void SelectObject(RenderManager manager, object context)
        {
            // Make sure the object index is valid.
            int objectIndex = (int)context;
            if (objectIndex < 0 || objectIndex >= this.itemPlacements.Length)
                throw new IndexOutOfRangeException("Item spawn index is invalid!");

            // Select the object.
            if (this.selectedSpawnIndices.Add(objectIndex) == true)
            {
                // Update selection properties for the object.
                this.itemPlacements[objectIndex].IsFocused = true;
                this.itemPlacements[objectIndex].ShowRotationCircles = true;
            }
            else
            {
                // The object is already selected, if there is more than 1 selected object and the control key is being held then deselect the object.
                if (this.selectedSpawnIndices.Count > 1 && manager.InputManager.KeyboardState[(int)Keys.ControlKey] == true)
                {
                    // Deselect the object.
                    DeselectObject(manager, context);
                }
            }
        }

        public bool DeselectObject(RenderManager manager, object context)
        {
            // If the context parameter is not null treat it as the object index to deselect, else deselect all selected objects.
            if (context != null)
            {
                // Deselect the object specified by the context parameter.
                int objectIndex = (int)context;
                this.selectedSpawnIndices.Remove(objectIndex);

                this.itemPlacements[objectIndex].IsFocused = false;
                this.itemPlacements[objectIndex].ShowRotationCircles = false;
            }
            else
            {
                // Deselect all objects.
                foreach (int objectIndex in this.selectedSpawnIndices)
                {
                    // Deselect and remove object from select objects list.
                    this.itemPlacements[objectIndex].IsFocused = false;
                    this.itemPlacements[objectIndex].ShowRotationCircles = false;
                }

                // Clear the selected spawns set.
                this.selectedSpawnIndices.Clear();
            }

            // Return true if all child objects have been deselected.
            return this.selectedSpawnIndices.Count == 0;
        }

        public bool HandleInput(RenderManager manager)
        {
            bool inputHandled = false;

            // Loop through all the selected items and handle input.
            foreach (int spawnIndex in this.selectedSpawnIndices)
            {
                // Handle input accordingly.
                if (manager.InputManager.ButtonState[(int)InputAction.LeftClick] == true)
                {
                    // TODO:
                    inputHandled = true;
                }
                else
                {
                    // Check for movement speed modifier.
                    float speedModifier = 1.0f;
                    if (manager.InputManager.KeyboardState[(int)Keys.ShiftKey] == true)
                        speedModifier = 10.0f;

                    // Check for keyboard input.
                    if (manager.InputManager.KeyboardState[(int)Keys.Up] == true)
                    {
                        this.itemPlacements[spawnIndex].Position += manager.Camera.CamForward * (Vector3.UnitX + Vector3.UnitZ) * speedModifier;
                        inputHandled = true;
                    }
                    if (manager.InputManager.KeyboardState[(int)Keys.Down] == true)
                    {
                        this.itemPlacements[spawnIndex].Position += manager.Camera.CamBackward * (Vector3.UnitX + Vector3.UnitZ) * speedModifier;
                        inputHandled = true;
                    }
                    if (manager.InputManager.KeyboardState[(int)Keys.Left] == true)
                    {
                        this.itemPlacements[spawnIndex].Position += manager.Camera.CamLeft * (Vector3.UnitX + Vector3.UnitZ) * speedModifier;
                        inputHandled = true;
                    }
                    if (manager.InputManager.KeyboardState[(int)Keys.Right] == true)
                    {
                        this.itemPlacements[spawnIndex].Position += manager.Camera.CamRight * (Vector3.UnitX + Vector3.UnitZ) * speedModifier;
                        inputHandled = true;
                    }
                    if (manager.InputManager.KeyboardState[(int)Keys.PageUp] == true)
                    {
                        this.itemPlacements[spawnIndex].Position += Vector3.UnitY * speedModifier;
                        inputHandled = true;
                    }
                    if (manager.InputManager.KeyboardState[(int)Keys.PageDown] == true)
                    {
                        this.itemPlacements[spawnIndex].Position -= Vector3.UnitY * speedModifier;
                        inputHandled = true;
                    }
                }

                // Update the layout info for this spawn.
                this.layoutInfoList[spawnIndex].CursorWorldPos = this.itemPlacements[spawnIndex].Position;
                this.layoutInfoList[spawnIndex].CursorWorldPosOffs = Vector3.Zero;
            }

            // Return the result.
            return inputHandled;
        }

        #endregion
    }
}
