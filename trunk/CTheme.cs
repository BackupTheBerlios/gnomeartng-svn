// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CTheme.cs created with MonoDevelop
// User: neo at 11:30 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Net;
using System.IO;
using Mono.Unix;

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

		protected CConfiguration config;
		
		protected bool revertIsAvailable=false;
		protected bool installationIsPossible=true;
		protected string smallThumbnailUrl="";
		protected string previewUrl="";
		protected string localThumbnailFile="";
		protected string localPreviewFile="";
		protected int installationSteps=1;
		public bool RevertIsAvailable{ get{ return revertIsAvailable;} }
		public bool InstallationIsPossible{ get{ return installationIsPossible;} }
		
		private WebClient webclient;
		
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
		
		//Theme installieren und Revert verfügbar machen 
		public virtual void StartInstallation(){
			CStatusWindow sw=new CStatusWindow(Catalog.GetString(String.Format("Installing \"{0}\"",Name)),installationSteps,false,false,true);
			sw.Mainlabel=Catalog.GetString("<i>Installing</i>\n\nYour selection is beeing downloaded and installed...please be"+
			                               " patient while the installation procedure proceeds. After everything's done, this"+
			                               " window will get closed.\n\n Have fun with your new theme, Greetings Tom");
			PreInstallation(sw);
			Installation(sw);
			PostInstallation(sw);
			sw.Close();
		}
		
		public abstract void Revert();
		
		//Temporär 
		//public virtual void Install(){}
		
		//3 Methoden: PreInstallation(), postInstallation() und Installation();
		//Eine wird vor der Installation ausgeführt, eine nachher und eine zur eigentlichen Installation 
		protected abstract void PreInstallation(CStatusWindow sw);
		protected abstract void PostInstallation(CStatusWindow sw);
		protected abstract void Installation(CStatusWindow sw);
		
		public CTheme(CConfiguration config)	{
			this.config = config;
			webclient = new WebClient();
		}
	}
}
