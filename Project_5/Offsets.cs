using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_5
{
    static class Offsets
    {
        public static IntPtr clientConnectionOffset = new IntPtr(0xC79CE0);                  
        public static IntPtr objectManagerOffset = new IntPtr(0x2ED0);                        
        public static IntPtr firstObjectOffset = new IntPtr(0xAC);                      
        public static IntPtr nextObjectOffset = new IntPtr(0x3C);
        public static IntPtr mouseoverLoc = new IntPtr(0x00BD07A0);
        //public static IntPtr PlayerName = new IntPtr(0xC79D18);

    }
}
