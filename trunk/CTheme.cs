// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CTheme.cs created with MonoDevelop
// User: neo at 11:30 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Net;
using System.IO;

namespace GnomeArtNG
{
	
	abstract public class CTheme
	{
		//TODO:Alles Properties
		public string Author="";
		public string Description="";
		public string Name="";
		public string Category="";
		public string ReleaseDate="";
		public string Timestamp="";
		public int VoteSum=0;
		public int VoteCount=0;
		public int DownloadCount=0;
		public string License="";
		public string SmallThumbnailUrl="";
		public string PreviewUrl="";
		public string LocalThumbnailFile="";
		public string LocalPreviewFile="";
		public string LocalThemeFile="";
		public string DownloadUrl="";
		
		protected bool revertIsAvailable=false;
		protected bool installationIsPossible=true;
		protected CConfiguration config;
		
		public bool RevertIsAvailable{ get{ return revertIsAvailable;} }
		public bool InstallationIsPossible{ get{ return installationIsPossible;} }
		
		private WebClient webclient;
		
		//Alle Themes herunterladen 
		public void GetThumbnailImage(){
			LocalThumbnailFile=Path.Combine(config.ThumbsPath,Path.GetFileName(SmallThumbnailUrl));
			FileInfo file = new FileInfo(LocalThumbnailFile);
			if(!file.Exists)
				webclient.DownloadFile(SmallThumbnailUrl,LocalThumbnailFile);
		}
		
		public void GetPreviewImage(){
			try{
				LocalPreviewFile=Path.Combine(config.PreviewPath,Path.GetFileName(PreviewUrl));
				FileInfo file = new FileInfo(LocalPreviewFile);
				if(!file.Exists)
					webclient.DownloadFile(PreviewUrl,LocalPreviewFile);
			} catch (Exception ex) {
				Console.WriteLine("Exception occured in GetPreviewImage");
				throw ex;
			}

		}
		
		/*abstract*/ public void DownloadTheme(){
			//Nacheinander download;
			//kann man abbrechen;
			//Legt die Bilder in thumbsDir+ArtType ab
			//setzt localThemeFile
		}
			
		//Lädt eine Datei herunter
		protected virtual void DownloadFile(string From, string To){
			webclient.DownloadFile(From,To);
		}
		
		//Theme installieren und refert verfügbar machen 
		public abstract void Install();
		public abstract void Revert();
		

		public CTheme(CConfiguration config)	{
			this.config = config;
			webclient = new WebClient();
		}
	}
}
