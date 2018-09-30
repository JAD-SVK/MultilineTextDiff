namespace Atrip.MultilineTextDiff
{
  partial class MainDialogue
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.changeRichTextBox = new Atrip.Utils.GUI.PlainTextPasteRichTextBox();
      this.originalRichTextBox = new Atrip.Utils.GUI.PlainTextPasteRichTextBox();
      this.modifiedRichTextBox = new Atrip.Utils.GUI.PlainTextPasteRichTextBox();
      this.tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.ColumnCount = 3;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel.Controls.Add(this.changeRichTextBox, 2, 0);
      this.tableLayoutPanel.Controls.Add(this.originalRichTextBox, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.modifiedRichTextBox, 1, 0);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.Size = new System.Drawing.Size(1181, 427);
      this.tableLayoutPanel.TabIndex = 0;
      // 
      // changeRichTextBox
      // 
      this.changeRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.changeRichTextBox.Location = new System.Drawing.Point(789, 3);
      this.changeRichTextBox.Name = "changeRichTextBox";
      this.changeRichTextBox.ReadOnly = true;
      this.changeRichTextBox.Size = new System.Drawing.Size(389, 421);
      this.changeRichTextBox.TabIndex = 2;
      this.changeRichTextBox.Text = "";
      // 
      // originalRichTextBox
      // 
      this.originalRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.originalRichTextBox.Location = new System.Drawing.Point(3, 3);
      this.originalRichTextBox.MaxLength = 10240;
      this.originalRichTextBox.Name = "originalRichTextBox";
      this.originalRichTextBox.Size = new System.Drawing.Size(387, 421);
      this.originalRichTextBox.TabIndex = 0;
      this.originalRichTextBox.Text = "";
      this.originalRichTextBox.TextChanged += new System.EventHandler(this.Source_TextChanged);
      // 
      // modifiedRichTextBox
      // 
      this.modifiedRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.modifiedRichTextBox.Location = new System.Drawing.Point(396, 3);
      this.modifiedRichTextBox.MaxLength = 10240;
      this.modifiedRichTextBox.Name = "modifiedRichTextBox";
      this.modifiedRichTextBox.Size = new System.Drawing.Size(387, 421);
      this.modifiedRichTextBox.TabIndex = 1;
      this.modifiedRichTextBox.Text = "";
      this.modifiedRichTextBox.TextChanged += new System.EventHandler(this.Source_TextChanged);
      // 
      // MainDialogue
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1181, 427);
      this.Controls.Add(this.tableLayoutPanel);
      this.Name = "MainDialogue";
      this.Text = "Simple Multiline Text Comparer";
      this.Load += new System.EventHandler(this.MainDialogue_Load);
      this.tableLayoutPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private Atrip.Utils.GUI.PlainTextPasteRichTextBox changeRichTextBox;
    private Atrip.Utils.GUI.PlainTextPasteRichTextBox originalRichTextBox;
    private Atrip.Utils.GUI.PlainTextPasteRichTextBox modifiedRichTextBox;
  }
}

