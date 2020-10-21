cmake_minimum_required(VERSION 3.15)

file(GLOB_RECURSE sources *.cpp *.h )
set(SRC_ROOT   "${CMAKE_CURRENT_SOURCE_DIR}/../../Src")

add_executable(StreamingImageSequenceTest  ${sources})
source_group(TREE ${CMAKE_CURRENT_SOURCE_DIR} FILES ${sources})

target_include_directories(StreamingImageSequenceTest PRIVATE 
    "${SRC_ROOT}/CommonLib/Include"
    "${SRC_ROOT}/StreamingImageSequence/Include"
    "${SRC_ROOT}"
    "${GTEST_INCLUDE_DIRS}"    
)
  
list(APPEND libs 
    CommonLib
    ${GTEST_BOTH_LIBRARIES}
    ${COCOA_LIBRARY}
)

if(LINUX)
    list(APPEND libs pthread)
endif()

             
target_link_libraries(StreamingImageSequenceTest ${libs})  

add_test(NAME StreamingImageSequenceTest COMMAND StreamingImageSequenceTest)


# Copy test data
set(num_test_images 100)
target_compile_definitions(StreamingImageSequenceTest PRIVATE NUM_TEST_IMAGES=${num_test_images})
foreach(loop_var RANGE ${num_test_images})     
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/../Data/PNGTestImage.png
        ${CMAKE_CURRENT_BINARY_DIR}/PNGTestImage_${loop_var}.png COPYONLY
    )    
    
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/../Data/TGATestImage.tga
        ${CMAKE_CURRENT_BINARY_DIR}/TGATestImage_${loop_var}.tga COPYONLY
    )    
endforeach(loop_var) 
configure_file(
    ${CMAKE_CURRENT_SOURCE_DIR}/../Data/InvalidTestImage.png
    ${CMAKE_CURRENT_BINARY_DIR}/InvalidTestImage.png COPYONLY
)    
configure_file(
    ${CMAKE_CURRENT_SOURCE_DIR}/../Data/InvalidTestImage.png
    ${CMAKE_CURRENT_BINARY_DIR}/InvalidTestImage.tga COPYONLY
)    

if(APPLE)
    # When run in XCode, the working directory is not in the root, but instead under the configuration folder
    # In the future, we should probably configure XCODE_SCHEME_WORKING_DIRECTORY, but I think it is still too new to be used now (CMake 3.17.3).
    # https://cmake.org/cmake/help/v3.17/prop_tgt/XCODE_SCHEME_WORKING_DIRECTORY.html#prop_tgt:XCODE_SCHEME_WORKING_DIRECTORY
    foreach(loop_var RANGE ${num_test_images})     
        add_custom_command(TARGET StreamingImageSequenceTest POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/../Data/PNGTestImage.png ${CMAKE_CURRENT_BINARY_DIR}/$<CONFIGURATION>/PNGTestImage_${loop_var}.png
            COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/../Data/TGATestImage.tga ${CMAKE_CURRENT_BINARY_DIR}/$<CONFIGURATION>/TGATestImage_${loop_var}.tga
        )
    endforeach(loop_var) 
    add_custom_command(TARGET StreamingImageSequenceTest POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/../Data/InvalidTestImage.png ${CMAKE_CURRENT_BINARY_DIR}/$<CONFIGURATION>/InvalidTestImage.png
        COMMAND ${CMAKE_COMMAND} -E copy ${CMAKE_CURRENT_SOURCE_DIR}/../Data/InvalidTestImage.png ${CMAKE_CURRENT_BINARY_DIR}/$<CONFIGURATION>/InvalidTestImage.tga
    )
    
    
endif()    

if(DEFINED MAX_IMAGE_MEMORY)
    target_compile_definitions(StreamingImageSequenceTest PRIVATE MAX_IMAGE_MEMORY=${MAX_IMAGE_MEMORY}) 
endif()


