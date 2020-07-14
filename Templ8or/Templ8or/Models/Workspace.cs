using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Templ8or.Models
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            property = value;
            var handler = PropertyChanged;
            if (handler != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RelayCommand : ICommand
    {
        readonly Action _execute;
        readonly Func<bool> _canExecute;
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new NullReferenceException("execute");
            _execute = execute;
            _canExecute = canExecute;

        }
        public RelayCommand(Action execute) : this(execute, null) { }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter) => _canExecute == null ? true : _canExecute();
        public void Execute(object paramter) => _execute.Invoke();

    }
    public class Workspace: ObservableObject
    {
        private string folderName;
        public string FolderName { get => folderName; set => OnPropertyChanged(ref folderName, value); }
        private string folderPath;
        public string FolderPath { get => folderPath; set => OnPropertyChanged(ref folderPath, value); }
        public ObservableCollection<Solution> Solutions { get; set; }
        public void LoadWorkspace()
        {
            if(string.IsNullOrWhiteSpace(FolderName) || string.IsNullOrWhiteSpace(FolderName))
            {
                throw new Exception("Workspace Unloaded");

            }
            this.Solutions = new ObservableCollection<Solution>();

            foreach(var folder in System.IO.Directory.GetDirectories(FolderName))
            {
                var sol = new Solution() { FolderName = System.IO.Path.GetFileName(folder), FolderPath = folder };
                sol.OnFileLoaded += Sol_OnFileLoaded;

                Solutions.Add(sol);
                sol.LoadSolution();

            }
        }
        private void Sol_OnFileLoaded(object sender, DocumentLoadingEvent args)
        {
            if (OnFileLoaded != null)
                OnFileLoaded(this, args);

        }

        public event DocumentLoadingEventHandler OnFileLoaded;
        public delegate void DocumentLoadingEventHandler(object sender, DocumentLoadingEvent args);
    }
    public class Solution: ObservableObject
    {
        private string folderName;
        public string FolderName { get => folderName; set => OnPropertyChanged(ref folderName, value); }
        private string folderPath;
        public string FolderPath { get => folderPath; set => OnPropertyChanged(ref folderPath, value); }
        public ObservableCollection<Document> Documents { get; set; }
        public void LoadSolution()
        {
            if (string.IsNullOrWhiteSpace(FolderName) || string.IsNullOrWhiteSpace(FolderName))
            {
                throw new Exception("Workspace Unloaded");

            }
            this.Documents = new ObservableCollection<Document>();

            foreach (var file in System.IO.Directory.GetFiles(FolderPath))
            {
                var sol = new Document() { FileName = System.IO.Path.GetFileName(file), FilePath = file };
                sol.OnFileLoaded += Sol_OnFileLoaded;
                Documents.Add(sol);

            }
        }

        private void Sol_OnFileLoaded(object sender, DocumentLoadingEvent args)
        {
            if (OnFileLoaded != null)
                OnFileLoaded(this, args);

        }

        public event DocumentLoadingEventHandler OnFileLoaded;
        public delegate void DocumentLoadingEventHandler(object sender, DocumentLoadingEvent args);
    }
    public class Document: ObservableObject
    {
        public Document()
        {
            LoadFile = new RelayCommand(loadFile);
        }
        private void loadFile()
        {
            if (OnFileLoaded != null)
            {
                OnFileLoaded(this, new DocumentLoadingEvent(this));

            }
        }
        public ICommand LoadFile { get; set; }
        public event DocumentLoadingEventHandler OnFileLoaded;
        public delegate void DocumentLoadingEventHandler(object sender, DocumentLoadingEvent args);

        private string fileName;
        public string FileName { get => fileName; set => OnPropertyChanged(ref fileName, value); }
        private string filePath;
        public string FilePath { get => filePath; set => OnPropertyChanged(ref filePath, value); }
        private string text;
        public string Text { get => text; set => OnPropertyChanged(ref text, value); }
    }

  

    public class DocumentLoadingEvent : EventArgs
    {
        public Document Document;

        public DocumentLoadingEvent(Document document)
        {
            this.Document = document;
        }
    }
}
