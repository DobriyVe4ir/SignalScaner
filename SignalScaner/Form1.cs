using System;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZedGraph;

namespace SignalScaner
{
    public partial class Form1 : Form
    {
        static GraphPane graphPane = null; 
        SerialPort port = new SerialPort();
        bool mode = false;
        int invert = 1; 

        public Form1()
        {
            InitializeComponent();
            
            graphInit();

            refreshComboBoxPorts();

            comboBoxPorts.SelectedIndex = 0;
            comboBoxBaudRate.SelectedIndex = 1;
            timerRefreshRate.Enabled = true;
        }

        bool checkConnection() {
            //запрос
            JObject json = new JObject();
            json.Add("command", "check");
            port.WriteLine(json.ToString(Formatting.None));

            //Ответ
            System.Threading.Thread.Sleep(1000);
            if (port.BytesToRead == 0) return false;
            string ans = port.ReadTo("\n");
            Console.WriteLine("Check ansver: "+ans);
            if (ans.Trim() != "Checked!") return false;
            return true;
        }

        void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = ((SerialPort)sender).ReadExisting(); // Прочитать полученные данные
            Console.WriteLine("Полученные данные: " + data);
            try
            {
                JObject jsonObject = JObject.Parse(data);
                int time = Convert.ToInt32(jsonObject["t"]);
                JArray valuesArray = jsonObject["v"].ToObject<JArray>();
                int[] values = valuesArray.ToObject<int[]>();
                if (invert == 1) {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] == 1) values[i] = 0;
                        else values[i] = 1;
                    }
                }

                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == 0) values[i] = 1;
                    else values[i] = 0;
                    graphPane.CurveList[i].AddPoint(time, values[i]+i*2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при разборе JSON: " + ex.Message);
            }
        }

        void refreshComboBoxPorts() {
            string[] ports = SerialPort.GetPortNames();
            comboBoxPorts.Items.Clear();
            comboBoxPorts.Items.AddRange(ports);
        }

        void graphInit() {
            graphPane = zedGraphControl1.GraphPane;
            graphPane.Title.Text = "Scaner";
            graphPane.XAxis.Title.Text = "Time, t";
            graphPane.YAxis.Title.Text = "Pins";

            if (graphPane.CurveList.Count == 0) {
                graphPane.AddCurve("PIN2", new PointPairList() { new PointPair(0, 0) }, Color.Red, SymbolType.None);
                graphPane.AddCurve("PIN3", new PointPairList() { new PointPair(0, 0) }, Color.Orange, SymbolType.None);
                graphPane.AddCurve("PIN4", new PointPairList() { new PointPair(0, 0) }, Color.YellowGreen, SymbolType.None);
                graphPane.AddCurve("PIN5", new PointPairList() { new PointPair(0, 0) }, Color.Green, SymbolType.None);
                graphPane.AddCurve("PIN6", new PointPairList() { new PointPair(0, 0) }, Color.Blue, SymbolType.None);
                graphPane.AddCurve("PIN7", new PointPairList() { new PointPair(0, 0) }, Color.Purple, SymbolType.None);
            }
            else {
                graphPane.CurveList.ForEach(curve => curve.Clear());
            }
            graphPane.AxisChange();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            port.Close();
            port.DataReceived -= SerialPortDataReceived;
            port.Encoding = Encoding.UTF8;
            port.PortName = comboBoxPorts.SelectedItem.ToString(); // Имя COM-порта
            port.BaudRate = Convert.ToInt32(comboBoxBaudRate.SelectedItem.ToString()); // Скорость передачи данных
            port.Parity = Parity.None; // Паритет
            port.DataBits = 8; // Биты данных
            port.StopBits = StopBits.One; // Биты остановки
            port.Open(); // Открыть порт

            if (checkConnection())
            {
                port.DataReceived += SerialPortDataReceived;
                labelStatus.Text = "Connected";
                labelStatus.ForeColor = Color.Lime;
                graphInit();
            }
            else
            {
                labelStatus.Text = "Not connected";
                labelStatus.ForeColor = Color.Crimson;
            }

        }

        private void buttonTranslateSignal_Click(object sender, EventArgs e)
        {
            if (labelStatus.Text != "Connected") return;

            JObject json = new JObject();
            json.Add("command", "gv");
            mode = true;
            port.WriteLine(json.ToString(Formatting.None));
            timerRefreshRate.Enabled = true;

        }

        private void buttonStopTranslate_Click(object sender, EventArgs e)
        {
            if (labelStatus.Text != "Connected") return;

            JObject json = new JObject();
            json.Add("command", "ngv");
            mode = false;
            port.WriteLine(json.ToString(Formatting.None));
            timerRefreshRate.Enabled = false;
        }

        private void timerRefreshRate_Tick(object sender, EventArgs e)
        {
            LineItem tmp = (LineItem) graphPane.CurveList[0];
            if (tmp.Points.Count != 0) {
                double curSize = graphPane.XAxis.Scale.Max - graphPane.XAxis.Scale.Min;

                graphPane.XAxis.Scale.Max = tmp.Points[tmp.Points.Count - 1].X + curSize * 0.1;
                graphPane.XAxis.Scale.Min = graphPane.XAxis.Scale.Max - curSize;
            }
                
            zedGraphControl1.Refresh();
        }

        private void buttonInvert_Click(object sender, EventArgs e)
        {
            invert*=-1;
        }
    }
}
