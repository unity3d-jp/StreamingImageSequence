#include "FileUtility.h"
#include <fstream> //ifstream

#ifdef _WIN32
#include <direct.h>
#define GetCurrentDir _getcwd
#else
#include <unistd.h>
#define GetCurrentDir getcwd
#endif

//----------------------------------------------------------------------------------------------------------------------


namespace StreamingImageSequencePluginTest {

bool FileUtility::FileExists(const std::string& filePath) {
    return FileExists(filePath.c_str());
}

//----------------------------------------------------------------------------------------------------------------------

bool FileUtility::FileExists(const charType* filePath) {
    std::ifstream stream(filePath);
    const bool ret = stream.is_open();
    return ret;

}

//----------------------------------------------------------------------------------------------------------------------

std::string FileUtility::GetCurrentDirectory() {
    char buff[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir(buff, FILENAME_MAX);
    std::string dir(buff);
    return dir;

}

} //end namespace
