// Decompiled with JetBrains decompiler
// Type: DevImgGen.Pages.CreateConfigPage
// Assembly: DevImgGen, Version=2021.1.31.1000, Culture=neutral, PublicKeyToken=null
// MVID: 8B415242-7DF5-46C1-83D1-ACA4F55A74B9
// Assembly location: C:\Users\Admin\Desktop\re\dig\DevImgGen.exe

using DevImgGen.Controls;
using DevImgGen.Dialogs;
using DigLib;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Button = System.Windows.Forms.Button;
using ProgressBar = System.Windows.Forms.ProgressBar;
using TextBox = System.Windows.Forms.TextBox;

namespace DevImgGen.Pages
{
  public class CreateConfigPage : BasePage
  {
    private DriverPrep m_PrepUtil;
    private IContainer components;
    private BackButton btnBack;
    private Label lblHeader;
    private Label lblDrvTip;
    private Button btnBrowseDrv;
    private TextBox tbDriverDir;
    private Label lblDriverLocation;
    private CommandButton cmdProcessDrv;
    private Label lblConfigTip;
    private Button btnBrowseCfg;
    private TextBox tbConfigDir;
    private Label lblConfigLocation;
    private Label lblProcesStatus;
    private ProgressBar pbProgress;
    private BackgroundWorker bgwProcess;

    public CreateConfigPage(string driverDirectory = null)
    {
      this.InitializeComponent();
      if (!string.IsNullOrEmpty(driverDirectory) && Directory.Exists(driverDirectory))
        this.tbDriverDir.Text = driverDirectory;
      this.tbConfigDir.Text = Path.Combine(Directory.GetCurrentDirectory(), "MyDevice");
      this.pbProgress.Visible = false;
      this.lblProcesStatus.Visible = false;
    }

    private void CreateConfigPage_Load(object sender, EventArgs e)
    {
      if (!Utils.ProgramExistsInPath("signtool.exe") || !Utils.ProgramExistsInPath("infverif.exe") || !Utils.ProgramExistsInPath("apivalidator.exe") || !Utils.ProgramExistsInPath("inf2cat.exe"))
      {
        int num = (int) MessageBox.Show("前置检查失败。创建配置包需要 SignTool、InfVerif、ApiValidator 和 Inf2Cat。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        this.OnPageChangeRequested(PageEnum.Landing);
      }
      else
      {
        this.m_PrepUtil = new DriverPrep();
        this.m_PrepUtil.ProgressInfo += new EventHandler<string>(this.PrepUtil_ProgressInfo);
        this.m_PrepUtil.ProgressWarn += new EventHandler<string>(this.PrepUtil_ProgressWarn);
        this.m_PrepUtil.OverlapRaised += new EventHandler<DriverOverlap>(this.PrepUtil_OverlapRaised);
      }
    }

    protected virtual void OnPackageLocationChanged(string e)
    {
      EventHandler<string> packageLocationChanged = this.PackageLocationChanged;
      if (packageLocationChanged == null)
        return;
      packageLocationChanged((object) this, e);
    }

    public event EventHandler<string> PackageLocationChanged;

    private void btnBrowseDrv_Click(object sender, EventArgs e)
    {
      using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = this.lblDriverLocation.Text;
        if (Directory.Exists(this.tbDriverDir.Text))
        {
          folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
          folderBrowserDialog.SelectedPath = this.tbDriverDir.Text;
        }
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK || !Directory.Exists(folderBrowserDialog.SelectedPath))
          return;
        this.tbDriverDir.Text = folderBrowserDialog.SelectedPath;
      }
    }

    private void btnBrowseSave_Click(object sender, EventArgs e)
    {
      using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = this.lblConfigLocation.Text;
        folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
        folderBrowserDialog.SelectedPath = Directory.Exists(this.tbConfigDir.Text) ? this.tbConfigDir.Text : Path.GetDirectoryName(this.tbConfigDir.Text);
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK || !Directory.Exists(folderBrowserDialog.SelectedPath))
          return;
        this.tbConfigDir.Text = folderBrowserDialog.SelectedPath;
      }
    }

    private void cmdProcessDrv_Click(object sender, EventArgs e)
    {
      this.cmdProcessDrv.Enabled = false;
      this.pbProgress.Style = ProgressBarStyle.Marquee;
      this.pbProgress.Visible = true;
      this.lblProcesStatus.Visible = true;
      if (!Directory.Exists(this.tbConfigDir.Text))
        Directory.CreateDirectory(this.tbConfigDir.Text);
      this.bgwProcess.RunWorkerAsync();
    }

    private void bgwProcess_DoWork(object sender, DoWorkEventArgs e)
    {
      this.m_PrepUtil.ProcessDrivers(this.tbDriverDir.Text, this.tbConfigDir.Text, false);
            this.OnPackageLocationChanged(this.tbConfigDir.Text);

           
            this.Invoke(new MethodInvoker(delegate ()
      {
        this.pbProgress.Value = 100;
        this.pbProgress.Style = ProgressBarStyle.Continuous;
        this.lblProcesStatus.Text = "完成";
        this.OnPageChangeRequested(PageEnum.Build);
      }));
    }

    private void PrepUtil_OverlapRaised(object sender, DriverOverlap e)
    {
      int num = (int) new IdenticalDriverOverlap(e).ShowDialog();
    }

    private void PrepUtil_ProgressWarn(object sender, string e)
    {
      int num = (int) MessageBox.Show(e, "警告", 
          MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

        private void PrepUtil_ProgressInfo(object sender, string e)
        {
            this.Invoke(new MethodInvoker(delegate () 
            { 
                this.lblProcesStatus.Text = e; 
            })); 
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.OnPageChangeRequested(PageEnum.Landing);
        }

        private void tbDriverDir_TextChanged(object sender, EventArgs e)
    {
      if (this.cmdProcessDrv.Enabled)
        return;
      this.cmdProcessDrv.Enabled = true;
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
      this.lblDrvTip = new Label();
      this.btnBrowseDrv = new Button();
      this.tbDriverDir = new TextBox();
      this.lblDriverLocation = new Label();
      this.btnBack = new BackButton();
      this.cmdProcessDrv = new CommandButton();
      this.lblConfigTip = new Label();
      this.btnBrowseCfg = new Button();
      this.tbConfigDir = new TextBox();
      this.lblConfigLocation = new Label();
      this.lblProcesStatus = new Label();
      this.pbProgress = new ProgressBar();
      this.bgwProcess = new BackgroundWorker();
      this.SuspendLayout();
      this.lblHeader.AutoSize = true;
      this.lblHeader.Font = new Font("Segoe UI", 11.5f);
      this.lblHeader.ForeColor = Color.FromArgb(0, 51, 153);
      this.lblHeader.Location = new Point(40, 14);
      this.lblHeader.Name = "lblHeader";
      this.lblHeader.Size = new Size(220, 21);
      this.lblHeader.TabIndex = 1;
      this.lblHeader.Text = "创建配置包";
      this.lblDrvTip.AutoSize = true;
      this.lblDrvTip.ForeColor = SystemColors.GrayText;
      this.lblDrvTip.Location = new Point(41, 104);
      this.lblDrvTip.Name = "lblDrvTip";
      this.lblDrvTip.Size = new Size(493, 30);
      this.lblDrvTip.TabIndex = 5;
      this.lblDrvTip.Text = "提示：所选文件夹将被递归搜索 *.inf 文件。压缩包内和自解压 OEM 安装程序中的驱动程序不适用。";
      this.btnBrowseDrv.Location = new Point(462, 74);
      this.btnBrowseDrv.Name = "btnBrowseDrv";
      this.btnBrowseDrv.Size = new Size(75, 25);
      this.btnBrowseDrv.TabIndex = 4;
      this.btnBrowseDrv.Text = "浏览...";
      this.btnBrowseDrv.UseVisualStyleBackColor = true;
      this.btnBrowseDrv.Click += new EventHandler(this.btnBrowseDrv_Click);
      this.tbDriverDir.Location = new Point(44, 75);
      this.tbDriverDir.Name = "tbDriverDir";
      this.tbDriverDir.Size = new Size(412, 23);
      this.tbDriverDir.TabIndex = 3;
      this.tbDriverDir.TextChanged += new EventHandler(this.tbDriverDir_TextChanged);
      this.lblDriverLocation.AutoSize = true;
      this.lblDriverLocation.Location = new Point(41, 54);
      this.lblDriverLocation.Name = "lblDriverLocation";
      this.lblDriverLocation.Size = new Size(268, 15);
      this.lblDriverLocation.TabIndex = 2;
      this.lblDriverLocation.Text = "选择要集成驱动程序的文件夹";
      this.btnBack.FlatAppearance.BorderSize = 0;
      this.btnBack.FlatAppearance.MouseDownBackColor = Color.Transparent;
      this.btnBack.FlatAppearance.MouseOverBackColor = Color.Transparent;
      this.btnBack.FlatStyle = FlatStyle.Flat;
      this.btnBack.ForeColor = Color.FromArgb(128, 128, 128);
      this.btnBack.Location = new Point(5, 14);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new Size(32, 25);
      this.btnBack.TabIndex = 0;
      this.btnBack.Text = "\uE830";
      this.btnBack.Useless = false;
      this.btnBack.UseVisualStyleBackColor = true;
      this.btnBack.Click += new EventHandler(this.btnBack_Click);
      this.cmdProcessDrv.Enabled = false;
      this.cmdProcessDrv.FlatStyle = FlatStyle.System;
      this.cmdProcessDrv.Location = new Point(43, 241);
      this.cmdProcessDrv.Name = "cmdProcessDrv";
      this.cmdProcessDrv.Note = "";
      this.cmdProcessDrv.Size = new Size(494, 44);
      this.cmdProcessDrv.TabIndex = 10;
      this.cmdProcessDrv.Text = "处理驱动程序";
      this.cmdProcessDrv.UseVisualStyleBackColor = true;
      this.cmdProcessDrv.Click += new EventHandler(this.cmdProcessDrv_Click);
      this.lblConfigTip.AutoSize = true;
      this.lblConfigTip.ForeColor = SystemColors.GrayText;
      this.lblConfigTip.Location = new Point(41, 198);
      this.lblConfigTip.Name = "lblConfigTip";
      this.lblConfigTip.Size = new Size(501, 30);
      this.lblConfigTip.TabIndex = 9;
      this.lblConfigTip.Text = "提示：Windows 设备镜像生成器将创建一些构建含驱动程序镜像所需的包。每个目标设备只需执行一次。";
      this.btnBrowseCfg.Location = new Point(462, 168);
      this.btnBrowseCfg.Name = "btnBrowseCfg";
      this.btnBrowseCfg.Size = new Size(75, 25);
      this.btnBrowseCfg.TabIndex = 8;
      this.btnBrowseCfg.Text = "浏览...";
      this.btnBrowseCfg.UseVisualStyleBackColor = true;
      this.btnBrowseCfg.Click += new EventHandler(this.btnBrowseSave_Click);
      this.tbConfigDir.Location = new Point(44, 169);
      this.tbConfigDir.Name = "tbConfigDir";
      this.tbConfigDir.Size = new Size(412, 23);
      this.tbConfigDir.TabIndex = 7;
      this.lblConfigLocation.AutoSize = true;
      this.lblConfigLocation.Location = new Point(41, 148);
      this.lblConfigLocation.Name = "lblConfigLocation";
      this.lblConfigLocation.Size = new Size(282, 15);
      this.lblConfigLocation.TabIndex = 6;
      this.lblConfigLocation.Text = "你想将配置包保存到哪里？";
      this.lblProcesStatus.AutoSize = true;
      this.lblProcesStatus.Location = new Point(41, 326);
      this.lblProcesStatus.Name = "lblProcesStatus";
      this.lblProcesStatus.Size = new Size(67, 15);
      this.lblProcesStatus.TabIndex = 12;
      this.lblProcesStatus.Text = "准备中...";
      this.pbProgress.Location = new Point(44, 297);
      this.pbProgress.MarqueeAnimationSpeed = 50;
      this.pbProgress.Name = "pbProgress";
      this.pbProgress.Size = new Size(493, 23);
      this.pbProgress.TabIndex = 11;
      this.bgwProcess.DoWork += new DoWorkEventHandler(this.bgwProcess_DoWork);
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.Controls.Add((Control) this.lblProcesStatus);
      this.Controls.Add((Control) this.pbProgress);
      this.Controls.Add((Control) this.lblConfigTip);
      this.Controls.Add((Control) this.btnBrowseCfg);
      this.Controls.Add((Control) this.tbConfigDir);
      this.Controls.Add((Control) this.lblConfigLocation);
      this.Controls.Add((Control) this.cmdProcessDrv);
      this.Controls.Add((Control) this.lblDrvTip);
      this.Controls.Add((Control) this.btnBrowseDrv);
      this.Controls.Add((Control) this.tbDriverDir);
      this.Controls.Add((Control) this.lblDriverLocation);
      this.Controls.Add((Control) this.lblHeader);
      this.Controls.Add((Control) this.btnBack);
      this.Font = new Font("Segoe UI", 9f);
      this.Name = nameof (CreateConfigPage);
      this.Size = new Size(550, 400);
      this.Load += new EventHandler(this.CreateConfigPage_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
