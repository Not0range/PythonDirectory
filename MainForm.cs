using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace PythonDirectory
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string text = File.ReadAllText("dictionary.txt");
            var mc = Regex.Matches(text, @"@topic=(?<topic>\w+);" +
                @"(chapter=(?<chapter>\w+);)?[^@\^]+(\^(?<solution>[^@\^]+))?@");
        }
    }
}
