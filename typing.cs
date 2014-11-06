using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Windows.Input;
using Typing;

namespace Typing
{
    class typing
    {
        public bool getParamsFromInifile()
        {
            bool r_inf = false;
            typingParams tP = typingParams.getInstance();

            try
            {
                StreamReader sr = this.readIniFile();                
                while (sr.EndOfStream != true)
                {
                    String rStr = sr.ReadLine();
                    rStr = rStr.Replace(" ", "");
                    if (rStr.IndexOf("#") == 0)
                    {
                        /* 先頭が # の行はコメントとして読み飛ばす */
                        continue;
                    }
                    else if (rStr.IndexOf(typingParams.NUMBER_OF_PLAYERS) != -1)
                    {
                        String[] t = rStr.Split('=');
                        tP.setNumberOfPlayer(Convert.ToInt32(t[1]));
                    }
                    else if (rStr.IndexOf(typingParams.INI_TAG_GAME_MODE) != -1)
                    {
                        String[] t = rStr.Split('=');
                        tP.setGameMode(t[1]);
                    }
                    else if(rStr.IndexOf(typingParams.INI_TAG_RANDOM_MODE) != -1)
                    {
                        String[] t = rStr.Split('=');
                        bool setVal = false;
                        t[1].ToLower();
                        if (t[1].IndexOf("true") != -1)
                        {
                            setVal = true;
                        }
                        tP.setRandomeMode(setVal);
                    }
                    else if (rStr.IndexOf(typingParams.INI_TAG_TIME_LIMIT) != -1)
                    {
                        String[] t = rStr.Split('=');
                        t[1].ToLower();
                        tP.setTimeLimit(t[1]);
                    }
                }
                sr.Close();
                tP.initParams();
                r_inf = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }

        public bool setTargetString()
        {
            bool r_inf = false;
            try
            {
                StreamReader sr = this.readIniFile();
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
                        continue;
                    }
                    else if (rStr.IndexOf(typingParams.INI_TAG_TARGET1) != -1)
                    {
                        String[] t = rStr.Split('=');
                        x = new String[t[1].Length];
                        x = t[1].Split(',');
                    }
                    else if (rStr.IndexOf(typingParams.INI_TAG_TARGET2) != -1)
                    {
                        String[] t = rStr.Split('=');
                        y = new String[t[1].Length];
                        y = t[1].Split(',');
                    }
                    else if (rStr.IndexOf(typingParams.INI_TAG_TARGET3) != -1)
                    {
                        String[] t = rStr.Split('=');
                        z = new String[t[1].Length];
                        z = t[1].Split(',');
                    }
                }

                if ((x != null) && (y != null) && (z != null))
                {
                    typingParams tP = typingParams.getInstance();
                    int len = x.Length + y.Length + z.Length;
                    int pos = 0;
                    if (tP.genTargetArray(len) == true)
                    {
                        /* ------------------------------------------------------------- */
                        /* FIXME: とてもイケてない実装なので暇な人直して下さい           */
                        /* THE REASON WHY: 1. 同じ処理を参加人数ごとに追加する必要が有る */
                        /*                 2. 類似の処理なのに複数回コールしている       */
                        /*                 3. setTargetString自体が出題数に依存する作り  */
                        /* iniパーサだけで独立させた方がいいような気はします             */
                        /* が、其処までのモチベーションが湧かないので出来てません        */
                        /* ------------------------------------------------------------- */
                        tP.setTargetArray(x, pos);
                        pos += x.Length;
                        tP.setPlayerLength(0, pos);
                        tP.setPlayerCenterPos(0, (x.Length / 2) - 1);
                        
                        tP.setTargetArray(y, pos);
                        pos += y.Length;
                        tP.setPlayerLength(1, pos);
                        tP.setPlayerCenterPos(1, (y.Length / 2) - 2);

                        tP.setTargetArray(z, pos);
                        pos += z.Length;
                        tP.setPlayerLength(2, pos);
                        tP.setPlayerCenterPos(2, (z.Length / 2) - 2);

                        r_inf = true;
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return r_inf;
        }

        private StreamReader readIniFile()
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(typingParams.INI_FILE_NAME);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return sr;
        }

        public bool writeResultDataToFile(String resultStr)
        {
            bool r_inf = false;
            try
            {
                typingParams tP = typingParams.getInstance();
                StringBuilder sb = new StringBuilder();
                DateTime dt = new DateTime();
                dt = DateTime.Now;
                String date = String.Format("{0:D4}{1:D2}{2:D2}",dt.Year, dt.Month, dt.Day);
                StreamWriter sw = new StreamWriter(typingParams.RESULT_FILE_NAME + date + ".txt", true);

                sb.Append("\n------------------------------\n\n");
                sb.Append(String.Format("{0:D2}時{1:D2}分{2:D2}秒({3})\n\n",
                                        dt.Hour, dt.Minute, dt.Second, tP.getGameModeString()));
                sb.Append(resultStr);
                sb.Append("\n");

                sw.WriteLine(sb.ToString());
                sw.Close();

                r_inf = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }

        private typingParams.GAME_STATE checkNextPlayer(int curPos)
        {
            typingParams.GAME_STATE r_inf = typingParams.GAME_STATE.CONTINUE_INPUT;
            typingParams tP = typingParams.getInstance();
            int termPos = tP.getCurrentPlayerLength();

            if (curPos == termPos)
            {
                tP.setTimerState(false);

                if (tP.getGameMode() == typingParams.GAME_MODE.CHAMPION_MODE)
                {
                    double tmp = tP.getInputTime();
                    int cP = tP.getCurrentPlayer();
                    for (int ii = 0; ii < cP; ii++)
                    {
                        tmp = tmp - tP.getPlayerTime(ii);
                    }
                    tP.setPlayerTime(cP, tmp);
                }
                else{
                    tP.setPlayerTime(tP.getCurrentPlayer(), tP.getInputTime());
                    tP.clearInputTime();
                }

                if (tP.getCurrentPlayer() == tP.getLastPlayer())
                {
                    r_inf = typingParams.GAME_STATE.GAME_IS_OVER;
                }
                else
                {
                    r_inf = typingParams.GAME_STATE.GO_TO_NEXT_PLAYER;
                }
            }
            return r_inf;
        }

        private typingParams.GAME_STATE getKeyInput(String enter)
        {
            typingParams.GAME_STATE r_inf = typingParams.GAME_STATE.NON_OF_PARAM;
            typingParams tP = typingParams.getInstance();
            String curTarget = tP.getCurrentString();
            if (enter.Equals(curTarget) == true)
            {
                tP.appendPoolString();
                tP.incCurrentPos();
                r_inf = checkNextPlayer(tP.getCurrentPos());
            }
            else if (enter.Equals("Escape") == true)
            {
                /* ESCキーでゲームを強制終了する */
                r_inf = typingParams.GAME_STATE.TERMINATE_GAME;
            }
            else
            {
                r_inf = typingParams.GAME_STATE.MISS_ENTER;
            }
            return r_inf;
        }

        public typingParams.GAME_STATE KeyDown(String enter)
        {
            typingParams.GAME_STATE r_inf = typingParams.GAME_STATE.NON_OF_PARAM;
            typingParams tP = typingParams.getInstance();

            if (tP.getTimerState() == true)
            {
                r_inf = this.getKeyInput(enter);
            }
            else  if(enter.Equals("Space") == true)
            {
                tP.setTimerState(true);
                r_inf = typingParams.GAME_STATE.GAME_STARTED;
            }
            return r_inf;
        }

        public String getTmpGameResult()
        {
            typingParams tP = typingParams.getInstance();
            StringBuilder sb = new StringBuilder();
            String tStr = "";
            String fmt = "";
            int playerNum = tP.getCurrentPlayer();

            if (tP.getGameMode() == typingParams.GAME_MODE.CHAMPION_MODE)
            {
                fmt = "{0}回目\n{1:F2}秒でした";
            }
            else
            {
                fmt = "{0}人目\n{1:F2}秒でした";
            }
            
            tStr = String.Format(fmt, playerNum+1, tP.getPlayerTime(playerNum));
            sb.Append(tStr);
            return sb.ToString();
        }

        public String getGameResult()
        {
            typingParams tP = typingParams.getInstance();
            StringBuilder sb = new StringBuilder();
            String tStr = "";
            String fmt = "";
            int numOfplayer = tP.getNumberOfPlayer();
            double total_time = tP.getTotalInputTime();

            if(tP.getGameMode() == typingParams.GAME_MODE.CHAMPION_MODE)
            {
                fmt = "{0:D0}回目：{1:F2}秒\n";
            }
            else
            {
                fmt = "{0:D0}人目：{1:F2}秒\n";
            }

            for(int ii=0; ii<tP.getNumberOfPlayer(); ii++)
            {
                tStr = String.Format(fmt, (ii+1), tP.getPlayerTime(ii));
                sb.Append(tStr);
            }
            tStr = String.Format("合計：{0:F2}秒でした", total_time);
            sb.Append(tStr);
            return sb.ToString();
        }
    }
}
