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

        public InfoCollection(Info[] infos)
        {
            list = new List<Info>(infos);
        }

        public Info this[int i]
        {
            get
            {
                return list[i];
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
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
