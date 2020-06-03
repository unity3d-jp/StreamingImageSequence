cmake_minimum_required(VERSION 3.15)

set(SRC_ROOT   "${CMAKE_CURRENT_SOURCE_DIR}/../../Src")

add_executable(LoaderTest main.cpp)
target_include_directories(LoaderTest PRIVATE 
    ${SRC_ROOT}
    ${GTEST_INCLUDE_DIRS}    
)
               
target_link_libraries(LoaderTest Loader ${GTEST_BOTH_LIBRARIES})  
  

add_test(NAME LoaderTest COMMAND LoaderTest)

