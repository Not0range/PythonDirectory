using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonDirectory
{
    class InfoCollection : IEnumerable<Info>
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

        public IEnumerator<Info> GetEnumerator()
        {
            return ((IEnumerable<Info>)list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }
}
