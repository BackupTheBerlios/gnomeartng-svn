// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CFileDownloader.cs created with MonoDevelop
// User: neo at 17:53 27.11.2007
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
	public class CFileDownloader
	{
		private bool massDownloadingActive=false;
		// The request to the web server for file information
		HttpWebRequest webRequest;
		// The response from the web server containing information about the file
		HttpWebResponse webResponse;
	
		public CFileDownloader() {
			
		}
		
		public void StartMassDownloading(){
			massDownloadingActive=true;
			//Setze einen Request ab für art.gnome.org mit KeepAlive
		}
		
		public void GetFile(string From, string To, Gtk.ProgressBar bar){
			DownloadFile(From,To,bar,massDownloadingActive);
		}
		
		void EndMassDownloading(){
			massDownloadingActive=true;
			//Schließe die KeepAlive Verbindung
		}
		
		public void DownloadFile(string From, string To, Gtk.ProgressBar bar){
			DownloadFile(From,To,bar,false);
		}
		
		public void DownloadFile(string From, string To, Gtk.ProgressBar bar, bool holdConnection){
			//public static void DownloadFile(string From, string To){
			// The stream of data retrieved from the web server
			Stream strResponse;
			// The stream of data that we write to the harddrive
			Stream strLocal;
			// A buffer for storing and writing the data retrieved from the server
			byte[] downBuffer = new byte[2048];
			
			using (WebClient wcDownload = new WebClient()) {
				try	{
					// Create a request to the file we are downloading
					webRequest = (HttpWebRequest)WebRequest.Create(From);
					webRequest.KeepAlive=holdConnection;
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
					// It will store the current number of bytes we receieved from the server
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

	}
}
