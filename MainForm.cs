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
            WriteLog("Запуск программы");
            var worker = new BackgroundWorker();
            worker.DoWork += ReadInfos;
            worker.RunWorkerAsync();
        }

        private void LogGetInfo(Info info)
        {
            WriteLog("Открыта тема " + info.topic);
        }

        private void Complete_CheckedChanged(Info info)
        {
            WriteLog("Тема " + info.topic + " отмечана как " + ((info as ICheck).IsChecked ?
                "выполненная" : "не выполненная"));
        }

        private void WriteLog(string msg)
        {
            try
            {
                var writer = new StreamWriter("log.txt", true);
                writer.WriteLine("[{0}] {1}", 
                    DateTime.Now.ToString("dd.MM.yy HH:mm:ss"), msg);
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при записи журнала:\n" + ex.Message, 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReadInfos(object sender, DoWorkEventArgs e)
        {
            int[] ind;
            if (File.Exists("checked.txt"))
                ind = File.ReadAllText("checked.txt")
                    .Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => int.Parse(i)).ToArray();
            else
                ind = new int[0];

            string text = File.ReadAllText("dictionary.txt");
            var mc = Regex.Matches(text, @"@topic=(?<topic>[^;]+);" +
                @"(chapter=(?<chapter>[^;]+);)?(?<text>[^@\^]+)" +
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
                if(infos[i] is ICheck)
                {
                    if (ind.Contains(i))
                        (infos[i] as ICheck).IsChecked = true;
                    (infos[i] as ICheck).CheckedChanged += Complete_CheckedChanged;
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
                    var gb = tabPage1.Controls.OfType<GroupBox>().FirstOrDefault(g =>
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

                LogGetInfo(info);

                var form = new Form
                {
                    Text = info.topic,
                    StartPosition = FormStartPosition.CenterScreen,
                    BackColor = SystemColors.Window,
                    MinimumSize = new Size(300, 200),
                    AutoScroll = true,
                    Padding = new Padding(5),
                };
                form.ResizeEnd += Form_ResizeEnd;
                form.Show(this);
                form.SuspendLayout();
                Application.DoEvents();

                foreach (var t in info.text)
                {
                    var tb = new TextBox
                    {
                        BorderStyle = BorderStyle.None,
                        Dock = DockStyle.Top,
                        Multiline = true,
                        Margin = new Padding(5),
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
                    tb.MouseWheel += (sender, ea) =>
                    {
                        if (form.VerticalScroll.Value - ea.Delta < form.VerticalScroll.Minimum)
                        {
                            form.VerticalScroll.Value = form.VerticalScroll.Minimum;
                            return;
                        }
                        else if (form.VerticalScroll.Value - ea.Delta > form.VerticalScroll.Maximum)
                        {
                            form.VerticalScroll.Value = form.VerticalScroll.Maximum;
                            return;
                        }
                        form.VerticalScroll.Value -= ea.Delta;
                        form.VerticalScroll.Value -= ea.Delta;
                    };
                    form.Controls.Add(tb);
                    tb.BringToFront();
                    form.ResumeLayout();
                    tb.Height = (tb.GetLineFromCharIndex(tb.TextLength) + 1) *
                        tb.Font.Height;
                    form.SuspendLayout();
                    Application.DoEvents();
                }
                if (info is ICheck)
                {
                    var cb = new CheckBox
                    {
                        Text = "Выполнено",
                        Checked = (info as ICheck).IsChecked,
                        Dock = DockStyle.Top
                    };
                    cb.CheckedChanged += (sender, ea) => 
                        (info as ICheck).IsChecked = cb.Checked;
                    form.Controls.Add(cb);
                }
                if (info is Exercise)
                    AddSolution(info, form);
                form.ResumeLayout();
            };
            return l;
        }

        private void Form_ResizeEnd(object sender, EventArgs e)
        {
            foreach (var tb in (sender as Form).Controls.OfType<TextBox>())
                tb.Height = (tb.GetLineFromCharIndex(tb.TextLength) + 1) *
                           tb.Font.Height;
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
            tb.Font = new Font("Courier New", tb.Font.Size);
            tb.KeyPress += (sender, ea) => ea.Handled = true;
            gb.Controls.Add(tb);

            form.Controls.Add(gb);
            gb.BringToFront(); 
            tb.Height = (tb.GetLineFromCharIndex(tb.TextLength) + 1) *
                tb.Font.Height;
        }

        private void search_TextChanged(object sender, EventArgs e)
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

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var ind = new List<int>();
            for (int i = 0; i < _info.Count; i++)
                if ((_info[i] is ICheck) && (_info[i] as ICheck).IsChecked)
                    ind.Add(i);

            var writer = new StreamWriter("checked.txt", false);
            writer.WriteLine(string.Join(", ", ind.ToArray()));
            writer.Close();

            WriteLog("Завершение работы");
        }
    }
}
