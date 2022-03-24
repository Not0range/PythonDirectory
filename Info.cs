using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonDirectory
{
    abstract class Info
    {
        public string topic;
        public string[] text;

        protected Info(string topic, string[] text)
        {
            this.topic = topic;
            this.text = text;
        }
    }

    interface ICheck
    {
        event Action<Info> CheckedChanged;
        bool IsChecked { get; set; }
    }

    class Directory : Info
    {
        public string chapter;

        public Directory(string topic, string[] text, string chapter) : base(topic, text)
        {
            this.chapter = chapter;
        }
    }

    class Exercise : Info, ICheck
    {
        public string solution;
        bool isChecked;
        Action<Info> checkedChanged;

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                if (checkedChanged != null)
                    checkedChanged(this);
            }
        }

        public event Action<Info> CheckedChanged
        {
            add
            {
                checkedChanged += value;
            }
            remove
            {
                checkedChanged -= value;
            }
        }

        public Exercise(string topic, string[] text, string solution) : base(topic, text)
        {
            this.solution = solution;
        }
    }

    class Example : Info, ICheck
    {
        bool isChecked;
        Action<Info> checkedChanged;

        public Example(string topic, string[] text) : base(topic, text) { }

        public bool IsChecked 
        {
            get
            {
                return isChecked;
            } 
            set
            {
                isChecked = value;
                if (checkedChanged != null)
                    checkedChanged(this);
            }
        }

        public event Action<Info> CheckedChanged
        {
            add
            {
                checkedChanged += value;
            }
            remove
            {
                checkedChanged -= value;
            }
        }
    }
}
