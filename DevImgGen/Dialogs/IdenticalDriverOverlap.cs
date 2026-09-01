// Decompiled with JetBrains decompiler
// Type: DevImgGen.Dialogs.IdenticalDriverOverlap
// Assembly: DevImgGen, Version=2021.1.31.1000, Culture=neutral, PublicKeyToken=null
// MVID: 8B415242-7DF5-46C1-83D1-ACA4F55A74B9
// Assembly location: C:\Users\Admin\Desktop\re\dig\DevImgGen.exe

using DigLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DevImgGen.Dialogs
{
  public class IdenticalDriverOverlap : Form
  {
    private DriverOverlap m_OverlapInfo;
    private bool m_Decided;
    private IContainer components;
    private Label lblGlyph;
    private Label lblInfo;
    private TextBox tbPrevHwIds;
    private Label lblCurrentIds;
    private TextBox tbNewHwIds;
    private Label lblQuestion;
    private Panel pnlOptions;
    private Panel pnlDivider;
    private Button btnKeepPrev;
    private Button btnKeepNew;
    private Label lblPrevIds;

    public IdenticalDriverOverlap(DriverOverlap overlapInfo)
    {
      this.InitializeComponent();
      this.m_OverlapInfo = overlapInfo;
      this.lblInfo.Text = string.Format(this.lblInfo.Text, (object) overlapInfo.Name, (object) overlapInfo.Version);
      this.tbPrevHwIds.Text = string.Join(Environment.NewLine, (IEnumerable<string>) overlapInfo.PreviousHwIDs);
      this.tbNewHwIds.Text = string.Join(Environment.NewLine, (IEnumerable<string>) overlapInfo.NewHwIDs);
    }

    private void btnKeepNew_Click(object sender, EventArgs e)
    {
      this.m_OverlapInfo.Approve();
      this.m_Decided = true;
      this.Close();
    }

    private void btnKeepPrev_Click(object sender, EventArgs e)
    {
      this.m_OverlapInfo.Deny();
      this.m_Decided = true;
      this.Close();
    }

    private void OnFormClosing(object sender, FormClosingEventArgs e)
    {
      if (this.m_Decided)
        return;
      e.Cancel = true;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.lblGlyph = new Label();
      this.lblInfo = new Label();
      this.tbPrevHwIds = new TextBox();
      this.lblCurrentIds = new Label();
      this.tbNewHwIds = new TextBox();
      this.lblQuestion = new Label();
      this.pnlOptions = new Panel();
      this.btnKeepPrev = new Button();
      this.btnKeepNew = new Button();
      this.pnlDivider = new Panel();
      this.lblPrevIds = new Label();
      this.pnlOptions.SuspendLayout();
      this.SuspendLayout();
      this.lblGlyph.AutoSize = true;
      this.lblGlyph.Font = new Font("Segoe MDL2 Assets", 24f);
      this.lblGlyph.ForeColor = Color.FromArgb(54, 116, 178);
      this.lblGlyph.Location = new Point(12, 14);
      this.lblGlyph.Name = "lblGlyph";
      this.lblGlyph.Size = new Size(47, 32);
      this.lblGlyph.TabIndex = 0;
      this.lblGlyph.Text = "\uE7BA";
      this.lblInfo.Location = new Point(65, 14);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new Size(449, 41);
      this.lblInfo.TabIndex = 1;
      this.lblInfo.Text = "已处理过相同版本的 '{0}'（{1}）";
      this.tbPrevHwIds.Location = new Point(68, 74);
      this.tbPrevHwIds.Multiline = true;
      this.tbPrevHwIds.Name = "tbPrevHwIds";
      this.tbPrevHwIds.ScrollBars = ScrollBars.Both;
      this.tbPrevHwIds.Size = new Size(440, 100);
      this.tbPrevHwIds.TabIndex = 3;
      this.lblCurrentIds.AutoSize = true;
      this.lblCurrentIds.Location = new Point(65, 181);
      this.lblCurrentIds.Name = "lblCurrentIds";
      this.lblCurrentIds.Size = new Size(181, 15);
      this.lblCurrentIds.TabIndex = 4;
      this.lblCurrentIds.Text = "当前 INF 支持的硬件 ID:";
      this.tbNewHwIds.Location = new Point(68, 200);
      this.tbNewHwIds.Multiline = true;
      this.tbNewHwIds.Name = "tbNewHwIds";
      this.tbNewHwIds.ScrollBars = ScrollBars.Both;
      this.tbNewHwIds.Size = new Size(440, 100);
      this.tbNewHwIds.TabIndex = 5;
      this.lblQuestion.AutoSize = true;
      this.lblQuestion.Location = new Point(65, 307);
      this.lblQuestion.Name = "lblQuestion";
      this.lblQuestion.Size = new Size(189, 15);
      this.lblQuestion.TabIndex = 6;
      this.lblQuestion.Text = "你想保留哪个 INF？";
      this.pnlOptions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.pnlOptions.BackColor = Color.FromArgb(240, 240, 240);
      this.pnlOptions.Controls.Add((Control) this.btnKeepPrev);
      this.pnlOptions.Controls.Add((Control) this.btnKeepNew);
      this.pnlOptions.Location = new Point(0, 343);
      this.pnlOptions.Name = "pnlOptions";
      this.pnlOptions.Size = new Size(524, 40);
      this.pnlOptions.TabIndex = 7;
      this.btnKeepPrev.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.btnKeepPrev.DialogResult = DialogResult.Cancel;
      this.btnKeepPrev.Location = new Point(396, 8);
      this.btnKeepPrev.Name = "btnKeepPrev";
      this.btnKeepPrev.Size = new Size(118, 23);
      this.btnKeepPrev.TabIndex = 1;
      this.btnKeepPrev.Text = "保留之前的 INF";
      this.btnKeepPrev.UseVisualStyleBackColor = true;
      this.btnKeepPrev.Click += new EventHandler(this.btnKeepPrev_Click);
      this.btnKeepNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.btnKeepNew.Location = new Point(272, 8);
      this.btnKeepNew.Name = "btnKeepNew";
      this.btnKeepNew.Size = new Size(118, 23);
      this.btnKeepNew.TabIndex = 0;
      this.btnKeepNew.Text = "使用当前 INF";
      this.btnKeepNew.UseVisualStyleBackColor = true;
      this.btnKeepNew.Click += new EventHandler(this.btnKeepNew_Click);
      this.pnlDivider.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.pnlDivider.BackColor = Color.FromArgb(223, 223, 223);
      this.pnlDivider.Location = new Point(0, 342);
      this.pnlDivider.Name = "pnlDivider";
      this.pnlDivider.Size = new Size(524, 1);
      this.pnlDivider.TabIndex = 8;
      this.lblPrevIds.AutoSize = true;
      this.lblPrevIds.Location = new Point(65, 55);
      this.lblPrevIds.Name = "lblPrevIds";
      this.lblPrevIds.Size = new Size(188, 15);
      this.lblPrevIds.TabIndex = 2;
      this.lblPrevIds.Text = "之前的 INF 支持的硬件 ID:";
      this.AcceptButton = (IButtonControl) this.btnKeepNew;
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.BackColor = SystemColors.ControlLightLight;
      this.CancelButton = (IButtonControl) this.btnKeepPrev;
      this.ClientSize = new Size(524, 383);
      this.Controls.Add((Control) this.lblPrevIds);
      this.Controls.Add((Control) this.pnlDivider);
      this.Controls.Add((Control) this.pnlOptions);
      this.Controls.Add((Control) this.lblQuestion);
      this.Controls.Add((Control) this.tbNewHwIds);
      this.Controls.Add((Control) this.lblCurrentIds);
      this.Controls.Add((Control) this.tbPrevHwIds);
      this.Controls.Add((Control) this.lblInfo);
      this.Controls.Add((Control) this.lblGlyph);
      this.Font = new Font("Segoe UI", 9f);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (IdenticalDriverOverlap);
      this.Text = "相同驱动程序重叠";
      this.FormClosing += new FormClosingEventHandler(this.OnFormClosing);
      this.pnlOptions.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
