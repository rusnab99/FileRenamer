using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using FileSystem;
using System.IO;
using System.Threading;
using System.Collections.ObjectModel;
using Курсовая;
using System.Windows.Threading;


namespace Курсовая
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetSource();
            SetPath();
        }
     
        public void SetSource()
        {
            var folder = FindObjects.FindFolders(currentPath);
            var files = FindObjects.FindFiles(currentPath);
            Elements = folder;
            for (int i = 0; i < files.Count; i++)
            {
                Elements.Add(files[i]);
                
            }
            try
            {
                ElementList.ItemsSource = Elements;
            }
            catch { }
            for(int i=0;i<Elements.Count;i++)
            for(int j=0;j<CheckedElements.Count;j++)
                    if(CheckedElements[j].path==Elements[i].path)
                    {
                        Elements[i].isSelected = CheckedElements[j].isSelected;
                        break;
                    }
            ShowingPath = currentPath;
            CurrDir.Text = ShowingPath;
            CheckList.ItemsSource = CheckedElements;
        }

        public void SetPath()
        { 
            string[] paths = dir.pathTree();
            
            ObservableCollection<Thread> threads = new ObservableCollection<Thread>();
            nodes = new ObservableCollection<Node>();
            for (int i = 0; i < paths.Length; i++)
            {
                nodes.Add(new Node { Name = paths[i],path=  paths[i] });
                Thread thread = new Thread(new ParameterizedThreadStart(dir.path));
                threads.Add(thread);
                thread.Start(nodes[i]);

            }
            for (int i = 0; i < threads.Count; i++)
                threads[i].Join();
            pathView.ItemsSource = nodes;
           

        }

        private void pathView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            try
            {
                for (int i = 0; i < ((Node)item.DataContext).Nodes.Count; i++) dir.path(((Node)item.DataContext).Nodes[i]);
                pathView.ItemsSource = nodes;
            }
            catch { }
        }

        private void Button_Click_path(object sender, RoutedEventArgs e)
        {
            UIElementCollection child = (((Grid)((Button)sender).Content).Children);
            string path = ((TextBlock)child[1]).Text;
            currentPath += path + "\\";
            SetSource();

        }

        private void pathView_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            try
            {
                Directory.GetFiles(((Node)item.DataContext).path);
                currentPath = ((Node)item.DataContext).path;
                SetSource();
            }
            catch (Exception ex)
            {
                if (ex is System.UnauthorizedAccessException) MessageBox.Show("Доступ запрещен");
            }
        }
  
        private void Button_Click_GoBack(object sender, RoutedEventArgs e)
        {
            if (CurrDir.Text == currentPath && currentPath != "C:\\")
                try
                {
                    currentPath += "\\\\\\";
                    currentPath = currentPath.Replace("\\\\", "\\");
                    currentPath = currentPath.Replace("\\\\", "");
                    currentPath = (Directory.GetParent(currentPath)).ToString();
                    currentPath += "\\";
                    currentPath = currentPath.Replace("\\\\", "\\");

                }
                catch { }
            SetSource();
        }
       
        private void Button_CancelSelection(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].type == "file")
                    Elements[i].isSelected = false;
            }
            CheckedElements.Clear();
            ElementList.ItemsSource = null;
            ElementList.ItemsSource = Elements;
        }

        private void Button_SelectAll(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].type == "file")
                {
                    Elements[i].isSelected = true;
                    CheckedElements.Add(Elements[i]);
                }
            }
            ElementList.ItemsSource = null;
            ElementList.ItemsSource = Elements;
        }
        filter Filter = new filter();
       private void Button_SearchShow(object sender, RoutedEventArgs e)
        {
            Filter.search = Search.Text;
            Filter.search_in = (bool)Searchin.IsChecked;
            try
            {
                Filter.SizeFrom = int.Parse(SizeFrom.Text);
                Filter.SizeTo = int.Parse(SizeTo.Text);

                Filter.SizeType = SizeType.SelectedItem.ToString();
            }
            catch { }
            try
            {
                Filter.CreationDateFrom = (DateTime)CreationDateFrom.SelectedDate;
                Filter.CreationDateTo = (DateTime)CreationDateTo.SelectedDate;
            }
            catch { }
            try
            {
                Filter.ChangeDateFrom =(DateTime) ChangeDateFrom.SelectedDate;
                Filter.ChangeDateTo = (DateTime)ChangeDateTo.SelectedDate;
            }
            catch { }
            ElementList.ItemsSource=FindObjects.Search(Filter,currentPath);
            CurrDir.Text = "Поиск";
            Search.Text = "";
            Searchin.IsChecked = false;
            CreationDateFrom.SelectedDate = null;
            ChangeDateFrom.SelectedDate = null;
            SizeFrom.Text = "";
            CreationDateTo.SelectedDate = null;
            ChangeDateTo.SelectedDate = null;
            SizeTo.Text = "";
            SizeType.SelectedItem = defaultSize;
            Filter = new filter();
            this.Show();
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (((Element)((CheckBox)sender).DataContext).type == "file")
            {
                CheckedElements.Add((Element)((CheckBox)sender).DataContext);
               
            }
            else
            {
                
                var files = FindObjects.FindFiles(((Element)((CheckBox)sender).DataContext).path);
                foreach (var t in files)
                {
                    
                    CheckedElements.Add(t);
                    CheckedElements[CheckedElements.Count - 1].isSelected = true;

                }
                    Folders((((Element)((CheckBox)sender).DataContext).path));
                
                
            }
        }
        internal void Folders(string path)
        {
            foreach (var p in FindObjects.FindFolders(path))
            {
                var files = FindObjects.FindFiles(p.path);
                foreach (var t in files) CheckedElements.Add(t);

                Folders(p.path);
            }
        }

        private void ViewList_Uncheck(object sender, RoutedEventArgs e)
        {
            if (((Element)((CheckBox)sender).DataContext).type == "Folder")
            {
                for (int i = 0; i < CheckedElements.Count; i++)
                    if (CheckedElements[i].path.Contains(((Element)((CheckBox)sender).DataContext).path))
                    
                        CheckedElements.RemoveAt(i--);
            }
            else
            {
                for (int i = 0; i < CheckedElements.Count; i++)
                    if (((Element)((CheckBox)sender).DataContext).path == CheckedElements[i].path)
                    {
                        CheckedElements.RemoveAt(i);
                        break;
                    }
            }
        }

        private void ElementList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            if (((Element)(((ListView)sender).SelectedItem)).type == "Folder")
            {
                currentPath = (((Element)(((ListView)sender).SelectedItem)).path);
                SetSource();
            }
        }

        private void Button_ToUpper(object sender, RoutedEventArgs e)
        {
            file.ChangeRegister("up", CheckedElements);
            SetSource();
        }

        private void Button_ToLower(object sender, RoutedEventArgs e)
        {
            file.ChangeRegister("low", CheckedElements);
            SetSource();
        }

        private void CheckList_Remove(object sender, RoutedEventArgs e)
        {
             for (int i = 0; i < CheckedElements.Count; i++)
                 if (((Element)((CheckBox)sender).DataContext).path == CheckedElements[i].path)
                 {
                     CheckedElements.RemoveAt(i);
                     break;
                 }
        }

        private void Button_Rename(object sender, RoutedEventArgs e)
        {
            if ((bool)ExtensionChanging.IsChecked)
                file.rename(NewName.Text, Extension.Text, CheckedElements);
            else
                file.rename(NewName.Text, CheckedElements);
            CheckedElements.Clear();
            SetSource();
            NewName.Text = "";
            Extension.Text = "";
            ExtensionChanging.IsChecked = false;
        }

        private void Button_CheckList_UncheckAll(object sender, RoutedEventArgs e)
        {
            foreach (var t in Elements)
                t.isSelected = false;
            CheckedElements.Clear();
        }

    }
    public class Element
    {
        public string type { get; set; }
        public string Title { get; set; }
        public string path { get; set; }

        public bool isSelected
        {
            get;
            set;
        }

    }
    public class TypeSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement elemnt = container as FrameworkElement;
            Element element = item as Element;
            if (element.type == "file")
            {
                return elemnt.FindResource("fileTemplate") as DataTemplate;
            }
            else
            if (element.type == "Folder")
            {
                return elemnt.FindResource("folderTemplate") as DataTemplate;
            }
            return null;
        }
    }
}
   

