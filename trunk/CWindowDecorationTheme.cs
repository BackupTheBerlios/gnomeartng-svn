// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CWindowDecorationTheme.cs created with MonoDevelop
// User: neo at 00:11 03.12.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

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
			
			LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			tarParams=config.GetTarParams(Path.GetExtension(DownloadUrl));
			sw.Mainlabel=Catalog.GetString(CConfiguration.txtDownloadTheme);
			if (!File.Exists(LocalThemeFile)){
				//Herunterladen
				DownloadFile(DownloadUrl, LocalThemeFile,sw.DetailProgressBar);
			}
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
