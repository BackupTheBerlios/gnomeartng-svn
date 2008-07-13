/*
	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; version 3 of the License.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.
*/
//
// UtilityClass.cs created with MonoDevelop
// Thomas Beck at 11:40 02.06.2008

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
	
namespace GnomeArtNG
{
	public class CUtility
	{
		
		public static System.Text.StringBuilder Execute(string FileName, string Arguments){
			return Execute(FileName,Arguments,true);
		}
	
		
		public static System.Text.StringBuilder Execute(string FileName, string Arguments, bool WaitForCompletion){
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.Arguments = Arguments;
			psi.FileName = FileName;
			//Console.WriteLine(psi.FileName+psi. Arguments);
			psi.RedirectStandardOutput = WaitForCompletion;
			psi.UseShellExecute = false;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			Process proc = Process.Start(psi);
			if (WaitForCompletion){ 
				while (proc.StandardOutput.Peek() != -1) 
					sb.Append(proc.StandardOutput.ReadLine());
			} else
			    sb.Append("Process "+FileName+" started");
			return sb;
		}
		
		//true if an update is available; false if not or an error has occured
		public static bool IsAnUpdateAvailable(string UpdateUrl, Gtk.ProgressBar bar, out string NewestVersionNumberOnServer, out string DownloadLocation,ProxyAttrStruct Proxy){
			NewestVersionNumberOnServer = CConfiguration.Version;
			DownloadLocation = "";
			try {
				string localUpdateFile = Environment.GetFolderPath(Environment.SpecialFolder.Personal)+Path.DirectorySeparatorChar+ Path.GetFileName(UpdateUrl);
				new CFileDownloader(Proxy).DownloadFile(UpdateUrl, localUpdateFile, bar);
				// version information contains the text "x.x.x" (without the quotes) and the download location in the second row
				if (File.Exists(localUpdateFile))
				{
					TextReader tr = new StreamReader(localUpdateFile);
					string newestVer = tr.ReadLine();
					DownloadLocation = tr.ReadLine();
					tr.Close();
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

		// TODO: Create a class Archive.cs and do all the archiving stuff there! This is just copy and paste crap
		public static bool UncompressTarFile(string Filename, string To, Gtk.ProgressBar bar){
			try{
				TarInputStream tarIn = new TarInputStream(File.OpenRead(Filename));				
				TarEntry entry;
				while ((entry = tarIn.GetNextEntry()) != null)
				{
					if (entry.IsDirectory)
						Directory.CreateDirectory(To+entry.Name);
					else {
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
				return true;
			}
			catch(Exception e) {
				Console.WriteLine("An exception occured while deflating the tar file: "+e.Message);
				return false;
			}
		}

		// TODO: Create a class Archive.cs and do all the archiving stuff there! This is just copy and paste crap
		public static bool UncompressZipFile(string Filename, string To, Gtk.ProgressBar bar){
			try{
				ZipInputStream zipIn = new ZipInputStream(File.OpenRead(Filename));				
				ZipEntry entry;
				while ((entry = zipIn.GetNextEntry()) != null)
				{
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
				return true;
			}
			catch(Exception e) {
				Console.WriteLine("An exception occured while deflating the zip file: "+e.Message);
				return false;
			}
		}
		public static void UncompressFile(string Filename, string To,bool DeleteOriginal, Gtk.ProgressBar bar){
			if (Path.GetExtension(Path.GetFileNameWithoutExtension(Filename)) == ".tar"){
				UncompressGzipFile(Filename,To,bar);
				string newfile=To+Path.GetFileNameWithoutExtension(Filename);
				UncompressTarFile(newfile,To,bar);
				if (DeleteOriginal){
					File.Delete(Filename);
					File.Delete(newfile);
				}
			} 
			else {
				string ext = Path.GetExtension(Filename);			
				if (ext == ".tar")
					UncompressTarFile(Filename,To,bar);
				else if (ext == ".zip")
					UncompressZipFile(Filename,To,bar);
				else if (ext == ".gz")
					UncompressGzipFile(Filename,To,bar);
			}
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
