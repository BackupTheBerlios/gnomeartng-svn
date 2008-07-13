/*
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; version 3 of the License.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
*/

/*
 * Thoughts: 
 * ProgramSettings in an other class!
 * */

using System;
using System.IO;
using System.Diagnostics;
using Mono.Unix;
using GnomeArtNG;
using System.Collections;

namespace GnomeArtNG

{
	public struct WindowAttrStruct {
		public int Height;
		public int Width;
		public int X;
		public int Y;
	}
		
	public struct ProxyAttrStruct {
		public bool Active;
		public int Port;
		private string ip; 
		public string Ip{
			get{return ip;}
			set{
				try{
					ip = CUtility.CheckAndSetIp(value);
				}
				catch {
					ip = "0.0.0.0";
				}
			}
		}
	}

	public class CConfiguration
	{
		public const string Version = "0.5.2";
		public enum ArtType:int{
			atBackground_gnome=10,
			atBackground_other, //11
			atBackground_all,//12
			atBackground_nature,//13
			atBackground_abstract,//14
			atApplication=20,
			atWindowDecoration, //21
			atIcon, //22
			atGdmGreeter=30,
			atSplash, //31
			atGtkEngine //32
		}
		
		public enum DistriType:int{
			dtUbuntu,
			dtKubuntu,
			dtSuse,
			dtFedora
		}
		
		//Texts for the installation procedures
		public static string txtExtracting=Catalog.GetString("<i>Extracting the theme</i>\n\n"+
			"Your theme is downloaded and has to be extracted now. You might be prompted to enter your "+
			"user password to legetimate the copying of all extracted files to the right path.");
		public static string txtSavingForRestore=Catalog.GetString("<i>Saving your previous settings</i>\n\n"+
			"Your previous settings are now stored to make it possible to revert your theme installation easily");
		public static string txtDownloadTheme=Catalog.GetString("<i>Downloading the theme from art.gnome.org</i>\n\n"+
			"Your theme is being downloaded...please be patient while the download is progressing");
		public static string txtInstalling=Catalog.GetString("<i>Installing the theme</i>\n\n"+
			"Your Theme is now being installed. Therefore system files will get changed and/or your "+
			"GConf settings are changed. To revert this action and make all undone simply click on Revert.");
		public static string txtInstallDone=Catalog.GetString("<i>Theme installation is complete</i>\n\n"+
			"Your Theme is now installed. You can now go on with your theme selection or (if you don't like the results) "+
			"revert your current selection. Have fun! ");

		private const string gConfPath ="/apps/gnome-art-ng/";
		public string GConfPath{get {return gConfPath;}}
		public const string UpdateUrl= "http://gnomeartng.plasmasolutions.de/version.info";
		public const string ThemeBulkUrl="http://download.berlios.de/gnomeartng/thumbs.tar.gz";
		public bool UpdateAvailable{
			get{
				return CUtility.IsAnUpdateAvailable(UpdateUrl,null,out NewestVersionNumberOnServer,out NewestVersionDownloadLocation,Proxy);
			}
		} 

		private string dirSep="";
		private string homePath="";
		private string settingsPath="";
		private string thumbsDir="thumbs";
		//private string themesPath="";
		private string themesDir="themes";
		//private string previewPath="";
		private string previewDir="preview";
		private string splashInstallDir=".gnome";
		private string splashInstallPath="";
		private string applicationInstallPath="";
		private string decorationInstallPath="";
		private string iconInstallPath="";
		private string iconDir=".icons";
		private string gdmInstallPath="/usr/share/gdm/themes/";
		private string gdmFile="";
		private string gdmCustomFile="";
		private string gdmPath="";
		private string themesDownloadPath ="";
		public string NewestVersionNumberOnServer="";
		public string NewestVersionDownloadLocation="";
		public bool DontBotherForUpdates=false;
		
		private bool neverStartedBefore=false;		
		private bool tarIsAvailable=false;
		private bool grepIsAvailable=false;
		private bool sedIsAvailable=false;
		private DistriType distribution = DistriType.dtUbuntu;
		private static string sudoCommand="gksudo";
		private string attribPrep="";
		private ArtType artType;
		
		//Anschauen.Durcheinander mit getset und Funktionen dafür
		public WindowAttrStruct Window;
		public ProxyAttrStruct Proxy;
		public string DirectorySeperator {get {return dirSep;}}
		public string ProgramSettingsPath { get{ return settingsPath;} }
		public string ThumbsPath { get{ return settingsPath+dirSep+thumbsDir+dirSep+((int)(artType)).ToString()+dirSep;} }
		public string ThemesPath { get{ return settingsPath+dirSep+themesDir+dirSep+((int)(artType)).ToString()+dirSep;} }
		public string PreviewPath { get{ return settingsPath+dirSep+previewDir+dirSep+((int)(artType)).ToString()+dirSep;} }
		public string HomePath { get { return homePath;} }
		public string SudoCommand { get { return sudoCommand;} }
		public string AttribPrep {get {return attribPrep;}}
		public DistriType Distribution {get {return distribution;}}
		public GConf.Client GConfClient;
		// interval in which a new xml file will be downloaded (in days)
		public int XmlRefreshInterval = 0;
		
		//Returns and sets the path that will be used to download themes to the hd 
		//(with "Slash" & "Backslash")
		public string ThemesDownloadPath{
			get{ return themesDownloadPath; }
			set{ 
				Console.Out.WriteLine(Path.GetFullPath(value));
				themesDownloadPath=Path.GetFullPath(value); 
			}
		}
		
		public ArtType ThemeType { 
			get { return artType;} 
			set { artType = value;} 
		}
				
		public string NoThumb{get{return "/usr/share/pixmaps/apple-red.png";}}
		
		public string SplashInstallPath{get{return splashInstallPath;}}
		public string ApplicationInstallPath{get{return applicationInstallPath;}}
		public string DecorationInstallPath{get{return decorationInstallPath;}}
		public string IconInstallPath{get{return iconInstallPath;}}
		public string GdmInstallPath{get{return gdmInstallPath;}}
		
		public bool TarIsAvailable { get { return tarIsAvailable;} }
		public bool GrepIsAvailable { get { return grepIsAvailable;} }
		public bool SedIsAvailable { get { return sedIsAvailable;} }
		public bool NeverStartedBefore{get{return neverStartedBefore;}}
		
		public string GdmFile{ get{return gdmFile;} }
		public string GdmCustomFile{ get{return gdmCustomFile;} }
		public string GdmPath{ get{return gdmPath;} }
		
		public string ArtFilePath(){
			return Path.Combine(settingsPath,"gnomeArt"+((int)artType).ToString()+".xml");
		}
		
		public string NodeEntryPath(){
			switch(artType){
				case ArtType.atBackground_all: 
				case ArtType.atBackground_gnome: 
				case ArtType.atBackground_other: 
				case ArtType.atBackground_nature: 
				case ArtType.atBackground_abstract: 
				  return Path.Combine("art","background");
				default:
				  return Path.Combine("art","theme"); 
			}
		}
		
		public bool BackgroundChoosen{
			get{return 
				(artType==ArtType.atBackground_gnome) | 
				(artType==ArtType.atBackground_other) |
				(artType==ArtType.atBackground_nature) |
				(artType==ArtType.atBackground_abstract);
			}
		}
		
		//Load all initial program settings (width, height, refresh interval, download folder...)
		public bool LoadProgramSettings(){
			try {
				Proxy.Active = false;
				//WindowAttributes
				Window.X = (int)GConfClient.Get(gConfPath+"xPosition");
				Window.Y = (int)GConfClient.Get(gConfPath+"yPosition");
				Window.Width = (int)GConfClient.Get(gConfPath+"width");
				Window.Height = (int)GConfClient.Get(gConfPath+"height");
				//Program paths				
				themesDownloadPath = (string)GConfClient.Get(gConfPath+"themesDownloadPath");
				XmlRefreshInterval = (int)GConfClient.Get(gConfPath+"xmlRefreshInterval");
				//ProxySettings
				Proxy.Active=(bool)GConfClient.Get(gConfPath+"useProxy");
				Proxy.Ip=(string)GConfClient.Get(gConfPath+"proxyIp");
				Proxy.Port=(int)GConfClient.Get(gConfPath+"proxyPort");
				
				//UpdateSettings
				DontBotherForUpdates = (bool)GConfClient.Get(gConfPath+"dontBotherMeForUpdates");
				return true;
			} catch {
				Console.Out.WriteLine("Old gconf settings detected...writing new settings!");
				SaveProgramSettings();
				return false;
			}
		}

		//All program relevant settings are saved here
		public void SaveProgramSettings(){
			try{
				GConfClient.Set(gConfPath+"xPosition",Window.X);
				GConfClient.Set(gConfPath+"yPosition",Window.Y);
				GConfClient.Set(gConfPath+"width",Window.Width);
				GConfClient.Set(gConfPath+"height",Window.Height);					
				GConfClient.Set(gConfPath+"themesDownloadPath",themesDownloadPath);
				GConfClient.Set(gConfPath+"xmlRefreshInterval",XmlRefreshInterval);
				GConfClient.Set(gConfPath+"useProxy",Proxy.Active);	
				GConfClient.Set(gConfPath+"proxyPort",Proxy.Port);	
				GConfClient.Set(gConfPath+"proxyIp",Proxy.Ip);	
				GConfClient.Set(gConfPath+"dontBotherMeForUpdates",DontBotherForUpdates);

			} catch (Exception e){
				Console.Out.WriteLine(Catalog.GetString("Error")+":"+Catalog.GetString("Program settings couldn't be saved")+", "+e.Message);
			}
		}
		
		private void setDistributionDependendSettings(){
			getDistribution();
			gdmFile="gdm.conf";
			gdmCustomFile="gdm.conf-custom";
			attribPrep="";
			switch (distribution) {
			 case DistriType.dtKubuntu: 
				sudoCommand="kdesu";
				break; 
			 case DistriType.dtUbuntu: 
				sudoCommand="gksudo"; 
				break;
			 case DistriType.dtSuse:
				sudoCommand="gnomesu"; 
				gdmFile="custom.conf";
				gdmCustomFile="custom.conf";
				attribPrep="--command=";
				break;
			 default: throw new Exception("Unknown distribution...aborting!!");
			 }
			gdmPath="/etc/gdm/";
		}
		
		private void getDistribution(){
			string issueFile="/etc/issue";
			if (!File.Exists(issueFile))
				throw new Exception("Could not get /etc/issue...aborting!!");
			StreamReader myFile = new StreamReader(issueFile, System.Text.Encoding.Default);
            string sContent = myFile.ReadToEnd();
			sContent=sContent.ToLower();
            myFile.Close();
			
			//(K)Ubuntu, Suse
			if (sContent.IndexOf("kubuntu",StringComparison.CurrentCulture)>-1)
				distribution=DistriType.dtKubuntu;
			if (sContent.IndexOf("ubuntu",StringComparison.CurrentCulture)>-1)
				distribution=DistriType.dtUbuntu;
			if (sContent.Contains("suse"))
				distribution=DistriType.dtSuse;

			Console.WriteLine(distribution);
		}
		
		public string XmlFileUrl(){
			return "http://art.gnome.org/xml.php?art="+((int)artType).ToString();
		}
		
		//Wenn ohne artType aufgerufen mit atBackground aufrufen
		public CConfiguration():this(CConfiguration.ArtType.atBackground_gnome)	{
			
		}
		
		public CConfiguration(ArtType type)	{
			GConfClient = new GConf.Client();
			Proxy = new ProxyAttrStruct();
			Proxy.Ip = ""; //TODO:Why is it important to initialize it...an error occurs otherwise. 
			artType=type;
			dirSep=Path.DirectorySeparatorChar.ToString();
			homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal)+dirSep;
			settingsPath = Path.Combine(homePath,".gnome2"+dirSep+"gnome-art-ng");
			splashInstallPath = homePath+splashInstallDir+dirSep;
			applicationInstallPath = homePath+"."+themesDir+dirSep;
			decorationInstallPath = homePath+"."+themesDir+dirSep;
			iconInstallPath = homePath+iconDir+dirSep;
			tarIsAvailable = CUtility.TestIfProgIsInstalled("tar","--version","gnu tar");
			grepIsAvailable = CUtility.TestIfProgIsInstalled("grep","--version","gnu grep");
			sedIsAvailable = CUtility.TestIfProgIsInstalled("sed","--version","gnu sed");
			setDistributionDependendSettings();
			CreateDownloadDirectories();
			if (UpdateAvailable)
				Console.WriteLine("An update is available, newest version is: "+NewestVersionNumberOnServer);
		}
				
		private void CreateDownloadFilesysStructure(string directoryName){
			string dirToCreate = "";
			Type enumType = typeof(ArtType);
			
			dirToCreate = settingsPath+dirSep+directoryName;
			if (!Directory.Exists(dirToCreate)){
				Directory.CreateDirectory(dirToCreate);
				Console.WriteLine("Folder created: "+dirToCreate);
				neverStartedBefore = true;
			}
			foreach (ArtType art in Enum.GetValues(enumType)){
				dirToCreate = settingsPath+dirSep+directoryName+dirSep+((int)art).ToString(); 
				if (!Directory.Exists(dirToCreate)){
					Directory.CreateDirectory(dirToCreate);
				Console.WriteLine("Folder created: "+ dirToCreate);
				}
			}
		}
		
		public bool CreateDownloadDirectories(){
			//Im Homeverzeichnis unter .gnome2 einen Ordner gnome-art-ng anlegen
			//Und: Ein Verzeichnis für jeden ArtType mit den VZ: thumbs/XX/... und themes/XX/
			try{
			
				// Create settings structure
				if (!Directory.Exists(settingsPath)){
					Directory.CreateDirectory(settingsPath);
					Console.WriteLine("Configuration folder created: "+settingsPath);
					neverStartedBefore=true;
				}

				//Create .gnome directory if not existant
				if (!Directory.Exists(homePath+".gnome")){
					Directory.CreateDirectory(homePath+".gnome");
					Console.WriteLine("Configuration folder created: "+homePath+".gnome");
					neverStartedBefore=true;					
				} 
				
				// create preview folders
				CreateDownloadFilesysStructure(previewDir);
				// create thumb folders
				CreateDownloadFilesysStructure(thumbsDir);
				// create themes folders
				CreateDownloadFilesysStructure(themesDir);
				
				return true;
			} catch { return false; }
		}
	}
}
