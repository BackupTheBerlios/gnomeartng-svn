/*
	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; version 3 of the License.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.
*/

// Thomas Beck at 11:40 02.06.2008

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.BZip2;	
using Mono.Unix;

namespace GnomeArtNG {
	
	public enum ImageType:int{
		itPng,
		itJpg,
		itSvg
	}
	
	public class CUtility
	{
		
		public static System.Text.StringBuilder Execute(string FileName, string Arguments){
			return Execute(FileName,Arguments,true);
		}
	
		
		public static System.Text.StringBuilder Execute(string FileName, string Arguments, bool WaitForCompletion){
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.Arguments = Arguments;
			psi.FileName = FileName;
			//Console.WriteLine(psi.FileName+' '+psi.Arguments);
			if (WaitForCompletion){
				psi.RedirectStandardOutput = true;
				psi.UseShellExecute = false;
			} else
				psi.RedirectStandardOutput = false;
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			Process proc = Process.Start(psi);
			if (WaitForCompletion){
				proc.WaitForExit();
				while (proc.StandardOutput.Peek() != -1){ 
					sb.Append(proc.StandardOutput.ReadLine());
					//Console.WriteLine("Read "+proc.StandardOutput.ReadLine());
				}
			} else
			    sb.Append("Process "+FileName+" started");
			return sb;
		}
		
		public static ImageType StrToImageType(string type){
			type = type.ToLower();
			if (type.Contains("png"))
				return ImageType.itPng;
			else if (type.Contains("jpg") || type.Contains("jpeg"))
				return ImageType.itJpg;
			else if (type.Contains("svg"))
				return ImageType.itSvg;
			else throw new Exception(String.Format("Image Type {0} couldn't been recognized",type));
		}

		public static string ImageTypeToStr(ImageType type){
			switch (type){
			case ImageType.itPng: 
				return "png";  
			case ImageType.itJpg: 
				return "jpeg"; 
			case ImageType.itSvg: 
				return "svg"; 
			default:
				throw new Exception("Unknown image type in typeToStr");
			}
		}

		//true if an update is available; false if not or an error occured
		public static bool IsAnUpdateAvailable(string UpdateUrl, Gtk.ProgressBar bar, out string NewestVersionNumberOnServer, out string DownloadLocation,CConfiguration config){
			NewestVersionNumberOnServer = CConfiguration.Version;
			DownloadLocation = "";
			try {
				string localUpdateFile = Environment.GetFolderPath(Environment.SpecialFolder.Personal)+Path.DirectorySeparatorChar+ Path.GetFileName(UpdateUrl);
				new CFileDownloader(config).DownloadFile(UpdateUrl, localUpdateFile, bar);
				// version information contains the text "x.x.x" (without the quotes) and the download location in the second row
				if (File.Exists(localUpdateFile))
				{
					TextReader tr = new StreamReader(localUpdateFile);
					string newestVer = tr.ReadLine();
					//Get the appropriate download location (Releasename@Location)
	                while (true) {
						DownloadLocation = tr.ReadLine();
	                    if (DownloadLocation != null) {
	                        if (DownloadLocation.ToLower().Contains(config.DistributionVersion.ToString().ToLower()+'@')){
								int pos = DownloadLocation.IndexOf('@')+1;
								DownloadLocation = DownloadLocation.Substring(pos);
								break;
							}
	                    } else 
							break;
	                }
					System.Console.WriteLine(DownloadLocation);
					tr.Close();
	                tr = null;
					File.Delete(localUpdateFile);
					string shortVersionFromFile = newestVer.Replace(".", String.Empty);
					string shortVersionFromApp = CConfiguration.Version.Replace(".", String.Empty);
					NewestVersionNumberOnServer=newestVer;
					return (Convert.ToInt32(shortVersionFromApp) < Convert.ToInt32(shortVersionFromFile));
				} else 
					return false;
			} catch {
				Console.WriteLine("Unable to get version information from "+UpdateUrl);
				return false;
			}			
		}
		
		public static System.Text.StringBuilder ExecuteSu(CConfiguration config, string Arguments){
			if (config != null)
				return Execute(config.SudoCommand,config.AttribPrep+"\""+Arguments+"\"");
			else
				throw new Exception("You have to supply a valid configuration object");
		}
		
		public static bool TestIfProgIsInstalled(string programName, string arguments, string lookFor) {
			lookFor=lookFor.ToLower();
			return (Execute(programName,arguments)).ToString().ToLower().Contains(lookFor);
		}

		public static string GetTarParams(string Filename){
			if (Path.GetExtension(Filename) == ".gz")
				return "-vxzf ";
			else
				return "-vxjf ";
		}
		public static Gdk.Pixbuf GetPixbuf(string Filename,CConfiguration config){
			Gdk.Pixbuf pic;
			try{
				pic = new Gdk.Pixbuf(Filename);
			} catch{
				pic = new Gdk.Pixbuf(config.NoThumb);
			}
			return pic;
		}
		

		// TODO: Create a class Archive.cs and do all the archiving stuff there!
		public static bool UncompressGzipFile(string Filename, string To, Gtk.ProgressBar bar){
			try{
				Console.WriteLine(Filename);
				GZipInputStream gzipIn = new GZipInputStream(File.OpenRead(Filename));				
				FileStream streamWriter = File.Create(To+Path.GetFileNameWithoutExtension(Filename));
				long size=0;
				byte[] data = new byte[1024];
				while (true)
					{
					size = gzipIn.Read(data, 0, data.Length);
					if (size > 0) streamWriter.Write(data, 0, (int) size);
					else break;
				}
				streamWriter.Close();
				Console.WriteLine("Deflating the gzip file done!");
				return true;
			}
			catch(Exception e) {
				Console.WriteLine("An exception occured while deflating the gzip file: "+e.Message);
				return false;
			}
		}

		// TODO: Create a class Archive.cs and do all the archiving stuff there!
		public static bool UncompressBzip2File(string Filename, string To, Gtk.ProgressBar bar){
			try{
				Console.WriteLine(Filename);
				BZip2InputStream bzipIn = new BZip2InputStream(File.OpenRead(Filename));				
				FileStream streamWriter = File.Create(To+Path.GetFileNameWithoutExtension(Filename));
				long size=0;
				byte[] data = new byte[1024];
				while (true)
					{
					size = bzipIn.Read(data, 0, data.Length);
					if (size > 0) streamWriter.Write(data, 0, (int) size);
					else break;
				}
				streamWriter.Close();
				Console.WriteLine("Deflating the gzip file done!");
				return true;
			}
			catch(Exception e) {
				Console.WriteLine("An exception occured while deflating the bzip2 file: "+e.Message);
				return false;
			}
		}

		// TODO: Create a class Archive.cs and do all the archiving stuff there! This is just copy and paste crap
		public static ArrayList UncompressTarFile(string Filename, string To, Gtk.ProgressBar bar){
			ArrayList entries=new ArrayList();
			try{
				TarInputStream tarIn = new TarInputStream(File.OpenRead(Filename));				
				TarEntry entry;
				while ((entry = tarIn.GetNextEntry()) != null)
				{
					string savepath = Path.GetDirectoryName(To+entry.Name);
					if (!Directory.Exists(savepath)){
						Directory.CreateDirectory(savepath);
						//Console.WriteLine(Path.GetDirectoryName(entry.Name));
					}					
					entries.Add(Path.GetDirectoryName(entry.Name));					
					if (!entry.IsDirectory) {
						FileStream streamWriter = File.Create(To + entry.Name);
						long size = entry.Size;
						byte[] data = new byte[size];
						while (true)
						{
							size = tarIn.Read(data, 0, data.Length);
							if (size > 0) streamWriter.Write(data, 0, (int) size);
							else break;
						}
						streamWriter.Close();
					}
				}
				Console.WriteLine("Deflating the tar file done!");
				return entries;
			}
			catch(Exception e) {
				Console.WriteLine("An exception occured while deflating the tar file: "+e.Message);
				return entries;
			}
		}

		// TODO: Create a class Archive.cs and do all the archiving stuff there! This is just copy and paste crap
		public static ArrayList UncompressZipFile(string Filename, string To, Gtk.ProgressBar bar){
			ArrayList entries=new ArrayList();			
			try{
				ZipInputStream zipIn = new ZipInputStream(File.OpenRead(Filename));				
				ZipEntry entry;
				while ((entry = zipIn.GetNextEntry()) != null)
				{
					if (entry.IsDirectory)
						entries.Add(entry.Name);
					FileStream streamWriter = File.Create(To+entry.Name);
					long size = entry.Size;
					byte[] data = new byte[size];
					while (true)
					{
						size = zipIn.Read(data, 0, data.Length);
						if (size > 0) streamWriter.Write(data, 0, (int) size);
						else break;
					}
					streamWriter.Close();
				}
				Console.WriteLine("Deflating the zip file done!");
				return entries;
			}
			catch(Exception e) {
				Console.WriteLine("An exception occured while deflating the zip file: "+e.Message);
				return entries;
			}
		}
		public static ArrayList UncompressFile(string Filename, string To,bool DeleteOriginal, Gtk.ProgressBar bar,ArrayList entries){
			if (bar	!= null)
				bar.Text = Catalog.GetString("Decompressing")+ " " +Catalog.GetString("file")+" "+Path.GetFileNameWithoutExtension(Filename);
			string ext = Path.GetExtension(Filename);			
			if (ext == ".tar")
				entries = UncompressTarFile(Filename,To,bar);
			else if (ext == ".zip")
				entries = UncompressZipFile(Filename,To,bar);
			else if (ext == ".bz2")
				UncompressBzip2File(Filename,To,bar);
			else if (ext == ".gz")
				UncompressGzipFile(Filename,To,bar);

			string newfile=To+Path.GetFileNameWithoutExtension(Filename);	
			ext=Path.GetExtension(newfile);
			if ((ext ==".tar") || (ext ==".zip") || (ext ==".bz2") || (ext ==".gz")){
				entries = UncompressFile(newfile,To,DeleteOriginal,bar,entries);
			}
			
			if (DeleteOriginal){
				File.Delete(Filename);
				File.Delete(newfile);
			} 	

			return entries;
		}
		
		public static string GetContentFromFile(string filename, bool lowercase){
			if (!File.Exists(filename))
				throw new Exception("Could not get "+ filename + "...aborting!!");
			StreamReader myFile = new StreamReader(filename, System.Text.Encoding.Default);
            string fileContent = myFile.ReadToEnd();
            myFile.Close();
			if (lowercase)
				fileContent=fileContent.ToLower();
			return fileContent;
		}

		public static string CheckAndSetIp(string ip){
			string pattern = @"\d\d?\d?\.\d\d?\d?\.\d\d?\d?\.\d\d?\d?";
			Regex regex = new Regex( pattern );
			Match match = regex.Match( ip );
			if (match.Success)
				return ip;
			else
				throw new Exception("No valid IP-address!");						
			}
	}
}
