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
		public bool LocalThumbExists=false;
		public bool LocalPreviewExists=false;

		protected CConfiguration config;
		
		//Benutze die Download-Url als Vorschaubild
		protected bool useUrlAsPreview=false;
		protected bool revertIsAvailable=false;
		protected bool installationIsPossible=true;
		protected string smallThumbnailUrl="";
		protected string previewUrl="";
		protected string downloadUrl="";
		protected string localThumbnailFile="";
		protected string localPreviewFile="";
		protected int installationSteps=1;
		public bool RevertIsAvailable{ get{ return revertIsAvailable;} }
		public bool InstallationIsPossible{ get{ return installationIsPossible;} }
		
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
			if (!LocalThumbExists){
				DownloadFile(SmallThumbnailUrl,localThumbnailFile,bar);
				LocalThumbExists=true;
			}
		}
		
		public void GetPreviewImage(Gtk.ProgressBar bar){
			try{
				if (!LocalPreviewExists){
					DownloadFile(PreviewUrl,LocalPreviewFile,bar);
					LocalPreviewExists=true;
				}
			} catch (Exception ex) {
				Console.WriteLine("Exception occured in GetPreviewImage");
				LocalPreviewExists=false;
				throw ex;
			}
			
		}
		

		public static void DownloadFile(string From, string To, Gtk.ProgressBar bar){
		//public static void DownloadFile(string From, string To){
			// The stream of data retrieved from the web server
			Stream strResponse;
			// The stream of data that we write to the harddrive
			Stream strLocal;
			// The request to the web server for file information
			HttpWebRequest webRequest;
			// The response from the web server containing information about the file
			HttpWebResponse webResponse;
			// A buffer for storing and writing the data retrieved from the server
			byte[] downBuffer = new byte[2048];
			
			using (WebClient wcDownload = new WebClient()) {
				try	{
					// Create a request to the file we are downloading
					webRequest = (HttpWebRequest)WebRequest.Create(From);
					// Set default authentication for retrieving the file
					webRequest.Credentials = CredentialCache.DefaultCredentials;
					// Retrieve the response from the server
					webResponse = (HttpWebResponse)webRequest.GetResponse();
					// Ask the server for the file size and store it
					Int64 fileSize = webResponse.ContentLength;
					//Close the request
					webResponse.Close();
					string byteString = Catalog.GetString("bytes");
					Int64 currentSize = 0;
					// It will store the current number of bytes we retrieved from the server
					int bytesSize = 0;
					if (bar!=null){
						if (fileSize>0){
							bar.Text = Catalog.GetString("Initialize download");
							bar.PulseStep=1.0/(fileSize/downBuffer.Length);
							bar.Fraction=0.0;
						} else{
							bar.Text = Catalog.GetString("downloading... (no progress available)");
							bar.PulseStep=0.005;
							bar.Fraction=1.0;
						}
					}
					Console.WriteLine("Filesize of "+From+" >> "+fileSize+" bytes");
						
					// Open the URL for download
					strResponse = wcDownload.OpenRead(From);
					// Create a new file stream where we will be saving the data (local drive)
					strLocal = new FileStream(To, FileMode.Create, FileAccess.Write, FileShare.None);
					
					// Loop through the buffer until the buffer is empty
					while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
					{
						// Write the data from the buffer to the local hard drive
						strLocal.Write(downBuffer, 0, bytesSize);

						if (bar!=null) {
							currentSize = strLocal.Length;
							if (fileSize>0){
							    if (currentSize>0){
									bar.Text=currentSize+byteString+" / "+fileSize+" "+byteString;
									bar.Fraction = (double)currentSize/fileSize;
								}
							} else { 
								bar.Pulse();
							}
							while (Gtk.Application.EventsPending ())
									Gtk.Application.RunIteration ();
						}
					}
					strResponse.Close();
					strLocal.Close();
				}
				catch (Exception ex) {
					throw new Exception("Download of file "+From+" failed:"+"\n"+ex.Message);
				}
			}
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
