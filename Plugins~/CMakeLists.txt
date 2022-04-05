CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

# Mac deployment target
if(APPLE)
    set(CMAKE_OSX_DEPLOYMENT_TARGET "10.12" CACHE STRING "Minimum OS X deployment version")
    set(CMAKE_OSX_ARCHITECTURES "arm64;x86_64" CACHE STRING "Target Architecture" FORCE)
endif()

PROJECT(StreamingImageSequencePlugin)

# allow CMake to organize files by folders
set_property(GLOBAL PROPERTY USE_FOLDERS ON)

#cmake variables
set(CMAKE_MODULE_PATH ${CMAKE_SOURCE_DIR}/Build/cmake_modules)

# Change compilation flags for testing
option(BUILD_UNIT_TESTS "Unit Tests" OFF)
if(BUILD_UNIT_TESTS)
    if(APPLE)
        find_library(COCOA_LIBRARY Cocoa)
    elseif(WIN32)
        set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT")
        set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd")
    endif()
endif()

# Custom scripts
include(AddPlugin)

set(src_root ${CMAKE_CURRENT_SOURCE_DIR}/Src)
set(tests_root ${CMAKE_CURRENT_SOURCE_DIR}/Tests)

# ----------------------------------------------------------------------------------------------------------------------

if(APPLE)
    set(PLATFORM "OSX")
    add_definitions(-DOSX)
    find_library(CORE_GRAPHICS_LIBRARY CoreGraphics)
    mark_as_advanced(CORE_GRAPHICS_LIBRARY)    

    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")

elseif(${CMAKE_SYSTEM_NAME} STREQUAL "Linux")
    set(PLATFORM "Linux")
    set(LINUX ON)
    
elseif(WIN32) 
    set(PLATFORM "Win64")

    add_definitions(-DPLUGIN_DLL_EXPORT)
    
    # To handle DLL loading error related to the requirement of having VCRUNTIME140_1.DLL
    # https://developercommunity.visualstudio.com/content/problem/852548/vcruntime140-1dll-is-missing.html
    add_definitions(-d2FH4-)
    add_link_options(-d2:-FH4-)
endif()


# Force Unicode character set
add_definitions(-DUNICODE -D_UNICODE)


# Tests
if(BUILD_UNIT_TESTS)
    SET(MAX_IMAGE_MEMORY "104857600" CACHE STRING "Max Image Memory") # 100 MB limit

    find_package(GTest REQUIRED)
    enable_testing()
    add_subdirectory(${tests_root}/CommonLib)
    add_subdirectory(${tests_root}/StreamingImageSequence)
endif()


#Specify available modules
add_subdirectory(${src_root}/CommonLib)
add_subdirectory(${src_root}/StreamingImageSequence)

