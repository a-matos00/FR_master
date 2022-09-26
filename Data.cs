using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Data
    {
        int offset = 1;
        int factor = 1;
        int value = 0;
        String name = "";

        public Data(string a_name, int a_factor, int a_offset)
        {
            name = a_name;
            factor = a_factor;
            offset = a_offset;
        }
    }
}
