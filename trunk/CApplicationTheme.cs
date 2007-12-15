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
		private GConf.Client client; 
		private string previousApplicationTheme="";
		static string GConfApplicationKey ="/desktop/gnome/interface/gtk_theme";
		override public void Install(){
			string tarParams="";
			string LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			string InstallThemeFile=config.ApplicationInstallPath+Path.GetFileName(DownloadUrl);

			tarParams=config.GetTarParams(Path.GetExtension(DownloadUrl));
			if (!File.Exists(InstallThemeFile)){
				//Herunterladen
				webclient.DownloadFile(DownloadUrl, LocalThemeFile);
			}
			//Entpacken
			Console.WriteLine("Command: tar"+tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
			System.Text.StringBuilder ConOutp = config.Execute("tar",tarParams+LocalThemeFile+" -C "+config.ApplicationInstallPath);
			//Installieren
			client = new GConf.Client();
			previousApplicationTheme = (string)client.Get(GConfApplicationKey);
			client.Set(GConfApplicationKey,ConOutp.ToString().Split('/')[0]);
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
