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
using static WpfApp1.TransmitWindow;
using System.Text.RegularExpressions;
using static WpfApp1.MainWindow;

namespace WpfApp1
{
    public partial class TransmitWindow : Window
    {      
        const int TX_MESSAGE_COUNT = 6;
        const int DATA_COLUMN_INDEX = 2;    //column index of firtst data box in main grid
        const int CAN_MSG_DATA_LENGTH = 8;
        const int DATA_BOX_BYTE_LENGTH = 2;
        const int MSG_ID_LENGTH = 3;

        public static canDevice transmitDevice;

        TXmessage[] TXmessages = new TXmessage[TX_MESSAGE_COUNT];
        idTextBox[] idTextBoxes = new idTextBox[TX_MESSAGE_COUNT];
        static dataByteTextBox[,] dataByteTextBoxes = new dataByteTextBox[TX_MESSAGE_COUNT, CAN_MSG_DATA_LENGTH];
        dataGrid[] dataGrids = new dataGrid[TX_MESSAGE_COUNT];
        messageSelectButton[] messageSelectButtons = new messageSelectButton[TX_MESSAGE_COUNT];

        public TransmitWindow(canDevice a_device)
        {
            if (a_device == null)
            {
                throw new ArgumentNullException();
            }
            else
            {
                InitializeComponent();
                initTXmessages();
                setupIdTextBoxes();
                setupDataGrids();
                generateDataIndexes();
                generateMessageButtons();
            }
        }

        public void initDataBoxes()
        {
            for(int i = 0; i < TX_MESSAGE_COUNT; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    dataByteTextBoxes[i, j] = new dataByteTextBox();
                    dataByteTextBoxes[i, j].row = i;
                    dataByteTextBoxes[i, j].column = j;
                }
            }
        }

        public void setupDataGrids()
        {
            initDataBoxes();
            for (int i = 0; i < dataGrids.Length; i++)
            {
                for (int j = 0; j < 8; j++) {
                    Grid.SetColumn(dataByteTextBoxes[i,j], j + DATA_COLUMN_INDEX);
                    Grid.SetRow(dataByteTextBoxes[i,j], i);
                    inputGrid.Children.Add(dataByteTextBoxes[i,j]);
                    dataByteTextBoxes[i,j].Text = "00";
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
                Grid.SetRow(idTextBoxes[i], i);
                idTextBoxes[i].row = i;
                inputGrid.Children.Add(idTextBoxes[i]);
            }
        }

        void initTXmessages()
        {
            for (int i = 0; i < TX_MESSAGE_COUNT; i++)
            {
                TXmessages[i] = new TXmessage();
            }
        }

        public static void clearInputFieldOnClick(object sender, RoutedEventArgs e)
        {
            TextBox temp = null;
            temp = (TextBox)sender;
            temp.Text = "";
        }

        public static void dataBoxInputHandler(object sender, RoutedEventArgs e)
        {
            dataByteTextBox dataBox = (dataByteTextBox)sender;
            if (new Regex(@"^[A-Fa-f0-9]*$").IsMatch(dataBox.Text) && dataBox.Text.Length < 3)
            {
                if (dataBox.Text.Length == 2 && dataBox.column != 7)
                {
                    Keyboard.Focus(dataByteTextBoxes[dataBox.row, dataBox.column + 1]);
                    dataByteTextBoxes[dataBox.row, dataBox.column + 1].Text = "";
                }
            }
            else
            {
                dataBox.Text = dataBox.Text.Remove(dataBox.Text.Length - 1);
            }
            if (dataBox.Text.Length > DATA_BOX_BYTE_LENGTH)
            {
                dataBox.Text = dataBox.Text.Remove(dataBox.Text.Length - 1);
            }

                
        }

        public static void idBoxInputHandler(object sender, RoutedEventArgs e)
        {
            idTextBox inputField = (idTextBox)sender;
            if (new Regex(@"^[A-Fa-f0-9]*$").IsMatch(inputField.Text) && inputField.Text.Length < 4)
            {
                if (inputField.Text.Length == 3)
                {
                    Keyboard.Focus(dataByteTextBoxes[inputField.row, 0]);
                    dataByteTextBoxes[inputField.row, 0].Text = "";
                }
            }
            else
            {
                inputField.Text = inputField.Text.Remove(inputField.Text.Length - 1);
            }
            if (inputField.Text.Length > MSG_ID_LENGTH)
            {
                inputField.Text = inputField.Text.Remove(inputField.Text.Length - 1);
            }

        }

        public static void dataBoxLostFocusHandler(object sender, RoutedEventArgs e)
        {
            dataByteTextBox dataBox = null;
            dataBox = (dataByteTextBox)sender;
            
            if(dataBox.Text.Length == 0)
            {
                dataBox.Text = "00";
            }
        }

        public static void idBoxLostFocusHandler(object sender, RoutedEventArgs e)
        {
            idTextBox dataBox = null;
            dataBox = (idTextBox)sender;

            if (dataBox.Text.Length == 0)
            {
                dataBox.Text = "000";
            }
        }

        public void SetCANDevice(canDevice device)
        {
            transmitDevice = device;
        }

        public void generateMessageButtons()
        {
            for (int i = 0; i < TX_MESSAGE_COUNT; i++)
            {
                TXmessages[i].msgButton.Click += handleMessageButtonClick;
                TXmessages[i].msgButton.Content = "Message " + (i + 1).ToString();
                Grid.SetColumn(TXmessages[i].msgButton, 0);
                Grid.SetRow(TXmessages[i].msgButton, i);

                inputGrid.Children.Add(TXmessages[i].msgButton);
            }
        }

        public void buttonColorChange(messageSelectButton clickedButton)
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
            messageSelectButton clickedButton = null;
            clickedButton = (messageSelectButton)sender;

            if(clickedButton.parentReference.status == false)
            {
                clickedButton.parentReference.status = true;
            }
            else
            {
                clickedButton.parentReference.status = false;
            }

            buttonColorChange(clickedButton);
        }

        public void Transmit(object sender, RoutedEventArgs e)
        {
            String tempString;

            for (int i = 0; i < TX_MESSAGE_COUNT; i++)
            {
                if (TXmessages[i].status == false) { }

                else
                {
                    TXmessages[i].id = Convert.ToUInt32(idTextBoxes[i].Text, 16);

                    for (int j = 0; j < 8; j++)
                    {
                        tempString = dataByteTextBoxes[i, j].Text;
                        byte value = Convert.ToByte(tempString, 16);
                        TXmessages[i].data[j] = value;

                    }
                    transmitDevice.TransmitMessage(TXmessages[i].id, TXmessages[i].data, 8);
                }
            }
        }

        public void generateDataIndexes()
        {
            for (int i = 0; i < 8; i++) {
                Label temp = new Label();
                temp.Width = 30;
                temp.Height = 50;
                temp.Content = i.ToString();
                temp.BorderThickness = new Thickness(1);
                temp.BorderBrush = new SolidColorBrush(Colors.Black);
                temp.VerticalContentAlignment = VerticalAlignment.Center;
                temp.HorizontalContentAlignment= HorizontalAlignment.Center;
                Grid.SetColumn(temp, i + DATA_COLUMN_INDEX);
                Grid.SetRow(temp, 0);
                headerGrid.Children.Add(temp);
            }
            
        }
        
    }

    public class repetitionInput : TextBox
    {
        public repetitionInput()
        {
            this.Width = 50;
            this.Height = 50;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
        }
    }

    public class idTextBox : TextBox
    {
        public int row = 0;
        public int column = 0;
        public idTextBox()
        {
            this.Width=30;
            this.Height = 50;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
            this.PreviewMouseDown += TransmitWindow.clearInputFieldOnClick;
            this.KeyUp += TransmitWindow.idBoxInputHandler;
            this.LostFocus += TransmitWindow.idBoxLostFocusHandler;
        }
    }

    public class dataByteTextBox : TextBox
    {
        public int row = 0;
        public int column = 0;
        public dataByteTextBox()
        {
            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
            this.PreviewMouseDown += TransmitWindow.clearInputFieldOnClick;
            this.KeyUp += TransmitWindow.dataBoxInputHandler;
            this.LostFocus += TransmitWindow.dataBoxLostFocusHandler;
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

    public class messageSelectButton : Button
    {
        public TXmessage parentReference = null;
        public messageSelectButton()
        {
            this.Width = 100;
            this.Background = Brushes.Red;
        }
    }

    
}
