namespace DeadRisingArcTool.Controls
{
    partial class TextEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextEditor));
            this.textbox = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.textbox)).BeginInit();
            this.SuspendLayout();
            // 
            // textbox
            // 
            this.textbox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textbox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.textbox.AutoIndentCharsPatterns = "";
            this.textbox.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.textbox.BackBrush = null;
            this.textbox.ChangedLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.textbox.CharHeight = 14;
            this.textbox.CharWidth = 8;
            this.textbox.CommentPrefix = null;
            this.textbox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textbox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textbox.IsReplaceMode = false;
            this.textbox.Language = FastColoredTextBoxNS.Language.XML;
            this.textbox.LeftBracket = '<';
            this.textbox.LeftBracket2 = '(';
            this.textbox.Location = new System.Drawing.Point(3, 3);
            this.textbox.Name = "textbox";
            this.textbox.Paddings = new System.Windows.Forms.Padding(0);
            this.textbox.RightBracket = '>';
            this.textbox.RightBracket2 = ')';
            this.textbox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textbox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textbox.ServiceColors")));
            this.textbox.Size = new System.Drawing.Size(436, 278);
            this.textbox.TabIndex = 0;
            this.textbox.Zoom = 100;
            this.textbox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.textbox_TextChanged);
            // 
            // TextEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textbox);
            this.Name = "TextEditor";
            this.Size = new System.Drawing.Size(442, 284);
            ((System.ComponentModel.ISupportInitialize)(this.textbox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox textbox;
    }
}
