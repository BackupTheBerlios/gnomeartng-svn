/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

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
			
		override public void Revert(){
			client = new GConf.Client();
			client.Set(GConfIconThemeKey,prevIconTheme);
			revertIsAvailable=false;
		}

		override protected void PreInstallation(CStatusWindow sw){
			//Herunterladen
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtDownloadTheme);
			GetThemeFile(sw);
			sw.SetProgress("1/"+installationSteps);
			
			//Entpacken
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtExtracting);
			System.Collections.ArrayList al = new System.Collections.ArrayList();
			al = CUtility.UncompressFile(LocalThemeFile,Path.GetTempPath(),false, null,al);
			Folder = ((string)(al[0])).Split('/');
			//Console.WriteLine(Folder[0]);
			sw.SetProgress("2/"+installationSteps);
			
			//Convertieren
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtConvertingIcons);
			sw.SetProgress("3/"+installationSteps);
			CIconThemeManager iconmanager = new CIconThemeManager();
			iconmanager.ThemeName = Folder[0];
			iconmanager.StatusWindow = sw;
			iconmanager.ImportIcons(Path.GetTempPath()+Folder[0]);
			sw.SetProgress("4/"+installationSteps);
			iconmanager.SaveAllIcons(config.IconInstallPath+"/",true);
			
			//Sichern
			client = new GConf.Client();
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtSavingForRestore);
			prevIconTheme=(string)client.Get(GConfIconThemeKey);
			sw.SetProgress("5/"+installationSteps); 
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
			installationSteps=6;
		}
			
	}
}
