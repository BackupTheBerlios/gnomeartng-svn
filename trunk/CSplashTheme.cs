// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CSplashTheme.cs created with MonoDevelop
// User: neo at 11:51 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using GConf;
using System.Net;
using System.Drawing;
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
			
		override public void Revert(){
			if (revertIsAvailable){
				client = new GConf.Client();
				client.Set(GConfShowSplashKey,splashWasActive);
				client.Set(GConfSplashImageKey,prevSplash);
				revertIsAvailable=false;
			}
		}
		
		override public void Install(){
			int stepCount=3;
			CStatusWindow sw=new CStatusWindow(Catalog.GetString(String.Format("Installing \"{0}\"",Name)),true,stepCount);
			try {
				string LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
				string InstallThemeFile=config.SplashInstallPath+Path.GetFileName(DownloadUrl);
				//Neuer GConfClient
				client = new GConf.Client();
				sw.Mainlabel=Catalog.GetString("Saving the previous settings");
				//Sicherung
				splashWasActive = (bool)client.Get(GConfShowSplashKey);
				prevSplash = (string)client.Get(GConfSplashImageKey);
				sw.SetProgress("1/"+stepCount);
				
				sw.Mainlabel=Catalog.GetString("Downloading the file from art.gnome.org");
				//Herunterladen
				if (!File.Exists(InstallThemeFile)){
					DownloadFile(DownloadUrl, LocalThemeFile);
					File.Copy(LocalThemeFile,InstallThemeFile);
				}
				sw.SetProgress("2/"+stepCount);
				sw.Mainlabel=Catalog.GetString("Installing the theme");
				//Installieren
				client.Set(GConfShowSplashKey,true);
				client.Set(GConfSplashImageKey,InstallThemeFile);
				sw.Mainlabel=Catalog.GetString("Install finished");
				sw.SetProgress("3/"+stepCount);
				//Revert verfügbar machen
				revertIsAvailable=true;
				sw.Close();
			} catch (Exception ex) {
				revertIsAvailable=false; 
				sw.Close();
				throw ex;
			}
			
		}
		
		public CSplashTheme(CConfiguration config):base(config) {

		}
		
	}
}
