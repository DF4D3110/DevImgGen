// Decompiled with JetBrains decompiler
// Type: DevImgGen.Pages.LandingPage
// Assembly: DevImgGen, Version=2021.1.31.1000, Culture=neutral, PublicKeyToken=null
// MVID: 8B415242-7DF5-46C1-83D1-ACA4F55A74B9
// Assembly location: C:\Users\Admin\Desktop\re\dig\DevImgGen.exe

using DevImgGen.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DevImgGen.Pages
{
  public class LandingPage : BasePage
  {
    private IContainer components;
    private CommandButton cmdCreateConfig;
    private Label lblHeader;
    private CommandButton cmdExportDrivers;
    private BackButton btnBack;
    private Label lblGlyph;
    private Label lblDisclaimer;
    private CommandButton cmdBuild;

    public LandingPage()
    {
      this.InitializeComponent();
      this.Dock = DockStyle.Fill;
    }

    private void cmdExportDrivers_Click(object sender, EventArgs e) => this.OnPageChangeRequested(PageEnum.Export);

    private void cmdCreateConfig_Click(object sender, EventArgs e) => this.OnPageChangeRequested(PageEnum.CreateConfig);

    private void cmdBuild_Click(object sender, EventArgs e) => this.OnPageChangeRequested(PageEnum.Build);

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.lblHeader = new Label();
      this.cmdCreateConfig = new CommandButton();
      this.cmdExportDrivers = new CommandButton();
      this.btnBack = new BackButton();
      this.lblGlyph = new Label();
      this.lblDisclaimer = new Label();
      this.cmdBuild = new CommandButton();
      this.SuspendLayout();
      this.lblHeader.AutoSize = true;
      this.lblHeader.Font = new Font("Segoe UI", 11.5f);
      this.lblHeader.ForeColor = Color.FromArgb(0, 51, 153);
      this.lblHeader.Location = new Point(40, 14);
      this.lblHeader.Name = "lblHeader";
      this.lblHeader.Size = new Size(199, 21);
      this.lblHeader.TabIndex = 6;
      this.lblHeader.Text = "你想做什么？";
      this.cmdCreateConfig.FlatStyle = FlatStyle.System;
      this.cmdCreateConfig.Location = new Point(43, 157);
      this.cmdCreateConfig.Name = "cmdCreateConfig";
      this.cmdCreateConfig.Note = "准备好一组驱动程序后，你需要创建配置包。这些配置包将用作构建设备镜像的输入。";
      this.cmdCreateConfig.Size = new Size(495, 85);
      this.cmdCreateConfig.TabIndex = 2;
      this.cmdCreateConfig.Text = "创建配置包";
      this.cmdCreateConfig.UseVisualStyleBackColor = true;
      this.cmdCreateConfig.Click += new EventHandler(this.cmdCreateConfig_Click);
      this.cmdExportDrivers.FlatStyle = FlatStyle.System;
      this.cmdExportDrivers.Location = new Point(43, 59);
      this.cmdExportDrivers.Name = "cmdExportDrivers";
      this.cmdExportDrivers.Note = "为你的设备构建镜像需要一组驱动程序。如果你还没有驱动程序，请选择此选项。";
      this.cmdExportDrivers.Size = new Size(495, 85);
      this.cmdExportDrivers.TabIndex = 1;
      this.cmdExportDrivers.Text = "从这台电脑导出驱动程序";
      this.cmdExportDrivers.UseVisualStyleBackColor = true;
      this.cmdExportDrivers.Click += new EventHandler(this.cmdExportDrivers_Click);
      this.btnBack.FlatAppearance.BorderSize = 0;
      this.btnBack.FlatStyle = FlatStyle.Flat;
      this.btnBack.ForeColor = Color.FromArgb(191, 191, 191);
      this.btnBack.Location = new Point(5, 14);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new Size(32, 25);
      this.btnBack.TabIndex = 7;
      this.btnBack.Text = "\uE830";
      this.btnBack.Useless = true;
      this.btnBack.UseVisualStyleBackColor = true;
      this.lblGlyph.AutoSize = true;
      this.lblGlyph.Font = new Font("Segoe MDL2 Assets", 24f);
      this.lblGlyph.ForeColor = Color.FromArgb(54, 116, 178);
      this.lblGlyph.Location = new Point(37, 362);
      this.lblGlyph.Name = "lblGlyph";
      this.lblGlyph.Size = new Size(47, 32);
      this.lblGlyph.TabIndex = 4;
      this.lblGlyph.Text = "\uE822";
      this.lblDisclaimer.Location = new Point(86, 364);
      this.lblDisclaimer.Name = "lblDisclaimer";
      this.lblDisclaimer.Size = new Size(448, 38);
      this.lblDisclaimer.TabIndex = 5;
      this.lblDisclaimer.Text = "你正在使用测试版的 Windows 设备镜像生成器。某些功能可能缺失或尚未完成。";
      this.cmdBuild.FlatStyle = FlatStyle.System;
      this.cmdBuild.Location = new Point(43, (int) byte.MaxValue);
      this.cmdBuild.Name = "cmdBuild";
      this.cmdBuild.Note = "准备好操作系统包和一组配置包后，选择此选项开始构建。";
      this.cmdBuild.Size = new Size(495, 90);
      this.cmdBuild.TabIndex = 3;
      this.cmdBuild.Text = "构建镜像";
      this.cmdBuild.UseVisualStyleBackColor = true;
      this.cmdBuild.Click += new EventHandler(this.cmdBuild_Click);
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.Controls.Add((Control) this.cmdBuild);
      this.Controls.Add((Control) this.lblDisclaimer);
      this.Controls.Add((Control) this.lblGlyph);
      this.Controls.Add((Control) this.btnBack);
      this.Controls.Add((Control) this.cmdCreateConfig);
      this.Controls.Add((Control) this.lblHeader);
      this.Controls.Add((Control) this.cmdExportDrivers);
      this.Font = new Font("Segoe UI", 9f);
      this.Name = nameof (LandingPage);
      this.Size = new Size(550, 400);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
