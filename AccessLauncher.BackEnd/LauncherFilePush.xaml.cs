using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace AccessLauncher.BackEnd
{
    /// <summary>
    /// Interaction logic for LauncherFilePush.xaml
    /// </summary>
    public partial class LauncherFilePush : Window
    {
        public List<FileInfo> selectedFiles { get; set; }
        public LauncherFilePush()
        {
            this.selectedFiles = new List<FileInfo>();
            InitializeComponent();
        }

        private void AddFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            var fileChooser = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Domain.PathManager.GetRolloutDirectoryPath(DTO.Enums.BackEndOrFrontEndEnum.BackEnd) + "Front End Installer Templates",
                Multiselect = true,
                Title = "Select one or more files to push out.",
            };

            if(fileChooser.ShowDialog().Value)
            {
                foreach(string fileName in fileChooser.FileNames)
                {
                    selectedFiles.Add(new FileInfo(fileName));
                }
                bindListBox();
            }

        }

        private void RemoveSelectedBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach(var item in SelectedFilesListBox.SelectedItems)
            {
                this.selectedFiles.RemoveAt(selectedFiles.FindIndex(p => p.Name == item.ToString()));
            }
            bindListBox();
        }

        private void bindListBox()
        {
            SelectedFilesListBox.ItemsSource = this.selectedFiles.Select(p => p.Name).ToList();
        }

        private void PushFilesOutBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to push these files out? They will be be immediately pushed out to all users.", 
                "Are you sure you want to push these out?", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                var launcherPush = new Domain.BackEnd.LauncherPush(selectedFiles);
                try
                {
                    launcherPush.Execute();
                    for (int i = 0; i < selectedFiles.Count; i++)
                    {
                        selectedFiles.RemoveAt(i);
                    }
                    bindListBox();
                    ErrorLabel.Content = "All selected files have successfully been pushed out. You may now close this window.";
                }
                catch (DTO.Exceptions.FileCopyException ex)
                {
                    ErrorLabel.Content = "You cannot select files from the Installer Files directory. Use the Installer Templates folder instead.";
                    this.selectedFiles.RemoveAt(selectedFiles.FindIndex(p => p.FullName == ex.FileThatCouldNotCopy.FullName));
                    bindListBox();
                }
                catch (Exception ex)
                {
                    ErrorLabel.Content = "An error was encountered: " + ex.Message;
                }
                
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
