cmake_minimum_required(VERSION 3.15)

file(GLOB sources *.cpp *.h )
set(SRC_ROOT   "${CMAKE_CURRENT_SOURCE_DIR}/../../Src")

add_executable(CommonLibTest  ${sources})
target_include_directories(CommonLibTest PRIVATE 
    ${SRC_ROOT}
    ${GTEST_INCLUDE_DIRS}    
)
               
target_link_libraries(CommonLibTest Loader 
    ${GTEST_BOTH_LIBRARIES}
    ${COCOA_LIBRARY}
)  
  

add_test(NAME CommonLibTest COMMAND CommonLibTest)


