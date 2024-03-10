using WpfApp1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


namespace WpfApp1
{
    public partial class WindowHistory : Window
    {
        private MainWindow mainWindow;

        public ObservableCollection<string> HistoryItems { get; set; }

        public WindowHistory(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
            HistoryItems = new ObservableCollection<string>();
            HistoryListBox.ItemsSource = HistoryItems;
        }

        private void HistoryListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string selectedSong = HistoryListBox.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedSong))
            {
                mainWindow.PlayAudioByPath(selectedSong);
                Close();
            }
        }
    }
}