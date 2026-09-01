// Decompiled with JetBrains decompiler
// Type: DevImgGen.Pages.ExportPage
// Assembly: DevImgGen, Version=2021.1.31.1000, Culture=neutral, PublicKeyToken=null
// MVID: 8B415242-7DF5-46C1-83D1-ACA4F55A74B9
// Assembly location: C:\Users\Admin\Desktop\re\dig\DevImgGen.exe

using DevImgGen.Controls;
using DigLib;
using DigLib.DriverStore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DevImgGen.Pages
{
  public class ExportPage : BasePage
  {
    private IntPtr m_DriverStoreHandle;
    private List<string> m_DriverList = new List<string>();
    private IContainer components;
    private Label lblHeader;
    private BackButton btnBack;
    private Label lblWhereExport;
    private ProgressBar pbProgress;
    private BackgroundWorker bgwExport;
    private TextBox tbSaveDir;
    private Button btnBrowse;
    private CheckBox chkZip;
    private CommandButton cmdStartExport;
    private Label lblExportStatus;
    private Label lblZipTip;
    private CommandButton cmdProceed;

    public ExportPage()
    {
      this.InitializeComponent();
      this.Dock = DockStyle.Fill;
      this.tbSaveDir.Text = Path.Combine(Directory.GetCurrentDirectory(), "DriverExport");
      this.pbProgress.Visible = false;
      this.lblExportStatus.Visible = false;
      this.cmdProceed.Visible = false;
    }

    protected virtual void OnDriverLocationChanged(string e)
    {
      EventHandler<string> driverLocationChanged = this.DriverLocationChanged;
      if (driverLocationChanged == null)
        return;
      driverLocationChanged((object) this, e);
    }

    public event EventHandler<string> DriverLocationChanged;

    private void btnBack_Click(object sender, EventArgs e) => this.OnPageChangeRequested(PageEnum.Landing);

    private void btnBrowse_Click(object sender, EventArgs e)
    {
      using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = this.lblWhereExport.Text;
        folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
        folderBrowserDialog.SelectedPath = Directory.Exists(this.tbSaveDir.Text) ? this.tbSaveDir.Text : Path.GetDirectoryName(this.tbSaveDir.Text);
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK || !Directory.Exists(folderBrowserDialog.SelectedPath))
          return;
        this.tbSaveDir.Text = folderBrowserDialog.SelectedPath;
      }
    }

    private void cmdStartExport_Click(object sender, EventArgs e)
    {
      this.cmdStartExport.Enabled = false;
      this.m_DriverStoreHandle = DigLib.DriverStore.NativeMethods.DriverStoreOpenW((string) null, (string) null, 0U, IntPtr.Zero);
      if (this.m_DriverStoreHandle == IntPtr.Zero || Marshal.GetLastWin32Error() != 0)
      {
        int num = (int) MessageBox.Show("无法打开系统驱动存储。取消导出。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        this.OnPageChangeRequested(PageEnum.Landing);
      }
      else
      {
        this.pbProgress.Style = ProgressBarStyle.Marquee;
        this.pbProgress.Visible = true;
        this.lblExportStatus.Visible = true;
        this.bgwExport.RunWorkerAsync();
      }
    }

    private void bgwExport_DoWork(object sender, DoWorkEventArgs e)
    {
      if (!DigLib.DriverStore.NativeMethods.DriverStoreEnumW(this.m_DriverStoreHandle, 2U, new DigLib.DriverStore.NativeMethods.StoreEnumCallback(this.StoreEnumProc), IntPtr.Zero))
      {
        int num = (int) MessageBox.Show(string.Format("枚举已安装驱动程序时出错。错误代码 = {0}", (object) Marshal.GetLastWin32Error()), "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      else
      {
        this.Invoke(new MethodInvoker(delegate ()
        {
          this.pbProgress.Style = ProgressBarStyle.Continuous;
          this.pbProgress.Maximum = this.m_DriverList.Count;
        }));
        int progress = 0;
        foreach (string driver in this.m_DriverList)
        {
          progress++;
          string pkgIdent = Path.GetFileName(Path.GetDirectoryName(driver));
          this.Invoke(new MethodInvoker(delegate ()
          {
            this.lblExportStatus.Text = string.Format("正在导出驱动程序 '{0}' ({1}/{2})...", 
                (object) pkgIdent, (object) progress, (object) this.m_DriverList.Count);
            this.pbProgress.Value = progress;
          }));
          string str = Path.Combine(this.tbSaveDir.Text, this.chkZip.Checked ? "zip" : string.Empty, pkgIdent);
          if (!Directory.Exists(str))
            Directory.CreateDirectory(str);
          DigLib.DriverStore.NativeMethods.DriverStoreCopyW(this.m_DriverStoreHandle, driver, ProcessorArchitecture.Amd64, (string) null, 0U, str);
        }
      }
      DigLib.DriverStore.NativeMethods.DriverStoreClose(this.m_DriverStoreHandle);
      if (this.chkZip.Checked)
      {
        this.Invoke(new MethodInvoker(delegate ()
        {
          this.pbProgress.Style = ProgressBarStyle.Marquee;
          this.lblExportStatus.Text = "正在创建 ZIP 压缩包...";
        }));
        string str1 = Path.Combine(this.tbSaveDir.Text, "zip");
        string str2 = Path.Combine(this.tbSaveDir.Text, "ArchivedExport.zip");
        if (File.Exists(str2))
          File.Delete(str2);
        ZipFile.CreateFromDirectory(str1, str2);
        Utils.CmdDirectoryDelete(str1);
      }
      else
        this.OnDriverLocationChanged(this.tbSaveDir.Text);
      this.Invoke(new MethodInvoker(delegate ()
      {
        this.pbProgress.Style = ProgressBarStyle.Continuous;
        this.lblExportStatus.Text = "导出完成";
        if (this.chkZip.Checked)
          this.cmdProceed.Text = "打开导出驱动所在文件夹";
        this.cmdProceed.Visible = true;
      }));
    }

    private bool StoreEnumProc(
      IntPtr hDriverStore,
      string DriverStoreFilename,
      IntPtr dataPtr,
      IntPtr lParam)
    {
      if (!Path.GetFileNameWithoutExtension(DriverStoreFilename).ToLowerInvariant().StartsWith("prnms0"))
        this.m_DriverList.Add(DriverStoreFilename);
      return true;
    }

    private void cmdProceed_Click(object sender, EventArgs e)
    {
      if (this.chkZip.Checked)
        Process.Start(this.tbSaveDir.Text);
      else
        this.OnPageChangeRequested(PageEnum.CreateConfig);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.lblHeader = new Label();
      this.btnBack = new BackButton();
      this.lblWhereExport = new Label();
      this.pbProgress = new ProgressBar();
      this.bgwExport = new BackgroundWorker();
      this.tbSaveDir = new TextBox();
      this.btnBrowse = new Button();
      this.chkZip = new CheckBox();
      this.cmdStartExport = new CommandButton();
      this.lblExportStatus = new Label();
      this.lblZipTip = new Label();
      this.cmdProceed = new CommandButton();
      this.SuspendLayout();
      this.lblHeader.AutoSize = true;
      this.lblHeader.Font = new Font("Segoe UI", 11.5f);
      this.lblHeader.ForeColor = Color.FromArgb(0, 51, 153);
      this.lblHeader.Location = new Point(40, 14);
      this.lblHeader.Name = "lblHeader";
      this.lblHeader.Size = new Size(101, 21);
      this.lblHeader.TabIndex = 2;
      this.lblHeader.Text = "驱动程序导出";
      this.btnBack.FlatAppearance.BorderSize = 0;
      this.btnBack.FlatStyle = FlatStyle.Flat;
      this.btnBack.ForeColor = Color.FromArgb(128, 128, 128);
      this.btnBack.Location = new Point(5, 14);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new Size(32, 25);
      this.btnBack.TabIndex = 1;
      this.btnBack.Text = "\uE830";
      this.btnBack.Useless = false;
      this.btnBack.UseVisualStyleBackColor = true;
      this.btnBack.Click += new EventHandler(this.btnBack_Click);
      this.lblWhereExport.AutoSize = true;
      this.lblWhereExport.Location = new Point(41, 54);
      this.lblWhereExport.Name = "lblWhereExport";
      this.lblWhereExport.Size = new Size(263, 15);
      this.lblWhereExport.TabIndex = 3;
      this.lblWhereExport.Text = "你想将导出的驱动程序保存到哪里？";
      this.pbProgress.Location = new Point(44, 226);
      this.pbProgress.MarqueeAnimationSpeed = 50;
      this.pbProgress.Name = "pbProgress";
      this.pbProgress.Size = new Size(493, 23);
      this.pbProgress.TabIndex = 9;
      this.bgwExport.DoWork += new DoWorkEventHandler(this.bgwExport_DoWork);
      this.tbSaveDir.Location = new Point(44, 75);
      this.tbSaveDir.Name = "tbSaveDir";
      this.tbSaveDir.Size = new Size(412, 23);
      this.tbSaveDir.TabIndex = 4;
      this.btnBrowse.Location = new Point(462, 74);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new Size(75, 25);
      this.btnBrowse.TabIndex = 5;
      this.btnBrowse.Text = "浏览...";
      this.btnBrowse.UseVisualStyleBackColor = true;
      this.btnBrowse.Click += new EventHandler(this.btnBrowse_Click);
      this.chkZip.AutoSize = true;
      this.chkZip.Location = new Point(44, 105);
      this.chkZip.Name = "chkZip";
      this.chkZip.Size = new Size(84, 19);
      this.chkZip.TabIndex = 6;
      this.chkZip.Text = "保存为 ZIP";
      this.chkZip.UseVisualStyleBackColor = true;
      this.cmdStartExport.FlatStyle = FlatStyle.System;
      this.cmdStartExport.Location = new Point(43, 170);
      this.cmdStartExport.Name = "cmdStartExport";
      this.cmdStartExport.Note = "";
      this.cmdStartExport.Size = new Size(494, 44);
      this.cmdStartExport.TabIndex = 8;
      this.cmdStartExport.Text = "开始导出";
      this.cmdStartExport.UseVisualStyleBackColor = true;
      this.cmdStartExport.Click += new EventHandler(this.cmdStartExport_Click);
      this.lblExportStatus.AutoSize = true;
      this.lblExportStatus.Location = new Point(41, (int) byte.MaxValue);
      this.lblExportStatus.Name = "lblExportStatus";
      this.lblExportStatus.Size = new Size(253, 15);
      this.lblExportStatus.TabIndex = 10;
      this.lblExportStatus.Text = "正在收集已安装驱动程序的信息...";
      this.lblZipTip.AutoSize = true;
      this.lblZipTip.ForeColor = SystemColors.GrayText;
      this.lblZipTip.Location = new Point(41, (int) sbyte.MaxValue);
      this.lblZipTip.Name = "lblZipTip";
      this.lblZipTip.Size = new Size(503, 30);
      this.lblZipTip.TabIndex = 7;
      this.lblZipTip.Text = "提示：仅当你计划在另一台电脑上构建时才使用此选项。构建需要解压后的驱动集，但压缩包更便于传输。";
      this.cmdProceed.FlatStyle = FlatStyle.System;
      this.cmdProceed.Location = new Point(43, 283);
      this.cmdProceed.Name = "cmdProceed";
      this.cmdProceed.Note = "";
      this.cmdProceed.Size = new Size(494, 44);
      this.cmdProceed.TabIndex = 11;
      this.cmdProceed.Text = "使用导出的驱动程序集构建镜像";
      this.cmdProceed.UseVisualStyleBackColor = true;
      this.cmdProceed.Click += new EventHandler(this.cmdProceed_Click);
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.Controls.Add((Control) this.cmdProceed);
      this.Controls.Add((Control) this.lblZipTip);
      this.Controls.Add((Control) this.lblExportStatus);
      this.Controls.Add((Control) this.cmdStartExport);
      this.Controls.Add((Control) this.chkZip);
      this.Controls.Add((Control) this.btnBrowse);
      this.Controls.Add((Control) this.tbSaveDir);
      this.Controls.Add((Control) this.pbProgress);
      this.Controls.Add((Control) this.lblWhereExport);
      this.Controls.Add((Control) this.btnBack);
      this.Controls.Add((Control) this.lblHeader);
      this.Font = new Font("Segoe UI", 9f);
      this.Name = nameof (ExportPage);
      this.Size = new Size(550, 400);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
