CMAKE_MINIMUM_REQUIRED(VERSION 3.13)

add_executable(LoaderTest main.cpp)
               
target_link_libraries(LoaderTest Loader) 

add_test(NAME LoaderTest COMMAND LoaderTest)

