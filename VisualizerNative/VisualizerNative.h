#pragma once

#include <Windows.h>

#ifdef VISUALIZERNATIVE_EXPORTS
#define VISUALIZER_API __declspec(dllexport)
#else
#define VISUALIZER_API __declspec(dllimport)
#endif

extern "C" {
// Create visualizer window and initialize DirectX
VISUALIZER_API HWND CreateVisualizerWindow(HWND parentHwnd, int width,
                                           int height);

// Update bar values (32 floats, 0.0-1.0 range)
VISUALIZER_API void UpdateBars(const float *barValues, int count);

// Render frame
VISUALIZER_API void Render();

// Cleanup
VISUALIZER_API void DestroyVisualizer();

// Resize
VISUALIZER_API void ResizeVisualizer(int width, int height);
}
