/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

using System;
using Mono.Unix;
using System.IO;

namespace GnomeArtNG
{
	public class CWindowDecorationTheme:CTheme
	{
		private GConf.Client client; 
		private string previousDecorationTheme="";
		private static string GConfDecorationKey ="/apps/metacity/general/theme";
		private System.Text.StringBuilder ConOutp;
			
		override protected void PreInstallation(CStatusWindow sw){
			string tarParams="";
			client = new GConf.Client();

			tarParams=config.GetTarParams(Path.GetExtension(DownloadUrl));
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtDownloadTheme);
	
			if (!File.Exists(LocalThemeFile))
				GetThemeFile(sw);
			sw.SetProgress("1/"+installationSteps);
			//Entpacken
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtExtracting);
			Console.WriteLine("Command: tar"+tarParams+LocalThemeFile+" -C "+config.DecorationInstallPath);
			ConOutp = config.Execute("tar",tarParams+LocalThemeFile+" -C "+config.DecorationInstallPath);
			sw.SetProgress("2/"+installationSteps);
			//Sichern
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtSavingForRestore);
			previousDecorationTheme = (string)client.Get(GConfDecorationKey);
			sw.SetProgress("3/"+installationSteps);
		}
		
		override protected void Installation(CStatusWindow sw){
			//Installieren
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtInstalling);
			System.Threading.Thread.Sleep(500);
			client.Set(GConfDecorationKey,ConOutp.ToString().Split('/')[0]);
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtInstallDone);
		}

		override protected void PostInstallation(CStatusWindow sw){
			//Revert verfügbar machen
			revertIsAvailable=true;
		}
	
		override public void Revert(){
			if (revertIsAvailable){
				new GConf.Client().Set(GConfDecorationKey,previousDecorationTheme);
				revertIsAvailable=false;
			}
		}
				
		public CWindowDecorationTheme(CConfiguration config):base(config) {
			installationSteps=4;
		}
	}
}
