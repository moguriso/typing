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

        public MainWindow()
        {
            InitializeComponent();
            this.MainForm.Background = new SolidColorBrush(Colors.AliceBlue);

            typingParams tP = typingParams.getInstance();

            this.label3.Visibility = Visibility.Hidden;
            this.label3.Content = "Miss!!";

            this.m_typing = new typing();
            initForm(true);

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
                }
                tP.clearPoolString();
                this.label1.FontSize = 65;
                this.label1.Content = "Hit\nSpace key!";
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
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            String enter = e.Key.ToString();

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
                        this.disTimer.Stop();
                        this.label1.Content = tP.getViewString();

                        String tex = this.m_typing.getGameResult();
                        System.Windows.MessageBox.Show(tex, "結果発表");
                        initForm(true);
                        this.label5.Content = tP.getGameModeString();
                        this.label6.Visibility = Visibility.Visible;
                        this.label7.Visibility = Visibility.Visible;
                        this.label8.Visibility = Visibility.Visible;
                        break;
                    }

                default:
                    break;
            }
            Debug.WriteLine(enter);
        }

        private void onMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://www.3ca.co.jp");
        }

        private void EasyMouseMove(object sender, MouseEventArgs e)
        {
            label6.Foreground = new SolidColorBrush(Colors.Aqua);
        }

        private void EasyMouseLeave(object sender, MouseEventArgs e)
        {
            label6.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void EasyModeOn(object sender, MouseButtonEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            tP.setGameMode("easy");
            this.label5.Content = tP.getGameModeString();
        }

        private void NormalMouseMove(object sender, MouseEventArgs e)
        {
            label7.Foreground = new SolidColorBrush(Colors.Aqua);
        }

        private void NormalMouseLeave(object sender, MouseEventArgs e)
        {
            label7.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void NormalModeOn(object sender, MouseButtonEventArgs e)
        {
            typingParams tP = typingParams.getInstance();
            tP.setGameMode("normal");
            this.label5.Content = tP.getGameModeString();
        }

        private void ChampionMouseMove(object sender, MouseEventArgs e)
        {
            label8.Foreground = new SolidColorBrush(Colors.Aqua);
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
