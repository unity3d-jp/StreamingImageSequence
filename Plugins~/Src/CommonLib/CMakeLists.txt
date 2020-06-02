CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

project(CommonLib)

file(GLOB sources *.cpp *.h)
set(CommonLib_dir "${CMAKE_CURRENT_SOURCE_DIR}")
if(WIN32)
    file(GLOB win_sources Win/*.h Win/*.cpp)
    list(APPEND sources ${win_sources})    
endif()
set(src_root   "${CMAKE_CURRENT_SOURCE_DIR}/..")

# setup as a library
add_library(CommonLib SHARED ${sources} )

#Include, pch
target_precompile_headers(CommonLib PRIVATE "${CommonLib_dir}/stdafx.h")
target_include_directories(CommonLib PRIVATE
    ${CommonLib_dir}    
    ${src_root}
)

