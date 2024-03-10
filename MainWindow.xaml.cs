using WpfApp1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;


namespace WpfApp1

{
    public partial class MainWindow : Window
    {
        private List<FileInfo> audioFiles = new List<FileInfo>();
        private bool isPlaying = false;
        private bool isRepeating = false;
        private bool isShuffling = false;
        private DispatcherTimer timer;
        private WindowHistory historyWindow;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            historyWindow = new WindowHistory(this);
            historyWindow.Show();
        }
        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                LoadAudioFiles(Directory.GetFiles(dialog.FileName));
                PlayFirstAudio();
            }
        }

        private void LoadAudioFiles(IEnumerable<string> fileNames)
        {
            audioFiles = fileNames
                .Select(file => new FileInfo(file))
                .Where(file => IsAudioFile(file.Extension))
                .ToList();

            ListBoxMusic.Items.Clear();
            foreach (var file in audioFiles)
            {
                ListBoxMusic.Items.Add(file.Name);
            }
        }

        private bool IsAudioFile(string extension)
        {
            string[] audioExtensions = { ".mp3", ".m4a", ".wav", ".flac", ".wav" };
            return audioExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }

        private void PlayFirstAudio()
        {
            if (audioFiles.Count > 0)
            {
                PlayAudioByIndex(0);
            }
        }
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTimeInfo();
            UpdatePositionSlider();
        }

        private void PlayAudioByIndex(int index)
        {
            if (index >= 0 && index < audioFiles.Count)
            {
                string audioFilePath = audioFiles[index].FullName;
                Filetxt.Text = audioFilePath;
                audioPlayer.Source = new Uri(audioFilePath);
                audioPlayer.Play();
                isPlaying = true;


                AddToHistory(audioFilePath);

                timer.Start();
            }
        }
        public void PlayAudioByPath(string audioFilePath)
        {
            if (!string.IsNullOrEmpty(audioFilePath))
            {
                audioPlayer.Source = new Uri(audioFilePath);
                audioPlayer.Play();
                isPlaying = true;

                AddToHistory(audioFilePath);

                timer.Start();
            }
        }

        private void AddToHistory(string audioFilePath)
        {
            if (historyWindow != null)
            {
                historyWindow.HistoryItems.Insert(0, audioFilePath);
            }
        }
        private async void UpdateTimeInfo()
        {
            while (isPlaying)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    TimeSpan currentPosition = audioPlayer.Position;
                    TimeSpan duration = audioPlayer.NaturalDuration.HasTimeSpan ? audioPlayer.NaturalDuration.TimeSpan : TimeSpan.Zero;

                    Filetxt.Text = $": {currentPosition:mm\\:ss} / : {duration:mm\\:ss}";
                });

                await Task.Delay(500);
            }
        }

        private async void UpdatePositionSlider()
        {
            while (isPlaying)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    double position = audioPlayer.Position.TotalSeconds;
                    PositionSlider.Value = position;
                });

                await Task.Delay(500);
            }
        }
        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();

            if (isRepeating)
            {
                audioPlayer.Position = TimeSpan.Zero;
                audioPlayer.Play();
            }
            else
            {
                NextAudio();
            }
        }

        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                audioPlayer.Pause();
                isPlaying = false;
                timer.Stop();
            }
            else
            {
                audioPlayer.Play();
                isPlaying = true;
                timer.Start();
            }
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            PrevAudio();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            NextAudio();
        }

        private void RepeatBtn_Click(object sender, RoutedEventArgs e)
        {
            isRepeating = !isRepeating;
        }

        private void ShuffleBtn_Click(object sender, RoutedEventArgs e)
        {
            isShuffling = !isShuffling;
            if (isShuffling)
            {
                Random rand = new Random();
                audioFiles = audioFiles.OrderBy(x => rand.Next()).ToList();
                PlayFirstAudio();
            }
            else
            {
                audioFiles = audioFiles.OrderBy(x => x.FullName).ToList();
                PlayFirstAudio();
            }
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioPlayer.Position = TimeSpan.FromSeconds(PositionSlider.Value);
        }

        private void NextAudio()
        {
            if (audioFiles.Count > 0)
            {
                int currentIndex = audioFiles.FindIndex(file => file.FullName == audioPlayer.Source.LocalPath);
                int nextIndex = (currentIndex + 1) % audioFiles.Count;

                PlayAudioByIndex(nextIndex);
            }
        }

        private void PrevAudio()
        {
            if (audioFiles.Count > 0)
            {
                int currentIndex = audioFiles.FindIndex(file => file.FullName == audioPlayer.Source.LocalPath);
                int prevIndex = (currentIndex - 1 + audioFiles.Count) % audioFiles.Count;

                PlayAudioByIndex(prevIndex);
            }
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            audioPlayer.Volume = VolumeSlider.Value;
        }
    }
}




