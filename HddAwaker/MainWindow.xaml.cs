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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using Ini.Net;

namespace HddAwaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // See this for more detail: https://social.msdn.microsoft.com/Forums/vstudio/en-US/21992d0b-a02c-4042-a188-47b0a2b99b0b/wpf-system-tray-application?forum=wpf
            // Icon comes from EasyIcon. Thanks mate!

            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.hddlogo;
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private static string driveLetterSettings = string.Empty;
        private static int durationTimeSettings = 30;
        private DispatcherTimer timer = new DispatcherTimer();

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            // ComboBoxItem selectedDriveItem = (ComboBoxItem)deviceListComboBox.SelectedItem;
            driveLetterSettings = deviceListComboBox.SelectedValue.ToString();
            int.TryParse(durationTimerSettingSlider.Value.ToString(), out durationTimeSettings);
            saveSettings(driveLetterSettings, durationTimeSettings);
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            refreshDeviceList();
        }

        private void refreshDeviceList()
        {
            deviceListComboBox.Items.Clear();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in allDrives)
            {
                deviceListComboBox.Items.Add(driveInfo.Name.ToString());
            }
        }

        private void startTimer(int seconds)
        {
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, seconds);
            timer.Start();
        }

        private void stopTimer()
        {
            timer.Stop();
        }

        private async void timerTick(object sender, EventArgs e)
        {
            await doDiskOperation();
        }

        private void saveSettings(string driveLetter, int durationSecond)
        {
            IniFile configFile = new IniFile("settings.ini");
            configFile.WriteString("Settings", "Drive", driveLetter);
            configFile.WriteInteger("Settings", "Duration", durationSecond);
            stopTimer(); startTimer(durationSecond);
            System.Windows.MessageBox.Show("Setting saved!\r\n" + "Value: " + durationSecond + " seconds.");
        }

        private void readSettings()
        {
            IniFile configFile = new IniFile("settings.ini");
            driveLetterSettings = configFile.ReadString("Settings", "Drive");
            durationTimeSettings = configFile.ReadInteger("Settings", "Duration");
        }

        private async Task doDiskOperation()
        {
            byte[] writtenArray = new byte[1024]; // 1MB data buffer
            Random randomGenerator = new Random();
            randomGenerator.NextBytes(writtenArray);

            DirectoryInfo directoryInfo = Directory.CreateDirectory(driveLetterSettings + @"awaker");
            directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileStream fileStream = File.Create(driveLetterSettings + @"\awaker\awaker.bin");
            await fileStream.WriteAsync(writtenArray, 0, 1024);
            fileStream.Dispose();
        }

    }
}
