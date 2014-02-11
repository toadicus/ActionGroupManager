# Makefile for building MechJeb

KSPDIR  := ${HOME}/.local/share/Steam/SteamApps/common/Kerbal\ Space\ Program
MANAGED := ${KSPDIR}/KSP_Data/Managed/
TOOLBAR := ../ksp_toolbar/

FILES := $(wildcard src/*.cs) \
         $(wildcard ${TOOLBAR}/Toolbar/*/*.cs) \
         $(wildcard ${TOOLBAR}/Toolbar/Internal/*/*.cs)

GMCS    := gmcs
GIT     := git
TAR     := tar
ZIP     := zip

all: build

info:
	@echo "== ActionGroupManager Build Information =="
	@echo "  gmcs:    ${GMCS}"
	@echo "  git:     ${GIT}"
	@echo "  tar:     ${TAR}"
	@echo "  zip:     ${ZIP}"
	@echo "  KSP Data: ${KSPDIR}"
	@echo "================================"
	@echo "${FILES}"

build: info
	mkdir -p build
	${GMCS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass,UnityEngine \
		-out:build/ActionGroupManager.dll \
		${FILES}

package: build
	mkdir -p package/ActionGroupManager/Plugins
	cp COPYING.txt package/ActionGroupManager/
	cp build/ActionGroupManager.dll package/ActionGroupManager/Plugins/

tar.gz: package
	${TAR} zcf ActionGroupManager-$(shell ${GIT} describe --tags --long --always).tar.gz package/ActionGroupManager

zip: package
	${ZIP} -9 -r ActionGroupManager-$(shell ${GIT} describe --tags --long --always).zip package/ActionGroupManager

clean:
	@echo "Cleaning up build and package directories..."
	rm -rf build/ package/

install: build
	mkdir -p ${KSPDIR}/GameData/ActionGroupManager/Plugins
	cp build/ActionGroupManager.dll ${KSPDIR}/GameData/ActionGroupManager/Plugins/

uninstall: info
	rm -rf ${KSPDIR}/GameData/ActionGroupManager/Plugins


.PHONY : all info build package tar.gz zip clean install uninstall
