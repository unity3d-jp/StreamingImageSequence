option(ENABLE_DEPLOY "Copy built binaries to plugins directory." ON)


function(add_plugin name)
    cmake_parse_arguments(arg "BUILD_OSX_BUNDLE" "PLUGINS_DIR" "SOURCES" ${ARGN})
    file(TO_NATIVE_PATH ${arg_PLUGINS_DIR} native_plugins_dir)
    
    if(${arg_BUILD_OSX_BUNDLE})
        add_library(${name} MODULE ${arg_SOURCES})
        set_target_properties(${name} PROPERTIES BUNDLE ON)
    else()
        add_library(${name} SHARED ${arg_SOURCES})
        set_target_properties(${name} PROPERTIES PREFIX "")
    endif()

    if(ENABLE_DEPLOY)
              
        if(WIN32) 
 
            # Win: Visual Studio Settings
            add_custom_command(TARGET ${name} POST_BUILD
                COMMAND del ${native_plugins_dir}\$(TargetFileName)
                COMMAND copy $(TargetPath) ${native_plugins_dir}               
                    
            )
        else()
            
            if(${arg_BUILD_OSX_BUNDLE})
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
