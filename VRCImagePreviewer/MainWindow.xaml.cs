
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace VRCImagePreviewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Photo> list = new ObservableCollection<Photo>();
        FileSystemWatcher? watcher = new FileSystemWatcher();
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string watchFolder = Properties.Settings.Default.watchFolder;
            if (watchFolder == "")
            {
                watchFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                Properties.Settings.Default.watchFolder = watchFolder;
                Properties.Settings.Default.Save();
            }

            watcher = new FileSystemWatcher();

            textBlock.Text = watchFolder;
            watcher.Path = watchFolder;
            watcher.Filter = "*.png";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Created += OnCreated;

            listView.DataContext = list;
            BindingOperations.EnableCollectionSynchronization(list, new object());
        }


        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedItem == null) return; 

            Photo item = (Photo)listView.SelectedItem; 
            
            ImgViewer.Source = new BitmapImage(new Uri(item.FilePath)); 
        }

        private void SelectImgFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください。",
                InitialDirectory = Properties.Settings.Default.watchFolder,
                IsFolderPicker = true,
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

                if (watcher != null)
                {
                    watcher.Dispose();
                    watcher = null;
                }
                watcher = new FileSystemWatcher();
                Properties.Settings.Default.watchFolder = dialog.FileName;
                Properties.Settings.Default.Save();
                
                textBlock.Text = dialog.FileName;
                watcher.Path = dialog.FileName;
                watcher.Filter = "*.png";
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;


                watcher.NotifyFilter = NotifyFilters.Attributes
                                     | NotifyFilters.CreationTime
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.Security
                                     | NotifyFilters.Size;

                watcher.Created += OnCreated;

        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Debug.WriteLine(value);
            FileStream stream = null;
            bool FileReady = false;
            while (!FileReady)
            {
                try
                {
                    using (stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        FileReady = true;
                    }
                }
                catch (IOException)
                {
                    //File isn't ready yet, so we need to keep on waiting until it is.
                }

                //We'll want to wait a bit between polls, if the file isn't ready.
                if (!FileReady) Thread.Sleep(1000);
            }
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var Image = new BitmapImage();
                Image.BeginInit();
                Image.CacheOption = BitmapCacheOption.OnLoad;
                Image.StreamSource = stream;
                Image.DecodePixelWidth = 200;
                Image.EndInit();
                stream.Close();

                ImgViewer.Source = new BitmapImage(new Uri(e.FullPath));
                list.Insert(0, new Photo { Source = Image, FilePath = e.FullPath });

            }));
        }
    }
    public class Photo
    {
        public string FilePath { get; set; }
        public BitmapImage Source { get; set; }
    }
}
