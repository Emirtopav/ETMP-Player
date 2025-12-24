using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ETMPClient.Controls
{
    public class DirectXVisualizerHost : HwndHost
    {
        private IntPtr _hwndHost;
        private const string DllName = "VisualizerNative.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateVisualizerWindow(IntPtr parentHwnd, int width, int height);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void UpdateBars([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] float[] barValues, int count);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Render();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyVisualizer();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ResizeVisualizer(int width, int height);

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _hwndHost = CreateVisualizerWindow(
                hwndParent.Handle,
                (int)Width,
                (int)Height
            );

            return new HandleRef(this, _hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyVisualizer();
        }

        public void UpdateVisualizerBars(float[] barValues)
        {
            if (barValues != null && barValues.Length > 0)
            {
                UpdateBars(barValues, Math.Min(barValues.Length, 32));
            }
        }

        public void RenderFrame()
        {
            Render();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (_hwndHost != IntPtr.Zero)
            {
                ResizeVisualizer((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height);
            }
        }
    }
}
