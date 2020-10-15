CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

project(StreamingImageSequence)

file(GLOB_RECURSE sources *.cpp *.h ../)
set(sis_dir "${CMAKE_CURRENT_SOURCE_DIR}")
if(APPLE)
    file(GLOB_RECURSE mac_sources Mac/*.h Mac/*.cpp Mac/*.mm)
    list(APPEND sources ${mac_sources})    

elseif(WIN32)
    file(GLOB_RECURSE win_sources Win/*.h Win/*.cpp)
    list(APPEND sources ${win_sources})    
endif()
set(src_root   "${CMAKE_CURRENT_SOURCE_DIR}/..")
set(plugins_dir "${src_root}/../../Runtime/Plugins/${PLATFORM}")

# setup as a library
add_plugin(StreamingImageSequence SOURCES ${sources} PLUGINS_DIR ${plugins_dir} BUILD_OSX_BUNDLE )
source_group(TREE ${CMAKE_CURRENT_SOURCE_DIR} FILES ${sources})

#Include, pch
target_precompile_headers(StreamingImageSequence PRIVATE "${sis_dir}/stdafx.h")
target_include_directories(StreamingImageSequence PRIVATE
    "${sis_dir}/Include"    
    "${sis_dir}"
    "${src_root}/CommonLib/Include"
    "${src_root}/External"
)

# Library dependencies
target_link_libraries( StreamingImageSequence 
    CommonLib
    ${CORE_GRAPHICS_LIBRARY}
)

if(BUILD_UNIT_TESTS)
    target_compile_definitions(StreamingImageSequence PUBLIC MAX_IMAGE_MEMORY=104857600) # 100 MB limit
endif()
