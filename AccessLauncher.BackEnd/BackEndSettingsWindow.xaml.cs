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
using AccessLauncher.Domain.XmlAccess;

namespace AccessLauncher.BackEnd
{
    /// <summary>
    /// Interaction logic for BackEndSettingsWindow.xaml
    /// </summary>
    public partial class BackEndSettingsWindow : Window
    {
        public int AssociateVersion { get; set; }
        public int AssociateRollbackVersion { get; set; }
        public int AdminVersion { get; set; }
        public int AdminRollbackVersion { get; set; }
        public int sliderSetting { get; set; }

        
        public BackEndSettingsWindow()
        {
            InitializeComponent();
            setCurrentVersions();
        }

        private void RollBackCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            switch (((ComboBoxItem)RollBackCombo.SelectedItem).Tag.ToString())
            {
                case "Admin":
                    this.sliderSetting = 1;
                    VersionSlider1.Maximum = this.AdminVersion;
                    VersionSlider1.Minimum = 0;
                    VersionSlider1.Value = VersionSlider1.Maximum;
                    Slider1Label.Content = "Admin rollback version: " + VersionSlider1.Value.ToString();
                    VersionSlider1.Visibility = System.Windows.Visibility.Visible;
                    Slider1Label.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Associates":
                    this.sliderSetting = 2;
                    VersionSlider1.Maximum = this.AssociateVersion;
                    VersionSlider1.Minimum = 0;
                    VersionSlider1.Value = VersionSlider1.Maximum;
                    Slider1Label.Content = "Associate rollback version: " + VersionSlider1.Value.ToString();
                    VersionSlider1.Visibility = System.Windows.Visibility.Visible;
                    Slider1Label.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "Both":
                    this.sliderSetting = 3;
                    VersionSlider1.Maximum = this.AssociateVersion;
                    VersionSlider1.Minimum = 0;
                    VersionSlider1.Value = VersionSlider1.Maximum;
                    Slider1Label.Content = "Associate rollback version: " + VersionSlider1.Value.ToString();
                    VersionSlider1.Visibility = System.Windows.Visibility.Visible;
                    Slider1Label.Visibility = System.Windows.Visibility.Visible;
                    VersionSlider2.Maximum = this.AdminVersion;
                    VersionSlider2.Minimum = 0;
                    VersionSlider2.Value = VersionSlider2.Maximum;
                    Slider2Label.Content = "Admin rollback version: " + VersionSlider2.Value.ToString();
                    VersionSlider2.Visibility = System.Windows.Visibility.Visible;
                    Slider2Label.Visibility = System.Windows.Visibility.Visible;
                    break;
                default:
                    this.sliderSetting = 0;
                    break;
            }
        }

        private void VersionSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch (this.sliderSetting)
            {
                case 1:
                    Slider1Label.Content = "Admin rollback version: " + VersionSlider1.Value.ToString();
                    this.AdminRollbackVersion = (int)VersionSlider1.Value;
                    break;
                case 2:
                    Slider1Label.Content = "Associate rollback version: " + VersionSlider1.Value.ToString();
                    this.AssociateRollbackVersion = (int)VersionSlider1.Value;
                    break;
                case 3:
                    goto case 2;
                default:
                    break;
            }
        }

        private void VersionSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider2Label.Content = "Admin rollback version: " + VersionSlider2.Value.ToString();
            this.AdminRollbackVersion = (int)VersionSlider2.Value;
        }

        private void ExecuteRollbackBtn_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (this.AdminRollbackVersion < this.AdminVersion)
                {
                    Domain.BackEnd.BackEndSettingsManager.RollBackVersionNumber(this.AdminRollbackVersion, DTO.Enums.UserTypeEnum.Admin);
                }
  
                if (this.AssociateRollbackVersion < this.AssociateVersion)
                {
                    Domain.BackEnd.BackEndSettingsManager.RollBackVersionNumber(this.AssociateRollbackVersion, DTO.Enums.UserTypeEnum.Associate);
                }
                ExecuteRollbackBtn.Visibility = System.Windows.Visibility.Hidden;
                this.DialogResult = true;
                statusLabel.Content = "The rollback attempt succeeded.";
                setCurrentVersions();
            }
            catch (Exception)
            {
                this.DialogResult = false;
                statusLabel.Content = "There was a problem with the roll back attempt. It did not succeed. Rollback will be undone.";
                setCurrentVersions();
            }
            

        }

        private void setCurrentVersions()
        {
            this.AssociateVersion = GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd, DTO.Enums.UserTypeEnum.Associate);
            this.AdminVersion = GetDataFromXml.GetCurrentRolloutVersionNumber(DTO.Enums.BackEndOrFrontEndEnum.BackEnd, DTO.Enums.UserTypeEnum.Admin);
            this.AdminRollbackVersion = this.AdminVersion;
            this.AssociateRollbackVersion = this.AssociateVersion;
            this.CurrentVersionLabel.Content = string.Format("Current Versions: {0} (Admin), {1} (Associate)", this.AdminVersion, this.AssociateVersion);
        }
    }
}
