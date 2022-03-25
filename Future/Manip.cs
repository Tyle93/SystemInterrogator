using System.Runtime.InteropServices;

namespace Future{
    public static class Manip{
        [DllImport("user32.dll", CharSet=CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle); 
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        
    }
}