// Modified BuildPage - custom OEMInput / imggen / architecture selection
// Original backed up as BuildPage.cs.bak

using DevImgGen.Controls;
using DigLib;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DevImgGen.Pages
{
  public class BuildPage : BasePage
  {
    private IContainer components;
    private BackButton btnBack;
    private Label lblHeader;

    // auto-size support
    private Form _parentFormRef;
    private Size _originalFormClientSize;
    private bool _sizeRestored;

    // OS package directory (cab folder)
    private Button btnBrowseOSPkg;
    private TextBox tbOSPkgDir;
    private Label lblOSPkgLocation;

    // OEMInput file (user-selected)
    private Button btnBrowseOEMInput;
    private TextBox tbOEMInputPath;
    private Label lblOEMInputLocation;

    // imggen path (user-selected)
    private Button btnBrowseImggen;
    private TextBox tbImggenPath;
    private Label lblImggenLocation;

    // architecture selection
    private Label lblArch;
    private ComboBox cmbArch;

    // optional driver config packages
    private Button btnBrowseCfg;
    private TextBox tbConfigDir;
    private Label lblConfigLocation;

    // display fallback
    private CheckBox chkDispFall;

    // output image
    private Button btnBrowseImg;
    private TextBox tbImageLocation;
    private Label lblImageLocation;

    private CommandButton cmdStartBuilding;
    private Label lblBuildTip;

    public BuildPage(string packageLocation = null)
    {
      this.InitializeComponent();
      if (!string.IsNullOrEmpty(packageLocation))
        this.tbConfigDir.Text = packageLocation;
      this.Load += new EventHandler(this.BuildPage_Load);
    }

    private void BuildPage_Load(object sender, EventArgs e)
    {
      // auto-size the parent form to fit this page's content
      if (this.ParentForm != null)
      {
        this._parentFormRef = this.ParentForm;
        this._originalFormClientSize = this.ParentForm.ClientSize;
        // needed height = last control's bottom + bottom margin (match original ~36px)
        int neededHeight = this.lblBuildTip.Bottom + 36;
        // width: keep original form width (581) since page width is fixed 550
        this.ParentForm.ClientSize = new Size(581, neededHeight);
      }
    }

    private void RestoreFormSize()
    {
      if (!this._sizeRestored && this._parentFormRef != null && !this._originalFormClientSize.IsEmpty)
      {
        try { this._parentFormRef.ClientSize = this._originalFormClientSize; } catch { }
        this._sizeRestored = true;
      }
    }

    private void btnBack_Click(object sender, EventArgs e) => this.OnPageChangeRequested(PageEnum.Landing);

    // --- OS package directory browse ---
    private void btnBrowseOSPkg_Click(object sender, EventArgs e)
    {
      using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = this.lblOSPkgLocation.Text;
        if (Directory.Exists(this.tbOSPkgDir.Text))
        {
          folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
          folderBrowserDialog.SelectedPath = this.tbOSPkgDir.Text;
        }
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK || !Directory.Exists(folderBrowserDialog.SelectedPath))
          return;
        this.tbOSPkgDir.Text = folderBrowserDialog.SelectedPath;
      }
    }

    // --- OEMInput file browse ---
    private void btnBrowseOEMInput_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.Filter = "OEM 输入 XML (*.xml)|*.xml|所有文件 (*.*)|*.*";
        openFileDialog.Title = "选择 OEMInput.xml";
        openFileDialog.InitialDirectory = Directory.Exists(this.tbOSPkgDir.Text) ? this.tbOSPkgDir.Text : Directory.GetCurrentDirectory();
        if (!string.IsNullOrEmpty(this.tbOEMInputPath.Text) && File.Exists(this.tbOEMInputPath.Text))
          openFileDialog.FileName = this.tbOEMInputPath.Text;
        if (openFileDialog.ShowDialog() != DialogResult.OK)
          return;
        this.tbOEMInputPath.Text = openFileDialog.FileName;
      }
    }

    // --- imggen path browse ---
    private void btnBrowseImggen_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.Filter = "imggen 命令 (*.cmd;*.bat)|*.cmd;*.bat|可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
        openFileDialog.Title = "选择 imggen.cmd 或 imageapp.exe";
        openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
        if (!string.IsNullOrEmpty(this.tbImggenPath.Text) && File.Exists(this.tbImggenPath.Text))
          openFileDialog.FileName = this.tbImggenPath.Text;
        if (openFileDialog.ShowDialog() != DialogResult.OK)
          return;
        this.tbImggenPath.Text = openFileDialog.FileName;
      }
    }

    // --- driver config directory browse ---
    private void btnBrowseCfg_Click(object sender, EventArgs e)
    {
      using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
      {
        folderBrowserDialog.Description = this.lblConfigLocation.Text;
        if (Directory.Exists(this.tbConfigDir.Text))
        {
          folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
          folderBrowserDialog.SelectedPath = this.tbConfigDir.Text;
        }
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK || !Directory.Exists(folderBrowserDialog.SelectedPath))
          return;
        this.tbConfigDir.Text = folderBrowserDialog.SelectedPath;
      }
    }

    // --- output image browse ---
    private void btnBrowseImg_Click(object sender, EventArgs e)
    {
      using (SaveFileDialog saveFileDialog = new SaveFileDialog())
      {
        saveFileDialog.Filter = "完整闪存更新 (*.ffu)|*.ffu|虚拟硬盘 (*.vhdx)|*.vhdx|虚拟硬盘 (*.vhd)|*.vhd";
        saveFileDialog.FileName = "Flash.ffu";
        saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
        saveFileDialog.Title = this.lblImageLocation.Text;
        if (saveFileDialog.ShowDialog() != DialogResult.OK)
          return;
        this.tbImageLocation.Text = saveFileDialog.FileName;
      }
    }

    // --- enable build button when required fields are filled ---
    private void RequiredField_TextChanged(object sender, EventArgs e)
    {
      bool ready = !string.IsNullOrWhiteSpace(this.tbOSPkgDir.Text)
                && !string.IsNullOrWhiteSpace(this.tbOEMInputPath.Text)
                && !string.IsNullOrWhiteSpace(this.tbImggenPath.Text)
                && !string.IsNullOrWhiteSpace(this.tbImageLocation.Text)
                && this.cmbArch.SelectedIndex >= 0;
      this.cmdStartBuilding.Enabled = ready;
    }

    // --- start building ---
    private void cmdStartBuilding_Click(object sender, EventArgs e)
    {
      // validate OS package directory exists
      if (!Directory.Exists(this.tbOSPkgDir.Text))
      {
        MessageBox.Show("操作系统包目录不存在。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return;
      }
      // validate Retail\<arch>\fre path exists (architecture must match selection)
      // architecture: raw case (AMD64/ARM64) for imageapp.exe /Retail path, lowercase (amd64/arm64) for imggen.cmd
      string archRaw = this.cmbArch.SelectedItem.ToString();
      string archLower = archRaw.ToLowerInvariant();
      string retailArchFre = Path.Combine(this.tbOSPkgDir.Text, "Retail", archRaw, "fre");
      if (!Directory.Exists(retailArchFre))
      {
        MessageBox.Show("在操作系统包目录中未找到必需的路径 'Retail\\" + archRaw + "\\fre'。\n\n期望路径: " + retailArchFre, "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return;
      }

      // validate OEMInput file
      if (!File.Exists(this.tbOEMInputPath.Text))
      {
        MessageBox.Show("所选的 OEMInput XML 文件不存在。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return;
      }

      // validate imggen path
      if (!File.Exists(this.tbImggenPath.Text))
      {
        MessageBox.Show("所选的 imggen 路径不存在。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return;
      }

      // validate output directory
      string outputDir = Path.GetDirectoryName(Path.GetFullPath(this.tbImageLocation.Text));
      if (!Directory.Exists(outputDir))
      {
        MessageBox.Show("镜像的输出目录不存在。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        return;
      }

      // optional: confirm building without driver config packages
      if (string.IsNullOrEmpty(this.tbConfigDir.Text) &&
          MessageBox.Show("未指定配置包目录。确定要在没有驱动程序的情况下构建吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
      {
        return;
      }

      // if driver config packages are specified, copy Volantis and add driver FMs to a *copy* of OEMInput
      string oemInputToUse = this.tbOEMInputPath.Text;
      if (!string.IsNullOrEmpty(this.tbConfigDir.Text))
      {
        // copy OEMInput to output dir so we can modify it
        string modifiedOEMInput = Path.Combine(outputDir, "OEMInput.generated.xml");
        try
        {
          XDocument xdocument = XDocument.Load(this.tbOEMInputPath.Text);
          XElement root = xdocument.Root;
          // find AdditionalFMs element (handle namespace)
          XElement additionalFMs = root.Elements().FirstOrDefault(el => el.Name.LocalName == "AdditionalFMs");
          if (additionalFMs != null)
          {
            // copy Volantis to OS package dir
            string volantisSrc = Path.Combine(this.tbConfigDir.Text, "Volantis");
            if (Directory.Exists(volantisSrc))
            {
              string volantisDst = Path.Combine(this.tbOSPkgDir.Text, "Volantis");
              Utils.RecursiveCopyDirectory(volantisSrc, volantisDst);
            }
            // add all *Driver*FM.xml from config dir
            foreach (string fmPath in Directory.EnumerateFileSystemEntries(this.tbConfigDir.Text, "*Driver*FM.xml", SearchOption.AllDirectories))
            {
              XNamespace ns = additionalFMs.Name.Namespace;
              additionalFMs.Add(new XElement(ns + "AdditionalFM", fmPath));
            }
            // optional display fallback
            if (this.chkDispFall.Checked)
            {
              string dispFallback = Path.Combine(this.tbOSPkgDir.Text, "Volantis", archLower, "DisplayFallbackFM.xml");
              if (File.Exists(dispFallback))
              {
                XNamespace ns = additionalFMs.Name.Namespace;
                additionalFMs.Add(new XElement(ns + "AdditionalFM", dispFallback));
              }
            }
          }
          xdocument.Save(modifiedOEMInput);
          oemInputToUse = modifiedOEMInput;
        }
        catch (Exception ex)
        {
          MessageBox.Show("处理带驱动程序包的 OEMInput 失败: " + ex.Message + "\n\n将按原样使用原始 OEMInput。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          oemInputToUse = this.tbOEMInputPath.Text;
        }
      }
      else if (this.chkDispFall.Checked)
      {
        // display fallback without driver dir: just warn
        MessageBox.Show("已选择显示回退，但未指定驱动程序配置目录。将不会添加 DisplayFallbackFM。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // launch build
      string currentDirectory = Directory.GetCurrentDirectory();
      string imggenDir = Path.GetDirectoryName(Path.GetFullPath(this.tbImggenPath.Text));
      string imggenFileName = Path.GetFileName(this.tbImggenPath.Text);
      string imggenExt = Path.GetExtension(imggenFileName).ToLowerInvariant();

      using (Process process = new Process())
      {
        if (imggenExt == ".cmd" || imggenExt == ".bat")
        {
          // cmd/bat (imggen.cmd): launch via cmd.exe /k, architecture in lowercase (matches original project behavior)
          process.StartInfo.FileName = "cmd.exe";
          process.StartInfo.Arguments = "/k " + Path.GetPathRoot(imggenDir).Substring(0, 2) + " && cd \"" + imggenDir + "\" && \"" + imggenFileName + "\" \"" + this.tbImageLocation.Text + "\" \"" + oemInputToUse + "\" \"" + this.tbOSPkgDir.Text + "\" " + archLower;
        }
        else
        {
          // exe (imageapp.exe): launch directly, architecture as-is with /CPUType flag
          process.StartInfo.FileName = this.tbImggenPath.Text;
          process.StartInfo.Arguments = "\"" + this.tbImageLocation.Text + "\" \"" + oemInputToUse + "\" \"" + this.tbOSPkgDir.Text + "\" +StrictSettingPolicies /CPUType:" + archRaw;
          process.StartInfo.WorkingDirectory = imggenDir;
        }
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.Verb = "runas";
        process.Start();
      }
      this.cmdStartBuilding.Enabled = false;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.RestoreFormSize();
        if (this.components != null)
          this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.btnBack = new BackButton();
      this.lblHeader = new Label();

      this.lblOSPkgLocation = new Label();
      this.tbOSPkgDir = new TextBox();
      this.btnBrowseOSPkg = new Button();

      this.lblOEMInputLocation = new Label();
      this.tbOEMInputPath = new TextBox();
      this.btnBrowseOEMInput = new Button();

      this.lblImggenLocation = new Label();
      this.tbImggenPath = new TextBox();
      this.btnBrowseImggen = new Button();

      this.lblArch = new Label();
      this.cmbArch = new ComboBox();

      this.lblConfigLocation = new Label();
      this.tbConfigDir = new TextBox();
      this.btnBrowseCfg = new Button();

      this.chkDispFall = new CheckBox();

      this.lblImageLocation = new Label();
      this.tbImageLocation = new TextBox();
      this.btnBrowseImg = new Button();

      this.cmdStartBuilding = new CommandButton();
      this.lblBuildTip = new Label();

      this.SuspendLayout();

      // --- back button ---
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

      // --- header ---
      this.lblHeader.AutoSize = true;
      this.lblHeader.Font = new Font("Segoe UI", 11.5f);
      this.lblHeader.ForeColor = Color.FromArgb(0, 51, 153);
      this.lblHeader.Location = new Point(40, 14);
      this.lblHeader.Name = "lblHeader";
      this.lblHeader.Size = new Size(113, 21);
      this.lblHeader.TabIndex = 1;
      this.lblHeader.Text = "构建镜像";

      // --- OS package directory ---
      this.lblOSPkgLocation.AutoSize = true;
      this.lblOSPkgLocation.Location = new Point(41, 50);
      this.lblOSPkgLocation.Name = "lblOSPkgLocation";
      this.lblOSPkgLocation.Size = new Size(280, 15);
      this.lblOSPkgLocation.TabIndex = 2;
      this.lblOSPkgLocation.Text = "操作系统包文件夹（包含 .cab 文件）";
      this.tbOSPkgDir.Location = new Point(44, 70);
      this.tbOSPkgDir.Name = "tbOSPkgDir";
      this.tbOSPkgDir.Size = new Size(412, 23);
      this.tbOSPkgDir.TabIndex = 3;
      this.tbOSPkgDir.TextChanged += new EventHandler(this.RequiredField_TextChanged);
      this.btnBrowseOSPkg.Location = new Point(462, 69);
      this.btnBrowseOSPkg.Name = "btnBrowseOSPkg";
      this.btnBrowseOSPkg.Size = new Size(75, 25);
      this.btnBrowseOSPkg.TabIndex = 4;
      this.btnBrowseOSPkg.Text = "浏览...";
      this.btnBrowseOSPkg.UseVisualStyleBackColor = true;
      this.btnBrowseOSPkg.Click += new EventHandler(this.btnBrowseOSPkg_Click);

      // --- OEMInput file ---
      this.lblOEMInputLocation.AutoSize = true;
      this.lblOEMInputLocation.Location = new Point(41, 105);
      this.lblOEMInputLocation.Name = "lblOEMInputLocation";
      this.lblOEMInputLocation.Size = new Size(200, 15);
      this.lblOEMInputLocation.TabIndex = 5;
      this.lblOEMInputLocation.Text = "OEMInput XML 文件";
      this.tbOEMInputPath.Location = new Point(44, 125);
      this.tbOEMInputPath.Name = "tbOEMInputPath";
      this.tbOEMInputPath.Size = new Size(412, 23);
      this.tbOEMInputPath.TabIndex = 6;
      this.tbOEMInputPath.TextChanged += new EventHandler(this.RequiredField_TextChanged);
      this.btnBrowseOEMInput.Location = new Point(462, 124);
      this.btnBrowseOEMInput.Name = "btnBrowseOEMInput";
      this.btnBrowseOEMInput.Size = new Size(75, 25);
      this.btnBrowseOEMInput.TabIndex = 7;
      this.btnBrowseOEMInput.Text = "浏览...";
      this.btnBrowseOEMInput.UseVisualStyleBackColor = true;
      this.btnBrowseOEMInput.Click += new EventHandler(this.btnBrowseOEMInput_Click);

      // --- imggen path ---
      this.lblImggenLocation.AutoSize = true;
      this.lblImggenLocation.Location = new Point(41, 160);
      this.lblImggenLocation.Name = "lblImggenLocation";
      this.lblImggenLocation.Size = new Size(250, 15);
      this.lblImggenLocation.TabIndex = 8;
      this.lblImggenLocation.Text = "imggen.cmd 或 imageapp.exe 路径";
      this.tbImggenPath.Location = new Point(44, 180);
      this.tbImggenPath.Name = "tbImggenPath";
      this.tbImggenPath.Size = new Size(412, 23);
      this.tbImggenPath.TabIndex = 9;
      this.tbImggenPath.TextChanged += new EventHandler(this.RequiredField_TextChanged);
      this.btnBrowseImggen.Location = new Point(462, 179);
      this.btnBrowseImggen.Name = "btnBrowseImggen";
      this.btnBrowseImggen.Size = new Size(75, 25);
      this.btnBrowseImggen.TabIndex = 10;
      this.btnBrowseImggen.Text = "浏览...";
      this.btnBrowseImggen.UseVisualStyleBackColor = true;
      this.btnBrowseImggen.Click += new EventHandler(this.btnBrowseImggen_Click);

      // --- architecture ---
      this.lblArch.AutoSize = true;
      this.lblArch.Location = new Point(41, 215);
      this.lblArch.Name = "lblArch";
      this.lblArch.Size = new Size(70, 15);
      this.lblArch.TabIndex = 11;
      this.lblArch.Text = "架构";
      this.cmbArch.DropDownStyle = ComboBoxStyle.DropDownList;
      this.cmbArch.Items.AddRange(new object[] { "x86", "AMD64", "ARM", "ARM64" });
      this.cmbArch.Location = new Point(44, 235);
      this.cmbArch.Name = "cmbArch";
      this.cmbArch.Size = new Size(150, 23);
      this.cmbArch.TabIndex = 12;
      this.cmbArch.SelectedIndexChanged += new EventHandler(this.RequiredField_TextChanged);

      // --- driver config directory (optional) ---
      this.lblConfigLocation.AutoSize = true;
      this.lblConfigLocation.Location = new Point(41, 270);
      this.lblConfigLocation.Name = "lblConfigLocation";
      this.lblConfigLocation.Size = new Size(320, 15);
      this.lblConfigLocation.TabIndex = 13;
      this.lblConfigLocation.Text = "驱动程序配置包文件夹（可选）";
      this.tbConfigDir.Location = new Point(44, 290);
      this.tbConfigDir.Name = "tbConfigDir";
      this.tbConfigDir.Size = new Size(412, 23);
      this.tbConfigDir.TabIndex = 14;
      this.btnBrowseCfg.Location = new Point(462, 289);
      this.btnBrowseCfg.Name = "btnBrowseCfg";
      this.btnBrowseCfg.Size = new Size(75, 25);
      this.btnBrowseCfg.TabIndex = 15;
      this.btnBrowseCfg.Text = "浏览...";
      this.btnBrowseCfg.UseVisualStyleBackColor = true;
      this.btnBrowseCfg.Click += new EventHandler(this.btnBrowseCfg_Click);

      // --- display fallback ---
      this.chkDispFall.AutoSize = true;
      this.chkDispFall.Location = new Point(44, 320);
      this.chkDispFall.Name = "chkDispFall";
      this.chkDispFall.Size = new Size(490, 19);
      this.chkDispFall.TabIndex = 16;
      this.chkDispFall.Text = "设置显示回退（如果缺少显示驱动程序或想使用非微软虚拟机，此选项很有用）";
      this.chkDispFall.UseVisualStyleBackColor = true;

      // --- output image ---
      this.lblImageLocation.AutoSize = true;
      this.lblImageLocation.Location = new Point(41, 350);
      this.lblImageLocation.Name = "lblImageLocation";
      this.lblImageLocation.Size = new Size(211, 15);
      this.lblImageLocation.TabIndex = 17;
      this.lblImageLocation.Text = "你想将镜像保存到哪里？";
      this.tbImageLocation.Location = new Point(44, 370);
      this.tbImageLocation.Name = "tbImageLocation";
      this.tbImageLocation.Size = new Size(412, 23);
      this.tbImageLocation.TabIndex = 18;
      this.tbImageLocation.TextChanged += new EventHandler(this.RequiredField_TextChanged);
      this.btnBrowseImg.Location = new Point(462, 369);
      this.btnBrowseImg.Name = "btnBrowseImg";
      this.btnBrowseImg.Size = new Size(75, 25);
      this.btnBrowseImg.TabIndex = 19;
      this.btnBrowseImg.Text = "浏览...";
      this.btnBrowseImg.UseVisualStyleBackColor = true;
      this.btnBrowseImg.Click += new EventHandler(this.btnBrowseImg_Click);

      // --- start building button ---
      this.cmdStartBuilding.Enabled = false;
      this.cmdStartBuilding.FlatStyle = FlatStyle.System;
      this.cmdStartBuilding.Location = new Point(43, 410);
      this.cmdStartBuilding.Name = "cmdStartBuilding";
      this.cmdStartBuilding.Note = "";
      this.cmdStartBuilding.Size = new Size(494, 44);
      this.cmdStartBuilding.TabIndex = 20;
      this.cmdStartBuilding.Text = "开始构建";
      this.cmdStartBuilding.UseVisualStyleBackColor = true;
      this.cmdStartBuilding.Click += new EventHandler(this.cmdStartBuilding_Click);

      // --- build tip ---
      this.lblBuildTip.AutoSize = true;
      this.lblBuildTip.ForeColor = SystemColors.GrayText;
      this.lblBuildTip.Location = new Point(41, 470);
      this.lblBuildTip.Name = "lblBuildTip";
      this.lblBuildTip.Size = new Size(486, 30);
      this.lblBuildTip.TabIndex = 21;
      this.lblBuildTip.Text = "提示：构建在单独的提权进程中进行。请留意新弹出的窗口以跟踪镜像的构建进度。";

      // --- form ---
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.Controls.Add((Control)this.lblBuildTip);
      this.Controls.Add((Control)this.cmdStartBuilding);
      this.Controls.Add((Control)this.btnBrowseImg);
      this.Controls.Add((Control)this.tbImageLocation);
      this.Controls.Add((Control)this.lblImageLocation);
      this.Controls.Add((Control)this.chkDispFall);
      this.Controls.Add((Control)this.btnBrowseCfg);
      this.Controls.Add((Control)this.tbConfigDir);
      this.Controls.Add((Control)this.lblConfigLocation);
      this.Controls.Add((Control)this.cmbArch);
      this.Controls.Add((Control)this.lblArch);
      this.Controls.Add((Control)this.btnBrowseImggen);
      this.Controls.Add((Control)this.tbImggenPath);
      this.Controls.Add((Control)this.lblImggenLocation);
      this.Controls.Add((Control)this.btnBrowseOEMInput);
      this.Controls.Add((Control)this.tbOEMInputPath);
      this.Controls.Add((Control)this.lblOEMInputLocation);
      this.Controls.Add((Control)this.btnBrowseOSPkg);
      this.Controls.Add((Control)this.tbOSPkgDir);
      this.Controls.Add((Control)this.lblOSPkgLocation);
      this.Controls.Add((Control)this.lblHeader);
      this.Controls.Add((Control)this.btnBack);
      this.Font = new Font("Segoe UI", 9f);
      this.Name = "BuildPage";
      this.Size = new Size(550, 520);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
