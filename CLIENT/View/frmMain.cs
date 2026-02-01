using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnCallVideo_Click(object sender, EventArgs e)
        {
            if (btnCallVideo.Enabled) { 
                frmVideoCall videoCallForm = new frmVideoCall();
                videoCallForm.Show();
            } 
        }

        private void btnSelectionChatBox_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnSelectionChatBox, new Point(0, btnSelectionChatBox.Height));
        }
    }
}
