
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            //string direcotryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            //string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(direcotryPath, "VRChat/2021-11"), "*.png", System.IO.SearchOption.AllDirectories);
            //foreach (string str in files)
            //{
            //    //Debug.WriteLine(str);
            //    var Image = new BitmapImage();
            //    FileStream stream = File.OpenRead(str);
            //    Image.BeginInit();
            //    Image.CacheOption = BitmapCacheOption.OnLoad;
            //    Image.StreamSource = stream;
            //    Image.DecodePixelWidth = 240;
            //    Image.EndInit();
            //    list.Add(new Photo { Source =Image, FilePath= str});
            //}
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

            //watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            //watcher.Deleted += OnDeleted;
            //watcher.Renamed += OnRenamed;
            //watcher.Error += OnError;

            listView.DataContext = list;
            BindingOperations.EnableCollectionSynchronization(list, new object());
        }


        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedItem == null) return; // ListViewで何も選択されていない場合は何もしない

            Photo item = (Photo)listView.SelectedItem; // ListViewで選択されている項目を取り出す
            
            ImgViewer.Source = new BitmapImage(new Uri(item.FilePath)); // 取り出された項目のプロパティをTextBlockに表示する
        }

        private void SelectImgFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください。",
                InitialDirectory = Properties.Settings.Default.watchFolder,
                IsFolderPicker = true,
            };







            //var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            //dialog.Description = "監視したいフォルダを選択...";
            //dialog.UseDescriptionForTitle = true;
            //dialog.ShowNewFolderButton = true;
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

                //watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                //watcher.Deleted += OnDeleted;
                //watcher.Renamed += OnRenamed;
                //watcher.Error += OnError;





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

                //ImgViewer.Source = new BitmapImage(new Uri(e.FullPath));
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
