/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
Thomas Beck at 11:50 24.11.2007
*/

using System;
using System.IO;
using Mono.Unix;

namespace GnomeArtNG
{
	

	public class CIconTheme: CTheme
	{
		
		private GConf.Client client; 
		// Die zu verändernden Werte in der Gnome Registry
		private static string GConfAppPath = "/desktop/gnome/interface";
		private static string GConfIconThemeKey = GConfAppPath + "/icon_theme";
		
		//Vor dem Installieren eingetragenes Theme
		private string prevIconTheme="";
		//Beim Installieren benötigt
		private string[] Folder;
		System.Text.StringBuilder ConOutp;
			
		override public void Revert(){
			client = new GConf.Client();
			client.Set(GConfIconThemeKey,prevIconTheme);
			revertIsAvailable=false;
		}

		override protected void PreInstallation(CStatusWindow sw){
			if (!installationIsPossible)
				throw new Exception("Installation is not possible - Tar is missing");
			string tarParams="";
			
			tarParams=config.GetTarParams(DownloadUrl);
			//Herunterladen
			GetThemeFile(sw);
			sw.SetProgress("1/"+installationSteps);
			
			//Entpacken
			ConOutp = config.Execute("tar",tarParams+LocalThemeFile+" -C "+config.IconInstallPath);
			Console.WriteLine(ConOutp.ToString());
			Folder = ConOutp.ToString().Split('/');
			sw.SetProgress("2/"+installationSteps);
			
			//Sichern
			client = new GConf.Client();
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtSavingForRestore);
			prevIconTheme=(string)client.Get(GConfIconThemeKey);
			sw.SetProgress("3/"+installationSteps);
		}
		
		override protected void Installation(CStatusWindow sw){
			//Installieren
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtInstalling);
			client.Set(GConfIconThemeKey,Folder[0]);
		}
		
		override protected void PostInstallation(CStatusWindow sw){
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtInstallDone);
			revertIsAvailable=true;
		}

		public CIconTheme(CConfiguration config):base(config)	{
			installationIsPossible = config.TarIsAvailable;
			installationSteps=4;
		}
			
	}
}
