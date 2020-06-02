CMAKE_MINIMUM_REQUIRED(VERSION 3.13)
PROJECT(StreamingImageSequencePlugin)


#cmake variables
set(CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/Build/cmake_modules)

# Custom scripts
include(AddPlugin)

set(src_root ${CMAKE_CURRENT_SOURCE_DIR}/Src)

# ----------------------------------------------------------------------------------------------------------------------

if(APPLE)
    set(PLATFORM "Mac")
    find_library(CORE_GRAPHICS_LIBRARY CoreGraphics)
    mark_as_advanced(CORE_GRAPHICS_LIBRARY)    
elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(PLATFORM "Linux")
    set(LINUX ON)

elseif(WIN32) 

    add_definitions(-DPLUGIN_DLL_EXPORT)
    
    # To handle DLL loading error related to the requirement of having VCRUNTIME140_1.DLL
    # https://developercommunity.visualstudio.com/content/problem/852548/vcruntime140-1dll-is-missing.html
    add_definitions(-d2FH4-)
    add_link_options(-d2:-FH4-)
endif()

# set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++14")

# Force Unicode character set
add_definitions(-DUNICODE -D_UNICODE)


# ----------------------------------------------------------------------------------------------------------------------

option(BUILD_TESTS "Tests" OFF)

# ----------------------------------------------------------------------------------------------------------------------


#Specify available modules
add_subdirectory(${src_root}/External/stb)
add_subdirectory(${src_root}/CommonLib)
add_subdirectory(${src_root}/Loader)

if(WIN32)
    add_subdirectory(${src_root}/DrawOverWindow)
endif()

# Tests
if(BUILD_TESTS)
    add_subdirectory(${src_root}/PluginTest)
endif()

