cmake_minimum_required(VERSION 3.20)
project(VisualizerNative)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Source files
set(SOURCES
    VisualizerNative.cpp
    VisualizerNative.h
)

# Create DLL
add_library(VisualizerNative SHARED ${SOURCES})

# Define UNICODE for wide character support
target_compile_definitions(VisualizerNative PRIVATE
    UNICODE
    _UNICODE
)

# Link DirectX libraries (they're in Windows SDK)
target_link_libraries(VisualizerNative PRIVATE
    d3d11.lib
    dxgi.lib
    d3dcompiler.lib
)

# Set output directory
set(OUTPUT_DIR "${CMAKE_SOURCE_DIR}/../ETMPClient/bin/Debug/net9.0-windows")
set_target_properties(VisualizerNative PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIR}"
    RUNTIME_OUTPUT_DIRECTORY_DEBUG "${OUTPUT_DIR}"
    RUNTIME_OUTPUT_DIRECTORY_RELEASE "${OUTPUT_DIR}"
)
