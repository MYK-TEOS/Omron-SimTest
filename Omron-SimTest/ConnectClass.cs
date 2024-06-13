using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Omron_SimTest
{
    internal class ConnectClass
    {
        public int iNode { get; set; }   // ノード番号
        public int iPortRead { get; set; }   // 読込ポート
        public int iSleepSec { get; set; }  // 返信ウェイト

        public bool bRunning;   // 読込動作中
        
        TcpReceive tRes;        // 読込用クラス

        bool bConnecting;


        /// <summary>
        /// 通信開始処理
        /// </summary>
        /// <returns></returns>
        public bool StartSocket()
        {
            if (tRes == null) tRes = new TcpReceive(iPortRead,  this);

            bRunning = true;
            return true;
        }

        /// <summary>
        /// 通信終了処理
        /// </summary>
        public void StopSocket()
        {
            if (tRes != null) tRes.Dispose();
            tRes = null;
            bRunning = false;
        }

        /// <summary>
        /// 接続待クラス
        /// </summary>
        public class TcpReceive
        {
            public int portNo;              // スレッド停止命令用
            public bool stop_flg = false;
            public ConnectClass PlcNodecls;
            readonly Thread thread = null;

            /// <summary>
            /// 待ち受け初期化
            /// </summary>
            /// <param name="portNo"></param>
            /// <param name="pm"></param>
            /// <param name="pc"></param>
            public TcpReceive(int portNo, ConnectClass pc)
            {
                this.portNo = portNo;
                this.PlcNodecls = pc;
                // 接続待ちのスレッドを開始
                thread = new Thread(new ThreadStart(ListenStart));
                // スレッドをスタート
                thread.Start();
            }

            /// <summary>
            /// スレッド廃棄処理
            /// </summary>
            public void Dispose()
            {
                stop_flg = true;
                while (thread.IsAlive)
                {
                    Thread.Sleep(0);
                }
                //GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 接続待ちスレッド本体
            /// </summary>
            private void ListenStart()
            {
                // スレッドに名前付け
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "待ち受けスレッド：" + this.portNo.ToString();

                try
                {
                    // Listenerの生成
                    TcpListener listener = new TcpListener(IPAddress.Any, this.portNo);
                    // 接続要求受け入れ開始
                    listener.Start();
                    while (!stop_flg)
                    {
                        // 接続待ちがあるか？
                        if (listener.Pending() == true)
                        {
                            if (PlcNodecls.bConnecting != true )
                            {
                                // 接続要求を受け入れる
                                TcpClient tcp = listener.AcceptTcpClient();
                                TcpReceiveWorker rcv = new TcpReceiveWorker(tcp, this, PlcNodecls);
                                Thread threadChild = new Thread(new ThreadStart(rcv.TCPClientProc));
                                // スレッドをスタート
                                threadChild.Start();
                            }
                        }
                        //else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    // 接続待ち終了
                    listener.Stop();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("待ち受けポート作成失敗 Port:" + portNo.ToString() + " : " + ex.Message);
                }
            }

        }




        /// <summary>
        /// 実通信クラス
        /// </summary>
        class TcpReceiveWorker
        {
            private readonly TcpClient tcp = null;
            readonly TcpReceive rcv = null;
            public ConnectClass PlcNodecls;

            /// <summary>
            /// 初期化処理
            /// </summary>
            /// <param name="tcp"></param>
            /// <param name="rcv"></param>
            /// <param name="pm"></param>
            public TcpReceiveWorker(TcpClient tcp, TcpReceive rcv, ConnectClass plcnode)
            {
                this.tcp = tcp;
                this.rcv = rcv;
                this.PlcNodecls = plcnode;
            }

            /// <summary>
            /// 通信プロシージャ
            /// </summary>
            public void TCPClientProc()
            {
                // スレッドに名前付け
                if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "PLC処理スレッド : " + rcv.portNo.ToString();

                NetworkStream st = tcp.GetStream();
                st.ReadTimeout = 2000;
                st.WriteTimeout = 2000;
                bool bRet = true;
                PlcNodecls.bConnecting = true;
                while (bRet == true)
                {
                    if (rcv.stop_flg == true) break;

                    if (tcp.Client.Connected == false) break;

                    bRet = TcpOmronZDLoop(st);
                    if (tcp.Connected == false) break;
                    if (bRet == false) break;

                    Thread.Sleep(10);
                }

                tcp.Close();
                st.Dispose();

                PlcNodecls.bConnecting = false;

            }

            readonly byte[] bLastBuffer = new byte[10000];


            /// <summary>
            /// 三菱のPLC対応ルーチン
            /// </summary>
            /// <param name="st"></param>
            /// <returns></returns>
            bool TcpOmronZDLoop(NetworkStream st)
            {
                int count = 0;
                
                int iReadNum = 10;

                byte[] bBuffer = new byte[10000];
                
                Random r1 = new System.Random();


                // 読み込み
                try
                {
                    int iData = 0;
                    while (count < iReadNum)
                    {
                        iData = st.Read(bBuffer, count, bBuffer.Length - count);
                        if (iData == 0) break;
                        count += iData;
                        Thread.Sleep(10);

                    }
                }
                catch
                {
                    return false;
                }
                if (tcp.Connected == false) return false;

                if (count == 0) return false;

                bool bHeaderError = false;
                if (bBuffer[0] != 0x41) bHeaderError = true; //'A'
                if (bBuffer[1] != 0x41) bHeaderError = true; //'A'
                if (bBuffer[2] != 0x35) bHeaderError = true; //'5'
                if (bBuffer[3] != 0x35) bHeaderError = true; //'5'

                if (bHeaderError == true)
                {

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    sb.AppendLine("BeforeCommand : ");
                    // 最初の21バイトを16進数文字列としてデバッグ出力する
                    for (int i = 0; i < iReadNum; i++)
                    {
                        string hexValue = bLastBuffer[i].ToString("X2"); // 2桁の16進数文字列に変換
                        sb.Append(hexValue + " ");
                    }
                    sb.AppendLine("");

                    sb.AppendLine("AfterCommand : ");
                    for (int i = 0; i < iReadNum; i++)
                    {
                        string hexValue = bBuffer[i].ToString("X2"); // 2桁の16進数文字列に変換
                        sb.Append(hexValue + " ");
                    }
                    sb.AppendLine("");
                    System.Diagnostics.Debug.WriteLine(sb.ToString());

                    //Clipboard.SetDataObject(sb.ToString(), true);
                    return false;
                }

                Array.Copy(bBuffer, bLastBuffer, bBuffer.Length);

                string sCommand = Encoding.Default.GetString(bBuffer, 8, 4);
                string sResponce = "";
                bool bErr = false;
                string Err = "00";
                string Mode = "00";
                string IDHIGH = "1";
                string IDLOW = "0";
                string IDX = "";

                switch ( sCommand)
                {
                    case "1001"://エラー状態取得
                        if (Form1.stData[PlcNodecls.iNode].bError_Hard == true) Err = "01";
                        if (Form1.stData[PlcNodecls.iNode].bError_Memory == true) Err = "02";
                        sResponce = "AA550C00FF1001000000F0F0F000"+Err+"00\r\n";
                        break;

                    case "1003"://モードスイッチ取得
                        if (Form1.stData[PlcNodecls.iNode].bMode_Thr == true) Mode = "01";
                        if (Form1.stData[PlcNodecls.iNode].bMode_Fun == true) Mode = "02";
                        sResponce = "AA550C00FF1003000000F0F0F000" + Mode + "00\r\n";
                        break;

                    case "8004"://バージョン情報取得
                        if (Form1.stData[PlcNodecls.iNode].bID_TempEnable == true) IDHIGH = "5";
                        if (Form1.stData[PlcNodecls.iNode].bID_PD50 == true) IDLOW = "1";
                        IDX = IDHIGH + IDLOW;
                        //Ver02.10.00例
                        sResponce = "AA550C00FF1003000000F0F0F000" + IDX+ "02100000\r\n";
                        break;

                    case "5100"://測定値取得

                        //未計測
                        if (Form1.stData[PlcNodecls.iNode].bUnCalc == true)
                        {
                            bErr = true;
                            break;
                        }

                        // レスポンスコード作成
                        if (Form1.stData[PlcNodecls.iNode].bError_Hard == true) Err = "01";
                        if (Form1.stData[PlcNodecls.iNode].bError_From == true) Err = "02";

                        //機種ID作成
                        if (Form1.stData[PlcNodecls.iNode].bID_TempEnable == true) IDHIGH = "5";
                        if (Form1.stData[PlcNodecls.iNode].bID_PD50 == true) IDLOW = "1";
                        IDX = IDHIGH + IDLOW;

                        // 小粒子作成
                        string sLit = String.Format("{0:X8}", Form1.stData[PlcNodecls.iNode].iParticle_little);
                        string sMid = String.Format("{0:X8}", Form1.stData[PlcNodecls.iNode].iParticle_Middle);
                        string sBig = String.Format("{0:X8}", Form1.stData[PlcNodecls.iNode].iParticle_Big);

                        //温度等作成
                        string sTmp = String.Format("{0:X4}", Form1.stData[PlcNodecls.iNode].iTemp);
                        string sCon = String.Format("{0:X4}", Form1.stData[PlcNodecls.iNode].iCon);
                        string sDp = String.Format("{0:X4}", Form1.stData[PlcNodecls.iNode].iDpTemp);
                        if (Form1.stData[PlcNodecls.iNode].bID_TempEnable == true)
                        {
                            sTmp = sCon = sDp ="7FFE";                            
                        }
                        sResponce = "AA55"+"81"+"00FF5100000000F0F0F0"+Err +"01"+IDX+ "000300"+ sLit     +sMid      +sBig      + sTmp + sCon + sDp  + "FFFFFFFFFFFFFFFFFFFF57\r\n";

                        
                        //機器正常ならこの文字列でOK
                        if (Form1.stData[PlcNodecls.iNode].bError_Hard == false) break;

                        //機器異常ならこの文字列にする
                        sResponce = "AA55"+"81"+"00FF5100000000F0F0F0"+"01"+"01"+"11"+"000300"+"00000000"+"00000000"+"00000000"+"7FFE"+"7FFE"+"7FFE"+ "FFFFFFFFFFFFFFFFFFFFB2\r\n";
                        break;
                    default:
                        bErr = true;
                        break;
                }

                if ( bErr == true )
                {
                    sResponce = "AA558000FF5100000000F0F0F0000120\r\n";
                }

                try
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(sResponce);
                    int iRetnum = bytes.Count();
                    // 返信ウェイト
                    if (rcv.PlcNodecls.iSleepSec != 0) System.Threading.Thread.Sleep(rcv.PlcNodecls.iSleepSec);
                    st.Write(bytes, 0, iRetnum);
                    st.Flush();
                    {
                        // 最初の21バイトを16進数文字列としてデバッグ出力する
                        for (int i = 0; i < iRetnum; i++)
                        {
                            string hexValue = bytes[i].ToString("X2"); // 2桁の16進数文字列に変換
                            Console.Write(hexValue + " ");
                        }
                        Console.Write("\n");

                    }
                }
                catch
                {
                    return false;
                }
                return true;
            }


        }

    }
}