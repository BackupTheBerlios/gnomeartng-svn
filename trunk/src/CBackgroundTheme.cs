/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 2 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

Thomas Beck at 11:51 24.11.2007
*/

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
		//Reihenfolge muss mit GetStyles übereinstimmen
		public enum ImageStyle:int{
			isCenter,
			isFill,
			isScale,
			isZoom,
			isTile
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
		private static string GConfISKey = "/desktop/gnome/background/picture_options";
		private string prevBackground="";
		private string prevStyle="";
		
		//Speichern des aktuell gewählten Index und Liste 
		private ImageStyle currentStyle; 
		private string currentStyleString;
		
		private ImageType bgType; 
		private ArrayList currentList;
		private int currentIndex;
		
		//Aktueller Bild-Index (gibt somit die gewählte Auflösung an)
		public int ImageIndex{
			get {return currentIndex;}
			set {currentIndex = value;}
		}
		
		public ImageStyle ImgStyle{
			get{return currentStyle;}
			set{
				currentStyle=value;
				switch (currentStyle) {
				case ImageStyle.isCenter:
					currentStyleString = "centered"; break;
				case ImageStyle.isFill:
					currentStyleString = "stretched"; break;
				case ImageStyle.isScale:
					currentStyleString = "scaled"; break;
				case ImageStyle.isZoom:
					currentStyleString = "zoom"; break;
				case ImageStyle.isTile:
					currentStyleString = "wallpaper"; break;
				default: 
					currentStyleString = "zoom"; break;
				}
			}
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
			addImage(CUtility.StrToImageType(type),image);
		}
	
		public void addImage(CBackgroundImage image){
			if (currentList==null)
				throw new Exception("You have to set the BgType first!!!");
			currentList.Add(image);
		}
		

					
		public CBackgroundImage addNewImage(ImageType type){
			BgType=type;
			CBackgroundImage res=new CBackgroundImage();
			currentList.Add(res);
			return res;		
		}
			
		override protected void PreInstallation(CStatusWindow sw){
			InstallThemeFile = config.SplashInstallPath+Path.GetFileName(Image.URL);	
			//Set the localPreviewFile to the downloaded file to fasten the preview
			localPreviewFile = LocalThemeFile;
			
			Console.WriteLine(LocalPreviewFile);
			
			sw.Mainlabel=CConfiguration.txtSavingForRestore;
			//Sichern
			client = new GConf.Client();
			prevBackground=(string)client.Get(GConfBgKey);
			prevStyle = (string)client.Get(GConfISKey);
			sw.SetProgress("1/"+installationSteps);
			
			//Index und Type wurden vorher schon gwählt
			sw.Mainlabel=CConfiguration.txtDownloadTheme;

			if (!File.Exists(LocalThemeFile)) {
				if (!File.Exists(LocalPreviewFile)) {
					GetThemeFile(sw);
				} else {
					File.Copy(LocalPreviewFile,LocalThemeFile);
					Console.Out.WriteLine(LocalPreviewFile + " --- " + LocalThemeFile);
				}
			}
			try{File.Copy(LocalThemeFile,InstallThemeFile);} catch{}

			///home/.../.gnome2/backgrounds.xml einlesen und Background anhängen...
			//Just a hack...TODO:paste in XMLDoc
			if (!File.Exists(config.HomePath+".gnome2/backgrounds.xml")){
				StreamWriter locFile = new StreamWriter(config.HomePath+".gnome2/backgrounds.xml");
				locFile.WriteLine("<?xml version=\"1.0\"?>");
				locFile.WriteLine("<!DOCTYPE wallpapers SYSTEM \"gnome-wp-list.dtd\"[]>");
				locFile.WriteLine("  <wallpapers>");
				locFile.WriteLine("  </wallpapers>");
				locFile.Close();
			}
				
			XmlDocument doc = new XmlDocument();
			doc.Load(config.HomePath+".gnome2/backgrounds.xml");
			
			//Nach schon vorhandenem Eintrag in Backgrounds suchen
			XmlNode foundNode = doc.SelectSingleNode("wallpapers/wallpaper[filename='"+InstallThemeFile+"']");
			if (foundNode != null){
				XmlNodeList list = foundNode.ChildNodes;
				//Console.WriteLine("Count:"+list.Count);
				foreach (XmlNode node in list){
					if (node.Name=="options"){
						Console.WriteLine(node.InnerText);
						node.InnerText=currentStyleString;
						break;
					}
				}
					
			}
			else {
				XmlNode newElem;
				XmlNode wallpaper = doc.CreateNode(XmlNodeType.Element,"wallpaper","");
				((XmlElement)wallpaper).SetAttribute("deleted","false");
				
				newElem = doc.CreateNode(XmlNodeType.Element, "name", "");  
				newElem.InnerText = Path.GetFileName(InstallThemeFile);
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "filename", "");  
				newElem.InnerText = InstallThemeFile;
				wallpaper.AppendChild(newElem);
				newElem = doc.CreateNode(XmlNodeType.Element, "options", "");  
				newElem.InnerText = currentStyleString;
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
				doc.DocumentElement.AppendChild(wallpaper);
			}
			doc.Save(config.HomePath+".gnome2/backgrounds.xml");
			sw.SetProgress("2/"+installationSteps);
		}
		
		override protected void Installation(CStatusWindow sw){
			//Installieren
			sw.Mainlabel=CConfiguration.txtInstalling;
			client.Set(GConfBgKey,InstallThemeFile);
			client.Set(GConfISKey,currentStyleString);
		}
		
		override protected void PostInstallation(CStatusWindow sw){
			revertIsAvailable=true;
		}
		
		override public void Revert(){
			if (revertIsAvailable){
				//TODO:XML Eintrag löschen und gconf zurücksetzen
				client.Set(GConfBgKey,prevBackground);
				client.Set(GConfISKey,prevStyle);
			}
		}
		
		public CBackgroundTheme(CConfiguration config):base(config) {
			installationSteps = 3;
			ImgStyle = (ImageStyle)0;
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
				Resolutions[0]=Catalog.GetString("resolution-independend");
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
