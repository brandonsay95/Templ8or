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

namespace Templ8or
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32")]
        static extern bool AllocConsole();
        public MainWindow()
        {            InitializeComponent();

            //  AllocConsole();
            this.textEditor.Text = @"#declare var:string
#declare var1:string
${var = ""Hello""}$
${var1 = ""World""}$
$var $var1 :)";
            Button_Click(null, null);
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
                textEditoroutput.Text = result.ToString();
            }
            catch(Exception exc)
            {
                textEditoroutput.Text = exc.ToString();
            }
        }

        private void textEditor_KeyDown(object sender, KeyEventArgs e)
        {
            Button_Click(null, null);

        }
    }

  
}
