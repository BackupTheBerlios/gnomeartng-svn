// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CArtManager.cs created with MonoDevelop
// User: neo at 11:49 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Xml;
using System.Net;
using System.IO;
using System.Collections;
using Mono.Unix;
using System.Threading;

namespace GnomeArtNG
{
	
	public class CArtManager
	{	
		//Aktuell gewählter Theme-Typ steckt in config.ThemeType
		
		private CConfiguration config;

		//Themelisten
		public ArrayList SplashThemeList;
		public ArrayList Gnome_BackgroundThemeList;
		public ArrayList Other_BackgroundThemeList;
		public ArrayList Nature_BackgroundThemeList;
		public ArrayList Abstract_BackgroundThemeList;
		public ArrayList GdmGreeterThemeList;
		public ArrayList GtkThemeList;
		public ArrayList WindowDecorationThemeList;
		public ArrayList ApplicationThemeList;
		public ArrayList IconThemeList;

		//Aktuelles Theme
		private CTheme currentTheme;
		//Aktueller zum Theme passender Index in der ThemeListe
		private int currentThemeIndex;
			
		/*Sobald der Typ geändert wird, wird die Interne Variable currentType gesetzt,
		  die XML-Datei heruntergeladen (falls sie alt ist), die Datei geparsed und die 
		  ArrayListen gefüllt
		 */ 
		public CConfiguration.ArtType ArtType {
			get{ return config.ThemeType; }
			set{
				config.ThemeType=value;
				getParseAndCreate(false);
				ThemeIndex = 0;
			}
		}
		
		public void GetThumb(){
			try{
				ArrayList list = getThemeList();
				((CTheme)list[currentThemeIndex]).GetThumbnailImage();
			} catch {
				//TODO:Fehlerbehandlung!
			}
		}
		
		public void GetThumb(int Index){
			ThemeIndex=Index;
			GetThumb();
		}
		
		public int ThemeIndex{
			get {return currentThemeIndex;}
			set {
				currentThemeIndex = value;
				ArrayList tempList=getThemeList();
				if ((value<0) || (value>tempList.Count-1))
					throw new Exception(String.Format("ThemeIndex has to be in range ({0}-{1})",0,tempList.Count));
				currentTheme=(CTheme)(tempList[currentThemeIndex]);
			}
		}
		
		public CTheme Theme{
			get{ return currentTheme; }
		}
	
		/*Gibt die Anzahl der vorhandenen Themes in einer Artgruppe an*/
		public int ThemeCount{
			get { return getThemeList().Count; }
		}
	
		public CArtManager(CConfiguration config) {
			
			//Konfigurationsobjekt: Enthält Pfade und Hilfsmethoden
			this.config=config;
			
			//Listen zum speichern der Themes anlegen
			SplashThemeList = new ArrayList();
			Gnome_BackgroundThemeList=new ArrayList();
			Other_BackgroundThemeList=new ArrayList();
			Nature_BackgroundThemeList=new ArrayList();
			Abstract_BackgroundThemeList=new ArrayList();
			GdmGreeterThemeList=new ArrayList();
			GtkThemeList=new ArrayList();
			ApplicationThemeList=new ArrayList();
			WindowDecorationThemeList=new ArrayList();
			IconThemeList = new ArrayList();
		}
		
		/*private bool DownloadFile(string From, string To){
	        try {
				WebClient myClient = new WebClient();
				Console.WriteLine(Catalog.GetString("Downloading")+ " " + Path.GetFileName(From));
				myClient.DownloadFile(From, To);
				Console.WriteLine(Catalog.GetString("Finished") + " " + Path.GetFileName(From));
				return true;
			} catch { return false; }
		}*/
		
		//Die für die jeweilige Option richtige XMLDatei herunterladen wenn sie veraltet ist
		private void getXMLFile(bool ForceReload){
			//TODO:Bisher Zwischenlösung: Da ich die statische DownloadFile-Methode im CTheme untergebracht habe
			//benutze ich CTHeme zum Download...auslagern..bloß wohin, CConfiguration? ...aber dann auch umbenennen
			string remoteUri = config.XmlFileUrl();
			string localFileName = config.ArtFilePath();
			bool downloadFile=false;
			if (!ForceReload){
		        if (File.Exists(localFileName)) {
		            if ( DateTime.Compare(File.GetCreationTime(localFileName).Date, DateTime.Now.Date) < 0) {
		                Console.WriteLine("Xml file one day old - downloading new one");
						downloadFile = true;
		            }
		        }
		        else {
		            downloadFile=true;
		        }
			} else 
				downloadFile=true;
	        
		
			if (downloadFile){
				CStatusWindow sw= new CStatusWindow(Catalog.GetString("Downloading new XML file"),1,false,true,true);
				sw.ButtonSensitive=false;
				sw.ExtInfoLabel=Catalog.GetString("Downloading")+": " + remoteUri;
				sw.Mainlabel=Catalog.GetString("<i>XML file is beeing downloaded</i>\n\nThis program needs a descriptive "+
				                               "file that contains information about \n\n1) where the theme is located,"+
				                               "\n2) what kind of theme is it, \n3) How often someone downloaded it,"+
				                               " and so on \n\nSo please be patient while the download progress is going on..");
				sw.SetProgress(Catalog.GetString("downloading... (no progress available)"));
				sw.Invalidate();
				try	{
					CTheme.DownloadFile(remoteUri,localFileName,sw.DetailProgressBar);
				}
				catch (Exception ex) {
					sw.Close();
					new CInfoWindow(Catalog.GetString("Warning: Could not download xml file!"),ex.Message,Gtk.Stock.DialogError,true);
				}
				sw.Close();
		    }

		}
	    			
		private int GetIntFromXML(string s){
			if (s=="")
				return 0;
			else
				return int.Parse(s);
		}
		
		private CTheme CreateNewThemeObject(){
			CTheme Theme;
			switch (config.ThemeType) {
				case CConfiguration.ArtType.atBackground_gnome:
				case CConfiguration.ArtType.atBackground_other:
				case CConfiguration.ArtType.atBackground_nature:
				case CConfiguration.ArtType.atBackground_abstract:
					Theme=new CBackgroundTheme(config);break;
				case CConfiguration.ArtType.atGdmGreeter:
					Theme=new CGdmTheme(config);break;
				case CConfiguration.ArtType.atGtkEngine:
					Theme=new CGtkTheme(config);break;
				case CConfiguration.ArtType.atIcon:
					Theme=new CIconTheme(config);break;
				case CConfiguration.ArtType.atSplash:
					Theme=new CSplashTheme(config); break;
				case CConfiguration.ArtType.atApplication:
					Theme=new CApplicationTheme(config); break;
				case CConfiguration.ArtType.atWindowDecoration:
					Theme=new CWindowDecorationTheme(config); break;
			default:Theme=new CBackgroundTheme(config); break;
			}
			return Theme;
		}

		private ArrayList getThemeList(){
			switch (config.ThemeType) {
				case CConfiguration.ArtType.atBackground_gnome:
					return Gnome_BackgroundThemeList;
				case CConfiguration.ArtType.atBackground_other:
					return Other_BackgroundThemeList;
				case CConfiguration.ArtType.atBackground_nature:
					return Nature_BackgroundThemeList;
				case CConfiguration.ArtType.atBackground_abstract:
					return Abstract_BackgroundThemeList;
				case CConfiguration.ArtType.atGdmGreeter:
					return GdmGreeterThemeList;
				case CConfiguration.ArtType.atGtkEngine:
					return GtkThemeList;
				case CConfiguration.ArtType.atIcon:
					return IconThemeList;
				case CConfiguration.ArtType.atSplash:
					return SplashThemeList;
				case CConfiguration.ArtType.atApplication:
					return ApplicationThemeList;
				case CConfiguration.ArtType.atWindowDecoration:
					return WindowDecorationThemeList;

			default: Console.WriteLine("GetThemeList:null"); return null;
			}
		}
		
		//Löscht die zu der currentArt passende Liste
		private void ClearThemeList(){
			getThemeList().Clear();
		}
		
		//Liest oder lädt die XMLDatei, parst falls noch nicht vorher geparst wurde
		private void getParseAndCreate(bool ForceReload){
			getXMLFile(ForceReload);
			if (ForceReload){
				ClearThemeList();
			}
			parseXMLFile();
		}
		
		//Die XMLDatei durchgehen und die Objekte erzeugen
		private void parseXMLFile() {
			if (getThemeList().Count==0){
				XmlDocument doc = new XmlDocument();
				doc.Load(config.ArtFilePath());
				
				// Retrieve all bg from the xml
				XmlNodeList entries = doc.SelectNodes(config.NodeEntryPath());
				Console.WriteLine("Number of themes: "+entries.Count);
				
				foreach(XmlNode node in entries){
					
					//Neues Objekt anlegen, wird zu einem anderen gecastet wenn es nötigt ist
					CTheme Theme=CreateNewThemeObject();
					
					//Background-Attribute auslesen 
					if (node.NodeType == XmlNodeType.Element) {
						XmlAttributeCollection attributes = node.Attributes;
						XmlNode item=null;
						for (int i=0;i<attributes.Count;i++){
							item = attributes.Item(i);
							if (item.Name=="release_date")
								Theme.ReleaseDate=item.InnerText;
							else if	(item.Name=="vote_sum")
								Theme.VoteSum=GetIntFromXML(item.InnerText);
							else if	(item.Name=="vote_count")
								Theme.VoteCount=GetIntFromXML(item.InnerText);
							else if	(item.Name=="download_start_timestamp")
								Theme.Timestamp=item.InnerText;
							else if	(item.Name=="download_count")
								Theme.DownloadCount=GetIntFromXML(item.InnerText);
						}
						
						XmlNodeList Childs = node.ChildNodes;
						for (int i=0;i<Childs.Count;i++){
							item = Childs.Item(i);
							if (item.Name=="name")
								Theme.Name=item.InnerText;
							else if	(item.Name=="description")
								Theme.Description=item.InnerText;
							else if	(item.Name=="category")
								Theme.Category=item.InnerText;
							else if	(item.Name=="author")
								Theme.Author=item.InnerText;
							else if	(item.Name=="license")
								Theme.License=item.InnerText;
							else if	(item.Name=="thumbnail"){
								//In der Background-XML ist "Thumbnail"==SmallThumbnailUrl
								if (config.BackgroundChoosen) { 
									Theme.SmallThumbnailUrl=item.InnerText;
									Theme.PreviewUrl=item.InnerText;;
								} else
									Theme.PreviewUrl=item.InnerText;
							}
							else if (item.Name=="small_thumbnail")
								Theme.SmallThumbnailUrl=item.InnerText;
							else if (item.Name=="url"){
								Theme.DownloadUrl=item.InnerText;
							}
							else if (item.Name=="background_resolution"){
								XmlNodeList BackgroundResolutions = item.ChildNodes;
								CBackgroundImage bgImg=new CBackgroundImage();
								for (int j=0;j<BackgroundResolutions.Count;j++){
									XmlNode bgitem = BackgroundResolutions.Item(j);
									if (bgitem.Name=="type"){
										bgImg.Type=bgitem.InnerText;
										((CBackgroundTheme)Theme).addImage(bgImg.Type,bgImg);
									}
									else if	(bgitem.Name=="resolution"){
										string tmp=bgitem.InnerText;
										if (tmp.Contains("scalable")){
											bgImg.xResolution=9999;
											bgImg.yResolution=9999;
										} else{
											string[] sa=tmp.Split('x');
											bgImg.xResolution=int.Parse(sa[0]);
											bgImg.yResolution=int.Parse(sa[1]);
										}
									}
									//Bei Hintergründen ist die URL auch gleichzeitig die DownloadUrl..jeweils die größte wählen
									else if	(bgitem.Name=="url"){
										bgImg.URL=bgitem.InnerText;
										Theme.DownloadUrl=bgImg.URL;
									}
								}
							}
						}
					}
					//Füge Theme in die VektorArrays ein
					AddThemeToVectorArray(Theme);
				}
			}  //if Cout==0
		}

		public CTheme GetTheme(int Index){
			ThemeIndex = Index;
			ArrayList list=getThemeList();
			if ((Index<list.Count)&(Index>=0))
				return Theme;
			else
				throw new IndexOutOfRangeException("Illegal index in GetTheme");
		}	
		
		public void GetAllThumbs(){
			ArrayList list = getThemeList();
			ArrayList downloadList = new ArrayList();
			CTheme theme;
			
			int themeCount=list.Count;
			CStatusWindow w= new CStatusWindow(Catalog.GetString("Reading thumbnails on harddisc"),themeCount,true,false,true);
			w.ButtonSensitive=false;
			//Thumbs von der Platte lesen
			
			for (int i=0;i<(int)(themeCount/5);i++){
				theme = ((CTheme)list[i]);
				w.SetProgress((i+1).ToString()+"/"+themeCount.ToString());
				w.ExtInfoLabel = theme.SmallThumbnailUrl;
				if (!theme.LocalThumbExists)
					downloadList.Add(theme);
				else 
					theme.GetThumbnailImage();
			}
			
			//Thumbs herunterladen
			int downloadCount=downloadList.Count;
			if (downloadCount>0){
				w.Headline=Catalog.GetString("Downloading missing thumbnails");
				w.ButtonSensitive=true;
				w.SetProgressStep(downloadList.Count);
				Console.WriteLine("FileCount: "+downloadList.Count);
				
				//Thread[] ta = new Thread[downloadCount];
				
				for (int i=0;i<downloadList.Count;i++){
					if (w.CloseRequested){
						break;
					}
					theme = ((CTheme)downloadList[i]);
					w.SetProgress((i+1).ToString()+"/"+downloadCount.ToString());
					w.ExtInfoLabel = Path.GetFileName(theme.SmallThumbnailUrl);
					/*Threading
					ta[i] = new Thread(new ThreadStart(theme.GetThumbnailImage));
					ta[i].Start(); // Threads werden gestartet
					*/
					theme.GetThumbnailImage();
				}
				/* Threading
				bool done=false;
				while (!done){
					done=true;
					for (int k=0;k<downloadCount;k++){
						if (ta[k].ThreadState != ThreadState.Stopped)
							done=false;
					}
							
				}
				*/
				w.Close();
			}
		}

		public void GetAllThumbs(CConfiguration.ArtType Type){
			ArtType=Type;
			GetAllThumbs();
		}
		
		private void AddThemeToVectorArray(CTheme Theme){
			switch (config.ThemeType){
				case CConfiguration.ArtType.atBackground_gnome:
					Gnome_BackgroundThemeList.Add((CBackgroundTheme)Theme); break;
				case CConfiguration.ArtType.atBackground_other:
					Other_BackgroundThemeList.Add((CBackgroundTheme)Theme); break;
				case CConfiguration.ArtType.atBackground_nature:
					Nature_BackgroundThemeList.Add((CBackgroundTheme)Theme); break;
				case CConfiguration.ArtType.atBackground_abstract:
					Abstract_BackgroundThemeList.Add((CBackgroundTheme)Theme); break;
				case CConfiguration.ArtType.atGdmGreeter:
					GdmGreeterThemeList.Add((CGdmTheme)Theme); break;
				case CConfiguration.ArtType.atGtkEngine:
					GtkThemeList.Add((CGtkTheme)Theme); break;
				case CConfiguration.ArtType.atIcon:
					IconThemeList.Add((CIconTheme)Theme); break;
				case CConfiguration.ArtType.atSplash:
					SplashThemeList.Add((CSplashTheme)Theme); break;
				case CConfiguration.ArtType.atApplication:
					ApplicationThemeList.Add((CApplicationTheme)Theme); break;
				case CConfiguration.ArtType.atWindowDecoration:
					WindowDecorationThemeList.Add((CWindowDecorationTheme)Theme); break;
			default: Console.WriteLine("AddThemeToVectorArray:null"); break;
			}
			
		}
	}

}