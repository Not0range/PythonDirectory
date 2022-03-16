using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonDirectory
{
    class InfoCollection
    {
        List<Info> list;
        public event Action GetInfo;

        public InfoCollection(Info[] infos)
        {
            list = new List<Info>(infos);
        }

        public Info this[int i]
        {
            get
            {
                GetInfo();
                return list[i];
            }
        }
    }
}
