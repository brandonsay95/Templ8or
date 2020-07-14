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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Runtime.InteropServices;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using Microsoft.Win32;
using Templ8or.Models;
using System.Globalization;

namespace Templ8or
{
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }


    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {


            this.DataContext = this;
            InitializeComponent();
            this.textEditor.Text = @"#declare var:string
#declare var1:string
${var = ""Hello""}$
${var1 = ""World""}$
$var $var1 :)";
            Button_Click(null, null);
            ResetFiles();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Workspace Workspace { get; set; }
        private void ResetFiles()
        {
            var starupDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            var workspace = new Workspace();
            workspace.FolderPath = starupDir;
            workspace.FolderName = "Files";
            workspace.LoadWorkspace();
            this.Workspace = workspace;
            workspace.OnFileLoaded += Workspace_OnFileLoaded;
            PropertyChanged(this, new PropertyChangedEventArgs("Workspace"));
          
            if (System.IO.Directory.Exists(starupDir))
            {
              


            }
            else
            {
                System.IO.Directory.CreateDirectory(starupDir);
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(starupDir, "ProjectA"));
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.Combine(starupDir, "ProjectA"), "File1.txt"), @"#declare var:string
#declare var1:string
${var = ""Hello""}$
${var1 = ""World""}$
$var $var1 :)");

                ResetFiles();
            }
        }
        public Document CurrentFile { get; set; }
        private void Workspace_OnFileLoaded(object sender, DocumentLoadingEvent args)
        {
          
                  this.textEditor.Text = System.IO.File.ReadAllText(args.Document.FilePath);
            this.CurrentFile = args.Document;

            PropertyChanged(this, new PropertyChangedEventArgs("CurrentFile"));
        }

        private static string ToLiteral(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {

                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
                    return writer.ToString();
                }
            }
        }
        private void handleFileDialog<T>(Action<T> onOpen) where T : FileDialog
        {
            var fileDialog = Activator.CreateInstance<T>();
            fileDialog.InitialDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            fileDialog.Filter = "Text File(*.txt)|*.txt";
            if (fileDialog.ShowDialog() ?? false)
            {
                
                onOpen(fileDialog);
            }
        }
        private void saveFileAs(object sender, RoutedEventArgs e)
        {
            handleFileDialog<SaveFileDialog>((saveFileDialog) => {
                File.WriteAllText(saveFileDialog.FileName, textEditor.Text);
                ResetFiles();
            });
        }
        private void SaveFile(object sender, RoutedEventArgs e)
        {
            if(this.CurrentFile == null)
            { MessageBox.Show( "You dont have a file selected", "Oh no!");
                return;

            }
            File.WriteAllText(CurrentFile.FilePath, textEditor.Text);
        }
        private void deletFile(object sender, RoutedEventArgs e)
        {
            if (this.CurrentFile == null)
            {
                MessageBox.Show("You dont have a file selected", "Oh no!");
                return;

            }
            File.Delete(CurrentFile.FilePath);
            this.CurrentFile = null;
            this.ResetFiles();
            PropertyChanged(this, new PropertyChangedEventArgs("CurrentFile"));

        }
        private void openFile(object sender, RoutedEventArgs e)
        {
            handleFileDialog<OpenFileDialog>((openFileDialog) => this.textEditor.Text = File.ReadAllText(openFileDialog.FileName));
        }
        private void newFile(object sender, RoutedEventArgs e)
        {
            this.textEditor.Text = "";

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string data = this.textEditor.Text;
                var matches = Regex.Matches(data, @"(#declare.+)");
                string compiledCode = $@"";
                List<string> keyValues = new List<string>();
                foreach (Match match in matches)
                {
                    data = data.Replace(match.Value + "\n", "");
                    var matchData = match.Value.Replace("#declare ", "").Replace('\n', ' ').Replace('\r', ' ');

                    var codeToEval = $@"{matchData.Split(':')[1]} {matchData.Split(':')[0]} = null;";
                    compiledCode += codeToEval + System.Environment.NewLine;
                    keyValues.Add(matchData.Split(':')[0]);

                }

                matches = Regex.Matches(data, @"(?s:\$\{.+?\}\$)");
                //compiledCode = $@"";
                foreach (Match match in matches)
                {
                    data = data.Replace(match.Value + System.Environment.NewLine, "");
                    var matchData = match.Value.Replace("${", "").Replace("}$", "").Replace('\n', ' ').Replace('\r', ' ');


                    compiledCode += matchData + ";" + System.Environment.NewLine;


                }
                int cindex = 0;

                keyValues = keyValues.OrderByDescending(o => o).ToList();

                foreach (var item in keyValues)
                {
                    data = data.Replace("$" + item, "{" + cindex + "}");
                    cindex++;

                }
                if (keyValues.Count == 0)
                {
                    throw new Exception("Must declare a variable use #declare Test:string");
                }
                string newCode = $@"return (string.Format({ToLiteral(data)},{keyValues.Aggregate((a, b) => a + "," + b)}));";
                newCode = compiledCode + System.Environment.NewLine + newCode;

                var result = CSharpScript.EvaluateAsync(newCode, ScriptOptions.Default.WithImports("System")).Result;
                output.Text = result.ToString();
            }
            catch (Exception exc)
            {
                output.Text = exc.ToString();
            }
        }

        private void textEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (this.CurrentFile == null)
                    this.saveFileAs(this,null);
                else
                {
                    this.SaveFile(this, null);

                }
            }

        }
        public Solution Solution { get; set; }
        public Dictionary<string, List<string>> Files { get; set; }

    

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
