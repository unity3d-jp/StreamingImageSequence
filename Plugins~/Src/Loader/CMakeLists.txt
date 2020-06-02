CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

project(Loader)

file(GLOB sources *.cpp *.h ../)
set(Loader_dir "${CMAKE_CURRENT_SOURCE_DIR}")
if(APPLE)
    file(GLOB mac_sources Mac/*.h Mac/*.cpp Mac/*.mm)
    list(APPEND sources ${mac_sources})    

elseif(WIN32)
    file(GLOB win_sources Win/*.h Win/*.cpp)
    list(APPEND sources ${win_sources})    
endif()
set(src_root   "${CMAKE_CURRENT_SOURCE_DIR}/..")
set(plugins_dir "${src_root}/../../Runtime/Plugins/x64")

# setup as a library
add_plugin(Loader SOURCES ${sources} PLUGINS_DIR ${plugins_dir})

#Include, pch
target_precompile_headers(Loader PRIVATE "${Loader_dir}/stdafx.h")
target_include_directories(Loader PRIVATE
    ${Loader_dir}    
    ${src_root}
)

# Library dependencies
target_link_libraries( Loader 
    CommonLib
    stb
)
