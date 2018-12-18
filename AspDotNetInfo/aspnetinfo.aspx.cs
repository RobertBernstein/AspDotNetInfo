// --------------------------------------------------------------------------------------------------------------------
// <copyright file="aspnetinfo.aspx.cs" company="Tardis Technologies">
//   Copyright 2013 Tardis Technologies. All rights reserved.
// </copyright>
// <summary>
//   Defines the AspDotNetInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AspDotNetInfo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using Microsoft.Win32;

    /// <summary>
    /// The asp dot net info.
    /// </summary>
    public partial class AspDotNetInfo : System.Web.UI.Page
    {
        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string progress = "Just starting";
            try
            {
                string dotNetVersionFromEnvironmentVariable = this.GetDotNetVersionFromEnvironment();
                string dotNetVersionFromMscorcfgFile = this.GetDotNetVersionFromMscorcfgDll();
                string dotNetVersionFromSystemCoreFile = this.GetDotNetVersionFromSystemCoreFile();
                string dotNetVersionFromRegistry = Get45PlusFromRegistry();
                progress = "Got .NET version information.";

                int days, hours, minutes, seconds;
                int tickCount = this.DetermineSystemUptime(out days, out hours, out minutes, out seconds);
                DateTime windowsInstallationDateTime = this.GetWindowsInstallationDate();
                progress = "Got Windows installation date and time.";

                Dictionary<string, string> serverInformation = new Dictionary<string, string>
                                            {
                                                {
                                                    "Operating System",
                                                    Environment.OSVersion.VersionString
                                                },
                                                {
                                                    "Windows Installation Date",
                                                                    windowsInstallationDateTime.ToLongDateString() + " "
                                                                    + windowsInstallationDateTime.ToLongTimeString()
                                                                },
                                                                {
                                                                    "Processor Count",
                                                                    Environment.ProcessorCount.ToString(
                                                                        CultureInfo.InvariantCulture)
                                                                },
                                                { "Machine Name", Environment.MachineName },
                                                { "User Domain Name", Environment.UserDomainName },
                                                {
                                                    ".NET Runtime Version (from Environment)",
                                                    dotNetVersionFromEnvironmentVariable
                                                },
                                                {
                                                    ".NET Runtime Version (from Mscorcfg.dll)",
                                                    dotNetVersionFromMscorcfgFile
                                                },
                                                {
                                                    ".NET Runtime Version (from System.Core.dll)",
                                                    dotNetVersionFromSystemCoreFile
                                                },
                                                {
                                                    ".NET Runtime Version (from Registry)",
                                                    dotNetVersionFromRegistry
                                                },
                                                {
                                                    "System Uptime",
                                                    string.Format(
                                                        "{0} days, {1} hours, {2} minutes, {3} seconds ({4} ticks total)",
                                                        days,
                                                        hours,
                                                        minutes,
                                                        seconds,
                                                        tickCount)
                                                },
                                                { "Windows Directory", Environment.GetEnvironmentVariable("windir") },
                                                { "System Directory", Environment.SystemDirectory }
                                            };

                this.ServerInformation.DataSource = serverInformation;
                this.ServerInformation.DataBind();

                this.RequestHeaders.DataSource = this.Request.Headers;
                this.RequestHeaders.DataBind();

                this.EnvironmentVars.DataSource = Environment.GetEnvironmentVariables();
                this.EnvironmentVars.DataBind();

                this.ServerVariables.DataSource = this.Request.ServerVariables;
                this.ServerVariables.DataBind();

                this.LogicalDrives.DataSource = this.GetLogicalDrives();
                this.LogicalDrives.DataBind();

                this.InstalledPrograms.DataSource = this.GetInstalledProgramList();
                this.InstalledPrograms.DataBind();
            }
            catch (Exception exception)
            {
                this.DisplayErrorMessage(exception.Message + " (Progress: " + progress + ")");
            }
        }

        protected DateTime GetWindowsInstallationDate()
        {
            const string BaseSubkey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(BaseSubkey);

                if (key != null)
                {
                    var startDate = new DateTime(1970, 1, 1, 0, 0, 0);
                    long secondsSinceJanuaryFirst1970 = Convert.ToInt64(key.GetValue("InstallDate").ToString());
                    key.Close();

                    if (secondsSinceJanuaryFirst1970 != 0)
                    {
                        return startDate.AddSeconds(secondsSinceJanuaryFirst1970);
                    }

                    // In Windows 7, this actually provides the wrong date -> it's actually the build date of the directory
                    // from the Windows CD/DVD
                    var windowsDirectory = Environment.GetEnvironmentVariable("windir");
                    if (windowsDirectory == null)
                    {
                        return DateTime.MinValue;
                    }

                    var di = new DirectoryInfo(windowsDirectory);
                    return di.CreationTime;
                }
            }
            catch (Exception exception)
            {
                this.Response.Write("Exception Thrown: " + exception.Message);
            }

            return DateTime.MinValue;
        }

        private string GetDotNetVersionFromEnvironment()
        {
            string dotNetVersionFromEnvironmentVariable;

            try
            {
                dotNetVersionFromEnvironmentVariable =
                    this.DotNetVersions[Environment.Version.ToString().Trim()] +
                    " (" + Environment.Version.ToString().Trim() + ")";
            }
            catch (Exception exception)
            {
                dotNetVersionFromEnvironmentVariable = "Unknown";

                var methodName = MethodBase.GetCurrentMethod().Name;
                this.DisplayErrorMessage($"Error in {methodName}: Version unknown.<br />Exception: {exception.Message}");
            }

            return dotNetVersionFromEnvironmentVariable;
        }

        protected string GetDotNetVersionFromMscorcfgDll()
        {
            string mscorcfgVersion = null;
            const string AssemblyFilename = "mscorcfg.dll";
            var mscorcfgVersionNumber = this.GetDotNetAssemblyVersion(@"GAC_32\mscorcfg", AssemblyFilename) ??
                                       (this.GetDotNetAssemblyVersion(@"GAC_64\mscorcfg", AssemblyFilename) ??
                                       (this.GetDotNetAssemblyVersion(@"GAC_MSIL\mscorcfg", AssemblyFilename) ??
                                       this.GetDotNetAssemblyVersion(@"GAC\mscorcfg", AssemblyFilename)));

            try
            {
                mscorcfgVersion = this.DotNetVersions[mscorcfgVersionNumber];
            }
            catch (Exception exception)
            {
                var methodName = MethodBase.GetCurrentMethod().Name;
                this.DisplayErrorMessage(
                    $"Error in {methodName}: Version {mscorcfgVersionNumber} was not found in the list of known " +
                    $".NET versions.<br />Exception: {exception.Message}");
            }

            if (!string.IsNullOrEmpty(mscorcfgVersion))
            {
                mscorcfgVersion += " (" + mscorcfgVersionNumber + ")";
            }
            else
            {
                mscorcfgVersion = "Unknown";
            }

            return mscorcfgVersion;
        }

        protected string GetDotNetVersionFromSystemCoreFile()
        {
            string systemCoreVersion = null;
            const string AssemblyFilename = "System.Core.dll";
            var systemCoreVersionNumber = this.GetDotNetAssemblyVersion(@"GAC_32\System.Core", AssemblyFilename) ??
                                       (this.GetDotNetAssemblyVersion(@"GAC_64\System.Core", AssemblyFilename) ??
                                       (this.GetDotNetAssemblyVersion(@"GAC_MSIL\System.Core", AssemblyFilename) ??
                                       this.GetDotNetAssemblyVersion(@"GAC\System.Core", AssemblyFilename)));

            try
            {
                systemCoreVersion = this.DotNetVersions[systemCoreVersionNumber];
            }
            catch (Exception exception)
            {
                var methodName = MethodBase.GetCurrentMethod().Name;
                this.DisplayErrorMessage(
                    $"Error in {methodName}: Version {systemCoreVersionNumber} was not found in the list of known " +
                    $".NET versions.<br />Exception: {exception.Message}");
            }

            if (!string.IsNullOrEmpty(systemCoreVersion))
            {
                systemCoreVersion += " (" + systemCoreVersionNumber + ")";
            }
            else
            {
                systemCoreVersion = "Unknown";
            }

            return systemCoreVersion;
        }

        /// <summary>
        /// Gets the installed version of the .NET Framework from the registry.
        /// </summary>
        /// <remarks>
        /// See https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed
        /// </remarks>
        protected static string Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    return ".NET Framework Version: " + CheckFor45PlusVersion((int)ndpKey.GetValue("Release"));
                }
                else
                {
                    return ".NET Framework Version 4.5 or later is not detected.";
                }
            }
        }

        // Checking the version using >= will enable forward compatibility.
        protected static string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 461808)
                return "4.7.2 or later";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        protected string GetDotNetAssemblyVersion(string pathToAssembly, string assemblyFilename)
        {
            var windowsDirectory = Environment.GetEnvironmentVariable("windir");
            if (windowsDirectory == null)
            {
                return null;
            }

            var baseDotNetFolder = Path.Combine(windowsDirectory, "assembly");

            if (!string.IsNullOrEmpty(windowsDirectory))
            {
                string dotNetAssemblyFolder = Path.Combine(baseDotNetFolder, pathToAssembly);

                if (Directory.Exists(dotNetAssemblyFolder))
                {
                    string[] dotNetAssemblySubdirs = Directory.GetDirectories(dotNetAssemblyFolder);
                    string dllFolder = Path.Combine(dotNetAssemblyFolder, dotNetAssemblySubdirs[dotNetAssemblySubdirs.Length - 1]);

                    string assemblyDll = Path.Combine(dllFolder, assemblyFilename);
                    if (File.Exists(assemblyDll))
                    {
                        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyDll);
                        if (fileVersionInfo.ProductVersion != null)
                        {
                            return fileVersionInfo.ProductVersion;
                        }
                    }
                }
            }

            return null;
        }

        protected string GetSystemCoreVersion()
        {
            ////String baseDotNetFolder = Path.Combine(Environment.GetEnvironmentVariable("windir"), @"Microsoft.NET\Framework");

            if (Environment.GetEnvironmentVariable("windir") != null)
            {
                string windowsDirectory = Environment.GetEnvironmentVariable("windir");
                if (!string.IsNullOrEmpty(windowsDirectory))
                {
                    string systemCoreFolder = Path.Combine(windowsDirectory, @"assembly\GAC_MSIL\System.Core");

                    if (Directory.Exists(systemCoreFolder))
                    {
                        string[] systemCoreSubdirs = Directory.GetDirectories(systemCoreFolder);
                        string dllFolder = Path.Combine(systemCoreFolder, systemCoreSubdirs[systemCoreSubdirs.Length - 1]);

                        string systemCoreDll = Path.Combine(dllFolder, "System.Core.dll");
                        if (File.Exists(systemCoreDll))
                        {
                            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(systemCoreDll);
                            if (fileVersionInfo.ProductVersion != null)
                            {
                                return fileVersionInfo.ProductVersion;
                            }
                        }
                    }
                }
            }

            return null;
        }

        protected int DetermineSystemUptime(out int days, out int hours, out int minutes, out int seconds)
        {
            var tickCount = Environment.TickCount & int.MaxValue;

            days = tickCount / (1000 * 60 * 60 * 24);
            hours = tickCount / (1000 * 60 * 60) - (days * 24);
            minutes = tickCount / (1000 * 60) - ((days * 24 * 60) + (hours * 60));
            seconds = tickCount / 1000 - ((days * 24 * 60 * 60) + (hours * 60 * 60) + (minutes * 60));

            return tickCount;
        }

        protected List<string> GetInstalledProgramList()
        {
            List<string> installedPrograms = null;
            const string BaseSubkey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(BaseSubkey);

                if (key == null)
                {
                    return null;
                }

                installedPrograms = new List<string>(key.SubKeyCount);

                for (var subKeyNumber = 0; subKeyNumber < key.SubKeyCount; subKeyNumber++)
                {
                    var currentSubkey = key.GetSubKeyNames()[subKeyNumber];
                    if (currentSubkey != null)
                    {
                        var appKey = Registry.LocalMachine.OpenSubKey(Path.Combine(BaseSubkey, currentSubkey));
                        if (appKey != null)
                        {
                            object displayName = appKey.GetValue("DisplayName") ?? currentSubkey;
                            installedPrograms.Add(displayName.ToString());
                            appKey.Close();
                        }
                    }
                }

                key.Close();
            }
            catch (Exception ex)
            {
                this.DisplayErrorMessage("GetInstalledProgramList Exception Thrown: " + ex.Message);
            }

            if (installedPrograms != null)
            {
                installedPrograms.Sort();
            }

            return installedPrograms;
        }

        protected Dictionary<string, DriveInfo> GetLogicalDrives()
        {
            var allDrives = DriveInfo.GetDrives();

            var logicalDrives = new Dictionary<string, DriveInfo>(allDrives.Length);
            foreach (var driveInfo in allDrives)
            {
                if (driveInfo.IsReady)
                {
                    logicalDrives.Add(driveInfo.Name, driveInfo);
                }
            }

            return logicalDrives;
        }

        protected void DisplayErrorMessage(string errorText)
        {
            this.Response.Write("<p>Error: " + errorText + ".</p>");
        }

        // See http://msdn.microsoft.com/en-us/kb/kb00318785.aspx,
        // http://dzaebel.net/NetVersions.htm, and
        // http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions
        // https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/versions-and-dependencies
        protected Dictionary<string, string> DotNetVersions = new Dictionary<string, string>
        {
            {
                "1.0.2204.21",
                "1.0 Beta 1, Nov 2000"
            },
            {
                "1.0.2914.16",
                "1.0 Beta 2, Jun 2001"
            },
            {
                "1.0.3512.0",
                "1.0 Pre-release RC3 (Visual Studio.NET 2002 RC3)"
            },
            {
                "1.0.3705.0",
                "1.0 Original Release, RTM (Visual Studio.NET 2002), Feb 2002"
            },
            { "1.0.3705.209", "1.0 Service Pack 1, Mar 2002" },
            { "1.0.3705.288", "1.0 Service Pack 2, Aug 2002" },
            {
                "1.0.3705.6018", "1.0 Service Pack 3, Aug 2004"
            },
            { "1.1.4322.510", "1.1 Final Beta, Oct 2002" },
            {
                "1.1.4322.573",
                "1.1 Original Release, RTM (Visual Studio.NET 2003 / Windows Server 2003), Feb 2003"
            },
            {
                "1.1.4322.2032", "1.1 Service Pack 1, Aug 2004"
            },
            {
                "1.1.4322.2300",
                "1.1 Service Pack 1 (Windows Server 2003 32-bit version*)"
            },
            {
                "1.1.4322.2379",
                "1.1 Orcas (Visual Studio 2008) Beta 1, Mar 2007"
            },
            {
                "1.1.4322.2407",
                "1.1 Orcas (Visual Studio 2008) Beta 2, Oct 2007"
            },
            {
                "1.1.4322.2443",
                "1.1 Service Pack 1 Security Update, KB953297, Oct 2009"
            },
            {
                "1.2.21213.1",
                "1.2 Whidbey (Visual Studio 2005) pre-Alpha build"
            },
            {
                "1.2.30703.27",
                "1.2 Whidbey (Visual Studio 2005) Alpha, PDC 2004, Nov 2003"
            },
            {
                "2.0.40301.9",
                "2.0 Whidbey (Visual Studio 2005) CTP, WinHEC 2004, Mar 2004"
            },
            {
                "2.0.40607.16",
                "2.0 Visual Studio.NET 2005 Beta 1, TechEd Europe 2004, Jun 2004"
            },
            {
                "2.0.40607.42",
                "2.0 SQL Server Yukon Beta 2, Jul 2004"
            },
            {
                "2.0.40607.85",
                "2.0 Visual Studio.NET 2005 Beta 1, Team System Refresh, Aug 2004"
            },
            {
                "2.0.40903.0",
                "2.0 Whidbey (Visual Studio 2005) CTP, Visual Studio Express, Oct 2004"
            },
            {
                "2.0.41115.19",
                "2.0 Visual Studio.NET 2005 Beta 1, Team System Refresh, Dec 2004"
            },
            {
                "2.0.50110.28",
                "2.0 Visual Studio.NET 2005 CTP, Professional Edition, Feb 2005"
            },
            {
                "2.0.50215.44",
                "2.0 Visual Studio.NET 2005 Beta 2, Visual Studio Express Beta 2, Apr 2005"
            },
            {
                "2.0.50601.0",
                "2.0 Visual Studio.NET 2005 CTP, June 2005"
            },
            {
                "2.0.50215",
                "2.0 WinFX SDK for Indigo/Avalon 2005 CTP, Jul 2005"
            },
            {
                "2.0.50712",
                "2.0 Visual Studio Team System 2005 (Drop3) CTP, Jul 2005"
            },
            {
                "2.0.50727.26",
                "2.0 Visual Studio Team System 2005 Release Candidate, Oct 2005"
            },
            {
                "2.0.50727.42", "2.0 Original Release, Oct 2005"
            },
            {
                "2.0.50727.312", "2.0 Vista Ultimate, Jan 2007"
            },
            {
                "2.0.50727.762",
                "2.0 Visual Studio Team Suite 2005 Service Pack 1"
            },
            {
                "2.0.50727.832",
                "2.0 Fix x86 Visual C++ 2005, Apr 2007"
            },
            {
                "2.0.50727.867",
                "2.0 Visual Studio Express Edition 2005 Service Pack 1, Apr 2007"
            },
            {
                "2.0.50727.1366",
                "2.0 Orcas (Visual Studio 2008) Beta 1, Mar 2007"
            },
            {
                "2.0.50727.1378",
                "2.0 Orcas (Visual Studio 2008) Beta 2, Oct 2007"
            },
            {
                "2.0.50727.1433", "2.0 Service Pack 1, Nov 2007"
            },
            {
                "2.0.50727.1434",
                "2.0 Service Pack 1, Windows Server 2008 and Windows Vista SP1, Dec 2007"
            },
            {
                "2.0.50727.3031",
                "2.0 Service Pack 2 Beta 1, May 2008"
            },
            {
                "2.0.50727.3053", "2.0 Service Pack 2, Aug 2008"
            },
            {
                "2.0.50727.3074",
                "2.0/3.5 Family Update Vista/Window Server 2008, Dec 2008"
            },
            {
                "2.0.50727.3082",
                "2.0/3.5 Family Update XP/Windows Server 2003, Dec 2008"
            },
            { "2.0.50727.3603", "2.0/4.0 Beta 2, Oct 2009" },
            {
                "2.0.50727.4200",
                "2.0 Service Pack 2, KB974470, Security Update, Oct 2009"
            },
            {
                "2.0.50727.4918",
                "2.0 Windows 7 Release Candidate, Jun 2009"
            },
            {
                "2.0.50727.4927", "2.0 Windows 7 RTM, Oct 2009"
            },
            { "3.0.4506.25", "3.0 Vista Ultimate, Jan 2007" },
            {
                "3.0.4506.30", "3.0 Original Release, Nov 2006"
            },
            {
                "3.0.4506.577",
                "3.0 Orcas (Visual Studio 2008) Beta 1, Mar 2007"
            },
            {
                "3.0.4506.590",
                "3.0 Orcas (Visual Studio 2008) Beta 2, Oct 2007"
            },
            { "3.0.4506.648", "3.0 Service Pack 1" },
            {
                "3.0.4506.2062",
                "3.0 Service Pack 1 Beta 1, May 2008"
            },
            {
                "3.0.4506.2123", "3.0 Service Pack 2, Aug 2008"
            },
            {
                "3.0.4506.2152",
                "3.0 Service Pack 2 / 4.0 Beta 1, May 2009"
            },
            {
                "3.0.4506.4918",
                "3.0 Windows 7 Release Candidate, Jun 2009"
            },
            {
                "3.0.6920.1500",
                "3.0 Family Update Vista/Windows Server 2008, Dec 2008"
            },
            {
                "3.5.20526.0",
                "3.5 Orcas (Visual Studio 2008) Beta 1, Mar 2007"
            },
            {
                "3.5.20706.1",
                "3.5 Orcas (Visual Studio 2008) Beta 2, Oct 2007"
            },
            {
                "3.5.21022.8",
                "3.5 Original Release, RTM, Jan 2008"
            },
            {
                "3.5.30428.1",
                "3.5 Service Pack 1 Beta 1, May 2008"
            },
            { "3.5.30729.1", "3.5 Service Pack 1, Aug 2008" },
            {
                "3.5.30729.196",
                "3.5 Family Update Vista/Windows Server 2008, Dec 2008"
            },
            {
                "3.5.30729.4918",
                "3.5 Service Pack 1 Windows 7 Release Candidate, Jun 2009"
            },
            {
                "3.5.30729.4926",
                "3.5 Service Pack 1 Windows 7 RTM, Jun 2009"
            },
            //// Rob added this based on System.Core
            {
                "4.0.11001.1",
                "4.0 CTP, Oct 2008"
            },
            {
                "4.0.20506.1",
                "4.0 Beta 1, May 2009"
            },
            {
                "4.0.21006.1",
                "4.0 Beta 2, Oct 2009"
            },
            {
                "4.0.30128.1",
                "4.0 Release Candidate, Feb 2010"
            },
            {
                "4.0.30319.1",
                "4.0 RTM Release, April 2010"
            },
            // TODO: Verify that the values below will ever get used or just remove them.
            {
                "4.5",
                "4.5"
            },
            {
                "4.5.1",
                "4.5.1"
            },
            {
                "4.5.2",
                "4.5.2"
            },
            {
                "4.6",
                "4.6"
            },
            {
                "4.6.1",
                "4.6.1"
            },
            {
                "4.6.2",
                "4.6.2"
            },
            {
                "4.7",
                "4.7"
            },
            {
                "4.7.1",
                "4.7.1"
            },
            {
                "4.7.2",
                "4.7.2"
            }
        };
    }
}