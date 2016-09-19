using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccessLauncher.Domain.BackEnd;
using System.Diagnostics;
using System.Threading;
using AccessLauncher.AccessDBInterface;
using AccessLauncher.Domain;

namespace AccessLauncher.FrontEnd
{
    class Program
    {
        /* This is the launcher program for users. It is a simple console application. Most of the method calls
         * are into the Domain layer, specifically the Domain.FrontEnd namespace.
         * 
         * Here is the process of this launcher:
         *      1. Check to see if there is another instance of the front end launcher running. If so, notify the user
         *          and cancel any further execution.
         *      2. Check to see if there is currently a global lockout activated on the launcher. IF so, notify the user and
         *          cancel any further execution.
         *      3. Compare the current launcher version with the most recently pushed out launcher version. If necessary, update the launcher.
         *      4. Check to see if the current Windows User exists within the Access DB User_Login_Info table.
         *          -If not, then the launcher will not launch and, upon hitting enter, it will uninstall the front end.
         *      5. If the current Windows User exists within the User_Login_Info table, it will compare the current user's
         *          frontend.xml file with the current rollout version for the user's UserType.
         *          -If the UserType is different or if the version number isn't the same as current, the front end will:
         *              i. Delete all current access files.
         *              ii. Extract the most recent rollout into the Access Files folder.
         *              iii. Then launch the current rollout's launchfile.
         *          -If the current version is the same as the current rollout, the front end will simply launch the current launchfile.
         *      6. Once the launcher has launched Access, it will remain open and monitor Access, periodically checking for updates and
         *          lockouts as long as access is running. (See documentation on Domain.FrontEnd.UpdateWatcher)
         */

        static void Main(string[] args)
        {
            //force only one instance of the launcher to be running at any given time using mutex.
            string mutex_id = "ACCESS_LAUNCHER_FRONTEND";
            using (Mutex mutex = new Mutex(false, mutex_id))
            {
                //If an application is already running with the above mutex_id, this will notify the user and
                //then do nothing else but close.
                if (!mutex.WaitOne(0, false))
                {
                    Console.WriteLine("The PB Launcher is already running. Press enter to exit");
                    Console.ReadLine();
                    return;
                }
                checkLockout();

                Console.WriteLine("Checking launcher version...");
                checkLauncherVersion();

                Console.WriteLine("Checking current user...");
                DTO.User user = null;
                try
                {
                    checkUser(out user);
                }
                catch (DTO.Exceptions.CouldNotAccessDBException)
                {
                    Console.WriteLine("The back end database could not be accessed. Therefore, you cannot launch Access at this time. " +
                                        "Please notify your database administrator.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                
                if(user != null)
                {
                    Console.WriteLine("Checking for updates...");
                    bool needsUpdate = checkForUpdates(user);

                    Console.WriteLine("Launching file...");
                    System.Diagnostics.Process.Start(Domain.FrontEnd.GetInfo.GetLaunchFilePath());
                    //After access is launched, the launcher should now fade into background and periodically check for instructions
                    try
                    {
                        var watcher = new Domain.FrontEnd.UpdateWatcher(user);
                        watcher.Excecute();
                    }
                    catch (DTO.Exceptions.QuitConsoleException)
                    {
                        Environment.Exit(0);
                    }
                    catch (DTO.Exceptions.CouldNotFindValueException)
                    {
                        Console.Clear();
                        Console.WriteLine("There is currently no version available to launch. If you believe you have recieved this " +
                            "message in error, please talk to the database administrator. Press enter to exit.");
                        Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        Console.Clear();
                        Console.WriteLine("The launcher encountered an exception: {0} Press enter to exit.", ex.Message);
                        Console.ReadLine();
                    }
                }
                
            }
        }

        private static bool checkForUpdates(DTO.User user)
        {
            bool needsUpdate = false;
            try
            {
                needsUpdate = Domain.FrontEnd.UpdateChecker.CompareUserInfo(user);
                if (needsUpdate)
                {
                    Console.WriteLine("Updating current version to latest rollout...");
                    Domain.FrontEnd.FrontEndUpdater.UpdateFrontEnd(user.UserType);
                    Console.WriteLine("Update complete.");
                }
            }
            catch (DTO.Exceptions.QuitConsoleException)
            {
                Environment.Exit(0);
            }
            catch (DTO.Exceptions.CouldNotFindValueException)
            {
                Console.Clear();
                Console.WriteLine("There is currently no version available to launch. If you believe you have recieved this " +
                    "message in error, please talk to the database administrator. Press enter to exit.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("The launcher encountered an exception: {0} Press enter to exit.", ex.Message);
                Console.ReadLine();
            }
            return needsUpdate;
        }

        private static void checkUser(out DTO.User user)
        {
            var userRepo = new UserInfoRepository(DTO.Enums.BackEndOrFrontEndEnum.FrontEnd);
            var userDbManager = new UserDbManager(userRepo);
            user = userDbManager.GetUserByUsername(Environment.UserName);
            //A null user would mean the current Windows username doesn't exist in the User_Login_Info table
            if (user == null)
            {
                Console.Clear();
                Console.WriteLine("Sorry, but you are not currently an authorized user. Please speak to the database administrator if you believe you have " +
                    "seen this message in error. Press enter to exit.");
                Console.ReadLine();
            }
        }

        private static void checkLauncherVersion()
        {
            try
            {
                Domain.FrontEnd.UpdateChecker.CompareLauncherVersions();
            }
            //This is a custom exception thrown out to ensure the launcher will quit if it needs to, no matter where in the execution
            //process this exception is thrown.
            catch (DTO.Exceptions.QuitConsoleException)
            {
                Environment.Exit(0);
            }
        }

        private static void checkLockout()
        {
            if (Domain.FrontEnd.GetInfo.LockoutIsEnabled())
            {
                Console.WriteLine("The PB Launcher is currently locked for maintenance and cannot be run. Please try again later.");
                Console.ReadLine();
                Environment.Exit(0);
            };
        }
    }
}
