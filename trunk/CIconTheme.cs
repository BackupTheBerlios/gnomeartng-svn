// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CIconTheme.cs created with MonoDevelop
// User: neo at 11:50 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;

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
			
		override public void Revert(){
			client = new GConf.Client();
			client.Set(GConfIconThemeKey,prevIconTheme);
			revertIsAvailable=false;
		}

		override public void Install(){
			if (!config.TarIsAvailable)
				throw new Exception("Installation is not possible - Tar is missing");
			string tarParams="";
			string LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			
			tarParams=config.GetTarParams(DownloadUrl);
			
			//Herunterladen
			if (!File.Exists(LocalThemeFile))
				DownloadFile(DownloadUrl, LocalThemeFile);
			
			//Entpacken
			System.Text.StringBuilder ConOutp = config.Execute("tar",tarParams+LocalThemeFile+" -C "+config.IconInstallPath);
			string[] Folder = ConOutp.ToString().Split('/');
			
			//Einrichten
			client = new GConf.Client();
			prevIconTheme=(string)client.Get(GConfIconThemeKey);
			client.Set(GConfIconThemeKey,Folder[0]);
			
			revertIsAvailable=true;
		
		}
	
		public CIconTheme(CConfiguration config):base(config)	{
			
		}
			
	}
}
