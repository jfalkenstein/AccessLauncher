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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;

namespace AccessLauncher.BackEnd
{
    /// <summary>
    /// Interaction logic for RollOutWindow.xaml
    /// </summary>
    
    public partial class RollOutWindow : Window
    {
        public bool AssociateDirectoryIsSelected { get; set; }
        public bool AdminDirectoryIsSelected { get; set; }
        public string AssociateDirectory { get; set; }
        public string AdminDirectory { get; set; }
        public string AssociateLaunchFile { get; set; }
        public string AdminLaunchFile { get; set; }
        public string ErrorMessage { get; set; }
        public RollOutWindow()
        {
                this.AdminDirectoryIsSelected = false;
                this.AssociateDirectoryIsSelected = false;
                InitializeComponent();
        }
        //This will first launch a folder browser. Once the user has selected a folder, it will launch a file chooser to select the
        //file the user wants to launch.
        private void BrowseForAssociateDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog()
            {
                Description = "Select a folder to use for the Associates Front End.",
                ShowNewFolderButton = true,
                SelectedPath = Domain.PathManager.GetTemplatePath(DTO.Enums.UserTypeEnum.Associate)
            };
            var result = folderBrowser.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    this.AssociateDirectory = Domain.Validator.ValidateDirectoryPath(folderBrowser.SelectedPath);
                    AssociatePathTB.Text = this.AssociateDirectory;
                    DirectoryInfo associateDirectory = new DirectoryInfo(this.AssociateDirectory);
                    this.statusLabel.Content = "You selected " + associateDirectory.Name + " for your associate rollout Directory.";
                    this.AssociateDirectoryIsSelected = true;
                    var fileChooser = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "Access Files|*.accdb|All Files|*.*",
                        Multiselect = false,
                        Title = "Select the file you want users to launch.",
                        InitialDirectory = this.AssociateDirectory
                    };
                    if (fileChooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.AssociateLaunchFile = fileChooser.FileName;
                        this.statusLabel.Content = "You selected " + fileChooser.SafeFileName + " for the launch file.";
                    }
                    else
                    {
                        ErrorLabel.Text = "You need to select a file to launch.";
                    }
                }
                catch (Exception)
                {
                }
            }
            

        }
        //This will first launch a folder browser. Once the user has selected a folder, it will launch a file chooser to select the
        //file the user wants to launch.
        private void BrowseForAdminDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog()
            {
                Description = "Select a folder to use for the Admin Front End.",
                ShowNewFolderButton = true,
                SelectedPath = Domain.PathManager.GetTemplatePath(DTO.Enums.UserTypeEnum.Admin),
            };
            var result = folderBrowser.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    this.AdminDirectory = Domain.Validator.ValidateDirectoryPath(folderBrowser.SelectedPath);
                    AdminPathTB.Text = this.AdminDirectory;
                    DirectoryInfo adminDirectory = new DirectoryInfo(this.AdminDirectory);
                    this.statusLabel.Content = "You selected " + adminDirectory.Name + " for your associate rollout Directory. ";
                    this.AdminDirectoryIsSelected = true;
                    var fileChooser = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "Access Files|*.accdb|All Files|*.*",
                        Multiselect = false,
                        Title = "Select the file you want users to launch.",
                        InitialDirectory = this.AdminDirectory
                    };
                    if (fileChooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.AdminLaunchFile = fileChooser.FileName;
                        this.statusLabel.Content = "You selected " + fileChooser.SafeFileName + " for the launch file.";
                    }
                    else
                    {
                        ErrorLabel.Text = "You need to select a launch file.";
                    }
                }
                catch (Exception)
                {
                }
            }
            
        }

        private void AssociatesToggle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(AssociatesToggle.Value==1)
            {
                showAssociates();
            }
            else
            {
                hideAssociates();
            }
        }
        
        private void showAssociates()
        {
            BrowseForAssociateDirectoryBtn.Visibility = System.Windows.Visibility.Visible;
            AssociatePathTB.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void hideAssociates()
        {
            BrowseForAssociateDirectoryBtn.Visibility = System.Windows.Visibility.Hidden;
            AssociatePathTB.Visibility = System.Windows.Visibility.Hidden;
        }
        
        private void AdminToggle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AdminToggle.Value == 1)
            {
                showAdmins();
            }
            else
            {
                hideAdmins();
            }
        }

        private void hideAdmins()
        {
            BrowseForAdminDirectoryBtn.Visibility = System.Windows.Visibility.Hidden;
            AdminPathTB.Visibility = System.Windows.Visibility.Hidden;
        }

        private void showAdmins()
        {
            BrowseForAdminDirectoryBtn.Visibility = System.Windows.Visibility.Visible;
            AdminPathTB.Visibility = System.Windows.Visibility.Visible;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void RolloutBtn_Click(object sender, RoutedEventArgs e)
        {
            if(this.AdminDirectoryIsSelected == false && this.AssociateDirectoryIsSelected == false)
            {
                ErrorLabel.Text = "You must select at least one directory to roll out.";
            }
            else if (this.AdminLaunchFile == null && this.AssociateLaunchFile == null)
            {
                ErrorLabel.Text = "There was a problem in establishing the file you want users to launch. " +
                    "Please try selecting a directory and launch file again.";
            }
            else
            {
                try
                {
                    //If the user has opted to roll out associates, the directory, launch file, and usertype are sent into the rollout constructor
                    //and then the rollout is executed.
                    if(this.AssociateDirectoryIsSelected)
                    {
                        statusLabel.Content = "Attempting to roll out associates directory...";
                        var rollout = new Domain.BackEnd.Rollout(this.AssociateDirectory, this.AssociateLaunchFile, DTO.Enums.UserTypeEnum.Associate);
                        rollout.Execute();
                        statusLabel.Content = "Associates directory was successfully rolled out.";
                        ErrorLabel.Text = "";
                    }
                    //Same goes for the  admin rollout, if selected.
                    if(this.AdminDirectoryIsSelected)
                    {
                        statusLabel.Content = "Attempting to roll out admin directory...";
                        var rollout = new Domain.BackEnd.Rollout(this.AdminDirectory, this.AdminLaunchFile, DTO.Enums.UserTypeEnum.Admin);
                        rollout.Execute();
                        statusLabel.Content = "Admin directory was successfully rolled out.";
                        ErrorLabel.Text = "";
                    }
                    RolloutBtn.Visibility = System.Windows.Visibility.Hidden;
                    string message;
                    //A success messagebox will be displayed if the rollout was successful.
                    if (this.AssociateDirectoryIsSelected && this.AdminDirectoryIsSelected) message = "Both the admin and associate directories have been successfully rolled out.";
                    else if (this.AssociateDirectoryIsSelected) message = "The associate directory has been successfully rolled out.";
                    else message = "The admin directory has been successfully rolled out.";
                    System.Windows.MessageBox.Show(message, "Success!",MessageBoxButton.OK);
                    this.DialogResult = true;
                }
                //If an exception is raised, the user will be notified.
                catch (DTO.Exceptions.FileToZipWasOpenException ex)
                {
                    ErrorLabel.Text = "The following file was open and therefore could not be zipped up: " + ex.FileThatCouldNotOpen + ". Please close it and try again.";
                }

            }
        }

        

        


    }
}
