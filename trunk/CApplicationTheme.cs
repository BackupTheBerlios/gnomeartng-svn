// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CApplicationTheme.cs created with MonoDevelop
// User: neo at 00:05 03.12.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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

			tarParams=config.GetTarParams(Path.GetExtension(DownloadUrl));
			if (!File.Exists(InstallThemeFile)){
				//Herunterladen
				DownloadFile(DownloadUrl, LocalThemeFile);
			}
			//Entpacken
			Console.WriteLine("Command: tar"+tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
			ConOutp = config.Execute("tar",tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);

			//Sichern
			previousApplicationTheme = (string)client.Get(GConfApplicationKey);
		}
		
		override protected void Installation(CStatusWindow sw){
			//Installieren
			client.Set(GConfApplicationKey,ConOutp.ToString().Split('/')[0]);
		}

		override protected void PostInstallation(CStatusWindow sw){
			//config.Execute("gnome-appearance-properties","");
			//Revert verfügbar machen
			revertIsAvailable=true;
		}
	
		override public void Revert(){
			if (revertIsAvailable){
				new GConf.Client().Set(GConfApplicationKey,previousApplicationTheme);
				revertIsAvailable=false;
			}
		}
		
		public CApplicationTheme(CConfiguration config):base(config)
		{
		}
	}
}
