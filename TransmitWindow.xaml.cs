using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace WpfApp1
{

    public partial class TransmitWindow : Window
    {
        public canDevice transmitDevice;
        const int TX_MESSAGE_COUNT = 6;
        const int DATA_COLUMN_INDEX = 2;

        TXmessage[] TXmessages = new TXmessage[TX_MESSAGE_COUNT];
        idTextBox[] idTextBoxes = new idTextBox[TX_MESSAGE_COUNT];
        dataByteTextBox[] dataByteTextBoxes = new dataByteTextBox[60];
        dataGrid[] dataGrids = new dataGrid[TX_MESSAGE_COUNT];

        public TransmitWindow()
        {
            InitializeComponent();
            initTXmessages();
            setupIdTextBoxes();
            setupDataGrids();
            generateDataIndexes();

        }

        public void setupDataGrids()
        {
            for (int i = 0; i < dataGrids.Length; i++)
            {
                dataGrids[i] = new dataGrid();
                Grid.SetColumn(dataGrids[i], DATA_COLUMN_INDEX);
                Grid.SetRow(dataGrids[i], i + 2);
                windowGrid.Children.Add(dataGrids[i]);
            
                dataGrids[i].RowDefinitions.Add(new RowDefinition());

                for (int j = 0; j < 8; j++) {
                    dataGrids[i].ColumnDefinitions.Add(new ColumnDefinition());
                    dataByteTextBoxes[j] = new dataByteTextBox();
                    Grid.SetColumn(dataByteTextBoxes[j], j);
                    Grid.SetRow(dataByteTextBoxes[j], 0);
                    dataGrids[i].Children.Add(dataByteTextBoxes[j]);
                    dataByteTextBoxes[j].Text = "00";
                }
            }
        }
        public void setupIdTextBoxes()
        {
            for (int i = 0; i < idTextBoxes.Length; i++)
            {
                idTextBoxes[i] = new idTextBox();
                idTextBoxes[i].Text = TXmessages[i].id.ToString("X3");
                Grid.SetColumn(idTextBoxes[i], 1);
                Grid.SetRow(idTextBoxes[i], i + 2);
                windowGrid.Children.Add(idTextBoxes[i]);

            }
        }

        void initTXmessages()
        {
            for (int i = 0; i < TX_MESSAGE_COUNT; i++)
            {
                TXmessages[i] = new TXmessage();
            }
        }

        public void SetCANDevice(canDevice device)
        {
            transmitDevice = device;
        }


        public void colorChange(Button clickedButton)
        {
            if (clickedButton.Background == Brushes.Lime)
                clickedButton.Background = Brushes.Red;
            else
            {
                clickedButton.Background = Brushes.Lime;
            }
        }

        public void handleMessageButtonClick(object sender, RoutedEventArgs e)
        {
            Button clickedButton = null;
            clickedButton = (Button)sender;
            colorChange(clickedButton);
        }

        public void Transmit(object sender, RoutedEventArgs e)
        {
            String tempString;

            for (int i = 0; i < TX_MESSAGE_COUNT; i++)
            {
                if(TXmessages[i].status == false) { break; }

                for(int j = 0; j < 8; j++)
                {
                    tempString = dataByteTextBoxes[j].Text;
                    uint value = Convert.ToByte(tempString, 16);
                    TXmessages[i].data[j] = value;
                    Trace.WriteLine(value);
                }

                //transmitDevice.TransmitMessage(TXmessages[i].id, TXmessages[i].data);
            }
        }

        public void generateDataIndexes()
        {
            Grid indexGrid = new Grid();
            indexGrid.Width = 240;
            indexGrid.Height = 50;

            indexGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(indexGrid, 2);
            Grid.SetRow(indexGrid, 1);
            windowGrid.Children.Add(indexGrid);


            for (int i = 0; i < 8; i++) {
                Label temp = new Label();
                temp.Width = 30;
                temp.Height = 50;
                temp.Content = i.ToString();
                temp.BorderThickness = new Thickness(1);
                temp.BorderBrush = new SolidColorBrush(Colors.Black);
                indexGrid.ColumnDefinitions.Add(new ColumnDefinition());
                temp.VerticalContentAlignment = VerticalAlignment.Center;
                temp.HorizontalContentAlignment= HorizontalAlignment.Center;
                Grid.SetColumn(temp, i);
                Grid.SetRow(temp, 0);
                indexGrid.Children.Add(temp);

            }
            
        }

        public class idTextBox : TextBox
        {
            public idTextBox()
            {
                this.VerticalAlignment = VerticalAlignment.Center;
                this.HorizontalAlignment = HorizontalAlignment.Center;
                this.HorizontalContentAlignment = HorizontalAlignment.Center;
                this.VerticalContentAlignment = VerticalAlignment.Center;
                this.Width = 50;
                this.Height = 50;
                //promjena
            }
        }

        public class dataByteTextBox : TextBox
        {
            public dataByteTextBox()
            {   
                this.HorizontalContentAlignment = HorizontalAlignment.Center;
                this.VerticalContentAlignment = VerticalAlignment.Center;
                this.Width = 30;
                this.Height = 50;
            }
        }

        public class dataGrid : Grid
        {
            public dataGrid()
            {
                this.Width = 240;
                this.Height = 50;
                
            }
        }
    }

    public class TXmessage
    {
        public uint id = 0;
        uint dlc = 0;
        public uint[] data = new uint[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public uint interval = 10; //ms
        public bool status = false;

        public TXmessage() { }

        public void setId(uint id) { this.id = id; }
        public uint getId() { return id; }
    }


}
