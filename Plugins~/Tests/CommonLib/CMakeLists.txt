cmake_minimum_required(VERSION 3.15)

file(GLOB_RECURSE sources *.cpp *.h )
set(SRC_ROOT   "${CMAKE_CURRENT_SOURCE_DIR}/../../Src")

add_executable(CommonLibTest  ${sources})
source_group(TREE ${CMAKE_CURRENT_SOURCE_DIR} FILES ${sources})

target_include_directories(CommonLibTest PRIVATE 
    "${SRC_ROOT}/CommonLib/Include"
    ${GTEST_INCLUDE_DIRS}    
)
               
list(APPEND libs 
    CommonLib
    ${GTEST_BOTH_LIBRARIES}
    ${COCOA_LIBRARY}
)

if(LINUX)
    list(APPEND libs pthread)
endif()

target_link_libraries(CommonLibTest ${libs})
  

add_test(NAME CommonLibTest COMMAND CommonLibTest)


