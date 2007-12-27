// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CSplashTheme.cs created with MonoDevelop
// User: neo at 11:51 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using GConf;
using System.Net;
using System.IO;
using Mono.Unix;

namespace GnomeArtNG
{
	
	
	public class CSplashTheme:CTheme
	{

		private GConf.Client client; 
		// Die zu verändernden Werte in der Gnome Registry
		static string GConfAppPath = "/apps/gnome-session/options";
		static string GConfSplashImageKey = GConfAppPath + "/splash_image";
		static string GConfShowSplashKey = GConfAppPath + "/show_splash_screen";
		
		//War vor dem Install der Splashscreen aktiv?
		private bool splashWasActive=false;
		//Alter Splashscreen
		private string prevSplash="";
		
		//zum Installieren benötigte Vars
		private string InstallThemeFile;
		
		override public void Revert(){
			if (revertIsAvailable){
				client = new GConf.Client();
				client.Set(GConfShowSplashKey,splashWasActive);
				client.Set(GConfSplashImageKey,prevSplash);
				revertIsAvailable=false;
			}
		}
		
		override protected void PreInstallation(CStatusWindow sw){
			LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			InstallThemeFile=config.SplashInstallPath+Path.GetFileName(DownloadUrl);
			//Neuer GConfClient
			client = new GConf.Client();
			//Sicherung
			sw.Mainlabel=CConfiguration.txtSavingForRestore;
			splashWasActive = (bool)client.Get(GConfShowSplashKey);
			prevSplash = (string)client.Get(GConfSplashImageKey);
			sw.SetProgress("1/"+installationSteps);
			sw.Mainlabel=CConfiguration.txtDownloadTheme;
			//Herunterladen
			if (!File.Exists(InstallThemeFile)){
				DownloadFile(DownloadUrl, LocalThemeFile,sw.DetailProgressBar);
				File.Copy(LocalThemeFile,InstallThemeFile);
			}
			sw.SetProgress("2/"+installationSteps);
		}

		override protected void Installation(CStatusWindow sw){
			//Installieren
			sw.Mainlabel=CConfiguration.txtInstalling;
			client.Set(GConfShowSplashKey,true);
			client.Set(GConfSplashImageKey,InstallThemeFile);
		}
		
		override protected void PostInstallation(CStatusWindow sw){
			//Revert verfügbar machen
			revertIsAvailable=true;
		}

		public void Install(){} 
		
		public CSplashTheme(CConfiguration config):base(config) {
			useUrlAsPreview = true;
			installationSteps=3;
		}
		
	}
}
