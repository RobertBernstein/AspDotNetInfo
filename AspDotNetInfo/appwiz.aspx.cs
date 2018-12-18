using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace Appwiz
{
	public partial class Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void DisplayListOfStrings(List<String> listOfStrings)
		{
			Response.Write("<p>");
			foreach (var s in listOfStrings)
			{
				Response.Write(s + "<br />");
			}
			Response.Write("</p>");
		}

		protected List<String> GetSysInfo()
		{
			var sysInfo = new List<String>(100)
			              	{
			              		Environment.MachineName,
								Environment.OSVersion.Platform.ToString(),
								Environment.OSVersion.VersionString,
								"Processor Count: " + System.Environment.ProcessorCount.ToString(),
								Environment.SystemDirectory,
								Environment.UserDomainName,
								Environment.Version.ToString(),
			              	};

			sysInfo.Sort();

			return sysInfo;
		}

		protected List<String> GetServerVariables()
		{
			var serverVariables = new List<String>(100);
			for (var variableNumber = 0; variableNumber < Request.ServerVariables.Count; variableNumber++)
			{
				var insideVals = Request.ServerVariables.GetValues(Request.ServerVariables.AllKeys[variableNumber]);
				for (var loop2 = 0; loop2 < insideVals.Length; loop2++)
				{
					serverVariables.Add(Request.ServerVariables.AllKeys[variableNumber] + " = " + insideVals[loop2] + "<br />");
				}
			}
			serverVariables.Sort();

			return serverVariables;
		}

		protected List<String> GetInstalledProgramList()
		{
			List<String> installedPrograms = null;
			RegistryKey key;
			const string baseSubkey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
			try
			{
				key = Registry.LocalMachine.OpenSubKey(baseSubkey);

				if (key == null)
				{
					return null;
				}

				installedPrograms = new List<String>(key.SubKeyCount);

				for (var subKeyNumber = 0; subKeyNumber < key.SubKeyCount; subKeyNumber++)
				{
					var currentSubkey = key.GetSubKeyNames()[subKeyNumber];
					if (currentSubkey != null)
					{
						var appKey = Registry.LocalMachine.OpenSubKey(Path.Combine(baseSubkey, currentSubkey));
						if (appKey != null)
						{
							var displayName = appKey.GetValue("DisplayName") ?? currentSubkey;
							installedPrograms.Add(displayName.ToString());
							appKey.Close();
						}
					}
				}

				key.Close();
			}
			catch (Exception ex)
			{
				Response.Write("Exception Thrown: " + ex.Message);
			}

			if (installedPrograms != null)
			{
				installedPrograms.Sort();
			}

			return installedPrograms;
		}
	}
}
