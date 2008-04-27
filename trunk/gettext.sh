#/bin/sh

#English-German
xgettext --from-code=UTF-8 *.cs gui.glade --no-location -o ./po/de_new.po
#English-Bulgarian
xgettext --from-code=UTF-8 *.cs gui.glade --no-location -o ./po/bg_new.po
#English-Portugese
xgettext --from-code=UTF-8 *.cs gui.glade --no-location -o ./po/pt_new.po

#Compile the texts
#msgfmt ./po/de.po -o locale/de/LC_MESSAGES/i18n.mo
#msgfmt ./po/bg.po -o locale/bg/LC_MESSAGES/i18n.mo
#msgfmt ./po/pt.po -o locale/pt/LC_MESSAGES/i18n.mo
