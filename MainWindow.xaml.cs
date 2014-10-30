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
using System.IO;
using System.Windows.Threading;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 共通パラメータ

        const String INI_TAG_TARGET1 = "TargetString1";
        const String INI_TAG_TARGET2 = "TargetString2";
        const String INI_TAG_TARGET3 = "TargetString3";
        const String INI_FILE_NAME = "typing.ini";

        private String[] targetArray;

        private StringBuilder poolString;
        private double inputTime = 0.0f;
        private int curPos = 0;

        private int curPlayer = 0;      /* 0:1人目, 1:2人目, 2:3人目 */
        private int[] player_len;
        private double[] player_time;

        private DispatcherTimer disTimer;

        private bool isStarted = false;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            long tick = 100000;

            disTimer = new DispatcherTimer();
            disTimer.Interval = new TimeSpan(tick);
            disTimer.Tick += new EventHandler(distTimer_Tick);
            disTimer.Stop();

            this.poolString = new StringBuilder();
            this.player_len = new int[3];
            this.player_time = new double[3];
            initForm(true);
        }

        private bool initForm()
        {
            return this.initForm(false);
        }

        private bool initForm(bool isClearParams)
        {
            bool r_inf = false;
            try
            {
                if (isClearParams == true)
                {
                    this.poolString = new StringBuilder();
                    this.player_len = new int[3];
                    this.player_time = new double[3];
                    this.targetArray = null;

                    if (setTargetString() == true)
                    {
                        this.disTimer.Stop();
                        this.inputTime = 0.0f;
                        this.curPos = 0;

                        this.curPlayer = 0;
                        this.isStarted = false;
                    }
                }
                this.poolString.Clear();
                this.label1.FontSize = 65;
                this.label1.Content = "Hit\nSpace key!";
                this.label2.Content = "";
                this.label3.Content = "";
                this.label4.Content = "";

                r_inf = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }

        private bool setTargetString()
        {
            return this.readIniFile();
        }

        private bool readIniFile()
        {
            bool r_inf = false;

            try
            {
                StreamReader sr = new StreamReader(MainWindow.INI_FILE_NAME);
                String[] x = null;
                String[] y = null;
                String[] z = null;

                while (sr.EndOfStream != true)
                {
                    String rStr = sr.ReadLine();
                    rStr = rStr.Replace(" ", "");
                    if (rStr.IndexOf("#") == 0)
                    {
                        /* 先頭が # の行はコメントとして読み飛ばす */
                        break;
                    }
                    else if (rStr.IndexOf(MainWindow.INI_TAG_TARGET1) != -1)
                    {
                        String[] t = rStr.Split('=');
                        x = new String[t[1].Length];
                        x = t[1].Split(',');
                    }
                    else if (rStr.IndexOf(MainWindow.INI_TAG_TARGET2) != -1)
                    {
                        String[] t = rStr.Split('=');
                        y = new String[t[1].Length];
                        y = t[1].Split(',');
                    }
                    else if (rStr.IndexOf(MainWindow.INI_TAG_TARGET3) != -1)
                    {
                        String[] t = rStr.Split('=');
                        z = new String[t[1].Length];
                        z = t[1].Split(',');
                    }
                }

                if ((x != null) && (y != null) && (z != null))
                {
                    int len = x.Length + y.Length + z.Length;
                    int pos = 0;
                    this.targetArray = new String[len];
                    x.CopyTo(this.targetArray, pos);
                    pos += x.Length;
                    this.player_len[0] = pos;
                    y.CopyTo(this.targetArray, pos);
                    pos += y.Length;
                    this.player_len[1] = pos;
                    z.CopyTo(this.targetArray, pos);
                    pos += z.Length;
                    this.player_len[2] = pos;
                    r_inf = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return r_inf;
        }

        private bool checkNextPlayer(int curPos)
        {
            bool r_inf = false;

            if (curPos == this.player_len[curPlayer])
            {
                this.disTimer.Stop();
                this.isStarted = false;
                this.player_time[curPlayer] = this.inputTime;
                this.inputTime = 0.0f;
                if (curPlayer == 2)
                {
                    double total_time = this.player_time[0] + this.player_time[1] + this.player_time[2];
                    StringBuilder sb = new StringBuilder();
                    sb.Append("1人目：{0:F2}秒\n");
                    sb.Append("2人目：{1:F2}秒\n");
                    sb.Append("3人目：{2:F2}秒\n");
                    sb.Append(" 合計：{3:F2}秒でした");
                    String tex = String.Format(sb.ToString(),
                                               this.player_time[0],
                                               this.player_time[1],
                                               this.player_time[2],
                                               total_time);
                    System.Windows.MessageBox.Show(tex, "結果発表");
                    initForm(true);
                }
                else
                {
                    String tex = String.Format("{0}人目\n{1:F2}秒でした",
                                               this.curPlayer + 1, this.player_time[this.curPlayer]);
                    System.Windows.MessageBox.Show(tex, "中間結果");
                    initForm();
                    curPlayer++;
                }
                r_inf = true;
            }

            return r_inf;
        }

        private void getKeyInput(String enter)
        {
            if (enter.Equals(targetArray[curPos]) == true)
            {
                this.label3.Content = "";
                this.poolString.Append(targetArray[curPos]);

                if (checkNextPlayer(++curPos) == false)
                {
                    this.label1.Content = targetArray[curPos];
                    this.label2.Content = this.poolString.ToString();
                }
            }
            else
            {
                this.label3.Content = "Miss!!";
            }
        }

        private void distTimer_Tick(object sender, EventArgs e)
        {
            this.inputTime += 0.01;
            label4.Content = String.Format("{0:F2}",this.inputTime);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            String enter = e.Key.ToString();
            if (isStarted == true)
            {
                this.getKeyInput(enter);
            }
            else
            {
                if (enter.Equals("Space") == true)
                {
                    this.label1.FontSize = 300;
                    this.label1.Content = targetArray[curPos];
                    this.isStarted = true;
                    this.disTimer.Start();
                }
            }
            Debug.WriteLine(enter);
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.3ca.co.jp");
        }
    }
}
