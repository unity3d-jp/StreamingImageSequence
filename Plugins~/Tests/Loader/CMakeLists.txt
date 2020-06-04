cmake_minimum_required(VERSION 3.15)

file(GLOB sources *.cpp *.h )
set(SRC_ROOT   "${CMAKE_CURRENT_SOURCE_DIR}/../../Src")

add_executable(LoaderTest  ${sources})
target_include_directories(LoaderTest PRIVATE 
    ${SRC_ROOT}
    ${GTEST_INCLUDE_DIRS}    
)
               
target_link_libraries(LoaderTest Loader 
    ${GTEST_BOTH_LIBRARIES}
    ${CORE_SERVICES_LIBRARY}
    ${COCOA_LIBRARY}
)  
  

add_test(NAME LoaderTest COMMAND LoaderTest)

# copy test data 
file(COPY ${CMAKE_CURRENT_SOURCE_DIR}/../Data/TestImage.png DESTINATION ${CMAKE_CURRENT_BINARY_DIR})


