using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Typing
{
    public class typingParams
    {
        private static typingParams instance = null;

        #region 固定パラメータ
        public const String INI_TAG_TARGET1 = "TargetString1";
        public const String INI_TAG_TARGET2 = "TargetString2";
        public const String INI_TAG_TARGET3 = "TargetString3";
        public const String NUMBER_OF_PLAYERS = "NumberOfPlayers";
        public const String INI_FILE_NAME = "typing.ini";

        public enum GAME_STATE {
                                   NON_OF_PARAM=0, 
                                   START_STATE,
                                   STOP_STATE,
                                   GAME_STARTED,
                                   CONTINUE_INPUT,
                                   GO_TO_NEXT_PLAYER,
                                   MISS_ENTER,
                                   GAME_IS_OVER,
                                   CONTINUE_GAME
                               };
        #endregion

        #region ゲーム用パラメータ
        private long Tick;  /* 入力時間取得用タイマーの閾値 */
        private int NumberOfPlayer;

        private String[] targetArray;
        private StringBuilder poolString;
        private int curPos = 0;

        private int curPlayer = 0;      /* 0:1人目, 1:2人目, 2:3人目 */
        private int[] player_len;
        private int[] player_center_pos;
        private double[] player_time;
        private bool TimerState = false;
        private double inputTime = 0.0f;
        #endregion

        private typingParams()
        {
            /* デフォルト値を設定 => iniファイルに設定がある場合は上書き */
            this.Tick = 100000;
            this.NumberOfPlayer = 3;
        }

        ~typingParams()
        {

        }

        static public typingParams getInstance()
        {
            if(typingParams.instance == null){
                typingParams.instance = new typingParams();
            }
            return typingParams.instance;
        }

        public bool initParams()
        {
            bool r_inf = false;
            try
            {
                int numOfplay = this.getNumberOfPlayer();
                this.poolString = new StringBuilder();
                this.player_len = new int[numOfplay];
                this.player_center_pos = new int[numOfplay];
                this.player_time = new double[numOfplay];
                this.curPos = 0;
                this.curPlayer = 0;
                this.inputTime = 0.0f;
                this.targetArray = null;
                r_inf = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }

        public void setFirstChar()
        {
            String inStr = this.targetArray[this.curPos];
            this.poolString.Append(inStr);
        }

        private int getPlayerCenterPos(int player)
        {
            return this.player_center_pos[player];
        }

        public void setPlayerCenterPos(int player, int pos)
        {
            this.player_center_pos[player] = pos;
        }

        public void setPlayerLength(int player, int len)
        {
            this.player_len[player] = len;
        }

        public void incCurrentPlayer()
        {
            this.curPlayer++;
        }

        public int getCurrentPlayer()
        {
            return this.curPlayer;
        }

        private int getPrevPlayerLength()
        {
            int r_inf = -1;
            int prev = this.getCurrentPlayer() - 1;
            if (prev >= 0) { 
                r_inf = this.player_len[prev]; 
            }
            return r_inf;
        }

        private int getCurrentPlayerArrayTop()
        {
            return getPrevPlayerLength();
        }

        public int getCurrentPlayerLength()
        {
            return this.player_len[this.getCurrentPlayer()];
        }

        public long getTick()
        {
            return this.Tick;
        }

        public int getLastPlayer()
        {
            return (this.NumberOfPlayer - 1);
        }

        public void setNumberOfPlayer(int num)
        {
            this.NumberOfPlayer = num;
        }

        public int getNumberOfPlayer()
        {
            return this.NumberOfPlayer;
        }

        public bool genTargetArray(int length)
        {
            bool r_inf = false;
            try { 
                this.targetArray = new String[length];
                r_inf = true;
            }
            catch(Exception ex){
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }

        public bool setTargetArray(String[] inStr, int pos)
        {
            bool r_inf = false;
            try
            {
                inStr.CopyTo(this.targetArray, pos);
                r_inf = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return r_inf;
        }
        
        public bool getTimerState()
        {
            return this.TimerState;
        }

        public void setTimerState(bool inTimerState)
        {
            this.TimerState = inTimerState;
        }

        public String getViewString()
        {
            String rStr = "";

            /* 各プレイヤーの先頭文字の場合は空白にして、以降は入力した文字が出るようにする */
            if ((this.curPos != 0) && (this.curPos != this.getCurrentPlayerArrayTop()))
            {
                int pos = this.curPos - 1;
                if (pos >= 0) rStr = this.targetArray[this.curPos - 1];
            }           

            return rStr;
        }

        public String getCurrentString()
        {
            return this.targetArray[this.curPos];
        }

        public String getTargetString(int pos)
        {
            String rStr = "";
            if (pos < this.targetArray.Length)
            {
                rStr = this.targetArray[pos];
            }
            return rStr;
        }

        public void appendPoolString()
        {
            int pos = this.getCurrentPos();
            if (pos < this.targetArray.Length)
            {
                this.poolString.Append(this.getTargetString(this.curPos + 1));

                /* 問題（文字）は1つの配列に詰め込んであるので、改行箇所(プレイヤー  *
                 * ごとの出題文字列の中間を計算するために、出題済みの長さ分を現在の  *
                 * 位置から減算する（０オリジンにする）                              */

                int separatePos = pos;
                int prevPlayer = this.getCurrentPlayer() - 1;
                if (prevPlayer >= 0)
                {
                    separatePos -= this.player_len[prevPlayer];
                }
                if (separatePos == this.getPlayerCenterPos(this.getCurrentPlayer()))
                {
                    this.poolString.Append("\n");
                }
            }
        }

        public void clearPoolString()
        {
            this.poolString.Clear();
        }

        public int getCurrentPos()
        {
            return this.curPos;
        }

        public void incCurrentPos()
        {
            this.curPos++;
        }

        public String getPoolString()
        {
            return this.poolString.ToString();
        }

        public void clearInputTime()
        {
            this.inputTime = 0.0f;
        }

        public void incInputTime()
        {
            this.inputTime += 0.01f;
        }

        public double getInputTime()
        {
            return this.inputTime;
        }

        public void setPlayerTime(int num, double inTime)
        {
            this.player_time[num] = inTime;
        }

        public double getPlayerTime(int num)
        {
            return this.player_time[num];
        }

        public double getTotalInputTime()
        {
            double total = 0.0f;

            for (int ii = 0; ii < this.player_time.Length; ii++)
            {
                total += this.player_time[ii];
            }

            return total;
        }
    }
}
