CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

project(stb)

file(GLOB sources *.cpp *.h)
set(stb_dir "${CMAKE_CURRENT_SOURCE_DIR}")

# setup as a library
add_library(stb STATIC ${sources} )

#Include, pch
target_include_directories(stb PRIVATE
    ${stb_dir}    
)

