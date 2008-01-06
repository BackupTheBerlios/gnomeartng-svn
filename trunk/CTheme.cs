// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CTheme.cs created with MonoDevelop
// User: neo at 11:30 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Net;
using System.IO;
using Mono.Unix;
using System.Threading;

namespace GnomeArtNG
{
	
	abstract public class CTheme
	{
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

		protected CConfiguration config;
		
		//Benutze die Download-Url als Vorschaubild
		protected bool useUrlAsPreview=false;
		protected bool revertIsAvailable=false;
		protected bool installationIsPossible=true;
		protected bool localThumbExists=false;
		protected bool localPreviewExists=false;
		protected string smallThumbnailUrl="";
		protected string previewUrl="";
		protected string downloadUrl="";
		protected string localThumbnailFile="";
		protected string localPreviewFile="";
		protected int installationSteps=1;

		public bool RevertIsAvailable{ get{ return revertIsAvailable;} }
		public bool InstallationIsPossible{ get{ return installationIsPossible;} }
		public bool LocalThumbExists{get{return localThumbExists;}}
		public bool LocalPreviewExists{get{return localPreviewExists;}}
		
		public string SmallThumbnailUrl{
			get{return smallThumbnailUrl;}
			set{
				smallThumbnailUrl=value;
				localThumbnailFile=Path.Combine(config.ThumbsPath,Path.GetFileName(smallThumbnailUrl));
				localThumbExists=File.Exists(localThumbnailFile);
				//Console.WriteLine("LocalThumb: "+localThumbnailFile);
			}
		}
		
		public string LocalPreviewFile{get{return localPreviewFile;}}
		public string LocalThumbnailFile{
			get{
				if(localThumbExists) 
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
				localPreviewExists=File.Exists(localPreviewFile);
				//Console.WriteLine("LocalPreview: "+localPreviewFile);
			}
		}
		
		public string DownloadUrl{
			get{return downloadUrl;}
			set{
				downloadUrl=value;
				if (useUrlAsPreview)
					PreviewUrl=downloadUrl;
			}
		}
		
		//Thumbnail herunterladen 
		public void GetThumbnailImage(Gtk.ProgressBar bar){
			if (!localThumbExists){
				DownloadFile(SmallThumbnailUrl,localThumbnailFile,bar);
				localThumbExists=true;
			}
		}
		
		public void GetPreviewImage(Gtk.ProgressBar bar){
			try{
				if (!localPreviewExists){
					DownloadFile(PreviewUrl,LocalPreviewFile,bar);
					localPreviewExists=true;
				}
			} catch (Exception ex) {
				Console.WriteLine("Exception occured in GetPreviewImage");
				localPreviewExists=false;
				throw ex;
			}
			
		}
		

		protected void DownloadFile(string From, string To, Gtk.ProgressBar bar){
			new CFileDownloader().DownloadFile(From, To, bar);
		}			
		
		//Theme installieren und Revert verfügbar machen 
		public virtual void StartInstallation(){
			CStatusWindow sw=new CStatusWindow(Catalog.GetString(String.Format("Installing \"{0}\"",Name)),installationSteps,false,false,true);
			sw.Mainlabel=Catalog.GetString("<i>Installing</i>\n\nYour selection is beeing downloaded and installed...please be"+
			                               " patient while the installation procedure proceeds. After everything's done, this"+
			                               " window will get closed.\n\n Have fun with your new theme, Greetings Tom");
			try{
				PreInstallation(sw);
				Installation(sw);
				PostInstallation(sw);
				sw.SetProgress(installationSteps+"/"+installationSteps);
				sw.Mainlabel=CConfiguration.txtInstallDone;
				sw.DetailProgressBar.Fraction=1.0;
				//Hint:It's enough to change the caption to the stock item!! No need for image transformations!
				sw.ButtonCaption = "gtk-ok";
			} 
			catch (Exception ex) {
				sw.Close();
				CInfoWindow iw = new CInfoWindow(Catalog.GetString("<b>Theme installation failed!</b>"),Catalog.GetString("This message was reported from the installation function:\n\n"),Gtk.Stock.DialogError,true);
				iw.Description = ex.Message;
			}
		}
		
		public abstract void Revert();
		
		//3 Methoden: PreInstallation(), postInstallation() und Installation();
		//Eine wird vor der Installation ausgeführt, eine nachher und eine zur eigentlichen Installation 
		protected abstract void PreInstallation(CStatusWindow sw);
		protected abstract void PostInstallation(CStatusWindow sw);
		protected abstract void Installation(CStatusWindow sw);
		
		public CTheme(CConfiguration config)	{
			this.config = config;
		}
	}
}
