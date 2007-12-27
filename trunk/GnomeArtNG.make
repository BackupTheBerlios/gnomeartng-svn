

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/GnomeArtNG.exe
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Debug
endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4
ASSEMBLY = ./bin/Release/GnomeArtNG.exe
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Release
endif



	
all: $(ASSEMBLY) 

FILES = \
	Main.cs \
	AssemblyInfo.cs \
	gtk-gui/generated.cs \
	CArtManager.cs \
	CBackgroundImage.cs \
	CBackgroundTheme.cs \
	CGdmTheme.cs \
	CGtkTheme.cs \
	CIconTheme.cs \
	CSplashTheme.cs \
	CTheme.cs \
	CConfiguration.cs \
	CFileDownloader.cs \
	CPreviewWindow.cs \
	CApplicationTheme.cs \
	CWindowDecorationTheme.cs \
	CStatusWindow.cs \
	CInfoWindow.cs \
	CIniWorker.cs 

DATA_FILES = 

RESOURCES = \
	gui.glade \
	gtk-gui/gui.stetic \
	autogen.sh \
	Makefile.am \
	Makefile.include \
	configure.ac \
	Makefile.in 

EXTRAS = 

REFERENCES =  \
	$(GTK_SHARP_20_LIBS) \
	$(GLIB_SHARP_20_LIBS) \
	$(GLADE_SHARP_20_LIBS) \
	$(GCONF_SHARP_20_LIBS) \
	-r:System \
	-r:System.Xml \
	-r:System.Web.Services \
	-r:System.Web \
	-r:Mono.Posix

DLL_REFERENCES = 



$(build_resx_resources) : %.resources: %.resx
	resgen2 '$<' '$@'

$(ASSEMBLY) $(ASSEMBLY).mdb: $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES)
	mkdir -p $(dir $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)

include $(top_srcdir)/Makefile.include