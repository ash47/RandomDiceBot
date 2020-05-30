using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RandomDiceBot
{
    class MouseControl
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        public static void MoveCursorToPoint(int x, int y, bool isAbsolute = false)
        {
            if(!isAbsolute)
            {
                x += Program.GetScreenLeft();
                y += Program.GetScreenTop();
            }

            SetCursorPos(x, y);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static void DoMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void ClickPos(int x, int y, bool isAbsolute=false)
        {
            MoveCursorToPoint(x, y, isAbsolute);
            DoMouseClick();
        }

        public static void DoMerge(int startX, int startY, int finishX, int finishY)
        {
            MoveCursorToPoint(startX, startY);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Program.Wait(0.1);

            MoveCursorToPoint(finishX, finishY);
            Program.Wait(0.1);

            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            Program.Wait(0.1);
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        public struct POINT
        {
            public int X;
            public int Y;
        }

        public static POINT GetCursorPos()
        {
            POINT point;
            GetCursorPos(out point);

            return point;
        }
    }
}
