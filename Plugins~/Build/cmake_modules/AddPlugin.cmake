# Expects BUILD_UNIT_TESTS variable to be defined

option(ENABLE_DEPLOY "Copy built binaries to plugins directory." ON)


function(add_plugin name)

    cmake_parse_arguments(arg "BUILD_OSX_BUNDLE" "PLUGINS_DIR" "SOURCES" ${ARGN})
    file(TO_NATIVE_PATH ${arg_PLUGINS_DIR} native_plugins_dir)
    
    if (BUILD_UNIT_TESTS)
        add_library(${name} STATIC ${arg_SOURCES})
        set_target_properties(${name} PROPERTIES PREFIX "")    
    elseif(APPLE AND ${arg_BUILD_OSX_BUNDLE})
        add_library(${name} MODULE ${arg_SOURCES})
        set_target_properties(${name} PROPERTIES BUNDLE ON)
    else()
        add_library(${name} SHARED ${arg_SOURCES})
        set_property(TARGET ${name} PROPERTY PREFIX "")

        # fPIC required to build shared libraries on Linux
        if (LINUX)
            set_property(TARGET ${name} PROPERTY POSITION_INDEPENDENT_CODE ON)
        endif()
    endif()

    # Don't deploy if we are building unit tests
    if(ENABLE_DEPLOY AND NOT BUILD_UNIT_TESTS)     
        if(WIN32) 
 
            # Win: Visual Studio Settings
            add_custom_command(TARGET ${name} POST_BUILD
                COMMAND del ${native_plugins_dir}\$(TargetFileName)
                COMMAND copy $(TargetPath) ${native_plugins_dir}               
                    
            )
        else()
            
            if(APPLE AND ${arg_BUILD_OSX_BUNDLE})
                SET(target_filename \${TARGET_BUILD_DIR}/${name}.bundle)
            else()
                SET(target_filename $<TARGET_FILE:${name}>)
            endif()
        
            # Linux or Mac
            add_custom_command(TARGET ${name} POST_BUILD
                COMMAND rm -rf ${arg_PLUGINS_DIR}/${target_filename}
                COMMAND cp -r ${target_filename} ${native_plugins_dir}               
            )
        endif()

    endif()
endfunction()
