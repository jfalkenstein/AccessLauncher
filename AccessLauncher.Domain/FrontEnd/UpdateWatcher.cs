using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.Domain.FrontEnd
{
    /* This class is responsible for monitoring Access after it is launched.
     * 
     * To instantiate the UpdateWatcher, a DTO.User needs to be passed in. This will be the current user object
     * obtained from the Access Back End UserInfo table. 
     * 
     * There is one public method for this class, Execute. For an explanation of this method's process,
     * see the comments on it.
     * 
     * However, here's a bird's eye view as to how this class functions:
     *      -It monitors whether Access is currently open by watching for the ".laccdb" files that Access
     *          generates whenever a file is open. It monitors these files using a FileSystemWatcher.
     *      -If the watcher detects that none of the local access files are open, it will automatically close.
     *          (You cannot close the launcher manually--it is keyed in on access.)
     *      -The watcher will ONLY watch for the Access files the launcher has downloaded and implemented from the
     *          latest rollout. You can have other Access files open without issue.
     *      -Every 2 minutes the launcher is open in this passive state, it will check whether a global lockout or
     *          new updates have been rolled out using the back end manager.
     *          -->If a lockout: The console window will flash (to get attention) and notify the user that they have five
     *              minutes to save their work and close, otherwise Access will close down. After 5 minutes, if Access is still open,
     *              the watcher will set the Front End to update on next launch(to eliminate any corruption) and then immediately kill
     *              all process instances of Access.
     *          -->If an update is necessary: The console window will flash and notify the user that they have 5 minutes to save their work
     *              and close, otherwise Access will close, update, and relaunch. After 5 minutes, if Access is still open, the watcher will
     *              kill all process instances of Access, run the front end updater, relaunch Access, then execute a the Watcher once more.
     *              
     */
    
    public class UpdateWatcher
    {
        /* The code below is necessary to lock the console window from closing */
        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern IntPtr RemoveMenu(IntPtr hMenu, uint nPosition, uint wFlags);
        internal const uint SC_CLOSE = 0xF060;
        internal const uint MF_GRAYED = 0x00000001;
        internal const uint MF_BYCOMMAND = 0x00000000;
        //The code above is for the locking the console window
        
        //The code below is for minimizing the console window
        internal const Int32 SW_MINIMIZE = 6;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow([In] IntPtr hWnd, [In] Int32 nCmdShow);

        private static void MinimizeConsoleWindow(IntPtr window)
        {
            ShowWindow(window, SW_MINIMIZE);
        }
        //The code above is for minimizing the console window


        //The code below is necessary for flashing the console window
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        public const UInt32 FLASHW_ALL = 3;
        public const UInt32 FLASHW_STOP = 0;
        //The code above is necessary for flashing the console window
        
        private bool _needsUpdate;
        private bool _lockout;
        private DTO.User _user;
        private bool _accessIsOpen;
        
        //The autoevent is used to hold continued execution until it is raised.
        private AutoResetEvent _autoEvent;
        
        private Timer _timer;

        public UpdateWatcher(DTO.User user)
        {
            this._user = user;
        }

        public void Excecute()
        {
            //1. instantiate _autoEvent so it can be used later.
            _autoEvent = new AutoResetEvent(false);
            //2. Put text on Console for background operation and disable the "x" button.
            disableConsoleClose();
            //3. Initial check for Access locks
            _accessIsOpen = accessLocksExist();
            
            //4. Watch local access files for locks. While they exist:.
            FileSystemWatcher watcher = new FileSystemWatcher(GetInfo.GetAppDataPath() + "\\Access Files");
            watcher.Deleted += new FileSystemEventHandler(onDeleted);
            watcher.NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Size;
            
            //4. Run a timer that will call a specific callback method every 5 minutes.
            watcher.EnableRaisingEvents = true;
            TimerCallback tcb = checkForUpdateTimerCallBack;
            this._timer = new Timer(tcb, null,new TimeSpan(0,2,0),new TimeSpan(0,2,0));
            //5. Wait for an autoevent to be raised to continue execution
            this._autoEvent.WaitOne();
            this._timer.Dispose();
            //6. If autoevent is triggered, then...
            if (_lockout) lockUserOut();
            else if (_needsUpdate) updateUser();
            else
            {
                watcher.Dispose();
                return;
            } 
        }
        //This will lock the console window from being closed and notify the user Access is running.
        //It will also change the console color to blue... for funzies.
        private void disableConsoleClose()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Title = "P&B Database Launcher - This window must stay open.";
            IntPtr hMenu = Process.GetCurrentProcess().MainWindowHandle;
            IntPtr hSystemMenu = GetSystemMenu(hMenu, false);
            EnableMenuItem(hSystemMenu, SC_CLOSE, MF_GRAYED);
            RemoveMenu(hSystemMenu, SC_CLOSE, MF_BYCOMMAND);
            Console.Clear();
            Console.WriteLine("The P&B Access Database is now running. Please leave this window open. \n");
            Console.WriteLine("Current user: {0} \nCurrent UserType: {1} \nCurrent RolloutVersion: {2} \nCurrent Launcher Version {3}",
                                _user.Name,
                                _user.UserType,
                                GetDataFromXml.GetFrontEndSettings().RolloutVersionString,
                                GetInfo.GetCurrentLauncherVersion());
            MinimizeConsoleWindow(hMenu);
        }
        //This will flash the console window, notify the user, then then (either after 5 minutes or the next autoevent from access closing)
        //it will close all instances of Access currently running.
        private void lockUserOut()
        {
            Console.Clear();
            FrontEndUpdater.UpdateOnNextLaunch();
            FlashWindow(Process.GetCurrentProcess().MainWindowHandle, true);
            Console.WriteLine("Access has been locked for maintenance. Please save your work and exit. Access will be automatically closed in 5 minutes.");
            this._autoEvent.WaitOne(new TimeSpan(0, 5, 0));
            closeAllInstancesOfAccess();

        }

        /*This will:
         *      1. Flash the console window
         *      2. Notify the user that Access has become outdated and will relaunch in 5 minutes to update if they do not close it.
         *   ---Either after 5 minutes or the next autoevent from access closing it will then...
         *      3. It will close all instances of Access currently running
         *      4. It will run the updater
         *      5. It will launch Access again
         *      6. It will stop the console window from flashing
         *      7. It will execute the watcher once more.
         */
        private void updateUser()
        {
            Console.Clear();
            FlashWindow(Process.GetCurrentProcess().MainWindowHandle, true);
            Console.WriteLine("It appears your access version has become outdated. Please save your work and relaunch Access. " 
                                + "Access will automatically close and relaunch in 5 minutes. You can shortcut this process by closing Access "
                                + "and then relaunching it. The update will be made when you relaunch.");
            this._autoEvent.WaitOne(new TimeSpan(0,5,0));
            closeAllInstancesOfAccess();
            FrontEndUpdater.UpdateFrontEnd(_user.UserType);
            this._needsUpdate = false;
            Process.Start(Domain.FrontEnd.GetInfo.GetLaunchFilePath());
            FlashWindow(Process.GetCurrentProcess().MainWindowHandle, false);
            this.Excecute();
        }

        //Whenever closeAllInstancesofAccess is called, it will close all running instances of Access.
        private void closeAllInstancesOfAccess()
        {
            Process[] processes;
            string procName = "MSACCESS";
            processes = Process.GetProcessesByName(procName);
            foreach (Process proc in processes)
            {
                proc.CloseMainWindow();
                proc.WaitForExit();
            }
        }
        //This determines whether access locks currently exist on the launcher's access files. This is an effective way to
        //determine whether or not any of the files is currently open.
        private bool accessLocksExist()
        {
            var locks = from filePath in Directory.EnumerateFiles(GetInfo.GetAppDataPath() + "Access Files")
                        where filePath.EndsWith(".laccdb")
                        select new FileInfo(filePath);
            if (locks.Count()>0) return true;
            else return false;
        }

        //This event is called whenever a file is deleted in the Access Files directory. It is called by the FileSystemWatcher.
        private void onDeleted(object source, FileSystemEventArgs e)
        {        
            this._accessIsOpen = accessLocksExist();
            //If access isn't open any longer, turn off the timer and throw an exception to quit.
            if (!this._accessIsOpen)
            {
                this._timer.Dispose();
                this._autoEvent.Set();
            }
        }
        //This method will be called every 5 minutes while the application is running.
        //Stateinfo is required for the timer callback delegate.
        private void checkForUpdateTimerCallBack(object stateInfo)
        {
            //1. Check for applicable updates to the backend.xml
            this._needsUpdate = UpdateChecker.CompareUserInfo(this._user);

            //2. check for lockout signal
            this._lockout = GetInfo.LockoutIsEnabled();
            
            //3. If either return true, trigger the autoEvent.
            if (this._needsUpdate || this._lockout)
            {
                _timer.Dispose();
                this._autoEvent.Set();
            } 
        }
        
        //This will flash the console window if flash is true. If flash is false, it will stop the console window from flashing.
        private static void FlashWindow(IntPtr hWnd, bool flash)
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = (flash) ? FLASHW_ALL : FLASHW_STOP;
            fInfo.uCount = UInt32.MaxValue;
            fInfo.dwTimeout = 0;

            FlashWindowEx(ref fInfo);
        }


         
    }
}
