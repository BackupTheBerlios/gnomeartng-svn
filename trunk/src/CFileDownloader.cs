/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Thomas Beck at 17:53 27.11.2007
*/

using System;
using System.Net;
using System.IO;
using Mono.Unix;
using System.Threading;
using System.Collections;

namespace GnomeArtNG
{
	
	public class CFileDownloader
	{
		private bool keepConnection=false;
		private WebProxy proxy;
		private string currentDownloadUrl="";
		private string currentDownloadLocation="";
		
		// The request to the web server for file information
		HttpWebRequest webRequest;

		public CFileDownloader(CConfiguration config) {
			ProxyAttrStruct ProxyAttr = config.Proxy;
			if (config.ProxyKind != CConfiguration.ProxyType.ptNone){
				proxy = new WebProxy(ProxyAttr.Ip, ProxyAttr.Port);
				if ((ProxyAttr.BypassList!=null) && (ProxyAttr.BypassList.Length > 0)) {
					for (int i = 0; i < ProxyAttr.BypassList.Length; i++)
						ProxyAttr.BypassList[i] = "http://" + ProxyAttr.BypassList[i];
					proxy.BypassList = ProxyAttr.BypassList;
				}
				proxy.Credentials = new NetworkCredential (ProxyAttr.User, ProxyAttr.Password);
			}
		}

		public CFileDownloader() {
			proxy = null;
		}
		
		private void initWebRequest(string From){
			//if (webRequest==null) {
			//	Console.WriteLine("New HTTP-connection established"); 
			webRequest = (HttpWebRequest)WebRequest.Create(From);
			webRequest.UserAgent="Gnome-Art NextGen, version "+ CConfiguration.Version;
			if (proxy!=null)
				webRequest.Proxy = proxy;
			webRequest.Timeout = 4000; 
			webRequest.KeepAlive=keepConnection;
			// Set default authentication for retrieving the file
			webRequest.Credentials = CredentialCache.DefaultCredentials;
			//} else {
			//	Console.WriteLine("Using existing HTTP-connection");
			//}
		}
		
		public void StartMassDownloading(){
			keepConnection=true;
			//Setze einen Request ab mit KeepAlive
		}
		
		void EndMassDownloading(){
			keepConnection=false;
			//Schließe die KeepAlive Verbindung
		}
		
		public void DownloadFiles(ArrayList FileUrls, ArrayList DownloadLocations, Gtk.ProgressBar bar){
			try {
				
				if ((FileUrls==null) || (DownloadLocations==null))
					throw new Exception("Files or DownloadLocations may not be null!");
				else if (FileUrls.Count != DownloadLocations.Count)
					Console.WriteLine("File and location count are different, using the last entry as a repeatable value");
				StartMassDownloading();			
				//Todo:unbedingt noch die grenzen der listen beachten 
				for (int i=0;i<FileUrls.Count;i++){
					currentDownloadUrl = (string)FileUrls[i];
					currentDownloadLocation = (string)DownloadLocations[i];
					DownloadFile(currentDownloadUrl, currentDownloadLocation, bar);
				}
			} finally  {
				EndMassDownloading();
			}			
		}

		public void Test(){
			ArrayList ar1 = new ArrayList();
			ArrayList ar2 = new ArrayList();
			
			for (int i=0;i<20;i++){
				ar1.Add((string) CConfiguration.UpdateUrl);
				ar2.Add((string) "/home/neo/version"+i.ToString()+".info");
			}
			DownloadFiles(ar1,ar2,null);			
		}
		
		public void DownloadFile(string From, string To, Gtk.ProgressBar bar){
			// The stream of data retrieved from the web server
			Stream strResponse;
			// The stream of data that we write to the harddrive
			Stream strLocal;
			// A buffer for storing and writing the data retrieved from the server
			byte[] downBuffer = new byte[2048];
			string byteString = Catalog.GetString("bytes");
			WebResponse webResponse;
			
			try	{
				// Create a request to the file we are downloading
				initWebRequest(From);
				// Retrieve the response from the server
				webResponse = (HttpWebResponse)webRequest.GetResponse(); 
				// Ask the server for the file size and store it
				Int64 fileSize = webResponse.ContentLength;
				
				// It will store the current number of bytes we receieved from the server
				int bytesSize = 0;
				InitializeDownloadBar(bar,fileSize,downBuffer.Length);
				Console.WriteLine("Filesize of "+From+" -> "+fileSize+" "+byteString);
					
				if (webRequest.HaveResponse) {
					strResponse = webResponse.GetResponseStream();
					// Create a new file stream where we will be saving the data (local drive)
					strLocal = new FileStream(To, FileMode.Create, FileAccess.Write, FileShare.None);
					// Loop through the buffer until the buffer is empty
					while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
					{
						// Write the data from the buffer to the local hard drive
						strLocal.Write(downBuffer, 0, bytesSize);
						SetDownloadBarProgress(bar,fileSize,strLocal.Length,byteString);
					}
					//Close the request
					if (!keepConnection){
						webResponse.Close();
						strResponse.Close();
						strLocal.Close();
					} 
				} else Console.WriteLine("WebRequest didn't get a response...strange!");
			}
			catch (Exception ex) {
				throw new Exception("Download of file "+From+" failed:"+"\n"+ex.Message);
			}
		}			
	
		private void InitializeDownloadBar(Gtk.ProgressBar bar, Int64 fileSize, int bufferSize){
			if (bar!=null){
				if (fileSize>0){
					bar.Text = Catalog.GetString("Initialize download");
					bar.PulseStep=1.0/(fileSize/bufferSize);
					bar.Fraction=0.0;
				} else{
					bar.Text = Catalog.GetString("downloading... (no progress available)");
					bar.PulseStep=0.005;
					bar.Fraction=1.0;
				}
			}
		}

		private void SetDownloadBarProgress(Gtk.ProgressBar bar, Int64 fileSize,Int64 currentSize, string byteString){
			if (bar!=null) {
				if (fileSize>0){
					if (currentSize>0){
						bar.Text = (int)(currentSize/1024)+" k"+byteString+" / "+(int)(fileSize/1024)+" k"+byteString;
						bar.Fraction = (double)currentSize/fileSize;
					}
				} else  
					bar.Pulse();
				if (currentSize % 2 == 0)	
					while (Gtk.Application.EventsPending ())
						Gtk.Application.RunIteration ();
			}
		}
		
	} //Class
}  //Namespace
