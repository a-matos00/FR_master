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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;



namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        canDevice device = new canDevice();
       
        public MainWindow()
        {
            InitializeComponent();
            device.RxEvent += DisplayRxMsg;
            device.initCanDriver();
            device.rxThread = new Thread(new ThreadStart(device.RXThread));
            device.rxThread.Start();
        }

        public void OpenTransmitWindow(object sender, RoutedEventArgs e)
        {
            TransmitWindow senderWindow = new TransmitWindow();
            senderWindow.SetCANDevice(device);
            senderWindow.Show();
        }

        public void DisplayRxMsg(uint id, UInt64 timestamp, byte[] data)
        {
            String dataString = "";

            this.Dispatcher.Invoke(() =>
            {
                recieveDisplay.Text = id.ToString("X3") + " ";

                for (int i = 0; i < 8; i += 2) {
                dataString += Convert.ToHexString(data, i, 2);
                }

                recieveDisplay.Text += dataString;
            });
        }
    }

 

} 