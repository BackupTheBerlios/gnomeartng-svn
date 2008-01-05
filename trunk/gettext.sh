#/bin/sh
xgettext --from-code=UTF-8 Main.cs CApplicationTheme.cs CArtManager.cs CBackgroundTheme.cs CConfiguration.cs CFileDownloader.cs CGdmTheme.cs CGtkTheme.cs CIconTheme.cs CPreviewWindow.cs CSplashTheme.cs CStatusWindow.cs CTheme.cs CWindowDecorationTheme.cs gui.glade -jo ./po/de.po
msgfmt ./po/de.po -o locale/de/LC_MESSAGES/i18n.mo
