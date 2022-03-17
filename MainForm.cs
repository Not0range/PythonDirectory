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
        const float SIZE_MULTIPLER = 2.5F;
        InfoCollection _info;
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += ReadInfos;
            worker.RunWorkerAsync();
        }

        private void ReadInfos(object sender, DoWorkEventArgs e)
        {
            string text = File.ReadAllText("dictionary.txt");
            var mc = Regex.Matches(text, @"@topic=(?<topic>\w+);" +
                @"(chapter=(?<chapter>\w+);)?(?<text>[^@\^]+)" +
                @"(\^(?<solution>[^@\^]+))?@");
            var infos = new Info[mc.Count];
            for (int i = 0; i < mc.Count; i++)
            {
                if (mc[i].Groups["chapter"].Value != "")
                {
                    infos[i] = new Directory(mc[i].Groups["topic"].Value,
                        mc[i].Groups["text"].Value.Split(new string[] { "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries),
                        mc[i].Groups["chapter"].Value);
                }
                else if (mc[i].Groups["solution"].Value != "")
                {
                    infos[i] = new Exercise(mc[i].Groups["topic"].Value,
                        mc[i].Groups["text"].Value.Split(new string[] { "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries),
                        mc[i].Groups["solution"].Value.Trim());
                }
                else
                {
                    infos[i] = new Example(mc[i].Groups["topic"].Value,
                        mc[i].Groups["text"].Value.Split(new string[] { "\r", "\n" },
                        StringSplitOptions.RemoveEmptyEntries));
                }
            }
            _info = new InfoCollection(infos);
            Invoke(new Action(AddInfosToForm));
        }

        private void AddInfosToForm()
        {
            foreach (var i in _info)
            {
                var l = CreateInfoControl(i);

                if (i is Directory)
                {
                    var gb = Controls.OfType<GroupBox>().FirstOrDefault(g =>
                        g.Text == (i as Directory).chapter);
                    if(gb == null)
                    {
                        gb = new GroupBox
                        {
                            Text = (i as Directory).chapter,
                            Dock = DockStyle.Top,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            AutoSize = true,
                        };
                        gb.Click += (sender, es) =>
                        {
                            var temp = (sender as GroupBox);
                            if (temp.Height != 15)
                            {
                                temp.AutoSize = false;
                                temp.Height = 15;
                            }
                            else
                                temp.AutoSize = true;
                        };
                        tabPage1.Controls.Add(gb);
                        gb.BringToFront();
                    }
                    gb.Controls.Add(l);
                }
                else if (i is Exercise)
                    tabPage2.Controls.Add(l);
                else
                    tabPage3.Controls.Add(l);
                l.BringToFront();
            }
            UseWaitCursor = false;
        }

        private Label CreateInfoControl(Info info)
        {
            var l = new Label
            {
                Text = info.topic,
                Dock = DockStyle.Top,
            };
            l.Click += (s, e) =>
            {
                var form = new Form
                {
                    Text = info.topic,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = SystemColors.Window,
                    MinimumSize = new Size(300, 200),
                    AutoScroll = true,
                    Padding = new Padding(5),
                };
                foreach (var t in info.text)
                {
                    var tb = new TextBox
                    {
                        BorderStyle = BorderStyle.None,
                        Dock = DockStyle.Top,
                        Multiline = true,
                    };
                    if (t[0] == '#')
                    {
                        tb.Text = t.Replace("#", "   ");
                        tb.Font = new Font("Courier New", tb.Font.Size);
                        tb.BackColor = SystemColors.Control;
                    }
                    else
                        tb.Text = t;
                    tb.KeyPress += (sender, ea) => ea.Handled = true;
                    form.Controls.Add(tb);
                    tb.BringToFront();
                }
                if(info is Exercise)
                    AddSolution(info, form);
                form.Show(this);
            };
            return l;
        }

        private static void AddSolution(Info info, Form form)
        {
            var gb = new GroupBox
            {
                Text = "Решение",
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Height = 15,
                Dock = DockStyle.Top,
            };
            gb.Click += (sender, ea) =>
            {
                var temp = (sender as GroupBox);
                if (temp.Height != 15)
                {
                    temp.AutoSize = false;
                    temp.Height = 15;
                }
                else
                    temp.AutoSize = true;
            };

            var tb = new TextBox
            {
                Text = (info as Exercise).solution,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Top,
                Multiline = true,
                BackColor = SystemColors.Control,
            };
            tb.Height = (int)((info as Exercise).solution.Split(new string[]
                { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Length *
                (tb.Font.Size * SIZE_MULTIPLER));
            tb.Font = new Font("Courier New", tb.Font.Size);
            tb.KeyPress += (sender, ea) => ea.Handled = true;
            gb.Controls.Add(tb);

            form.Controls.Add(gb);
            gb.BringToFront();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            searchResult.Controls.Clear();
            if (searchBox.Text.Length < 3)
                return;

            foreach (var info in _info.Where(i => 
                i.topic.ToLower().Contains(searchBox.Text.ToLower()) || 
                i.text.Any(t => t.ToLower().Contains(searchBox.Text.ToLower()))))
            {
                searchResult.Controls.Add(CreateInfoControl(info));
                Application.DoEvents();
            }
        }
    }
}
