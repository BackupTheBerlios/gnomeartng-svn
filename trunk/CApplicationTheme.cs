// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CApplicationTheme.cs created with MonoDevelop
// User: neo at 00:05 03.12.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

namespace GnomeArtNG
{
	
	
	public class CApplicationTheme:CTheme
	{
		override public void Install(){
			string tarParams="";
			string LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			string InstallThemeFile=config.ApplicationInstallPath+Path.GetFileName(DownloadUrl);

			tarParams=config.GetTarParams(Path.GetExtension(DownloadUrl));
			if (!File.Exists(InstallThemeFile)){
				//Herunterladen
				webclient.DownloadFile(DownloadUrl, LocalThemeFile);
				//Entpacken
				Console.WriteLine("Command: tar"+tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
				System.Text.StringBuilder ConOutp = config.Execute("tar",tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
				//Eigenschaftendialog ausführen
				//TODO: GCONF wert setzen in desktop/gnome/---
				config.Execute("gnome-appearance-properties","");
			}
			
			//Installieren

			
			//Revert verfügbar machen
			revertIsAvailable=true;
			/*
            command = "tar #{command_param} #{temp_file} -C " + folder
            system(command), dann nachfragen
            system("gnome-theme-manager &")
			 */
		}
		override public void Revert(){}
		public CApplicationTheme(CConfiguration config):base(config)
		{
		}
	}
}
