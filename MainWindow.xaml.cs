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
using System.ComponentModel;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static canDevice device = new canDevice();
        TransmitWindow transmitWindow = null;
        BusMonitorWindow monitorWindow = null;
        DiagnosticWindow diagnosticWindow = null;

        public MainWindow()
        {
            InitializeComponent();      
        }

        public void OnMonitorWindowClosing(object sender, CancelEventArgs e)
        {
            monitorWindow = null;
        }

        public void OnTransmitWindowClosing(object sender, CancelEventArgs e)
        {
            transmitWindow = null;
        }

        public void OnDiagnosticWindowClosing(object sender, CancelEventArgs e)
        {
            diagnosticWindow = null;
        }

        //handle button click
        public void OpenTransmitWindow(object sender, RoutedEventArgs e)   
        {
            if (transmitWindow == null) //prevents opening multiple windows
            {
                transmitWindow = new TransmitWindow(device);
                transmitWindow.SetCANDevice(device);
                transmitWindow.Show();
                transmitWindow.Closing += OnTransmitWindowClosing;
            }
        }

        public void OpenMonitorWindow(object sender, RoutedEventArgs e)
        {
            if (monitorWindow == null) //prevents opening multiple windows
            {
                monitorWindow = new BusMonitorWindow(device);
                monitorWindow.Show();
                monitorWindow.Closing += OnMonitorWindowClosing;
            }
        }

        public void OpenDiagnosticWindow(object sender, RoutedEventArgs e)
        {
            if (diagnosticWindow == null) //prevents opening multiple windows
            {
                diagnosticWindow = new DiagnosticWindow();
                diagnosticWindow.Show();
                diagnosticWindow.Closing += OnDiagnosticWindowClosing;
            }
        }
    }

 

} 