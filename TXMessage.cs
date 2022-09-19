using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class TXmessage
    {
        public uint id = 0;
        uint dlc = 0;
        public byte[] data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public uint interval = 10; //ms
        public bool status = false;
        public messageSelectButton msgButton;

        public TXmessage()
        {
            msgButton = new messageSelectButton();
            msgButton.parentReference = this;
        }

        public void setId(uint id) { this.id = id; }
        public uint getId() { return id; }
    }
}
