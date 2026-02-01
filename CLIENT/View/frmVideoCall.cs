using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmVideoCall : Form
    {
        public frmVideoCall()
        {
            InitializeComponent();
        }

        private void btnSelectionVideoCall_Click(object sender, EventArgs e)
        {
            contextMenuStrip1.Show(btnSelectionVideoCall, new Point(0, btnSelectionVideoCall.Height));
        }
    }
}
