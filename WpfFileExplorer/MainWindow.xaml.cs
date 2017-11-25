using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

namespace WpfFileExplorer           
{

        #region CreateBlurEffect  




    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }
    #endregion


        #region OnLoaded

    public partial class MainWindow
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        public MainWindow()
        {
        
            InitializeComponent();
            Loaded += MainWindow_Loaded;
       
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
           EnableBlur();
            foreach (var drive in Directory.GetLogicalDrives())
            {
                
                var item = new TreeViewItem();


             
                item.Header = drive;
          
                item.Tag = drive;

                

               
                
                item.Items.Add(null);
               
                item.Expanded += Folder_Expended;

         
                FolderView.Items.Add(item);
            }



            var all = (TreeViewItem)FolderView.Items[0];
            string userName = Environment.UserName;
            JumpToNode(all, $"C:\\");
            Console.WriteLine(all.Tag);

        }

        #endregion

        private void ItemSelected(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;

            Console.WriteLine((string)item.Tag);
        }


        #region Folder_Expended

      

        private void Folder_Expended(object sender, RoutedEventArgs e)
        {

            #region Inital Checks
            var item = (TreeViewItem)sender;

            
            if (item.Items.Count != 1 || item.Items[0] != null)
                return;

            
            item.Items.Clear();

            
            var fullPath = (string)item.Tag;

            #endregion

            #region Get Folders
         
            var directories = new List<string>();

           
            try
            {

                var dirs = Directory.GetDirectories(fullPath);

                if (dirs.Length > 0)
                    directories.AddRange(dirs);

            }
            catch { }

           
            directories.ForEach(directoryPath => {


                
                var subItem = new TreeViewItem() {

                  
                    Header = GetFileFolderName(directoryPath),

                  
                    Tag = directoryPath

                };
             

              
                subItem.Items.Add(null);

              
                subItem.Expanded += Folder_Expended;

                item.Items.Add(subItem);

            });
            #endregion

            #region Get Files

          
            var files = new List<string>();

         
            try
            {

                var fs = Directory.GetFiles(fullPath);

                if (fs.Length > 0)
                    files.AddRange(fs);

            }
            catch { }

           
            files.ForEach(filePath =>
            {


                
                var subItem = new TreeViewItem()
                {

                    
                    Header = GetFileFolderName(filePath),

                    
                    Tag = filePath

                };



                
                item.Items.Add(subItem);


               
            });
            #endregion
        }

        #endregion


        #region Helpers
     
        public static string GetFileFolderName(string path)
        {
            //C:\Something\a folder
            //C:\Something\a file.png
          


            if (string.IsNullOrEmpty(path))
                return string.Empty;


           
            var normalizePath = path.Replace('/', '\\');

            var lastIndex = normalizePath.LastIndexOf("\\");

           
            if (lastIndex <= 0)
                return path;

          
            return path.Substring(lastIndex + 1 );

        }

        #endregion


        private void SingleFileSelected(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(sender);

            var source = (ListBox)e.Source;
            var selected = (FileDetails)source.SelectedItem;

        }


        private void ItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            try
            {

                ListBox items = sender as ListBox;
                var selected = items.SelectedItem as FileDetails;
                var path = selected.Path;
                var allFolders = new DirectoryInfo(path).GetDirectories();





                var details = new List<FileDetails>();

                for (int i = 0; i < allFolders.Length; i++)
                {
                    var detail = new FileDetails();
                    detail.FileName = allFolders[i].Name;

                    details.Add(detail);
                }

                myList.ItemsSource = details;
                Console.WriteLine(selected);

            }
            catch {  }
        }

        void JumpToNode(TreeViewItem tvi, string NodeName)
        {
            if (tvi.Tag.ToString() == NodeName)
            {
                tvi.IsExpanded = true;
                tvi.BringIntoView();
                return;
            }
            else
                tvi.IsExpanded = false;

            if (tvi.HasItems)
            {
                foreach (var item in tvi.Items)
                {
                    TreeViewItem temp = item as TreeViewItem;
                    JumpToNode(temp, NodeName);
                }
            }

            

        }



        private void SelectedItem(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selected = (TreeViewItem)e.NewValue;
            var fullPath = (string)selected.Tag;
            var metaData = new FileInfo(fullPath);

            //labelResult.Content = metaData.CreationTime;


            //myDataGrid.ItemsSource = new DirectoryInfo(fullPath).Name;

            //myGrid.DataContext = new DirectoryInfo(fullPath).GetFiles();

            try
            {
                var files = new List<DirectoryInfo>();

                var details = new List<FileDetails>();
                var allFiles = new DirectoryInfo(fullPath).GetFiles();
                var allFolders = new DirectoryInfo(fullPath).GetDirectories();



                for (int i = 0; i < allFiles.Length; i++)
                {
                    var fd = new FileDetails();
                    fd.FileName = allFiles[i].Name;
                    fd.FileCreation = allFiles[i].CreationTime.ToString();
                    fd.FileImage = $"pack://application:,,,/Image/file.png";
                    fd.IsFile = true;
                    details.Add(fd);
                }


                for (int i = 0; i < allFolders.Length; i++)
                {
                    var fd = new FileDetails();
                    fd.FileName = allFolders[i].Name;
                    fd.FileCreation = allFolders[i].CreationTime.ToString();
                    fd.FileImage = $"pack://application:,,,/Image/folder.png";
                    fd.IsFolder = true;
                    fd.Path = fullPath + "\\" + allFolders[i].Name;
                    details.Add(fd);
                }



                myList.ItemsSource = details;

                Console.WriteLine(fullPath);
            }
            catch { }

        }

     

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
