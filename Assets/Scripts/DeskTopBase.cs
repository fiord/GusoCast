using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class DeskTopBase : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern int GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    private static extern bool SetLayerdWindowAttributes(int hwnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong(int hWnd, int nIndex);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong(int hWnd, int nIndex, int dwNewLong);

    const int LWA_COLORKEY = 0x1;
    const int LWA_ALPHA = 0x2;
    const int GWL_EXSTYLE = -20;
    const int WS_EX_LAYERED = 0x80000;

    const uint TRANSPARENT_COLOR = 0x00FF00;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
#else
        int handle = GetForegroundWindow();
        int extStyle = GetWindowLong(handle, GWL_EXSTYLE);
        SetWindowLong(handle, GWL_EXSTYLE, extStyle | WS_EX_LAYERED);
        SetLayerdWindowAttributes(handle, TRANSPARENT_COLOR, 0, LWA_COLORKEY);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
