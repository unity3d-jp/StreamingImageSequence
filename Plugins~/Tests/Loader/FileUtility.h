#include "CommonLib/Types.h" //charType

namespace StreamingImageSequencePluginTest {

class FileUtility {
public:
    static bool FileExists(const std::string&);
    static bool FileExists(const charType*);
    static std::string GetCurrentDirectory();

};

} // end namespace
