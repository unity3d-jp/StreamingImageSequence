CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

project(DrawOverWindow)

file(GLOB sources *.cpp *.h)
set(DrawOverWindow_dir "${CMAKE_CURRENT_SOURCE_DIR}")
if(WIN32)
    file(GLOB win_sources Win/*.h Win/*.cpp)
    list(APPEND sources ${win_sources})    
endif()
set(plugins_dir "${src_root}/../../Runtime/Plugins/${PLATFORM}")

# setup as a library
add_plugin(DrawOverWindow SOURCES ${sources} PLUGINS_DIR ${plugins_dir})

#Include, pch
target_precompile_headers(DrawOverWindow PRIVATE "${DrawOverWindow_dir}/stdafx.h")
target_include_directories(DrawOverWindow PRIVATE
    ${DrawOverWindow_dir}    
    ${src_root}
)

# Library dependencies
target_link_libraries( DrawOverWindow 
    CommonLib
)

