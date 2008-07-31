/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

//Thomas Beck at 10:51 20.07.2008

using System;
using System.Xml;
using System.IO;
using System.Collections;
using Gtk;
using Mono.Unix;

namespace GnomeArtNG
{
	public enum ContextType{
		cActions,
		cAnimations,
		cApps,
		cCategories,
		cDevices,
		cEmblems,
		cMimetypes,
		cPlaces,
		cStatus,
		cNone
	}
	
	public struct IconList{
		public ArrayList Icons;
		public int Size;
	}
	
	//An IconThemeManager is responsible for one theme with different sizes
	public class CIconThemeManager {
		public string ThemeName;
		public int IconCount = 0;
		public CStatusWindow StatusWindow = null;
		//FreeDesktopOrg icon names
		private ArrayList fdoIcons = new ArrayList();
		//All Icons
		private ArrayList themeIcons = new ArrayList();
		//All directories, looking from the theme-dir
		private ArrayList directoryList = new ArrayList();
		private string themeDir = "";
		public string ThemeDir{
			get{return themeDir;}
			set{themeDir = Path.GetFullPath(value);}
		}
		//Load fd.o fdoIcons from the ressource
		public CIconThemeManager(){
			importIconDefinitions();
		}

		private string getDirectoryEntries(){
			string dir = "";		
			directoryList.Sort();
			foreach (string listentry in directoryList){
				dir += listentry+',';
			}	
			dir=dir.Remove(dir.Length-1);
			return dir;
		}

		private void insertIntoDirectoryList(string Entry){
			bool found = false;
			foreach (string listentry in directoryList){
				if (listentry==Entry){
					found =  true;
					break;
				}
			}
			if (!found)
				directoryList.Add(Entry);
		}
		
		public void SaveAllIcons(string DestinationThemeDir){
			SaveAllIcons(DestinationThemeDir, true);
		}

		public void SaveAllIcons(string DestinationThemeDir, bool CreateSymlinks){
			Console.Write("Saving all icons...");
			ThemeDir = DestinationThemeDir+'/'+ThemeName;
			if (Directory.Exists(ThemeDir)){
				Directory.Delete(ThemeDir,true);
			}
			Directory.CreateDirectory(ThemeDir);
			if (StatusWindow != null)
				StatusWindow.SetDetailProgressStep(IconCount+1);
			int i=0;
			foreach (IconList iconlist in themeIcons){
				foreach (CIcon icon in iconlist.Icons){
					if (StatusWindow != null){
						i++;
						StatusWindow.SetDetailProgress(Catalog.GetString("Converting icon")+" "+i.ToString()+"/"+IconCount.ToString());
					}
					icon.Save(themeDir,true,CreateSymlinks);
				}
			}
			StatusWindow.DetailProgressBar.Text = Catalog.GetString("Creating icon theme index");
			saveIconDescription();
			StatusWindow.DetailProgressBar.Text = Catalog.GetString("Icon conversion has been completed");
			Console.WriteLine("done");
		}

		
		private void saveIconDescription(){
			StreamWriter sw = new StreamWriter(ThemeDir+"/index.theme", false); 
			sw.WriteLine("[Icon Theme]"); 
			sw.WriteLine("Name="+ThemeName); 
			//sw.WriteLine("Name[en]="+ThemeName); 
			sw.WriteLine("Comment="+ThemeName+", Converted by Gnome-ArtNG " +CConfiguration.Version);
			sw.WriteLine("Inherits=Tango,Tango-Blue-Materia,gnome,crystalsvg");
			sw.WriteLine("Example=folder");
			sw.WriteLine("");
			sw.WriteLine("Directories="+getDirectoryEntries());
			sw.WriteLine("");
			foreach(string dir in directoryList){
				sw.WriteLine('['+dir+']');
				string[] test=dir.Split('x');
				if (test[0].Contains("/")){
					sw.WriteLine("Size=32");
					sw.WriteLine("Context="+getContextFromString(dir));
					sw.WriteLine("Type=Scalable");
					sw.WriteLine("MinSize=24");
					sw.WriteLine("MaxSize=256");
				} else { 
					sw.WriteLine("Size="+test[0]);
					sw.WriteLine("Context="+getContextFromString(dir));
					sw.WriteLine("Type=Fixed");
				}
				sw.WriteLine("");
				}
			sw.Close();
		}

		private ArrayList getFiles(string path, string extension,SearchOption searchoption){
			ArrayList files = new ArrayList();
			foreach (string file in Directory.GetFiles(path, extension, searchoption))
				files.Add(file);
			return files;
		}
		
		
		public void ImportIcons(string Path){
			int filecount = 0;
			Console.Write("Searching for icons...");
			//Find all icon files
			ArrayList filenameTypes = new ArrayList();
			filenameTypes.Add(getFiles(Path,"*.png",SearchOption.AllDirectories));
			filenameTypes.Add(getFiles(Path,"*.jpeg",SearchOption.AllDirectories));
			filenameTypes.Add(getFiles(Path,"*.jpg",SearchOption.AllDirectories));
			filenameTypes.Add(getFiles(Path,"*.svg",SearchOption.AllDirectories));
			if (StatusWindow != null){
				foreach (ArrayList files in filenameTypes)
					filecount+=files.Count;
				StatusWindow.SetDetailProgressStep(filecount+1);
			}
			int stcount=0;
			int nstcount=0;
			foreach(ArrayList filelist in filenameTypes){
				foreach(string file in filelist){
					if (StatusWindow != null){
						StatusWindow.SetDetailProgress(stcount.ToString()+" standard icons found, "+nstcount.ToString()+" non standard icons found");
					}
					try {
						CIcon fdoIcon = getFdoIcon(file);
						CIcon icon=new CIcon();
						icon.Filename = file;
						icon.LoadImage(file);
						if (fdoIcon!=null) {
							icon.FdoName = fdoIcon.FdoName;
							icon.Context = fdoIcon.Context;
							icon.LinkedNames = fdoIcon.LinkedNames; //it's ok to pass a pointer...search names are not changing
							stcount++;
						} else{
							icon.FdoName = System.IO.Path.GetFileNameWithoutExtension(file);
							nstcount++;	
						}
						addThemeIcon(icon);
						//Console.WriteLine("Orig : "+file+", Now: "+fdoIcon.FdoName);
						insertIntoDirectoryList(icon.GetSizeDir()+'/'+contextToStr(icon.Context));
					}catch (Exception e){
						Console.WriteLine("ImportError: "+e.Message);
					}
				}
			}
			Console.WriteLine(stcount.ToString()+" fd.o icons found");
			Console.WriteLine(nstcount.ToString()+" non fd.o icons found");
			
//			Console.WriteLine("Directory-Entries: "+getDirectoryEntries());
		}
		
		private string getStringFromRessource(string RessourceName){
			try {
				string tempstring;
				System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
					using (Stream s = assembly.GetManifestResourceStream (RessourceName)) {
					StreamReader reader = new StreamReader (s);
					tempstring = reader.ReadToEnd ();
					s.Close ();
				}
			return tempstring;
			} catch (Exception e) {
				Console.WriteLine (e);
				return "";
			}
		}	

		private void importIconDefinitions() {
			Console.Write("Reading icon definitions...");
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(getStringFromRessource("legacy-icon-mapping.xml"));
			
			// Retrieve all ContextType from the xml
			XmlNodeList entries = doc.SelectNodes("mapping/context");
			
			foreach(XmlNode context in entries){
				string con = context.Attributes["dir"].Value;				
				//Console.WriteLine(con);
				XmlNodeList fdoIconNodes = context.SelectNodes("icon");
				foreach(XmlNode icon in fdoIconNodes){
					CIcon iconObj = new CIcon();
					iconObj.Context = strToContext(con);
					iconObj.FdoName = icon.Attributes["name"].Value;					
					//Console.WriteLine(iconObj.NewName);
					XmlNodeList links = icon.SelectNodes("link");
					foreach(XmlNode link in links)
						iconObj.LinkedNames.Add(link.InnerText);
					fdoIcons.Add(iconObj);
				}
			}
			Console.WriteLine("finished");
		}
		
		private CIcon getFdoIcon(string Name){
			Name=Path.GetFileNameWithoutExtension(Name);
			foreach (CIcon fdoicon in fdoIcons){					
				if (fdoicon.FdoName == Name){
					return fdoicon;
				} else {
					foreach(string oldname in fdoicon.LinkedNames){
						//Console.WriteLine(oldname);
						if ((oldname == Name)){
							return fdoicon;
						}
					}
				}
			}
			return null;
		}

		private string contextToStr(ContextType context){
			switch (context){
			case ContextType.cActions:return "actions";
			case ContextType.cAnimations:return "animations";
			case ContextType.cApps:return "apps";
			case ContextType.cCategories:return "categories";
			case ContextType.cDevices:return "devices";
			case ContextType.cEmblems:return "emblems";
			case ContextType.cMimetypes:return "mimetypes";
			case ContextType.cPlaces:return "places";
			case ContextType.cStatus:return "status";
			default: return "";
			}
		}


		private string getContextFromString(string test){
			if (test.Contains(contextToStr(ContextType.cApps)))
				return "Applications";
			if (test.Contains(contextToStr(ContextType.cDevices)))
				return "Devices";
			if (test.Contains(contextToStr(ContextType.cMimetypes)))
				return "MimeTypes";
			if (test.Contains(contextToStr(ContextType.cEmblems)))
				return "Emblems";
			if (test.Contains(contextToStr(ContextType.cActions)))
				return "Actions";
			if (test.Contains(contextToStr(ContextType.cAnimations)))
				return "Animations";
			if (test.Contains(contextToStr(ContextType.cStatus)))
				return "Status";
			if (test.Contains(contextToStr(ContextType.cPlaces)))
				return "Places";
			if (test.Contains(contextToStr(ContextType.cCategories)))
				return "Categories";
			if (test.Contains(contextToStr(ContextType.cNone)))
				return "TooOldToConvert";

			return "";
		}

		private ContextType strToContext(string context){
			switch (context){
			case "actions":return ContextType.cActions; 
			case "animations":return ContextType.cAnimations; 
			case "apps":return ContextType.cApps; 
			case "categories":return ContextType.cCategories; 
			case "devices":return ContextType.cDevices; 
			case "emblems":return ContextType.cEmblems; 
			case "mimetypes":return ContextType.cMimetypes; 
			case "places":return ContextType.cPlaces; 
			case "status":return ContextType.cStatus;
			case "toooldtoconvert": return ContextType.cNone;	
			default: return ContextType.cActions;
			}
		}
		
		private void addThemeIcon(CIcon icon){			
			IconList list = getIconList(icon.Size);
			//Console.WriteLine("Icons: "+list.Icons.ToString());
			if (findThemeIcon(icon.FdoName,list) == null){
				list.Icons.Add(icon);
				IconCount+=1;
			}
		}

		private CIcon findThemeIcon(string FdoIconName,IconList List){
			foreach (CIcon icon in List.Icons){
				if (icon.FdoName == FdoIconName)
					return icon;
			}
			return null;
		}
		
		private IconList getIconList(int IconSize){
			IconList il = new IconList();
			foreach (IconList list in themeIcons){
				if (list.Size == IconSize){
			//		Console.WriteLine("found!");
					il = list;
					return il;
				}
			}
			//Console.WriteLine("Not found!");
			il.Size = IconSize;
			il.Icons = new ArrayList();
			themeIcons.Add(il);
			return il;
		}
		
		
	} //Class IconThemeManager	
	
	public class CIcon{
		public ContextType Context;
		public string Filename =""; // The filename the icon is currently loaded from
		public string FdoName= "";  //New name after renaming to the fdo scheme
		private Gdk.Pixbuf Pixbuf;
		public ArrayList LinkedNames = new ArrayList();
		public int Size;
		public ImageType Type;

		public string GetSizeDir(){
			if (Pixbuf!=null){
				if (Type != ImageType.itSvg)
					return Size.ToString()+"x"+Size.ToString();
				else
					return "scalable";
			}
			else
				return "NoImageLoaded";
		}
		
		public CIcon(){
			Context = ContextType.cNone;
			Size = -1;
		}

		private ContextType guessContextFromStr(string context){
			context = context.ToLower();
			if (context.Contains("apps") || context.Contains("applications"))
				return ContextType.cApps;
			if (context.Contains("devices"))
				return ContextType.cDevices;
			if (context.Contains("mime"))
				return ContextType.cMimetypes;
			if (context.Contains("emblem") || context.Contains("emoti"))
				return ContextType.cEmblems;
			if (context.Contains("action"))
				return ContextType.cActions;
			if (context.Contains("anim"))
				return ContextType.cAnimations;
			if (context.Contains("stat"))
				return ContextType.cStatus;
			if (context.Contains("place"))
				return ContextType.cPlaces;
			if (context.Contains("categor"))
				return ContextType.cCategories;
			return ContextType.cNone;
		}

		public void LoadImage(string Filename){
			Pixbuf = new Gdk.Pixbuf(Filename);
			if (Pixbuf.Width != Pixbuf.Height){
				Pixbuf.ScaleSimple(Pixbuf.Width,Pixbuf.Width,Gdk.InterpType.Bilinear);
			}
			Size = Pixbuf.Width;
			Type = CUtility.StrToImageType(Path.GetExtension(Filename));
		}

		public bool Save(string DestinationThemeDir,bool OverwriteFile, bool CreateSymlinks){
			char Sep = Path.DirectorySeparatorChar;
			string newpath = DestinationThemeDir + Sep + GetSizeDir() + Sep + contextToStr(Context) + Sep ;
			string newfilename = newpath + FdoName + Path.GetExtension(Filename);
			try{				
				if (Context == ContextType.cNone){
					Context = guessContextFromStr(Path.GetFullPath(Filename));
					//Console.WriteLine("Guessing the icon "+Filename+", belongs probably to "+ Context.ToString());
				}
				if (!Directory.Exists(newpath))
					Directory.CreateDirectory(Path.GetFullPath(newpath));
				//Console.WriteLine("Old: "+Filename+",New: "+ newfilename);
				if (Filename == "")
					Pixbuf.Save(newfilename,CUtility.ImageTypeToStr(Type));
				else
					File.Copy(Filename, newfilename,OverwriteFile);  //File.Move

				//Has to be included in the icon-class if this unit will be the base of a icon-generator-app
				//For now, it's ok to copy it while saving
				string IconFile = Path.ChangeExtension(Filename,"icon");
				if (File.Exists(IconFile))
				    File.Copy(IconFile, Path.ChangeExtension(newfilename,"icon"),OverwriteFile);  //File.Move
				
				//for max compatibility
				if (CreateSymlinks){
					foreach (string link in LinkedNames)
						CUtility.Execute("ln","-s "+newfilename+" "+newpath + link + Path.GetExtension(Filename));
				}
			} catch (Exception e) {
				Console.WriteLine("Warning: "+e.Message);
			}
			return true;
		}

		private string contextToStr(ContextType context){
			switch (context){
			case ContextType.cActions:return "actions";
			case ContextType.cAnimations:return "animations";
			case ContextType.cApps:return "apps";
			case ContextType.cCategories:return "categories";
			case ContextType.cDevices:return "devices";
			case ContextType.cEmblems:return "emblems";
			case ContextType.cMimetypes:return "mimetypes";
			case ContextType.cPlaces:return "places";
			case ContextType.cStatus:return "status";
			case ContextType.cNone:return "toooldtoconvert";
			default: return "";
				
			}
		}

	}


}
