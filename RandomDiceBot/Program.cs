using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;

namespace RandomDiceBot
{
    class Program
    {
        // Reference images
        public static Bitmap referencePvpMenu;
        public static Bitmap referenceCoopWatchAd;
        public static Bitmap referenceCoopQuickMatch;
        public static Bitmap referenceInGame;
        public static Bitmap referenceMatchEnd;
        public static Bitmap referenceCoopQuickMatch2;
        public static Bitmap referenceBlank;

        // Tolerance level for merge
        public static int toleranceLevelMerge = 60000;
        public static int toleranceLevelBlank = 60000;

        // Position of app tab
        public static int appTabX = -1500;
        public static int appTabY = 384;

        // Ad positions
        public static int adNormalScreenLeft = 567;
        public static int adNormalScreenTop = 15;

        public static int adWideScreenLeft = -125;
        public static int adWideScreenTop = 483;

        public static int GetScreenLeft()
        {
            return SystemInformation.VirtualScreen.Left + 669;
        }

        public static int GetScreenTop()
        {
            return SystemInformation.VirtualScreen.Top + 42 + 318 + 42;
        }

        static void Main(string[] args)
        {
            // Ensure screens directory exists
            Directory.CreateDirectory("screens");

            // Load in reference images
            referencePvpMenu = new Bitmap("reference/pvpMenu.png");
            referenceCoopWatchAd = new Bitmap("reference/coopWatchAd.png");
            referenceCoopQuickMatch = new Bitmap("reference/coopQuickMatch.png");
            referenceInGame = new Bitmap("reference/ingame.png");
            referenceMatchEnd = new Bitmap("reference/endgame.png");
            referenceCoopQuickMatch2 = new Bitmap("reference/coopQuickMatch2.png");

            referenceBlank = new Bitmap("reference/cell_empty.png");

            while (true)
            {
                // Do some work
                DoWork();

                // Wait for 0.1 seconds
                Wait(0.1);
            }
        }

        public static void DoWork()
        {
            if(Control.IsKeyLocked(Keys.CapsLock))
            {
                MouseControl.POINT mousePos = MouseControl.GetCursorPos();

                Console.WriteLine("Capslock is on, I am not going to do anything, turn capslock off! " + mousePos.X + ", " + mousePos.Y);
                return;
            }

            // Get the screen
            Bitmap fullScreen = GetScreen();

            fullScreen.Save("screens/current.png", System.Drawing.Imaging.ImageFormat.Png);

            // Where are we?

            // Are we in the main menu?
            if (GetDifference(fullScreen, referencePvpMenu, 135, 783))
            {
                Console.WriteLine("Detected the main menu");

                // Do we need to watch an ad?
                if (GetDifference(fullScreen, referenceCoopWatchAd, 440, 783))
                {
                    Console.WriteLine("There is an ad to watch :/ Let's watch it...");

                    // Click ad
                    MouseControl.ClickPos(495, 840);

                    // We good, sleep for 31 seconds
                    Wait(31);

                    // Click on the close ad button ??? profit
                    MouseControl.ClickPos(542, 40);

                    // Wait a second for it to process
                    Wait(1);
                }

                Console.WriteLine("Attempting to start coop mode...");

                // No ad required, let's click play coop
                MouseControl.ClickPos(400, 830);

                // Give it 1 second to find a match
                Wait(1);

                // Stop
                return;
            }

            // Are we in the quick match menu?
            if (GetDifference(fullScreen, referenceCoopQuickMatch, 290, 548) || GetDifference(fullScreen, referenceCoopQuickMatch2, 316, 583))
            {
                Console.WriteLine("Detected quick play menu, clicking play...");

                MouseControl.ClickPos(400, 610);

                // Give it time to process
                Wait(1);

                return;
            }

            // Are we in game?
            if (GetDifference(fullScreen, referenceInGame, 348, 833))
            {
                Console.WriteLine("We are ingame, let's play");

                // Click each upgrade
                MouseControl.ClickPos(107, 961);
                Wait(0.1);

                MouseControl.ClickPos(198, 961);
                Wait(0.1);

                MouseControl.ClickPos(293, 961);
                Wait(0.1);

                MouseControl.ClickPos(388, 961);
                Wait(0.1);

                MouseControl.ClickPos(480, 961);
                Wait(0.1);

                // Click buy dice
                MouseControl.ClickPos(294, 860);
                Wait(0.1);

                // Merge dection logic

                int boardLeft = 125;
                int boardTop = 522;

                int totalBlank = 0;

                List<SquareInfo> seenImages = new List<SquareInfo>();

                int theOffsetMerge = 20;

                for (int x = 0; x < 5; ++x)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        int startPosX = boardLeft + x * 67;
                        int startPosY = boardTop + y * 65;

                        Bitmap toCheck = CropImage(fullScreen, startPosX, startPosY, 62, 62);

                        int theValue = GetDifferenceValue(toCheck, referenceBlank);

                        if (theValue < toleranceLevelBlank)
                        {
                            // This is blank
                            totalBlank += 1;
                        }
                        else
                        {
                            SquareInfo thisInfo = new SquareInfo();
                            thisInfo.x = x;
                            thisInfo.y = y;
                            thisInfo.screenX = startPosX;
                            thisInfo.screenY = startPosY;
                            thisInfo.image = toCheck;

                            seenImages.Add(thisInfo);
                        }

                        toCheck.Save("screens/" + x + "," + y + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                Console.WriteLine("Detected " + totalBlank + " blank tiles");

                if (totalBlank == 0)
                {
                    Console.WriteLine("Looking for stuff to merge...");

                    int lowestMatch = int.MaxValue;
                    SquareInfo a = null;
                    SquareInfo b = null;

                    for (int i = 0; i < seenImages.Count; ++i)
                    {
                        SquareInfo firstImage = seenImages[i];

                        for (int j = i + 1; j < seenImages.Count; ++j)
                        {
                            SquareInfo squareInfo = seenImages[j];

                            int thisCompareValue = GetDifferenceValue(firstImage.image, squareInfo.image);

                            if (thisCompareValue < toleranceLevelMerge)
                            {
                                Console.WriteLine("Found a merge! " + squareInfo.x + "," + squareInfo.y + " -> " + firstImage.x + "," + firstImage.y);
                                MouseControl.DoMerge(squareInfo.screenX + theOffsetMerge, squareInfo.screenY + theOffsetMerge, firstImage.screenX + theOffsetMerge, firstImage.screenY + theOffsetMerge);
                            }

                            if(thisCompareValue < lowestMatch)
                            {
                                lowestMatch = thisCompareValue;
                                a = firstImage;
                                b = squareInfo;
                            }
                        }
                    }

                    if(a != null && b != null)
                    {
                        Console.WriteLine("Doing guessed merge! " + a.x + "," + a.y + " -> " + b.x + "," + b.y);
                        MouseControl.DoMerge(a.screenX + theOffsetMerge, a.screenY + theOffsetMerge, b.screenX + theOffsetMerge, b.screenY + theOffsetMerge);
                    }
                }

                return;
            }

            // Are we at match end?
            if (GetDifference(fullScreen, referenceMatchEnd, 225, 819))
            {
                Console.WriteLine("We are at the match end");

                // Click
                MouseControl.ClickPos(297, 863);

                // Give it a second to do
                Wait(1);

                return;
            }

            Console.WriteLine("No idea where we are");

            // Regular screen ad
            Console.WriteLine("Lets try guess the position of the ad???");
            MouseControl.ClickPos(adNormalScreenLeft, adNormalScreenTop);
            Wait(1);

            Console.WriteLine("Clicking back into app...");
            MouseControl.ClickPos(appTabX, appTabY, true);
            Wait(1);

            // Wide screen ad
            Console.WriteLine("Let's guess a wide screen ad?");
            MouseControl.ClickPos(adWideScreenLeft, adWideScreenTop, true);
            Wait(1);

            Console.WriteLine("Clicking back into app...");
            MouseControl.ClickPos(appTabX, appTabY, true);
            Wait(1);

            //CropImage(fullScreen, 290, 548, 205, 111).Save("reference/coopQuickMatch.png", System.Drawing.Imaging.ImageFormat.Png);



            //Console.WriteLine("diff = " + theDifference);
            //Console.ReadLine();

            // PVP
            //CropImage(fullScreen, 100, 775, 160, 120).Save("reference/pvpMenu.png", System.Drawing.Imaging.ImageFormat.Png);

            // Coop Watch Ad
            //CropImage(fullScreen, 440, 783, 93, 93).Save("reference/coopWatchAd.png", System.Drawing.Imaging.ImageFormat.Png);

            // 

            // 101, 775 offset
            // 157, 118 size

        }

        public static void Wait(double seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        public static bool GetDifference(Bitmap fullScreen, Bitmap comparision, int fullscreenX = 0, int fullscreenY = 0, int threashold = 0)
        {
            int width = comparision.Width;
            int height = comparision.Height;

            for (int xx = 0; xx < width; ++xx)
            {
                for (int yy = 0; yy < height; ++yy)
                {
                    Color a = fullScreen.GetPixel(fullscreenX + xx, fullscreenY + yy);
                    Color b = comparision.GetPixel(xx, yy);

                    threashold -= Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
                    if (threashold < 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static int GetDifferenceValue(Bitmap fullScreen, Bitmap comparision, int fullscreenX = 0, int fullscreenY = 0)
        {
            int width = comparision.Width;
            int height = comparision.Height;

            int currentCount = 0;

            for (int xx = 0; xx < width; ++xx)
            {
                for (int yy = 0; yy < height; ++yy)
                {
                    Color a = fullScreen.GetPixel(fullscreenX + xx, fullscreenY + yy);
                    Color b = comparision.GetPixel(xx, yy);

                    currentCount += Math.Abs(a.R - b.R) + Math.Abs(a.G - b.G) + Math.Abs(a.B - b.B);
                }
            }

            return currentCount;
        }

        public static Bitmap GetScreen()
        {
            int screenLeft = GetScreenLeft();
            int screenTop = GetScreenTop();
            int screenWidth = 581;
            int screenHeight = 1034;

            Bitmap bitmap = new Bitmap(screenWidth, screenHeight);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(screenLeft, screenTop, 0, 0, new Rectangle(0, 0, 1920, 1080).Size);
            }

            return bitmap;
        }

        public static Bitmap CropImage(Bitmap source, int x, int y, int width, int height)
        {
            Bitmap newBitmap = new Bitmap(width, height);

            for(int xx=0; xx<width; ++xx)
            {
                for(int yy=0; yy<height; ++yy)
                {
                    newBitmap.SetPixel(xx, yy, source.GetPixel(x + xx, y + yy));
                }
            }

            return newBitmap;
        }
    }
}
