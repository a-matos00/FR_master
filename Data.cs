using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Data
    {
        int index = 0;
        int offset = 1;
        int factor = 1;
        int value = 0;
        int size = 2;
        String name = "";

        public Data(int a_index, int a_size, int a_factor, int a_offset)
        {
            index = a_index;
            size = a_size;
            factor = a_factor;
            offset = a_offset;
        }
    }

    class dataHandler
    {
        Data[] canDataList;

        public void storeNewData(int a_index, int a_size, int a_factor, int a_offset)
        {
            for(int i = 0; i < canDataList.Length; i++)
            {
                canDataList = new Data[canDataList.Length + 1];
                canDataList[i] = new Data(a_index, a_size, a_factor, a_offset);
            }
        }
    }
}
