using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for BusMonitorWindow.xaml
    /// </summary>
    ///   
    public partial class BusMonitorWindow : Window
    {
        const int MAIN_GRID_ROW_COUNT = 3;
        const int MAIN_GRID_COLUMN_COUNT = 2;

        public static canDevice device;
        private System.Data.DataSet dataSet;
        RXmessage[] RXmessages;

        DataGridTextColumn idColumn;
        DataGridTextColumn dataColumn;
        DataGridTextColumn timestampColumn;
        public BusMonitorWindow(canDevice a_device)
        {
            if (a_device == null)
            {
                throw new ArgumentNullException();
            }
            else
            {
                InitializeComponent();
                initMainGrid();
                device = a_device;
                device.RxEvent += DisplayRxMsg;
                device.initCanDriver();
                device.rxThread = new Thread(new ThreadStart(device.RXThread));
                device.rxThread.Start();
            }
        }

        public void DisplayRxMsg(uint id, string timestamp, byte[] data)
        {

            this.Dispatcher.Invoke(() =>
            {

                if (RXmessages == null)
                {
                    Array.Resize(ref RXmessages, 1);
                    RXmessages[0] = new RXmessage(timestamp, id);
                    RXmessages[0].id = id;
                    RXmessages[0].timestamp = timestamp;
                    RXmessages[0].data = data;
                    monitorDataGrid.ItemsSource = RXmessages;
                    monitorDataGrid.Items.Refresh();
                    return;
                }

                for (int i = 0; i < RXmessages.Length; i++)
                {        
                    if(id == RXmessages[i].id)
                    {
                        RXmessages[i] = new RXmessage(timestamp, id);
                        RXmessages[i].id = id;
                        RXmessages[i].timestamp = timestamp;
                        RXmessages[i].data = data;
                        monitorDataGrid.Items.Refresh();
                        return;
                    }
                }
                Array.Resize(ref RXmessages, RXmessages.Length + 1);
                RXmessages[RXmessages.Length - 1] = new RXmessage(timestamp, id);
                RXmessages[RXmessages.Length - 1].id = id;
                RXmessages[RXmessages.Length - 1].timestamp = timestamp;
                RXmessages[RXmessages.Length - 1].data = data;
                monitorDataGrid.ItemsSource = RXmessages;
                monitorDataGrid.Items.Refresh();
            });

            
        }

        public void initMainGrid()
        {
            idColumn = new DataGridTextColumn();
            dataColumn =  new DataGridTextColumn();
            timestampColumn = new DataGridTextColumn();
            
        }

        public void SetCANDevice(canDevice a_device)
        {
            device = a_device;
        }
    }

    class RXmessage
    {
        public uint id = 0;
        uint dlc { get; set; }
        public string timestamp = "";
        public byte[] data = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public RXmessage(string a_timestamp, uint a_id)
        {
            timestamp = a_timestamp;
            id = a_id;
        }

        public string TIMESTAMP
        {
            get
            {
                return timestamp;
            }
        }
        public string ID
        {
            get
            {
                return id.ToString("X3"); ;
            }
        }

        public String DATA
        {
            get
            {
                string s = BitConverter.ToString(data);
                Trace.WriteLine(s);
                return s;
            }
        }


    }

}
