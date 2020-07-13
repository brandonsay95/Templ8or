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

namespace Templ8or
{
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

        private void ResetFiles()
        {
            var starupDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");

            Files = new Dictionary<string, List<string>>();
            if (System.IO.Directory.Exists(starupDir))
            {
                foreach (var folder in System.IO.Directory.GetDirectories(starupDir))
                {
                    var f = new KeyValuePair<string, List<string>>(System.IO.Path.GetFileName(folder), new List<string>());
                    foreach (var file in System.IO.Directory.GetFiles(folder))
                    {
                        f.Value.Add(System.IO.Path.GetFileName(file));

                    }
                    Files.Add(f.Key, f.Value);
                }


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

            PropertyChanged(this, new PropertyChangedEventArgs("Files"));
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
            Button_Click(null, null);

        }
        public Dictionary<string, List<string>> Files { get; set; }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var fileName = ((System.Windows.Controls.TextBlock)sender).Text;
            var starupDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files");
            if (System.IO.Directory.Exists(starupDir))
            {
                foreach (var folder in System.IO.Directory.GetDirectories(starupDir))
                {
                    foreach (var file in System.IO.Directory.GetFiles(folder))
                    {
                        if (fileName == System.IO.Path.GetFileName(file)){
                            this.textEditor.Text = System.IO.File.ReadAllText(file);
                            Button_Click(null, null);
                        }

                    }
                   
                }


            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
