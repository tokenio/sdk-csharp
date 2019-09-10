using System.Collections.Generic;

namespace Tokenio
{
    public class PagedList<T>
    {
        public PagedList(IList<T> list, string offset)
        {
            List = list;
            Offset = offset;
        }

        public IList<T> List { get; }

        public string Offset { get; }
    }
}
