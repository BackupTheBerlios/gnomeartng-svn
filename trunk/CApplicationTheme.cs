/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Thomas Beck at 00:05 03.12.2007
*/

using System;
using System.IO;
using Mono.Unix;

namespace GnomeArtNG
{
	
	
	public class CApplicationTheme:CTheme
	{
		private GConf.Client client; 
		private string previousApplicationTheme="";
		private static string GConfApplicationKey ="/desktop/gnome/interface/gtk_theme";
		private System.Text.StringBuilder ConOutp;
			
		override protected void PreInstallation(CStatusWindow sw){
			string tarParams="";
			client = new GConf.Client();
			
			LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			string InstallThemeFile=config.ApplicationInstallPath+Path.GetFileName(DownloadUrl);

			tarParams=CUtility.GetTarParams(Path.GetExtension(DownloadUrl));
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtDownloadTheme);
			//Herunterladen			
			if (!File.Exists(InstallThemeFile))
				GetThemeFile(sw);
			sw.SetProgress("1/"+installationSteps);
			//Entpacken
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtExtracting);
			Console.WriteLine("Command: tar"+tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
			ConOutp = CUtility.Execute("tar",tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
			sw.SetProgress("2/"+installationSteps);
			//Sichern
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtSavingForRestore);
			previousApplicationTheme = (string)client.Get(GConfApplicationKey);
			sw.SetProgress("3/"+installationSteps);
		}
		
		override protected void Installation(CStatusWindow sw){
			//Installieren
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtInstalling);
			client.Set(GConfApplicationKey,ConOutp.ToString().Split('/')[0]);
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtInstallDone);
		}

		override protected void PostInstallation(CStatusWindow sw){
			//Revert verfügbar machen
			revertIsAvailable=true;
		}
	
		override public void Revert(){
			if (revertIsAvailable){
				new GConf.Client().Set(GConfApplicationKey,previousApplicationTheme);
				revertIsAvailable=false;
			}
		}
		
		public CApplicationTheme(CConfiguration config):base(config) {
			installationSteps=4;
		}
	}
}
