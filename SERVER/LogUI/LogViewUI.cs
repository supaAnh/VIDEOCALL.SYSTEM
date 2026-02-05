using System;
using System.Windows.Forms;

namespace SERVER.LogUI
{
    public static class LogViewUI
    {
        // Biến lưu trữ tham chiếu đến ListView thực tế trên Form
        private static ListView _listView;
        private static ListView _lvClient;

        public static void Initialize(ListView lv, ListView clientLv)
        {
            _listView = lv;
            _lvClient = clientLv; // Lưu tham chiếu đến listView danh sách client
        }

        public static void AddLog(string message)
        {
            // Nếu chưa khởi tạo thì thoát để tránh lỗi NullReferenceException
            if (_listView == null) return;

            // Xử lý nếu được gọi từ một Thread khác với Thread UI
            if (_listView.InvokeRequired)
            {
                _listView.Invoke(new Action(() => AddLog(message)));
                return;
            }

            // Tạo dòng thông tin mới
            string time = DateTime.Now.ToString("HH:mm:ss");
            ListViewItem item = new ListViewItem(new[] { time, message });

            // Cấu hình màu sắc
            item.UseItemStyleForSubItems = false;
            item.SubItems[0].ForeColor = System.Drawing.Color.ForestGreen;
            item.SubItems[1].ForeColor = System.Drawing.Color.Black;

            _listView.Items.Add(item);

            // Tự động cuộn xuống dòng cuối cùng
            _listView.EnsureVisible(_listView.Items.Count - 1);
        }


        //
        //
        // THÊM CLIENT VÀO DANH SÁCH listViewClientConnected
        //
        //
        public static void AddClient(string ip)
        {
            if (_lvClient == null) return;
            if (_lvClient.InvokeRequired)
            {
                _lvClient.Invoke(new Action(() => AddClient(ip)));
                return;
            }

            // Kiểm tra xem IP này đã có trong danh sách chưa để tránh trùng lặp
            foreach (ListViewItem item in _lvClient.Items)
            {
                if (item.Text == ip) return;
            }

            _lvClient.Items.Add(new ListViewItem(ip));
        }

        public static void RemoveClient(string ip)
        {
            if (_lvClient == null) return;
            if (_lvClient.InvokeRequired)
            {
                _lvClient.Invoke(new Action(() => RemoveClient(ip)));
                return;
            }

            foreach (ListViewItem item in _lvClient.Items)
            {
                if (item.Text == ip)
                {
                    _lvClient.Items.Remove(item);
                    break;
                }
            }
        }
    }
}