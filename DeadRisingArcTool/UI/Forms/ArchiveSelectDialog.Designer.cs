namespace DeadRisingArcTool.Forms
{
    partial class ArchiveSelectDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lstArchives = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnLoadArchives = new System.Windows.Forms.Button();
            this.listViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.treeViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newArchiveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.listViewContextMenu.SuspendLayout();
            this.treeViewContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstArchives
            // 
            this.lstArchives.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstArchives.CheckBoxes = true;
            this.lstArchives.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstArchives.ContextMenuStrip = this.listViewContextMenu;
            this.lstArchives.FullRowSelect = true;
            this.lstArchives.GridLines = true;
            this.lstArchives.HideSelection = false;
            this.lstArchives.Location = new System.Drawing.Point(12, 11);
            this.lstArchives.Name = "lstArchives";
            this.lstArchives.Size = new System.Drawing.Size(453, 426);
            this.lstArchives.TabIndex = 0;
            this.lstArchives.UseCompatibleStateImageBehavior = false;
            this.lstArchives.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Archive";
            this.columnHeader1.Width = 449;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(12, 443);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(131, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnLoadArchives
            // 
            this.btnLoadArchives.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoadArchives.Location = new System.Drawing.Point(149, 443);
            this.btnLoadArchives.Name = "btnLoadArchives";
            this.btnLoadArchives.Size = new System.Drawing.Size(316, 23);
            this.btnLoadArchives.TabIndex = 2;
            this.btnLoadArchives.Text = "Load Selected";
            this.btnLoadArchives.UseVisualStyleBackColor = true;
            this.btnLoadArchives.Click += new System.EventHandler(this.btnLoadArchives_Click);
            // 
            // listViewContextMenu
            // 
            this.listViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newArchiveToolStripMenuItem,
            this.toolStripSeparator1,
            this.selectAllToolStripMenuItem,
            this.clearAllToolStripMenuItem});
            this.listViewContextMenu.Name = "listViewContextMenu";
            this.listViewContextMenu.Size = new System.Drawing.Size(142, 76);
            // 
            // newArchiveToolStripMenuItem
            // 
            this.newArchiveToolStripMenuItem.Name = "newArchiveToolStripMenuItem";
            this.newArchiveToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.newArchiveToolStripMenuItem.Text = "New Archive";
            this.newArchiveToolStripMenuItem.Click += new System.EventHandler(this.newArchiveToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(138, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // clearAllToolStripMenuItem
            // 
            this.clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            this.clearAllToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.clearAllToolStripMenuItem.Text = "Clear All";
            this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.clearAllToolStripMenuItem_Click);
            // 
            // treeView1
            // 
            this.treeView1.ContextMenuStrip = this.treeViewContextMenu;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(12, 11);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(453, 426);
            this.treeView1.TabIndex = 5;
            // 
            // treeViewContextMenu
            // 
            this.treeViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newArchiveToolStripMenuItem1});
            this.treeViewContextMenu.Name = "treeViewContextMenu";
            this.treeViewContextMenu.Size = new System.Drawing.Size(181, 48);
            // 
            // newArchiveToolStripMenuItem1
            // 
            this.newArchiveToolStripMenuItem1.Name = "newArchiveToolStripMenuItem1";
            this.newArchiveToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.newArchiveToolStripMenuItem1.Text = "New Archive";
            this.newArchiveToolStripMenuItem1.Click += new System.EventHandler(this.newArchiveToolStripMenuItem_Click);
            // 
            // ArchiveSelectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 476);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lstArchives);
            this.Controls.Add(this.btnLoadArchives);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ArchiveSelectDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Archive Select";
            this.Load += new System.EventHandler(this.ArchiveSelectDialog_Load);
            this.listViewContextMenu.ResumeLayout(false);
            this.treeViewContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lstArchives;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnLoadArchives;
        private System.Windows.Forms.ContextMenuStrip listViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem newArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllToolStripMenuItem;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ContextMenuStrip treeViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem newArchiveToolStripMenuItem1;
    }
}