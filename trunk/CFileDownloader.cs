// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CFileDownloader.cs created with MonoDevelop
// User: neo at 17:53 27.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
//not used yet 
using System;
using System.Net;
//using System.Drawing;
using System.Collections;
using System.Threading;
using System.IO;

namespace GnomeArtNG
{
	
	
	public class CFileDownloader
	{
		/*
		private static int maxThreads=10;
		private string[] urls = new String[maxThreads];
		private string[] savePaths = new String[maxThreads];
		private int currentFieldIndex=0;
		private int currentRunningThreads=0;
		private int currentThreadIndex=0;
		private Thread[] WorkerThreads=new Thread[maxThreads]; // 20 Threads
		
		public void abort() {
			for (int i=0;i<maxThreads;i++){
				WorkerThreads[i].Abort();
			}
		}
		
		//TODO: System.Image wieder zu den Referenzen falls die Klasse zum Einsatz kommt 
		private void startWorker(){
			currentRunningThreads++;
			string localSavePath=savePaths[currentThreadIndex];
			string remotePath=urls[currentThreadIndex];
			// Die Anforderung erstellen.
			WebRequest requestPic = WebRequest.Create(remotePath);
			requestPic.Timeout=1000*600;
	        // Die Antworten abrufen.
	        WebResponse responsePic = requestPic.GetResponse();
	        // Den Antwort-Stream lesen.
			Image downloadedImage = Image.FromStream(responsePic.GetResponseStream());
			downloadedImage.Save(localSavePath);
			responsePic.Close();
			currentRunningThreads--;
		}
		
		private void downloadFile(string Url, string LocalSavePath){
			if (currentRunningThreads<maxThreads){
				urls[currentThreadIndex]=Url;
				savePaths[currentThreadIndex]=LocalSavePath;
				WorkerThreads[currentThreadIndex] = new Thread(new ThreadStart(startWorker)); // Threads werden erzeugt
			}
		}
		*/
		public CFileDownloader() {
			
		}
	}
}
