// Decompiled with JetBrains decompiler
// Type: DigLib.DriverPrep
// Assembly: DigLib, Version=2021.1.31.1000, Culture=neutral, PublicKeyToken=null
// MVID: 8630F1AA-3914-41FE-A6B1-9C741E0FFE01
// Assembly location: C:\Users\Admin\Desktop\re\dig\DigLib.dll

using DigLib.DriverStore;
using Microsoft.Composition.Packaging;
using Microsoft.Composition.ToolBox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;

namespace DigLib
{
  public class DriverPrep
  {
    private string m_CurrentDriverFile;
    private bool m_SkipCurrentDriver;
    private List<string> m_CurrentDriverHwIds = new List<string>();
    private Dictionary<string, string> m_XroMap = new Dictionary<string, string>();
    private HashSet<string> m_CbsDirectories = new HashSet<string>();
    private string m_SystemDrive = Environment.GetEnvironmentVariable("SystemDrive");

    protected virtual void OnProgressInfo(string e)
    {
      EventHandler<string> progressInfo = this.ProgressInfo;
      if (progressInfo == null)
        return;
      progressInfo((object) this, e);
    }

    public event EventHandler<string> ProgressInfo;

    protected virtual void OnProgressWarn(string e)
    {
      EventHandler<string> progressWarn = this.ProgressWarn;
      if (progressWarn == null)
        return;
      progressWarn((object) this, e);
    }

    public event EventHandler<string> ProgressWarn;

    protected virtual void OnOverlapRaised(DriverOverlap e)
    {
      EventHandler<DriverOverlap> overlapRaised = this.OverlapRaised;
      if (overlapRaised == null)
        return;
      overlapRaised((object) this, e);
    }

    public event EventHandler<DriverOverlap> OverlapRaised;

    public int ProcessDrivers(string infSource, string outputDirectory, bool treatAsArm64)
    {
      Dictionary<string, DriverCandidate> dictionary = this.FilterDrivers(infSource, treatAsArm64);
      if (dictionary == null)
        return 0;
      this.OnProgressInfo(string.Format("正在处理 {0} 个驱动程序的文件列表...", (object) dictionary.Count));
      foreach (DriverCandidate driverCandidate in dictionary.Values)
      {
        this.m_CurrentDriverFile = Path.GetFileName(driverCandidate.InfPath).ToLowerInvariant();
        DigLib.DriverStore.NativeMethods.DriverPackageEnumFilesW(driverCandidate.PackageHandle, IntPtr.Zero, DriverPackageEnumFilesFlags.Copy | DriverPackageEnumFilesFlags.Binaries | DriverPackageEnumFilesFlags.UniqueSource, new DigLib.DriverStore.NativeMethods.PackageEnumCallback(this.EnumFilesProc), IntPtr.Zero);
        DigLib.DriverStore.NativeMethods.DriverPackageClose(driverCandidate.PackageHandle);
      }
      if (!Directory.Exists(outputDirectory))
        Directory.CreateDirectory(outputDirectory);
      if (this.m_XroMap.Count > 0)
      {
        string infPath = this.MakeAndSignXROInf(outputDirectory, treatAsArm64);
        dictionary["XROMapping.inf"] = new DriverCandidate(infPath, (Version) null, IntPtr.Zero, (List<string>) null);
      }
      this.MakeDriverPackagesFM(outputDirectory, (IEnumerable<DriverCandidate>) dictionary.Values);
      string path3 = treatAsArm64 ? "arm64" : "amd64";
      string str = Path.Combine(outputDirectory, "Volantis", path3);
      string path = Path.Combine(Directory.GetCurrentDirectory(), "Volantis", path3);
      Directory.CreateDirectory(str);
      foreach (string enumerateFile in Directory.EnumerateFiles(path))
        File.Copy(enumerateFile, Path.Combine(str, Path.GetFileName(enumerateFile)), true);
      this.CompileDriverBridgeConfig(str, treatAsArm64);
      return dictionary.Count;
    }

    private Dictionary<string, DriverCandidate> FilterDrivers(
      string infSource,
      bool treatAsArm64)
    {
      string[] strArray;
      if (Directory.Exists(infSource))
      {
        strArray = Directory.GetFiles(infSource, "*.inf", SearchOption.AllDirectories);
      }
      else
      {
        if (!File.Exists(infSource))
          return (Dictionary<string, DriverCandidate>) null;
        strArray = new string[1]{ infSource };
      }
      Dictionary<string, DriverCandidate> dictionary = new Dictionary<string, DriverCandidate>();
      this.OnProgressInfo(string.Format("正在评估 {0} 个驱动程序...", (object) strArray.Length));
      foreach (string str in strArray)
      {
        this.m_CurrentDriverFile = Path.GetFileName(str).ToLowerInvariant();
        IntPtr num = DigLib.DriverStore.NativeMethods.DriverPackageOpenW(str, treatAsArm64 ? ProcessorArchitecture.Arm64 : ProcessorArchitecture.Amd64, (string) null, DriverPackageOpenFlags.None, IntPtr.Zero);
        if (num == IntPtr.Zero)
        {
          this.OnProgressWarn(string.Format("无法打开驱动程序包 '{0}'，错误代码=0x{1:x8}", (object) this.m_CurrentDriverFile, (object) Marshal.GetLastWin32Error()));
        }
        else
        {
          DriverPackageVersionInfo pVersionInfo = new DriverPackageVersionInfo();
          pVersionInfo.Size = Marshal.SizeOf<DriverPackageVersionInfo>(pVersionInfo);
          if (!DigLib.DriverStore.NativeMethods.DriverPackageGetVersionInfoW(num, ref pVersionInfo))
          {
            this.OnProgressWarn(string.Format("无法获取 '{0}' 的版本信息，错误代码=0x{1:x8}", (object) this.m_CurrentDriverFile, (object) Marshal.GetLastWin32Error()));
            DigLib.DriverStore.NativeMethods.DriverPackageClose(num);
          }
          else if (pVersionInfo.ClassName == "Firmware")
          {
            DigLib.DriverStore.NativeMethods.DriverPackageClose(num);
          }
          else
          {
            this.m_SkipCurrentDriver = false;
            this.m_CurrentDriverHwIds.Clear();
            if (!DigLib.DriverStore.NativeMethods.DriverPackageEnumDriversW(num, 1U, new DigLib.DriverStore.NativeMethods.PackageEnumCallback(this.EnumDriversProc), IntPtr.Zero))
            {
              this.OnProgressWarn(string.Format("无法枚举 '{0}' 支持的硬件 ID，错误代码=0x{1:x8}", (object) this.m_CurrentDriverFile, (object) Marshal.GetLastWin32Error()));
              DigLib.DriverStore.NativeMethods.DriverPackageClose(num);
            }
            else if (this.m_SkipCurrentDriver)
            {
              DigLib.DriverStore.NativeMethods.DriverPackageClose(num);
            }
            else
            {
              Version version = new Version((int) pVersionInfo.DriverVersion.Major, (int) pVersionInfo.DriverVersion.Minor, (int) pVersionInfo.DriverVersion.Build, (int) pVersionInfo.DriverVersion.Revision);
              DriverCandidate driverCandidate = dictionary.ContainsKey(this.m_CurrentDriverFile) ? dictionary[this.m_CurrentDriverFile] : (DriverCandidate) null;
              if (driverCandidate != null)
              {
                if (driverCandidate.Version > version)
                {
                  DigLib.DriverStore.NativeMethods.DriverPackageClose(num);
                  continue;
                }
                if (driverCandidate.Version == version)
                {
                  ManualResetEvent waitEvent = new ManualResetEvent(false);
                  DriverOverlap e = new DriverOverlap(this.m_CurrentDriverFile, version, driverCandidate.HwIds, this.m_CurrentDriverHwIds, waitEvent);
                  this.OnOverlapRaised(e);
                  waitEvent.WaitOne();
                  if (!e.Replace)
                  {
                    DigLib.DriverStore.NativeMethods.DriverPackageClose(num);
                    continue;
                  }
                }
              }
              if (driverCandidate != null)
                DigLib.DriverStore.NativeMethods.DriverPackageClose(driverCandidate.PackageHandle);
              dictionary[this.m_CurrentDriverFile] = new DriverCandidate(str, version, num, this.m_CurrentDriverHwIds);
            }
          }
        }
      }
      return dictionary;
    }

    private bool EnumDriversProc(IntPtr hDriverPackage, IntPtr dataPtr, IntPtr lParam)
    {
      DriverInfo structure = Marshal.PtrToStructure<DriverInfo>(dataPtr);
      if (structure.HardwareDescription.ToLowerInvariant().Contains("firmware update"))
        this.m_SkipCurrentDriver = true;
      if (!string.IsNullOrEmpty(structure.HardwareId))
        this.m_CurrentDriverHwIds.Add(structure.HardwareId);
      return true;
    }

    private bool EnumFilesProc(IntPtr hDriverPackage, IntPtr dataPtr, IntPtr lParam)
    {
      DriverFile structure = Marshal.PtrToStructure<DriverFile>(dataPtr);
      string destinationPath = structure.DestinationPath;
      if (destinationPath.StartsWith(".\\"))
        return true;
      string str1 = destinationPath.Replace(this.m_SystemDrive, "\\Device\\BootDevice");
      string str2 = Utils.AliasPath(str1);
      if (str2 != null)
        this.m_CbsDirectories.Add(str2);
      this.m_XroMap[Path.Combine(this.m_CurrentDriverFile, structure.SourcePath, structure.SourceFile)] = Path.Combine(str1, structure.DestinationFile);
      return true;
    }

    private string MakeAndSignXROInf(string outputDirectory, bool treatAsArm64)
    {
      string str1 = Path.Combine(outputDirectory, "XRO");
      Directory.CreateDirectory(str1);
      string str2 = treatAsArm64 ? "arm64" : "amd64";
      List<string> contents = new List<string>()
      {
        "[Version]",
        "Signature = \"$WINDOWS NT$\"",
        "Class=System",
        "ClassGUID={4D36E97D-E325-11CE-BFC1-08002BE10318}",
        "Provider=%ManufacturerName% ",
        "CatalogFile=XROMapping.cat",
        "DriverVer=01/01/2021,1.3.3.7",
        "PnpLockDown=1",
        "",
        "[Manufacturer]",
        "%ManufacturerName%=Standard,NT" + str2,
        "",
        "[Standard.NT" + str2 + "]",
        "%XROMapping.DeviceDesc%  = NullInstallSection,ACPI\\MSHW0000",
        "",
        "[NullInstallSection.NT" + str2 + "]",
        "AddReg=XROMapping",
        "",
        "[NullInstallSection.NT" + str2 + ".Services]",
        "AddService = ,2  ; no value for the service name",
        "",
        "[XROMapping]"
      };
      foreach (KeyValuePair<string, string> xro in this.m_XroMap)
        contents.Add("HKLM,\"SYSTEM\\CurrentControlSet\\Services\\XRO\\Parameters\\BspVolumeFileRedirectionMappings\",\"" + xro.Value + "\",%REG_SZ%,\"" + xro.Key + "\"");
      contents.AddRange((IEnumerable<string>) new string[9]
      {
        "",
        "[Strings]",
        "REG_SZ = 0x00000000",
        "REG_EXPAND_SZ = 0x00020000",
        "REG_DWORD = 0x00010001",
        "",
        "ManufacturerName=\"Volantis\"",
        "",
        "XROMapping.DeviceDesc = \"XRO BSP-to-MainOS file map\""
      });
      string path = Path.Combine(str1, "XROMapping.inf");
      File.WriteAllLines(path, (IEnumerable<string>) contents);
      Utils.CreateInfCatalog(str1, treatAsArm64);
      Utils.TestSign(Path.Combine(str1, "xromapping.cat"));
      return path;
    }

    private string MakeDriverPackagesFM(
      string outputDirectory,
      IEnumerable<DriverCandidate> candidates)
    {
      List<string> contents = new List<string>()
      {
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<FeatureManifest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://schemas.microsoft.com/embedded/2004/10/ImageUpdate\" Revision=\"1\" SchemaVersion=\"1.2\">",
        "  <Drivers>",
        "    <BaseDriverPackages>"
      };
      foreach (DriverCandidate candidate in candidates)
        contents.Add("      <DriverPackageFile Name=\"" + Path.GetFileName(candidate.InfPath) + "\" Path=\"" + Path.GetFullPath(Path.GetDirectoryName(candidate.InfPath)) + "\" />");
      contents.AddRange((IEnumerable<string>) new string[3]
      {
        "    </BaseDriverPackages>",
        "  </Drivers>",
        "</FeatureManifest>"
      });
      string path = Path.Combine(outputDirectory, "DeviceDriverPackagesFM.xml");
      File.WriteAllLines(path, (IEnumerable<string>) contents);
      return path;
    }

    private string CompileDriverBridgeConfig(string outputDirectory, bool treatAsArm64)
    {
      XDocument xdocument = XDocument.Load("DriverBridgeConfig.xml");
      XElement xelement1 = xdocument.Element((XName) "{urn:schemas-microsoft-com:asm.v3}assembly");
      XElement xelement2 = xelement1.Element((XName) "{urn:schemas-microsoft-com:asm.v3}assemblyIdentity");
      XElement xelement3 = xelement1.Element((XName) "{urn:schemas-microsoft-com:asm.v3}directories");
      string str1 = xelement2.Attribute((XName) "name").Value;
      string version = xelement2.Attribute((XName) "version").Value;
      string str2 = xelement2.Attribute((XName) "publicKeyToken").Value;
      string[] source = str1.Split('-');
      xelement2.Attribute((XName) "processorArchitecture").Value = treatAsArm64 ? "arm64" : "amd64";
      if (this.m_CbsDirectories.Count > 0)
      {
        foreach (string cbsDirectory in this.m_CbsDirectories)
        {
          XElement content = new XElement((XName) "{urn:schemas-microsoft-com:asm.v3}directory");
          content.Add((object) new XAttribute((XName) "destinationPath", (object) cbsDirectory));
          xelement3.Add((object) content);
        }
      }
      else
        xelement3.Remove();
      CbsPackage cbs = new CbsPackage()
      {
        BuildType = BuildType.Release,
        BinaryPartition = false,
        Owner = source[0],
        Partition = "mainos",
        OwnerType = PhoneOwnerType.OEM,
        PhoneReleaseType = PhoneReleaseType.Production,
        ReleaseType = "Feature Pack",
        HostArch = treatAsArm64 ? CpuArch.ARM64 : CpuArch.AMD64,
        Version = new Version(version),
        Component = string.Join(".", ((IEnumerable<string>) source).Skip<string>(1)),
        PackageName = str1 + "-Package",
        SubComponent = "",
        PublicKey = str2
      };
      string assemblyId = Utils.CreateAssemblyID(cbs);
      string str3 = Path.Combine(Path.GetTempPath(), assemblyId) + ".manifest";
      xdocument.Save(str3);
      cbs.AddFile(FileType.Manifest, str3, "\\windows\\WinSxS\\" + assemblyId + ".manifest", cbs.PackageName);
      cbs.Validate();
      string cabPath = Path.Combine(outputDirectory, string.Format("{0}~31bf3856ad364e35~{1}~~.cab", (object) cbs.PackageName, (object) cbs.HostArch));
      cbs.SaveCab(cabPath);
      File.Delete(str3);
      return cabPath;
    }
  }
}
