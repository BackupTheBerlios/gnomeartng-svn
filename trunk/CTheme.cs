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
		public string LocalThemeFile="";
		public string DownloadUrl="";
		public bool LocalThumbExists=false;
		public bool LocalPreviewExists=false;

		public string SmallThumbnailUrl{
			get{return smallThumbnailUrl;}
			set{
				smallThumbnailUrl=value;
				localThumbnailFile=Path.Combine(config.ThumbsPath,Path.GetFileName(smallThumbnailUrl));
				LocalThumbExists=File.Exists(localThumbnailFile);
				//Console.WriteLine("LocalThumb: "+localThumbnailFile);
			}
		}
		
		public string LocalPreviewFile{get{return localPreviewFile;}}
		public string LocalThumbnailFile{
			get{
				if(LocalThumbExists) 
					return localThumbnailFile;
				else
					return config.NoThumb;
			}
		}
		public Gdk.Pixbuf ThumbnailPic{
			get{return new Gdk.Pixbuf(LocalThumbnailFile);}
		}
		
		public string PreviewUrl{
			get{return previewUrl;}
			set{
				previewUrl=value;
				localPreviewFile=Path.Combine(config.PreviewPath,Path.GetFileName(previewUrl));
				LocalPreviewExists=File.Exists(localPreviewFile);
				//Console.WriteLine("LocalPreview: "+localPreviewFile);
			}
		}
		
		protected CConfiguration config;
		
		protected bool revertIsAvailable=false;
		protected bool installationIsPossible=true;
		protected string smallThumbnailUrl="";
		protected string previewUrl="";
		protected string localThumbnailFile="";
		protected string localPreviewFile="";
		
		public bool RevertIsAvailable{ get{ return revertIsAvailable;} }
		public bool InstallationIsPossible{ get{ return installationIsPossible;} }
		
		private WebClient webclient;
		
		//Thumbnail herunterladen 
		public void GetThumbnailImage(){
			if (!LocalThumbExists){
				DownloadFile(SmallThumbnailUrl,localThumbnailFile);
				LocalThumbExists=true;
			}
		}
		
		public void GetPreviewImage(){
			try{
				if (!LocalPreviewExists){
					DownloadFile(PreviewUrl,LocalPreviewFile);
					LocalPreviewExists=true;
				}
			} catch (Exception ex) {
				Console.WriteLine("Exception occured in GetPreviewImage");
				LocalPreviewExists=false;
				throw ex;
			}

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
