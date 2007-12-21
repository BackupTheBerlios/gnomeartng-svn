// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CConfiguration.cs created with MonoDevelop
// User: neo at 23:13 25.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Diagnostics;
namespace GnomeArtNG

{
	
	
	public class CConfiguration
	{
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
		
		//Text constants for the installation procedures
		public static string txtExtracting="<i>Extracting the theme</i>\n\n"+
			"Your theme is downloaded and has to be extracted now. You might be prompted to enter your "+
			"user password to legetimate the copying of all extracted files to the right path.";
		public static string txtSavingForRestore="<i>Saving your previous settings</i>\n\n"+
			"Your previous settings are now stored to make it possible to revert your theme installation easily";
		public static string txtDownloadTheme="<i>Downloading the theme from art.gnome.org</i>\n\n"+
			"Your theme is being downloaded... There is no progress notification in this version, so all "+
			"you have to do is wait...the control is coming back, I promise :)";
		public static string txtInstalling="<i>Installing the theme</i>\n\n"+
			"Your Theme is now being installed. Therefore system files will get changed and/or your "+
			"GConf settings are changed. To revert this action and make all undone simply click on Revert.";
		public static string txtInstallDone="<i>Theme installation is complete</i>\n\n"+
			"Your Theme is now installed. You can now go on with your theme selection or (if you don't like the results) "+
			"revert your current selection. Have fun! ";
		
		//TODO: Möglichkeit die Pfade selbst zu setzen!
		private string dirSep="";
		private string homePath="";
		private string settingsPath="";
		private string thumbsPath="";
		private string thumbsDir="thumbs";
		private string themesPath="";
		private string themesDir="themes";
		private string previewPath="";
		private string previewDir="preview";
		private string splashInstallDir=".gnome";
		private string splashInstallPath="";
		private string applicationInstallPath="";
		private string iconInstallPath="";
		private string iconDir=".icons";
		private string gdmInstallPath="/usr/share/gdm/themes/";
		
		private bool tarIsAvailable=false;
		private bool grepIsAvailable=false;
		private bool sedIsAvailable=false;
		private ArtType artType;
		
		//Anschauen.Durcheinander mit getset und Funktionen dafür
		public string ProgramSettingsPath { get{ return settingsPath;} }
		public string ThumbsPath { get{ return settingsPath+dirSep+thumbsDir+dirSep+((int)(artType)).ToString()+dirSep;} }
		public string ThemesPath { get{ return settingsPath+dirSep+themesDir+dirSep+((int)(artType)).ToString()+dirSep;} }
		public string PreviewPath { get{ return settingsPath+dirSep+previewDir+dirSep+((int)(artType)).ToString()+dirSep;} }
		public string HomePath { get { return homePath;} }
		public ArtType ThemeType { 
			get { return artType;} 
			set { artType = value;} 
		}

		public string NoThumb{get{return "./linuxfirefox.gif";}}
		
		public string SplashInstallPath{get{return splashInstallPath;}}
		public string ApplicationInstallPath{get{return applicationInstallPath;}}
		public string IconInstallPath{get{return iconInstallPath;}}
		public string GdmInstallPath{get{return gdmInstallPath;}}
		
		public bool TarIsAvailable { get { return tarIsAvailable;} }
		public bool GrepIsAvailable { get { return grepIsAvailable;} }
		public bool SedIsAvailable { get { return sedIsAvailable;} }
		
		
		public string ArtFilePath(){
			return Path.Combine(settingsPath,"gnomeArt"+((int)artType).ToString()+".xml");
		}
		
		public string GetTarParams(string Filename){
			if (Path.GetExtension(Filename) == ".gz")
				return "-vxzf ";
			else
				return "-vxjf ";
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
		
		public string XmlFileUrl(){
			return "http://art.gnome.org/xml.php?art="+((int)artType).ToString();
		}
		
		//Wenn ohne artType aufgerufen mit atBackground aufrufen
		public CConfiguration():this(CConfiguration.ArtType.atBackground_gnome)	{
			
		}
		
		public CConfiguration(ArtType type)	{
			artType=type;
			dirSep=Path.DirectorySeparatorChar.ToString();
			homePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			settingsPath = Path.Combine(homePath,".gnome2"+dirSep+"gnome-art-ng");
			splashInstallPath = homePath+dirSep+splashInstallDir+dirSep;
			applicationInstallPath = homePath+dirSep+"."+themesDir+dirSep;
			iconInstallPath = homePath+dirSep+iconDir+dirSep;
			tarIsAvailable = TestIfProgIsInstalled("tar","--version","gnu tar");
			grepIsAvailable = TestIfProgIsInstalled("grep","--version","gnu grep");
			sedIsAvailable = TestIfProgIsInstalled("sed","--version","gnu sed");
			CreateDirectories();
		}
		
		public System.Text.StringBuilder Execute(string FileName, string Arguments){
			ProcessStartInfo psi = new ProcessStartInfo();
			psi.Arguments = Arguments;
			psi.FileName = FileName;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			Process proc = Process.Start(psi);
			while (proc.StandardOutput.Peek() != -1) 
				sb.Append(proc.StandardOutput.ReadLine());
			return sb;
		
		}
		
		private bool TestIfProgIsInstalled(string programName, string arguments, string lookFor) {
			lookFor=lookFor.ToLower();
			return (Execute(programName,arguments)).ToString().ToLower().Contains(lookFor);
		}
		
		public bool CreateDirectories(){
			//Im Homeverzeichnis unter .gnome2 einen Ordner gnome-art-ng anlegen
			//Des weiteren :
			//Ein Verzeichnis für jeden ArtType mit den VZ: thumbs/XX/... und themes/XX/
			//Somit kann später geprüft werden, ob u.U das Theme schon heruntergeladen wurde. 
			try{
				string dirSep=Path.DirectorySeparatorChar.ToString();
				Type enumType = typeof(ArtType);
				// Settings-Verzeichnis erzeugen
				if (!Directory.Exists(settingsPath)){
					Directory.CreateDirectory(settingsPath);
					Console.WriteLine("Configuration folder created: "+settingsPath);
				}
				
				// Vorschauverzeichnis
				if (!Directory.Exists(settingsPath+dirSep+thumbsDir)){
					Directory.CreateDirectory(settingsPath+dirSep+thumbsDir);
					foreach (ArtType art in Enum.GetValues(enumType))
						Directory.CreateDirectory(settingsPath+dirSep+thumbsDir+dirSep+((int)art).ToString());
					Console.WriteLine("Thumb folder created: "+thumbsPath);
				}
				
				//Previewverzeichnis
				if (!Directory.Exists(settingsPath+dirSep+previewDir)){
					Directory.CreateDirectory(settingsPath+dirSep+previewDir);
					foreach (ArtType art in Enum.GetValues(enumType))
						Directory.CreateDirectory(settingsPath+dirSep+previewDir+dirSep+((int)art).ToString());
					Console.WriteLine("Preview folder created: "+previewPath);
				}
				
				// Themeverzeichnis
				if (!Directory.Exists(settingsPath+dirSep+themesDir)){
					Directory.CreateDirectory(settingsPath+dirSep+themesDir);
					foreach (ArtType art in Enum.GetValues(enumType))
						Directory.CreateDirectory(settingsPath+dirSep+themesDir+dirSep+((int)art).ToString());
					Console.WriteLine("Themes folder created: "+themesPath);
				}
				return true;
			} catch { return false; }
			
		}
		
	}
}
