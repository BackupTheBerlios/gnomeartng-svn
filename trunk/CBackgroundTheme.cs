// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CBackgroundTheme.cs created with MonoDevelop
// User: neo at 11:51 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Collections;
using System.Net;
using GConf;
using System.Xml;
using Mono.Unix;

namespace GnomeArtNG
{
	
	public class CBackgroundTheme: CTheme
	{
		public enum ImageType:int{
			itPng,
			itJpg,
			itSvg
		}
		
		//Listen mit den Hintergrundbildern
		private ArrayList PngResolutionList;
		private ArrayList JpgResolutionList;
		//Diese Liste hat immer nur einen Eintrag..wegen einfacherem Zugriff ist es trotzdem eine Liste
		private ArrayList SvgResolutionList;
		
		//Vars zum Installieren
		private string InstallThemeFile="";
		private GConf.Client client; 
		// Die zu verändernden Werte in der Gnome Registry
		private static string GConfBgKey = "/desktop/gnome/background/picture_filename";
		private string prevBackground="";
		
		//Speichern des aktuell gewählten Index und Liste 
		private ImageType bgType; 
		private ArrayList currentList;
		private int currentIndex;
		
		//Aktueller Bild-Index (gibt somit die gewählte Auflösung an)
		public int ImageIndex{
			get {return currentIndex;}
			set {currentIndex = value;}
		}
		
		//Png,Jpg...welchen Typ hat das Bild?
		public ImageType BgType{
			get{ return bgType; }
			set{ 
				bgType=value;
				currentList = getResolutionList();
				currentIndex = 0;
			}
		}
		
		public CBackgroundImage Image{
			get{ return ((CBackgroundImage)currentList[currentIndex]);}
		}
		
		private ArrayList getResolutionList(){
			switch (bgType){
			case ImageType.itPng: 
				currentList = PngResolutionList; break; 
			case ImageType.itJpg: 
				currentList = JpgResolutionList; break;
			case ImageType.itSvg: 
				currentList = SvgResolutionList; break;
			default:
				throw new Exception("Unknown image type in GetResolutionList");
			}
			return currentList;
		}
		
		public void addImage(ImageType type, CBackgroundImage image){
			BgType = type;
			addImage(image);
		}
			
		public void addImage(string type, CBackgroundImage image){
			//Console.WriteLine(type.ToLower());
			addImage(GetImageTypeFromString(type),image);
		}
	
		public void addImage(CBackgroundImage image){
			if (currentList==null)
					throw new Exception("You have to set the BgType first!!!");
			currentList.Add(image);
		}
		
		public ImageType GetImageTypeFromString(string type){
			if (type.ToLower()=="png")
				return ImageType.itPng;
			else if (type.ToLower()=="jpg")
				return ImageType.itJpg;
			else if (type.ToLower()=="svg")
				return ImageType.itSvg;
			else throw new Exception(String.Format("Image Type {0} couldn't been recognized",type));
		}
					
		public CBackgroundImage addNewImage(ImageType type){
			BgType=type;
			CBackgroundImage res=new CBackgroundImage();
			currentList.Add(res);
			return res;		
		}
			
		override protected void PreInstallation(CStatusWindow sw){
			LocalThemeFile=config.ThemesPath+Path.GetFileName(Image.URL);
			InstallThemeFile=config.SplashInstallPath+Path.GetFileName(Image.URL);	
			
			sw.Mainlabel=CConfiguration.txtSavingForRestore;
			//Sichern
			client = new GConf.Client();
			prevBackground=(string)client.Get(GConfBgKey);
			sw.SetProgress("1/"+installationSteps);
			
			//Index und Type wurden vorher schon gwählt
			sw.Mainlabel=CConfiguration.txtDownloadTheme;
			if (!File.Exists(InstallThemeFile)) {
				if (!File.Exists(LocalThemeFile)){
					if (!File.Exists(LocalPreviewFile)){
						DownloadFile(Image.URL, LocalThemeFile,sw.DetailProgressBar);
					} else 
						File.Copy(LocalPreviewFile,LocalThemeFile);
				}
				File.Copy(LocalThemeFile,InstallThemeFile);

				///home/.../.gnome2/backgrounds.xml einlesen und Background anhängen...
				XmlDocument doc = new XmlDocument();
				doc.Load(config.HomePath+"/.gnome2/backgrounds.xml");
				XmlNode newElem;
				XmlElement root = doc.DocumentElement;
				XmlNode wallpaper = doc.CreateNode(XmlNodeType.Element,"wallpaper","");
				((XmlElement)wallpaper).SetAttribute("deleted","false");
				
				newElem = doc.CreateNode(XmlNodeType.Element, "name", "");  
				newElem.InnerText = Path.GetFileName(InstallThemeFile);
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "filename", "");  
				newElem.InnerText = InstallThemeFile;
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "options", "");  
				newElem.InnerText = "zoom";
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "shade_type", "");  
				newElem.InnerText = "solid";
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "pcolor", "");  
				newElem.InnerText = "#dadab0b08282";
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "scolor", "");  
				newElem.InnerText = "#dadab0b08282";
				wallpaper.AppendChild(newElem);
				root.AppendChild(wallpaper);
				doc.Save(config.HomePath+"/.gnome2/backgrounds.xml");
			}
			sw.SetProgress("2/"+installationSteps);
		}
		

		override protected void Installation(CStatusWindow sw){
			//Installieren
			sw.Mainlabel=CConfiguration.txtInstalling;
			client.Set(GConfBgKey,InstallThemeFile);
		}
		
		override protected void PostInstallation(CStatusWindow sw){
			revertIsAvailable=true;
		}
		
		override public void Revert(){
			if (revertIsAvailable){
				//XML Eintrag löschen und gconf zurücksetzen
				client.Set(GConfBgKey,prevBackground);
			}
		}
		
		public CBackgroundTheme(CConfiguration config):base(config) {
			installationSteps = 3;
			useUrlAsPreview = true;
			PngResolutionList = new ArrayList();
			JpgResolutionList = new ArrayList();
			SvgResolutionList = new ArrayList();
		}
		
		//Same as GetResolutions() but sets the ImageType first
		public string[] GetAvailableResolutions(ImageType type){
			BgType=type;
			return GetAvailableResolutions();
		}
		
		//Liefert alle zum aktuell gewählten Typ verfügbaren Auflösungen zurück
		public string[] GetAvailableResolutions(){
			string[] Resolutions=new string[currentList.Count];
			if (bgType==ImageType.itSvg)
				Resolutions[0]="SVG is resolution-independend";
			else for(int i=0;i<currentList.Count;i++){
				CBackgroundImage bgImg=(CBackgroundImage)(currentList[i]);
				Resolutions[i]=bgImg.xResolution +"x"+ bgImg.yResolution;
			}
			return Resolutions;
		}
		
		public string[] GetAvailableTypes(){
			ArrayList list = new ArrayList();
			if (PngResolutionList.Count>0)
				list.Add("Png");
			if (JpgResolutionList.Count>0)
				list.Add("Jpg");
			if (SvgResolutionList.Count>0)
				list.Add("Svg");
			return (string[])list.ToArray(typeof(string));  
		}

	}
}
