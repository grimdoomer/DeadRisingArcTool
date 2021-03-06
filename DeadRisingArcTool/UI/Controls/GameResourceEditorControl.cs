﻿using DeadRisingArcTool.FileFormats;
using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.Controls
{
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<GameResourceEditorControl, UserControl>))]
    public abstract class GameResourceEditorControl : UserControl
    {
        /// <summary>
        /// Owner of this resource editor
        /// </summary>
        public IResourceEditorOwner EditorOwner { get; set; }

        /// <summary>
        /// Game resource the editor is currently displaying.
        /// </summary>
        public GameResource GameResource { get; protected set; }

        /// <summary>
        /// Arc file that contains <see cref="GameResource"/>
        /// </summary>
        public Archive ArcFile { get; protected set; }

        /// <summary>
        /// Indicates if the resource data has been modified since it was loaded
        /// </summary>
        public bool HasBeenModified { get; protected set; }

        /// <summary>
        /// Determins if this editor can edit the specified resource type
        /// </summary>
        /// <param name="resource">Resource type to open in editor</param>
        /// <returns>True if this editor is capable of editing the specified resource type, false otherwise</returns>
        public virtual bool CanEditResource(ResourceType resource)
        {
            // Get the GameResourceEditorAttribute attached to this control.
            object[] attributes = this.GetType().GetCustomAttributes(typeof(GameResourceEditorAttribute), false);
            if (attributes == null || attributes.Length != 1)
            {
                // Control must contain a GameResourceEditorAttribute if it extends this class.
                throw new InvalidOperationException("Editor control must have a GameResourceEditorAttribute if it extends GameResourceEditorControl");
            }

            // Cast the attribute to the correct type and check if it supports this resource type.
            GameResourceEditorAttribute attr = (GameResourceEditorAttribute)attributes[0];
            return attr.ResourceTypes.Contains(resource);
        }

        public void UpdateResource(Archive arcFile, GameResource gameResource)
        {
            // Update our tracked arc file and game resource.
            this.GameResource = gameResource;
            this.ArcFile = arcFile;

            // Call to the GUI layer to update.
            OnGameResourceUpdated();
        }

        /// <summary>
        /// Called when the resource editor should save any changes made back to the arc files.
        /// </summary>
        /// <returns>True if the file(s) were saved successfully, false otherwise</returns>
        public abstract bool SaveResource();

        /// <summary>
        /// Called when <see cref="GameResource"/> is changed so the UI can update.
        /// </summary>
        protected abstract void OnGameResourceUpdated();
    }
}
