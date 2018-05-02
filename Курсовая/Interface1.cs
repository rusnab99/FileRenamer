using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;
using System.Collections.ObjectModel;
using Курсовая;
using System.IO;
using System.Diagnostics;

namespace FileSystem
{
    public class file
    {

        public static void rename(string name, string type, ObservableCollection<Element> Elements)
        {           
            name = name.Replace("\\old\\", "\\%old%\\");
            name = name.Replace("\\crt\\", "\\%crt%\\");
            name = name.Replace("\\cht\\", "\\%cht%\\");
            name = name.Replace("\\ind\\", "\\%ind%\\");            
            string[] NewName = name.Split('\\');
            string Filename;
            int ind = 1;
            FileInfo f;
            string extension;
            string[] buf;
            string path;
            foreach (var element in Elements)
            {
                Filename = "";
                buf = element.Title.Split('.');
                
                f = new FileInfo(element.path);
                extension = buf[buf.Length - 1];
               
                foreach (var param in NewName)
                    switch (param)
                    {
                        case "":  break;
                        case "%old%":Filename += element.Title.Replace("."+extension,"");  break;
                        case "%ind%":Filename += ind.ToString();  break;
                        case "%crt%":
                            
                            f = new FileInfo(element.path);
                            Filename += f.CreationTime.ToString().Replace(':', '.');
                            break;
                        case "cht":
                             f = new FileInfo(element.path);
                            Filename += f.LastWriteTime.ToString().Replace(':', '.');
                            break;
                        default: Filename += param;break;
                    }
                path = element.path.Replace(element.Title, "");
                try
                {
                    File.Move(element.path, path + Filename + "." + type);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                ind++;
            }
        }
        public static void rename(string name, ObservableCollection<Element> Elements)
        {
            name = name.Replace("\\old\\", "\\%old%\\");
            name = name.Replace("\\crt\\", "\\%crt%\\");
            name = name.Replace("\\cht\\", "\\%cht%\\");
            name = name.Replace("\\ind\\", "\\%ind%\\");
            string[] NewName = name.Split('\\');
            string Filename;
            int ind = 1;
            FileInfo f;
            string extension;
            string[] buf;
            string path;
            foreach (var element in Elements)
            {
                Filename = "";
                buf = element.Title.Split('.');
                extension = buf[buf.Length - 1];
                foreach (var param in NewName)
                    switch (param)
                    {
                        case "": break;
                        case "%old%": Filename += element.Title.Replace("." + extension, ""); break;
                        case "%ind%": Filename += ind.ToString(); break;
                        case "%crt%":
                            f = new FileInfo(element.path);
                            Filename += f.CreationTime.ToString().Replace(':', '.');
                            break;
                        case "cht":
                            f = new FileInfo(element.path);
                            Filename += f.LastWriteTime.ToString().Replace(':', '.');
                            break;
                        default: Filename += param; break;
                    }
                path = element.path.Replace(element.Title, "");
                try
                { 
                    File.Move(element.path, path + Filename + "." + extension);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                ind++;
            }
        }

        public static void ChangeRegister(string register, ObservableCollection<Element> Elements)
        {
            string path;
            switch (register)
            {
                
                case "up":
                    {
                        
                        for (int i = 0; i < Elements.Count; i++)
                        {
                            if (Elements[i].isSelected)
                            {
                                path = Elements[i].path.Replace(Elements[i].Title, "");
                                File.Move(Elements[i].path, path + Elements[i].Title.ToUpper());
                                Elements[i] = new Element { isSelected = Elements[i].isSelected,
                                    Title = Elements[i].Title.ToUpper(),
                                    path = Elements[i].path.Replace(Elements[i].Title, Elements[i].Title.ToUpper()) };
                            }
                        }
                                break;
                    }
                case "low":
                    {
                        for (int i = 0; i < Elements.Count; i++)
                        {
                            if (Elements[i].isSelected)
                            {
                                path = Elements[i].path.Replace(Elements[i].Title, "");
                                File.Move(Elements[i].path, path + Elements[i].Title.ToLower());
                                Elements[i] = new Element { isSelected = Elements[i].isSelected,
                                    Title = Elements[i].Title.ToLower(),
                                    path = Elements[i].path.Replace(Elements[i].Title, Elements[i].Title.ToLower()) };
                                
                            }
                        }
                        break;
                    }
               
                    
            }
            

        }        
    }
    public class FindObjects
    {
        public ObservableCollection<Element> Elements { get; set; }
        public static ObservableCollection<Element> FindFolders(string DirName)
        {
            try
            {
                string[] x = Directory.GetDirectories(DirName);
                var Dir = new ObservableCollection<Element>();
                for (int i = 0; i < x.Length; i++) Dir.Add(new Element { type = "Folder", Title = ((x[i].Replace(DirName, "")).Replace("\\","")), path=x[i] });
                return Dir;
            }
            catch(Exception ex)
            {
                var Dir = new ObservableCollection<Element>();
                if (ex is System.UnauthorizedAccessException) MessageBox.Show("Доступ запрещен");
                return Dir;
            }
        }
        public static ObservableCollection<Element> FindFiles(string DirName)
        {
            try
            {
                string[] x = Directory.GetFiles(DirName);
                var FileList = new ObservableCollection<Element>();
                for (int i = 0; i < x.Length; i++) FileList.Add(new Element { type = "file", path = x[i], Title = ((x[i].Replace(DirName, "")).Replace("\\", "")) });
                return FileList;
            }
            catch {
                var Dir = new ObservableCollection<Element>();
                
                return Dir;
            }
        }
        static ObservableCollection<Element> Result = new ObservableCollection<Element>();
        public static ObservableCollection<Element> Search(filter Filter,string currentPath)
        {
            if (Filter.SizeFrom == -1 && Filter.CreationDateFrom == null && Filter.ChangeDateFrom == null && Filter.SizeTo == 0 && Filter.CreationDateTo == null && Filter.ChangeDateTo == null)
                return SearchNoAtt(Filter,currentPath);
            else return SearchAtt(Filter,currentPath);
        }
        private static ObservableCollection<Element> SearchAtt(filter Filter,string currentPath)
        {
            ObservableCollection<Element> Result = new ObservableCollection<Element>();
            
            if (Filter.search_in)
            {
                var search = FindFiles(currentPath);
                foreach (var r in search)
                    if (r.Title.Contains(Filter.search)) Result.Add(r);
                foreach (var path in Folders(currentPath))
                    files(path, Filter.search);

            }
            else
            {
                var search = FindFiles(currentPath);
                foreach (var r in search)
                    if (r.Title.Contains(Filter.search)) Result.Add(r);
            }
            FileInfo f;
            {
                if (Filter.CreationDateFrom != DateTime.MinValue && Filter.CreationDateTo != DateTime.MaxValue)
                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        if (!(f.CreationTime >= Filter.CreationDateFrom && f.CreationTime <= Filter.CreationDateTo))
                            Result.RemoveAt(i);
                    }
                else if (Filter.CreationDateFrom != DateTime.MinValue)
                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        if (!(f.CreationTime >= Filter.CreationDateFrom))
                            Result.RemoveAt(i);
                    }
                else if (Filter.CreationDateTo != DateTime.MaxValue)
                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        if (!(f.CreationTime <= Filter.CreationDateTo))
                            Result.RemoveAt(i);
                    }
            }
            {
                if (Filter.ChangeDateFrom != DateTime.MinValue && Filter.ChangeDateTo != DateTime.MaxValue)

                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        if (!(f.LastAccessTime >= Filter.ChangeDateFrom && f.LastAccessTime <= Filter.ChangeDateTo))
                            Result.RemoveAt(i);
                    }
                else if (Filter.ChangeDateFrom != DateTime.MinValue )

                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        if (!(f.LastAccessTime >= Filter.ChangeDateFrom ))
                            Result.RemoveAt(i);
                    }
                else if( Filter.ChangeDateTo != DateTime.MaxValue)

                    for (int i = 0; i < Result.Count; i++)
                {
                    f = new FileInfo(Result[i].path);
                    if (!( f.LastAccessTime <= Filter.ChangeDateTo))
                        Result.RemoveAt(i);
                }
                
            }
            {
                if (Filter.SizeFrom != -1 && Filter.SizeTo != -1)
                {
                    long fSize;
                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        fSize = f.Length;
                        switch (Filter.SizeType)
                        {
                            case "B": break;
                            case "kB": fSize /= 1024; break;
                            case "MB": fSize /= 1024 * 1024; break;
                            case "GB": fSize /= 1024 * 1024 * 1024; break;

                        }
                        if (!(fSize >= Filter.SizeFrom && fSize <= Filter.SizeTo))
                            Result.RemoveAt(i);
                    }
                }else
                if (Filter.SizeFrom != -1 )
                {
                    long fSize;
                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        fSize = f.Length;
                        switch (Filter.SizeType)
                        {
                            case "B": break;
                            case "kB": fSize /= 1024; break;
                            case "MB": fSize /= 1024 * 1024; break;
                            case "GB": fSize /= 1024 * 1024 * 1024; break;

                        }
                        if (!(fSize >= Filter.SizeFrom ))
                            Result.RemoveAt(i);
                    }
                }else
                if ( Filter.SizeTo != -1)
                {
                    long fSize;
                    for (int i = 0; i < Result.Count; i++)
                    {
                        f = new FileInfo(Result[i].path);
                        fSize = f.Length;
                        switch (Filter.SizeType)
                        {
                            case "B": break;
                            case "kB": fSize /= 1024; break;
                            case "MB": fSize /= 1024 * 1024; break;
                            case "GB": fSize /= 1024 * 1024 * 1024; break;

                        }
                        if (!( fSize <= Filter.SizeTo))
                            Result.RemoveAt(i);
                    }
                }
            }
            
            return Result;
        }
        internal static string[] Folders(string path)
        {
            try
            {
                string[] x = Directory.GetDirectories(path);
                
                return x;
            }
            catch
            {
                string[] x = new string[0];
                return x;
            }
        }
        internal static void files(string path, string filter)
        {
            var search = FindFiles(path);
            foreach (var r in search)
                if (r.Title.Contains(filter)) Result.Add(r);
            foreach (var Path in Folders(path))
                files(Path, filter);
        }
        private static ObservableCollection<Element> SearchNoAtt(filter Filter, string currentPath)
        {
            
            if(Filter.search_in)
            {
                var search = FindFiles(currentPath);
                foreach (var r in search)
                    if (r.Title.Contains(Filter.search)) Result.Add(r);
                foreach (var path in Folders(currentPath))
                    files(path,Filter.search);

            }
            else
            {
                var search = FindFiles(currentPath);
                foreach (var r in search)
                    if (r.Title.Contains(Filter.search)) Result.Add(r);
            }
            return Result;
        }
    }
    public class dir
    {
        public static string[] pathTree(string Dir)
        {
            
            try
            {
                string[] x = Directory.GetDirectories(Dir);

                for (int i = 0; i < x.Length; i++)
                {
                    x[i] = x[i].Replace(Dir, "");
                    x[i] = x[i].Replace("\\", "");
                }
                return x;
            }
            catch
            {
                string[] x = new string[0];
                return x;
                
            }
            
        }
        public static string[] pathTree()
        {
            var drive = Directory.GetLogicalDrives();
            return drive;

        }
        public static void path( object nodes)
        {
            string[] paths = pathTree(((Node)nodes).path);
            if (paths.Length > 0)
            {
                ((Node)nodes).Nodes = new ObservableCollection<Node>();
                for (int i = 0; i < paths.Length; i++)
                {
                    ((Node)nodes).Nodes.Add(new Node { Name = paths[i], path = ((Node)nodes).path + paths[i]+"\\",Nodes=new ObservableCollection<Node>() });
                    
                }
            }
        }
    }
    
    }
namespace Курсовая
{//объявление основных пользовательзских классов программы
    /// <summary>
      /// Класс элемента отображаемых элементов
      /// </summary>
   
    public class filter
    {
        public string search { get; set; }
        public bool search_in { get; set; }
        public DateTime CreationDateFrom { get; set; }
        public DateTime ChangeDateFrom { get; set; }
        public DateTime CreationDateTo { get; set; }
        public DateTime ChangeDateTo { get; set; }
        public int SizeFrom { get; set; }
        public int SizeTo { get; set; }
        public string SizeType { get; set; }
        public filter()
        {
            search = "";
            search_in = false;
            ChangeDateFrom = DateTime.MinValue;
            CreationDateFrom = DateTime.MinValue;
            SizeFrom = -1;
            ChangeDateTo = DateTime.MaxValue;
            CreationDateTo = DateTime.MaxValue;
            SizeTo = -1;
            SizeType = "";

        }
    }
    public partial class MainWindow : Window
    {
        public string Tip = @"Для задания нового имени используются следущие токены:
                /old/ - старое имя файла, расширение не включается;
                /crt/ - время создания файла;
                /cht/ - время последнего изменения файла;
                /ind/ - индекс, начиная 1;
                Новое имя задается произвольно.";
        public static string register = null;
        public class folders
        {
            public string name { get; set; }
        }



        public static ObservableCollection<Element> Elements { get; set; }
        public static ObservableCollection<Element> CheckedElements { get; set; } = new ObservableCollection<Element>();
        ObservableCollection<Node> nodes { get; set; }
        /// <summary>
        /// установка ресурсов для вывода
        /// </summary>
        /// <param name="currentPath">Текущая рабочая директива</param>
        public static string currentPath = "C:\\";
        public static string ShowingPath;
        
    }

    public class Node
    {
        public string Name { get; set; }
        public string path { get; set; }
        public ObservableCollection<Node> Nodes { get; set; }

    }

}