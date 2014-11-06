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
using Typing;

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer disTimer;
        private typing m_typing = null;

        private byte m_bgRatio = 0;
        private int m_changeTimingCount = 0;
        private bool isGameOver = false;

        public MainWindow()
        {
            InitializeComponent();
            this.MainForm.Background = new SolidColorBrush(Colors.AliceBlue);

            typingParams tP = typingParams.getInstance();

            this.label3.Visibility = Visibility.Hidden;
            this.label3.Content = "Miss!!";

            this.m_typing = new typing();
            initForm(true);

            this.skel.Visibility = Visibility.Hidden;
            this.yamochi.Visibility = Visibility.Hidden;
            this.label5.Content = tP.getGameModeString();
            this.label6.Content = "Easy";
            this.label7.Content = "Normal";
            this.label8.Content = "Champion";
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
                typingParams tP = typingParams.getInstance();
                if (isClearParams == true)
                {
                    long tick = tP.getTick();
                    int numOfplay = tP.getNumberOfPlayer();

                    /* 古いタイマはnullクリアしてGC対象にする */
                    if (disTimer != null) disTimer = null;

                    disTimer = new DispatcherTimer();
                    disTimer.Interval = new TimeSpan(tick);
                    disTimer.Tick += new EventHandler(distTimer_Tick);
                    disTimer.Stop();

                    if (this.m_typing.getParamsFromInifile() == true)
                    {
                        if (this.m_typing.setTargetString() == true)
                        {
                            this.disTimer.Stop();
                        }
                    }
                    tP.incPlayCount();
                }
                tP.clearPoolString();
                this.label1.FontSize = 60;

                this.label1.Content = tP.getCurrentPlayerString();
                if(tP.getGameMode() == typingParams.GAME_MODE.CHAMPION_MODE)
                {
                    this.label1.Foreground = new SolidColorBrush(Colors.Red);
                    this.label1.FontWeight = FontWeights.ExtraBold;
                    this.label1.Content += " Try!!!\nHit Space key!";
                }
                else
                {
                    this.label1.Foreground = new SolidColorBrush(Colors.Blue);
                    this.label1.FontWeight = FontWeights.Normal;
                    this.label1.Content += " Player\nHit Space key!";
                }
                this.label2.Content = "";
                this.label4.Content = "";

                r_inf = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }

        private void distTimer_Tick(object sender, EventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            tP.incInputTime();
            label4.Content = String.Format("{0:F2}", tP.getInputTime());

            if (tP.getGameMode() == typingParams.GAME_MODE.CHAMPION_MODE)
            {
                /* xx秒(default:30)でタイムオーバー */
                if (tP.getInputTime() >= tP.getTimeLimit())
                {
                    this.disTimer.Stop();

                    /* 動作上 30.01秒で止まって見栄えが良くないので */
                    /* 見せかけだけ 30秒に見える様に調整            */
                    label4.Content = String.Format("{0:F2}", Convert.ToDouble(tP.getTimeLimit()));

                    this.label3.FontSize = 60;
                    this.label3.Content = "GAME\nOVER";

                    /* プレイ回数３回につき１回は骸骨じゃなく矢持さんを表示 */
                    if((tP.getPlayCount() % 3) == 0){
                        this.yamochi.Visibility = Visibility.Visible;
                    }
                    else{
                        this.skel.Visibility = Visibility.Visible;
                    }

                    this.label3.Visibility = Visibility.Visible;
                    this.isGameOver = true;

                }

                /* 0.1秒ごとに色を変える */
                if(++m_changeTimingCount == 10){
                    m_changeTimingCount = 0;
                    m_bgRatio = (m_bgRatio < 255) ? Convert.ToByte(++m_bgRatio) : Convert.ToByte(255);
                    byte b = 0xFF;
                    byte bd = Convert.ToByte(b - m_bgRatio);
                    this.MainForm.Background
                        = new SolidColorBrush(Color.FromArgb(b, b, bd, bd));
                
                }
            }

        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            String enter = e.Key.ToString();
            
            Debug.WriteLine(enter);

            if (this.isGameOver == true)
            {
                if ((enter.Equals("Space") == true) || (enter.Equals("Eacape") == true))
                {
                    this.isGameOver = false;
                    this.skel.Visibility = Visibility.Hidden;
                    this.yamochi.Visibility = Visibility.Hidden;

                    tP.setTimerState(false);
                    tP.clearInputTime();

                    initForm(true);
                    this.m_bgRatio = 0x0;
                    this.label1.Visibility = Visibility.Visible;
                    this.MainForm.Background = new SolidColorBrush(Colors.AliceBlue);
                    this.label3.Visibility = Visibility.Hidden;
                    this.label5.Content = tP.getGameModeString();
                    this.label6.Visibility = Visibility.Visible;
                    this.label7.Visibility = Visibility.Visible;
                    this.label8.Visibility = Visibility.Visible;
                }
                return;
            }

            typingParams.GAME_STATE r_inf = this.m_typing.KeyDown(enter);
            this.MainForm.Background = new SolidColorBrush(Colors.AliceBlue);
            this.label1.Visibility = Visibility.Visible;
            this.label3.Visibility = Visibility.Hidden;

            switch (r_inf)
            {
                case typingParams.GAME_STATE.GAME_STARTED:
                    {
                        String modeStr = tP.getGameModeString();

                        tP.randomTargetString();
                        tP.championTargetString();

                        tP.setFirstChar();

                        this.label1.Foreground = new SolidColorBrush(Colors.Blue);
                        this.label1.FontWeight = FontWeights.Normal;
                        this.label1.FontSize = 300;
                        this.label1.Content = tP.getViewString();
                        this.label6.Visibility = Visibility.Hidden;
                        this.label7.Visibility = Visibility.Hidden;
                        this.label8.Visibility = Visibility.Hidden;
                        
                        if (tP.getGameMode() == typingParams.GAME_MODE.EASY_MODE)
                        {
                            this.label2.Content = tP.getCurrentString();
                        }

                        if (tP.getAorZmode() == true)
                        {
                            modeStr += "\n(A → Z)";
                        }
                        else
                        {
                            modeStr += "\n(Z → A)";
                        }

                        label5.Content = modeStr;

                        this.disTimer.Start();
                        break;
                    }

                case typingParams.GAME_STATE.CONTINUE_INPUT:
                    {
                        this.label1.Content = tP.getViewString();
                        this.label2.Content = tP.getPoolString();
                        break;
                    }

                case typingParams.GAME_STATE.MISS_ENTER:
                    {
                        this.label3.Content = "Miss!!";
                        this.label3.FontSize = 180;
                        this.label1.Visibility = Visibility.Hidden;
                        this.label3.Visibility = Visibility.Visible;
                        this.MainForm.Background = new SolidColorBrush(Colors.PaleVioletRed);
                        break;
                    }

                case typingParams.GAME_STATE.GO_TO_NEXT_PLAYER:
                    {
                        this.disTimer.Stop();
                        this.label1.Content = tP.getViewString();

                        String tex = this.m_typing.getTmpGameResult();
                        System.Windows.MessageBox.Show(tex, "中間発表");

                        this.label3.Visibility = Visibility.Hidden;
                        tP.incCurrentPlayer();
                        initForm();
                        break;
                    }

                case typingParams.GAME_STATE.GAME_IS_OVER:
                    {
                        String resultString = "";
                        this.disTimer.Stop();
                        this.label1.Content = tP.getViewString();

                        resultString = this.m_typing.getGameResult();
                        this.m_typing.writeResultDataToFile(resultString);
                        System.Windows.MessageBox.Show(resultString,
                                                       "結果発表");
                        initForm(true);
                        this.label5.Content = tP.getGameModeString();
                        this.label6.Visibility = Visibility.Visible;
                        this.label7.Visibility = Visibility.Visible;
                        this.label8.Visibility = Visibility.Visible;
                        break;
                    }

                case typingParams.GAME_STATE.TERMINATE_GAME:
                    {
                        this.disTimer.Stop();
                        
                        tP.setTimerState(false);
                        tP.clearInputTime();

                        initForm(true);
                        this.m_bgRatio = 0x0;
                        this.label5.Content = tP.getGameModeString();
                        this.label6.Visibility = Visibility.Visible;
                        this.label7.Visibility = Visibility.Visible;
                        this.label8.Visibility = Visibility.Visible;
                        break;
                    }

                default:
                    break;
            }
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.3ca.co.jp");
        }

        private void EasyMouseMove(object sender, MouseEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            label6.Foreground = new SolidColorBrush(Colors.Aqua);
            this.label1.Foreground = new SolidColorBrush(Colors.Blue);
            this.label1.FontWeight = FontWeights.Normal;
            this.label1.Content = tP.getCurrentPlayerString() +
                                  " Player\nHit Space key!";
            this.label5.Content = tP.getGameModeString();
        }

        private void EasyMouseLeave(object sender, MouseEventArgs e)
        {
            label6.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void EasyModeOn(object sender, MouseButtonEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            tP.setGameMode("easy");
        }

        private void NormalMouseMove(object sender, MouseEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            label7.Foreground = new SolidColorBrush(Colors.Aqua);
            this.label1.Foreground = new SolidColorBrush(Colors.Blue);
            this.label1.FontWeight = FontWeights.Normal;
            this.label1.Content = tP.getCurrentPlayerString() +
                                  " Player\nHit Space key!";
            this.label5.Content = tP.getGameModeString();
        }

        private void NormalMouseLeave(object sender, MouseEventArgs e)
        {
            label7.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void NormalModeOn(object sender, MouseButtonEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            tP.setGameMode("normal");
        }

        private void ChampionMouseMove(object sender, MouseEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            label8.Foreground = new SolidColorBrush(Colors.Aqua);
            this.label1.Foreground = new SolidColorBrush(Colors.Red);
            this.label1.FontWeight = FontWeights.ExtraBold;
            this.label1.Content = "Hey Champ!\nHit Space key!";
        }

        private void ChampionMouseLeave(object sender, MouseEventArgs e)
        {
            label8.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void ChampionModeOn(object sender, MouseButtonEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            tP.setGameMode("champion");
            this.label5.Content = tP.getGameModeString();
        }
    }
}
