#define VISUALIZERNATIVE_EXPORTS
#include "VisualizerNative.h"
#include <DirectXMath.h>
#include <d3d11.h>
#include <d3dcompiler.h>
#include <vector>
#include <wrl/client.h>


#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "d3dcompiler.lib")

using namespace Microsoft::WRL;
using namespace DirectX;

// DirectX resources
static ComPtr<ID3D11Device> g_device;
static ComPtr<ID3D11DeviceContext> g_context;
static ComPtr<IDXGISwapChain> g_swapChain;
static ComPtr<ID3D11RenderTargetView> g_renderTargetView;
static ComPtr<ID3D11Buffer> g_vertexBuffer;
static ComPtr<ID3D11VertexShader> g_vertexShader;
static ComPtr<ID3D11PixelShader> g_pixelShader;
static ComPtr<ID3D11InputLayout> g_inputLayout;

static HWND g_hwnd = nullptr;
static int g_width = 200;
static int g_height = 50;
static float g_barValues[32] = {0.1f}; // Initialize to 10%

struct Vertex {
  XMFLOAT3 position;
  XMFLOAT4 color;
};

// Vertex Shader (simple passthrough)
const char *g_vertexShaderCode = R"(
struct VS_INPUT {
    float3 pos : POSITION;
    float4 color : COLOR;
};
struct VS_OUTPUT {
    float4 pos : SV_POSITION;
    float4 color : COLOR;
};
VS_OUTPUT main(VS_INPUT input) {
    VS_OUTPUT output;
    output.pos = float4(input.pos, 1.0f);
    output.color = input.color;
    return output;
}
)";

// Pixel Shader (simple color output)
const char *g_pixelShaderCode = R"(
struct PS_INPUT {
    float4 pos : SV_POSITION;
    float4 color : COLOR;
};
float4 main(PS_INPUT input) : SV_TARGET {
    return input.color;
}
)";

bool InitializeDirectX() {
  DXGI_SWAP_CHAIN_DESC swapChainDesc = {};
  swapChainDesc.BufferCount = 2;
  swapChainDesc.BufferDesc.Width = g_width;
  swapChainDesc.BufferDesc.Height = g_height;
  swapChainDesc.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
  swapChainDesc.BufferDesc.RefreshRate.Numerator = 60;
  swapChainDesc.BufferDesc.RefreshRate.Denominator = 1;
  swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
  swapChainDesc.OutputWindow = g_hwnd;
  swapChainDesc.SampleDesc.Count = 1;
  swapChainDesc.Windowed = TRUE;
  swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;

  D3D_FEATURE_LEVEL featureLevel;
  HRESULT hr = D3D11CreateDeviceAndSwapChain(
      nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, 0, nullptr, 0,
      D3D11_SDK_VERSION, &swapChainDesc, &g_swapChain, &g_device, &featureLevel,
      &g_context);

  if (FAILED(hr))
    return false;

  // Create render target view
  ComPtr<ID3D11Texture2D> backBuffer;
  g_swapChain->GetBuffer(0, IID_PPV_ARGS(&backBuffer));
  g_device->CreateRenderTargetView(backBuffer.Get(), nullptr,
                                   &g_renderTargetView);

  // Compile shaders
  ComPtr<ID3DBlob> vsBlob, psBlob, errorBlob;

  hr =
      D3DCompile(g_vertexShaderCode, strlen(g_vertexShaderCode), nullptr,
                 nullptr, nullptr, "main", "vs_5_0", 0, 0, &vsBlob, &errorBlob);
  if (FAILED(hr))
    return false;

  hr =
      D3DCompile(g_pixelShaderCode, strlen(g_pixelShaderCode), nullptr, nullptr,
                 nullptr, "main", "ps_5_0", 0, 0, &psBlob, &errorBlob);
  if (FAILED(hr))
    return false;

  g_device->CreateVertexShader(vsBlob->GetBufferPointer(),
                               vsBlob->GetBufferSize(), nullptr,
                               &g_vertexShader);
  g_device->CreatePixelShader(psBlob->GetBufferPointer(),
                              psBlob->GetBufferSize(), nullptr, &g_pixelShader);

  // Create input layout
  D3D11_INPUT_ELEMENT_DESC layout[] = {
      {"POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0,
       D3D11_INPUT_PER_VERTEX_DATA, 0},
      {"COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, 12,
       D3D11_INPUT_PER_VERTEX_DATA, 0}};
  g_device->CreateInputLayout(layout, 2, vsBlob->GetBufferPointer(),
                              vsBlob->GetBufferSize(), &g_inputLayout);

  // Create vertex buffer (will be updated each frame)
  D3D11_BUFFER_DESC bufferDesc = {};
  bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
  bufferDesc.ByteWidth =
      sizeof(Vertex) * 32 * 6; // 32 bars, 6 vertices each (2 triangles)
  bufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;
  bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
  g_device->CreateBuffer(&bufferDesc, nullptr, &g_vertexBuffer);

  return true;
}

LRESULT CALLBACK VisualizerWndProc(HWND hwnd, UINT msg, WPARAM wParam,
                                   LPARAM lParam) {
  switch (msg) {
  case WM_SIZE:
    if (g_swapChain) {
      g_context->OMSetRenderTargets(0, nullptr, nullptr);
      g_renderTargetView.Reset();

      RECT rect;
      GetClientRect(hwnd, &rect);
      g_width = rect.right - rect.left;
      g_height = rect.bottom - rect.top;

      g_swapChain->ResizeBuffers(0, g_width, g_height, DXGI_FORMAT_UNKNOWN, 0);

      ComPtr<ID3D11Texture2D> backBuffer;
      g_swapChain->GetBuffer(0, IID_PPV_ARGS(&backBuffer));
      g_device->CreateRenderTargetView(backBuffer.Get(), nullptr,
                                       &g_renderTargetView);
    }
    return 0;
  }
  return DefWindowProc(hwnd, msg, wParam, lParam);
}

extern "C" {

HWND CreateVisualizerWindow(HWND parentHwnd, int width, int height) {
  g_width = width;
  g_height = height;

  WNDCLASSEX wc = {};
  wc.cbSize = sizeof(WNDCLASSEX);
  wc.lpfnWndProc = VisualizerWndProc;
  wc.hInstance = GetModuleHandle(nullptr);
  wc.lpszClassName = L"VisualizerNativeClass";
  RegisterClassEx(&wc);

  g_hwnd = CreateWindowEx(
      0, L"VisualizerNativeClass", L"Visualizer", WS_CHILD | WS_VISIBLE, 0, 0,
      width, height, parentHwnd, nullptr, GetModuleHandle(nullptr), nullptr);

  if (!g_hwnd)
    return nullptr;

  if (!InitializeDirectX()) {
    DestroyWindow(g_hwnd);
    return nullptr;
  }

  return g_hwnd;
}

void UpdateBars(const float *barValues, int count) {
  if (count > 32)
    count = 32;
  for (int i = 0; i < count; i++) {
    g_barValues[i] = barValues[i];
  }
}

void Render() {
  if (!g_context || !g_renderTargetView)
    return;

  // Clear to dark background
  float clearColor[4] = {0.0f, 0.0f, 0.0f, 0.0f}; // Transparent
  g_context->ClearRenderTargetView(g_renderTargetView.Get(), clearColor);

  // Update vertex buffer with bar data
  D3D11_MAPPED_SUBRESOURCE mappedResource;
  g_context->Map(g_vertexBuffer.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0,
                 &mappedResource);
  Vertex *vertices = (Vertex *)mappedResource.pData;

  float barWidth = 2.0f / 32.0f; // NDC space (-1 to 1)
  float barSpacing = barWidth * 0.1f;
  float actualBarWidth = barWidth - barSpacing;

  // Accent color (cyan from theme)
  XMFLOAT4 accentColor(0.0f, 0.88f, 1.0f, 0.7f); // #00E0FF with 70% opacity

  for (int i = 0; i < 32; i++) {
    float x = -1.0f + (i * barWidth);
    float height = g_barValues[i] * 2.0f; // Scale to NDC (-1 to 1)
    float y = -1.0f;                      // Bottom of screen

    // Two triangles per bar
    vertices[i * 6 + 0] = {XMFLOAT3(x, y, 0.0f), accentColor};
    vertices[i * 6 + 1] = {XMFLOAT3(x + actualBarWidth, y, 0.0f), accentColor};
    vertices[i * 6 + 2] = {XMFLOAT3(x, y + height, 0.0f), accentColor};

    vertices[i * 6 + 3] = {XMFLOAT3(x + actualBarWidth, y, 0.0f), accentColor};
    vertices[i * 6 + 4] = {XMFLOAT3(x + actualBarWidth, y + height, 0.0f),
                           accentColor};
    vertices[i * 6 + 5] = {XMFLOAT3(x, y + height, 0.0f), accentColor};
  }

  g_context->Unmap(g_vertexBuffer.Get(), 0);

  // Set pipeline state
  g_context->IASetInputLayout(g_inputLayout.Get());
  UINT stride = sizeof(Vertex);
  UINT offset = 0;
  g_context->IASetVertexBuffers(0, 1, g_vertexBuffer.GetAddressOf(), &stride,
                                &offset);
  g_context->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

  g_context->VSSetShader(g_vertexShader.Get(), nullptr, 0);
  g_context->PSSetShader(g_pixelShader.Get(), nullptr, 0);

  D3D11_VIEWPORT viewport = {};
  viewport.Width = (float)g_width;
  viewport.Height = (float)g_height;
  viewport.MinDepth = 0.0f;
  viewport.MaxDepth = 1.0f;
  g_context->RSSetViewports(1, &viewport);

  g_context->OMSetRenderTargets(1, g_renderTargetView.GetAddressOf(), nullptr);

  // Draw
  g_context->Draw(32 * 6, 0);

  // Present
  g_swapChain->Present(1, 0); // VSync
}

void DestroyVisualizer() {
  g_vertexBuffer.Reset();
  g_pixelShader.Reset();
  g_vertexShader.Reset();
  g_inputLayout.Reset();
  g_renderTargetView.Reset();
  g_swapChain.Reset();
  g_context.Reset();
  g_device.Reset();

  if (g_hwnd) {
    DestroyWindow(g_hwnd);
    g_hwnd = nullptr;
  }
}

void ResizeVisualizer(int width, int height) {
  g_width = width;
  g_height = height;
  if (g_hwnd) {
    SetWindowPos(g_hwnd, nullptr, 0, 0, width, height,
                 SWP_NOMOVE | SWP_NOZORDER);
  }
}

} // extern "C"
