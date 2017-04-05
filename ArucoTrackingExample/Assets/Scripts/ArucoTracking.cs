using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class ArucoTracking {

    public static float marker_size;
    public static int size_reduce;

    public static int marker_count;
    public static int[] ids;
    public static float[] corners;
    public static double[] rvecs;
    public static double[] tvecs;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void PrintDelegate(string str);

    [DllImport("aruco_plugin", EntryPoint = "init")]
    private static extern void lib_init(int width, int height, float marker_size, IntPtr camera_params, int size_reduce);

    [DllImport("aruco_plugin", EntryPoint = "detect_markers")]
    private static extern int lib_detect_markers(IntPtr unity_img, ref int marker_count, ref IntPtr out_ids, ref IntPtr out_corners, ref IntPtr out_rvecs, ref IntPtr out_tvecs);

    [DllImport("aruco_plugin", EntryPoint = "set_debug_cb")]
    private static extern void lib_set_debug_cb(IntPtr ptr);

    [DllImport("aruco_plugin", EntryPoint = "destroy")]
    private static extern void lib_destroy();

    public static bool lib_inited = false;

    public static void init(int _width, int _height, float _marker_size, float[] _camera_params, int _size_reduce) {
        GCHandle params_handle = GCHandle.Alloc(_camera_params, GCHandleType.Pinned);
        lib_init(_width, _height, _marker_size, params_handle.AddrOfPinnedObject(), _size_reduce);
        params_handle.Free();

        set_debug_delegate(new PrintDelegate(plugin_debug_log));
        lib_inited = true;
    }

    public static void destroy() {
        if(lib_inited) {
            lib_destroy();
        }
    }
        //This call may trigger a stack overflow exception from within the dll if the application is close to the memory limit of the HoloLens. 
        //This isn't really a stack overflow, but instead an exception thrown from with OpenCV when failing to allocate memory.
        //This can happen if the unity application allocates large amounts of memory over time, in which case the GC may wait until the heap reaches the maximum memory available to collect and free memory.
        //In that case, this unmanaged call may run before space is freed up by the GC, so internal allocations done by OpenCV hit the memory limit, throwing the exception.
        //The easiest way to prevent this is to never hit the memory limit, or never allocate so fast that the GC will let the heap approach maximum size.
        //However, if necessary it is possible to use GC.AddMemoryPressure to make it aware of an approximate amount of memory used by the dll (probably 1 - 2 times the image size in bytes, as it creates some copies of it for color conversion)
        //I tried this, but it seems to just cause the GC to collect every frame, which is terrible for performance. Because of that it's not implemented here.
        //:todo: Look into a way to detect that the application is close to the maximum memory allowed (through a C# call or possibly a hardcoded value adjusted for the HoloLens (on emulator: 964 MB +- 4 MB maybe)) and force a GC.collect call if we're close to it.
    public static void detect_markers(Color32[] _image) {
        GCHandle img_handle = GCHandle.Alloc(_image, GCHandleType.Pinned);

        marker_count = 0;
        IntPtr out_ids = IntPtr.Zero;
        IntPtr out_corners = IntPtr.Zero;
        IntPtr out_rvecs = IntPtr.Zero;
        IntPtr out_tvecs = IntPtr.Zero;

        lib_detect_markers(img_handle.AddrOfPinnedObject(), ref marker_count, ref out_ids, ref out_corners, ref out_rvecs, ref out_tvecs);
        img_handle.Free();

        if (marker_count > 0) {
            //Copy over data from plugin side to c# managed arrays
            ids = new int[marker_count];
            Marshal.Copy(out_ids, ids, 0, marker_count);

            corners = new float[marker_count * 8];
            Marshal.Copy(out_corners, corners, 0, marker_count * 8);

            rvecs = new double[marker_count * 3];
            Marshal.Copy(out_rvecs, rvecs, 0, marker_count * 3);

            tvecs = new double[marker_count * 3];
            Marshal.Copy(out_tvecs, tvecs, 0, marker_count * 3);
        }
        else {
            ids = null;
            corners = null;
            rvecs = null;
            tvecs = null;
        }
    }

    public static void set_debug_delegate(PrintDelegate _callback) {
        IntPtr delegate_ptr = Marshal.GetFunctionPointerForDelegate(_callback);
        lib_set_debug_cb(delegate_ptr);
    }

    private static void plugin_debug_log(string _msg) {
        Debug.Log("Aruco plugin: " + _msg);
    }
}
