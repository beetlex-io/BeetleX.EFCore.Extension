using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.EFCore.Extension
{
    public class Region
    {
        public Region()
        {
        }
        public Region(int pageindex, int size)
        {
            Size = size;
            Start = pageindex * size;
        }
        public int Start
        {
            get;
            set;
        }
        public int Size
        {
            get;
            set;
        }
    }
}
