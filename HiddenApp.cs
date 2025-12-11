using System;

namespace AppHiderNet
{
    public class HiddenApp
    {
        public long Hwnd { get; set; } // Use long for JSON serialization of IntPtr
        public string Title { get; set; }
        public string Password { get; set; }
        public bool IsBlurred { get; set; }
    }
}
