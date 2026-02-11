using AForge.Video.DirectShow;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CLIENT.View
{
    public partial class frmSelectOption : Form
    {

        private FilterInfoCollection _videoDevices; // Cho cbCamera
        private List<MMDevice> _inputDevices;       // Cho cbInput (Microphone)
        private List<MMDevice> _outputDevices;      // Cho cbOutput (Loa/Tai nghe)

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedMoniker { get; set; } // ID Camera truyền ra frmMain

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedInputId { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedOutputId { get; set; }

        public frmSelectOption()
        {
            InitializeComponent();
        }

        private void frmSelectOption_Load(object sender, EventArgs e)
        {
            // 1. Nạp Camera vào cbCamera
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in _videoDevices)
            {
                cbCamera.Items.Add(device.Name);
            }
            if (cbCamera.Items.Count > 0) cbCamera.SelectedIndex = 0;

            // 2. Nạp thiết bị Âm thanh (dùng NAudio)
            var enumerator = new MMDeviceEnumerator();

            // Nạp Mic vào cbInput
            var captureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            _inputDevices = captureDevices.ToList();
            foreach (var dev in _inputDevices) cbInput.Items.Add(dev.FriendlyName);
            if (cbInput.Items.Count > 0) cbInput.SelectedIndex = 0;

            // Nạp Loa vào cbOutput
            var renderDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            _outputDevices = renderDevices.ToList();
            foreach (var dev in _outputDevices) cbOutput.Items.Add(dev.FriendlyName);
            if (cbOutput.Items.Count > 0) cbOutput.SelectedIndex = 0;
        }


        private void btnExit_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            if (cbCamera.SelectedIndex >= 0)
                this.SelectedMoniker = _videoDevices[cbCamera.SelectedIndex].MonikerString;

            // Lưu ID Mic và Loa (Nếu cần dùng cho NAudio sau này)
            if (cbInput.SelectedIndex >= 0)
                this.SelectedInputId = _inputDevices[cbInput.SelectedIndex].ID;

            if (cbOutput.SelectedIndex >= 0)
                this.SelectedOutputId = _outputDevices[cbOutput.SelectedIndex].ID;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
