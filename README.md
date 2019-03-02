# VisualEDK2
Allow building official TianoCore EDK2 with Visual Studio 2017

# How to build

* Download (or clone) this repo and submodules
* Open the *.sln file
* Build / Build Solution
* Press F5 to debug (choose StartUp project you want)

# How to make an EFI project

* Make a Makefile project
* Apply GlobalMacros.props to the project
* Make a new Property Sheet and add user macros PROJECT_FILES, BUILD_TARGET, TARGET_ARCH,...
* Make an INF for the project and link it to MdePkg, MdeModulePkg,... or custom one
* Build / Build Solution
* If you want to run it on QEMU just copy Debugging configuration from Shell project, and modify it as you want (the 1st agrument if the path to output EFI, and the second is PLATFORM_ARCH

# Credit
* [@TianoCore](https://github.com/tianocore) for EDK2
* [@pbatard](https://github.com/pbatard) for debug.vbs
* [@dung11112003](https://github.com/dung11112003)

