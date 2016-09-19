using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using AccessLauncher.Domain.BackEnd;
using AccessLauncher.Domain.XmlAccess;
using System.ComponentModel;

namespace AccessLauncher.BackEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public bool Installed { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            this.Show();
            //On instantiation, checks to see if back end is installed (i.e. both the BackEndSettings.xml and backend.xml have been created.
            if (!Domain.Validator.BackEndIsInstalled())
            {
                /*If the the backend is not fully installed, checks to see if the back end settings file exists.
                    If the backend settings file EXISTS, this means that the system has been installed but the backend.xml file has been deleted.
                    In this case, it the back end will attempt to repair itself (using the TryFixBrokenBackEnd method on the installer.)
                */
                if(Domain.Validator.BackEndSettingsFileExists())
                {
                    var result = MessageBox.Show("The backend.xml file cannot be located. AccessLauncher can attempt to reconstruct it from the contents " +
                                                    "of the rollouts folder. Would you like AccessLauncher to make this attempt? (If you select \"No\", you " +
                                                    "will be redirected to reinstall the back end entirely.) If you select yes, this could take " + 
                                                    "a minute or more.",
                                                 "Backend.xml not found!",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Error,
                                                 MessageBoxResult.Yes);
                    if(result == MessageBoxResult.Yes)
                    {
                        var accessPath = GetDataFromXml.GetAccessPathFromCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
                        var installer = new Domain.BackEnd.Installer(Domain.PathManager.GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd), accessPath);
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.ProgressChanged += updateValue;
                        worker.RunWorkerCompleted += worker_RunWorkerCompleted;
                        worker.WorkerReportsProgress = true;
                        installer.TryFixBrokenBackEnd(ref worker);
                        this.ReconstructionPB.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        launchInstaller();
                    }
                }
                //If the backend settings file does not exist, the installer window will open and the main window will not actually open.
                else
                {
                    launchInstaller();
                }
            }
            //As long as everything is installed correctly, the mainwindow will launch.
            else
            {
                backEndIsInstalled();
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Error == null && !e.Cancelled)
            {
                MessageBox.Show("The back end was successfully reconstructed!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                this.StatusLabel.Content = "";
                backEndIsInstalled();
            }
            else
            {
                MessageBox.Show("There was a problem encounted in attempting to reconstruct the back end. You will need to reinstall the back end. Click \"Ok\" " +
                                    "to reinstall now.",
                                 "Reconstruction failed!",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Error);
                launchInstaller();
            }
            this.ReconstructionPB.Visibility = System.Windows.Visibility.Hidden;
        }

        private void updateValue(object sender, ProgressChangedEventArgs e)
        {
            this.ReconstructionPB.Value = e.ProgressPercentage;
            this.StatusLabel.Content = e.UserState;
        }
        
        private void launchInstaller()
        {
            this.Installed = false;
            var installerWindow = new InstallerWindow();
            installerWindow.Show();
            this.Close();
        }
        
        private void backEndIsInstalled()
        {
            this.Installed = true;
            setStats();
        }
        
        private void GoToRollOutBtn_Click(object sender, RoutedEventArgs e)
        {
            var rolloutWindow = new RollOutWindow();
            var result = rolloutWindow.ShowDialog();
            if (result == true) setStats();
        }
        //This will set the last rollout and current version number stats when called. If an exception is raised by either, the labels will
        //simply be hidden.
        private void setStats()
        {
            try
            {
                LastRolloutLabel.Content = "Last Rollout Date: " + GetDataFromXml.GetMostRecentRolloutDate(DTO.Enums.BackEndOrFrontEndEnum.BackEnd);
            }
            catch (Exception)
            {
                LastRolloutLabel.Visibility = System.Windows.Visibility.Hidden;
            }
            try
            {
                versionNumber.Content = "Current Version Numbers: " +
                    GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd,
                        DTO.Enums.UserTypeEnum.Associate).ToString() + 
                    " (Associate); " +
                    GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd,
                        DTO.Enums.UserTypeEnum.Admin).ToString() +
                    " (Admin)";
            }
            catch (Exception)
            {
                versionNumber.Visibility = System.Windows.Visibility.Hidden;
            }
            if (Domain.BackEnd.BackEndSettingsManager.LockoutIsEnabled())
            {
                LockoutSlider.Value = 1;
                lockoutOnLabel();
            }
            else
            {
                LockoutSlider.Value = 0;
                lockoutOffLabel();
            }
        }

        private void GoToChangeSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new BackEndSettingsWindow();
            var result = settingsWindow.ShowDialog();
            if (result == true) setStats();
        }

        private void LockoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(e.NewValue == 1)
            {
                lockoutOnLabel();
                Domain.BackEnd.BackEndSettingsManager.SetLockout(true);
            }
            else
            {
                lockoutOffLabel();
                Domain.BackEnd.BackEndSettingsManager.SetLockout(false);
            }
        }

        private void lockoutOnLabel()
        {
            LockoutLabel.Content = "Global lockout is enabled.";
            LockoutLabel.Foreground = new SolidColorBrush(Colors.Red);
            LockoutLabel.FontWeight = FontWeights.Bold;
        }
        
        private void lockoutOffLabel()
        {
            LockoutLabel.Content = "Global lockout is off.";
            LockoutLabel.Foreground = new SolidColorBrush(Colors.Black);
            LockoutLabel.FontWeight = FontWeights.Regular;
        }

        private void PushNewLauncherFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            var launcherWindow = new LauncherFilePush();
            launcherWindow.Show();
        }

        private void ReinstallBackEndMenu_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Warning: If you reinstall the Back End, this will allow you to change the rollout directory, but all users will need to" +
                                                " reinstall from the NEW installer link placed in that new rollout directory. Are you SURE you want to reinstall the back end?",
                                        "Warning!",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning,
                                        MessageBoxResult.No);
            if(result == MessageBoxResult.Yes)
            {
                var installer = new InstallerWindow();
                installer.Show();
                this.Close();
            }
        }

        private void ChangeAccessTargetMenu_Click(object sender, RoutedEventArgs e)
        {
            string accessPath = "";
            var result = MessageBox.Show("Are you sure you want to change the Access back end target? This change will be picked up by users the next time they launch. " +
                                            "If this target is wrong, it will effectively lock all users out of Access until it is retargeted correctly.",
                                         "Are you sure you want to retarget the Access back end?",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Stop,
                                         MessageBoxResult.No);
            if(result == MessageBoxResult.Yes)
            {
                var fileChooser = new System.Windows.Forms.OpenFileDialog()
                {
                    CheckFileExists = true,
                    Filter = "Access Files|*.accdb",
                    Multiselect = false,
                    Title = "Select the location of the Access Back End database file",
                };
                if (fileChooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    accessPath = Domain.PathManager.GetUNCPath(fileChooser.FileName);
                }
                try
                {
                    Domain.BackEnd.BackEndSettingsManager.UpdateConnectionString(accessPath);
                    MessageBox.Show("The Access back end target has now been updated. The current target path is: " +
                                        Domain.XmlAccess.GetDataFromXml.GetAccessPathFromCurrentConnectionString(DTO.Enums.BackEndOrFrontEndEnum.BackEnd),
                                    "The Access back end target has been updated.",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    MessageBox.Show("There was an error encountered in pushing out this change.");
                }
            }
            
            
        }
    }
}
