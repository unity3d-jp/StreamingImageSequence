# Building Plugins

1. [Windows](#building-on-windows)
1. [Mac OSX](#building-on-mac-osx)


## Building on Windows

### Prerequisites (Win)

1. Install [cmake](https://cmake.org/)  version 3.13 or later.  
   Make sure to choose one of the "Add CMake to the System PATH ..." options as shown below.  
   ![CMakeInstallation](../Images/CMakeInstallation.png)
   
1. Install Visual Studio 2019.
1. Install git. For example: [SourceTree](https://www.sourcetreeapp.com/)
    

### Build Steps (Win)


Start "Developer Command Prompt for VS 2019" and execute the following:

    ``` 
    $ git clone https://github.com/unity3d-jp/StreamingImageSequence.git
    $ cd StreamingImageSequence\Plugins~\Build 
    $ cmake -G "Visual Studio 16 2019" -A x64 ..
    $ msbuild StreamingImageSequencePlugin.sln /t:Build /p:Configuration=Release /p:Platform=x64 /m /nologo
    ```  

> For a regular "Command Prompt", there is a script: *VsDevCmd_2019.bat* 
> under the *Build* folder, which if executed, will turn the prompt into a 
> "Developer Command Prompt for VS 2019".


## Building on Mac OSX

### Prerequisites (Mac)

1. Install [cmake](https://cmake.org/)  version 3.5 or later, if not already installed.
1. Install [XCode](https://developer.apple.com/xcode/).
1. Install XCode Command Line tools.
    ``` 
    xcode-select --install
    ```  
1. Install git. For example: [SourceTree](https://www.sourcetreeapp.com/)


### Build Steps (Mac)

Open a terminal and execute the following

``` 
$ git clone https://github.com/unity3d-jp/StreamingImageSequence.git
$ cd StreamingImageSequence/Plugins~/Build 
$ cmake -GXcode ..
$ xcodebuild -scheme StreamingImageSequencePlugin -configuration Release build
```



