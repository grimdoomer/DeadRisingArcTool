namespace DeadRisingArcTool
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dEBUGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchExtractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.texturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.treeViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortByToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.arcFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resourceTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileInfoBox = new System.Windows.Forms.GroupBox();
            this.lblFileType = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblArcFile = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblDecompressedSize = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCompressedSize = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblOffset = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.restoreBackupsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.treeViewContextMenu.SuspendLayout();
            this.FileInfoBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dEBUGToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(891, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "Close";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // dEBUGToolStripMenuItem
            // 
            this.dEBUGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.batchExtractToolStripMenuItem,
            this.restoreBackupsToolStripMenuItem});
            this.dEBUGToolStripMenuItem.Name = "dEBUGToolStripMenuItem";
            this.dEBUGToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.dEBUGToolStripMenuItem.Text = "DEBUG";
            // 
            // batchExtractToolStripMenuItem
            // 
            this.batchExtractToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.texturesToolStripMenuItem});
            this.batchExtractToolStripMenuItem.Name = "batchExtractToolStripMenuItem";
            this.batchExtractToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.batchExtractToolStripMenuItem.Text = "Batch Extract";
            // 
            // texturesToolStripMenuItem
            // 
            this.texturesToolStripMenuItem.Name = "texturesToolStripMenuItem";
            this.texturesToolStripMenuItem.Size = new System.Drawing.Size(118, 22);
            this.texturesToolStripMenuItem.Text = "Textures";
            this.texturesToolStripMenuItem.Click += new System.EventHandler(this.texturesToolStripMenuItem_Click);
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.ContextMenuStrip = this.treeViewContextMenu;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(3, 3);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(291, 666);
            this.treeView1.TabIndex = 1;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // treeViewContextMenu
            // 
            this.treeViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem,
            this.sortByToolStripMenuItem,
            this.renderToolStripMenuItem});
            this.treeViewContextMenu.Name = "treeViewContextMenu";
            this.treeViewContextMenu.Size = new System.Drawing.Size(112, 70);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.extractToolStripMenuItem.Text = "Extract";
            this.extractToolStripMenuItem.Click += new System.EventHandler(this.extractToolStripMenuItem_Click);
            // 
            // sortByToolStripMenuItem
            // 
            this.sortByToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNameToolStripMenuItem,
            this.arcFileToolStripMenuItem,
            this.resourceTypeToolStripMenuItem});
            this.sortByToolStripMenuItem.Name = "sortByToolStripMenuItem";
            this.sortByToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.sortByToolStripMenuItem.Text = "Sort By";
            // 
            // fileNameToolStripMenuItem
            // 
            this.fileNameToolStripMenuItem.Name = "fileNameToolStripMenuItem";
            this.fileNameToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.fileNameToolStripMenuItem.Text = "File Name";
            this.fileNameToolStripMenuItem.Click += new System.EventHandler(this.fileNameToolStripMenuItem_Click);
            // 
            // arcFileToolStripMenuItem
            // 
            this.arcFileToolStripMenuItem.Name = "arcFileToolStripMenuItem";
            this.arcFileToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.arcFileToolStripMenuItem.Text = "Arc File";
            this.arcFileToolStripMenuItem.Click += new System.EventHandler(this.arcFileToolStripMenuItem_Click);
            // 
            // resourceTypeToolStripMenuItem
            // 
            this.resourceTypeToolStripMenuItem.Name = "resourceTypeToolStripMenuItem";
            this.resourceTypeToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.resourceTypeToolStripMenuItem.Text = "Resource Type";
            this.resourceTypeToolStripMenuItem.Click += new System.EventHandler(this.resourceTypeToolStripMenuItem_Click);
            // 
            // renderToolStripMenuItem
            // 
            this.renderToolStripMenuItem.Name = "renderToolStripMenuItem";
            this.renderToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.renderToolStripMenuItem.Text = "Render";
            this.renderToolStripMenuItem.Click += new System.EventHandler(this.renderToolStripMenuItem_Click);
            // 
            // FileInfoBox
            // 
            this.FileInfoBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileInfoBox.Controls.Add(this.lblFileType);
            this.FileInfoBox.Controls.Add(this.label5);
            this.FileInfoBox.Controls.Add(this.lblArcFile);
            this.FileInfoBox.Controls.Add(this.label4);
            this.FileInfoBox.Controls.Add(this.lblDecompressedSize);
            this.FileInfoBox.Controls.Add(this.label3);
            this.FileInfoBox.Controls.Add(this.lblCompressedSize);
            this.FileInfoBox.Controls.Add(this.label2);
            this.FileInfoBox.Controls.Add(this.lblOffset);
            this.FileInfoBox.Controls.Add(this.label1);
            this.FileInfoBox.Location = new System.Drawing.Point(8, 3);
            this.FileInfoBox.Name = "FileInfoBox";
            this.FileInfoBox.Size = new System.Drawing.Size(570, 139);
            this.FileInfoBox.TabIndex = 2;
            this.FileInfoBox.TabStop = false;
            this.FileInfoBox.Text = "File Info";
            // 
            // lblFileType
            // 
            this.lblFileType.AutoSize = true;
            this.lblFileType.Location = new System.Drawing.Point(125, 114);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(0, 13);
            this.lblFileType.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "File Type:";
            // 
            // lblArcFile
            // 
            this.lblArcFile.AutoSize = true;
            this.lblArcFile.Location = new System.Drawing.Point(125, 26);
            this.lblArcFile.Name = "lblArcFile";
            this.lblArcFile.Size = new System.Drawing.Size(0, 13);
            this.lblArcFile.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Arc File:";
            // 
            // lblDecompressedSize
            // 
            this.lblDecompressedSize.AutoSize = true;
            this.lblDecompressedSize.Location = new System.Drawing.Point(125, 92);
            this.lblDecompressedSize.Name = "lblDecompressedSize";
            this.lblDecompressedSize.Size = new System.Drawing.Size(0, 13);
            this.lblDecompressedSize.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Decompressed Size:";
            // 
            // lblCompressedSize
            // 
            this.lblCompressedSize.AutoSize = true;
            this.lblCompressedSize.Location = new System.Drawing.Point(125, 70);
            this.lblCompressedSize.Name = "lblCompressedSize";
            this.lblCompressedSize.Size = new System.Drawing.Size(0, 13);
            this.lblCompressedSize.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Compressed Size:";
            // 
            // lblOffset
            // 
            this.lblOffset.AutoSize = true;
            this.lblOffset.Location = new System.Drawing.Point(125, 48);
            this.lblOffset.Name = "lblOffset";
            this.lblOffset.Size = new System.Drawing.Size(0, 13);
            this.lblOffset.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Offset:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.FileInfoBox);
            this.splitContainer1.Size = new System.Drawing.Size(891, 672);
            this.splitContainer1.SplitterDistance = 297;
            this.splitContainer1.TabIndex = 3;
            // 
            // restoreBackupsToolStripMenuItem
            // 
            this.restoreBackupsToolStripMenuItem.Name = "restoreBackupsToolStripMenuItem";
            this.restoreBackupsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.restoreBackupsToolStripMenuItem.Text = "Restore Backups";
            this.restoreBackupsToolStripMenuItem.Click += new System.EventHandler(this.restoreBackupsToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 696);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ArcTool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.treeViewContextMenu.ResumeLayout(false);
            this.FileInfoBox.ResumeLayout(false);
            this.FileInfoBox.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.GroupBox FileInfoBox;
        private System.Windows.Forms.Label lblDecompressedSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCompressedSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblOffset;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblArcFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ContextMenuStrip treeViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem sortByToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem arcFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resourceTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dEBUGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchExtractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem texturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreBackupsToolStripMenuItem;
    }
}

