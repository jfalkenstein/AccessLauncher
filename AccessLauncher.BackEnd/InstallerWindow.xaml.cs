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
using AccessLauncher.Domain.BackEnd;

namespace AccessLauncher.BackEnd
{
    /// <summary>
    /// Interaction logic for InstallerWindow.xaml
    /// </summary>
    public partial class InstallerWindow : Window
    {
        public bool isAbleToSubmit { get; set; }
        public InstallerWindow()
        {
            this.isAbleToSubmit = false;
            InitializeComponent();
        }

        private void BrowseForDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowser = new FolderBrowserDialog() 
            {
                Description = "Select a folder to use for the rollout directory.",
                ShowNewFolderButton = true,
                RootFolder = Environment.SpecialFolder.MyComputer,
            };
            folderBrowser.ShowDialog();
            try
            {
                RolloutDirectoryTB.Text = Domain.Validator.ValidateDirectoryPath(folderBrowser.SelectedPath);
                ErrorLabel.Content = "";
                this.isAbleToSubmit = true;
            }
            catch (Exception)
            {
                ErrorLabel.Content = "You must select a valid directory.";
            }
        }

        private void BrowseForAccessBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileChooser = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Access Files|*.accdb",
                Multiselect = false,
                Title = "Select the location of the Access Back End database file",
            };
            if (fileChooser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.AccessDbTB.Text = Domain.PathManager.GetUNCPath(fileChooser.FileName);
            }

        }

        
        private void ApplicationCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.isAbleToSubmit)
            {
                InstallPB.Visibility = System.Windows.Visibility.Visible;
                var installer = new Installer(RolloutDirectoryTB.Text, AccessDbTB.Text);
                try
                {
                    installer.Install();
                    if(Domain.XmlAccess.GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd,DTO.Enums.UserTypeEnum.Admin) > 0 ||
                       Domain.XmlAccess.GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd,DTO.Enums.UserTypeEnum.Associate) > 0)
                    {
                        System.Windows.MessageBox.Show("The installer found pre-existing rollouts within the rollout directory you specified and therefore " +
                                                        "has reconstructed your back end to account for those. If you would like to eliminate those old " +
                                                        "rollouts, use the rollback tool to roll back the version numbers to the desired version.",
                                                        "The installer found pre-existing rollouts!",
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Exclamation);
                    }

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                catch (Exception)
                {
                    ErrorLabel.Content = "There was a problem with the installation attempt.";
                }
            }
            else ErrorLabel.Content = "You need to select a valid rollout directory.";
           
            
            

        }

        

    }
}
