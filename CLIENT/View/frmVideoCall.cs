using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmVideoCall : Form
    {
        private string _targetIP;
        private string _myMoniker;

        private CLIENT.Process.VideoCallProcess _videoCallLogic;

        public frmVideoCall(string targetIP, string moniker, CLIENT.Process.VideoCallProcess videoLogic)
        {
            InitializeComponent();

            _videoCallLogic = videoLogic;

            _targetIP = targetIP;
            _myMoniker = moniker;
            // Bắt đầu stream ngay khi mở form
            _videoCallLogic.StartStreaming(_targetIP, _myMoniker);
        }

        private void btnSelectionVideoCall_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnSelectionVideoCall, new Point(0, btnSelectionVideoCall.Height));
        }

        private void thoátToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitCall();
        }

        private void ExitCall()
        {
            // 1. Gửi tín hiệu rời khỏi cuộc gọi
            _videoCallLogic.SendSignal(_targetIP, "Leave");

            // 2. Dừng tất cả các luồng Camera/Mic/Record
            _videoCallLogic.StopAll();

            // 3. Đóng Form
            this.Close();
        }


    }
}
