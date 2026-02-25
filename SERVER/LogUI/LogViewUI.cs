using System;
using System.Windows.Forms;

namespace SERVER.LogUI
{
    public static class LogViewUI
    {
        // Biến lưu trữ tham chiếu đến ListView thực tế trên Form
        private static ListView _listView;
        private static ListView _lvClient;

        // Biến lưu trữ SessionID hiện tại để liên kết log với phiên làm việc cụ thể
        public static string CurrentSessionID { get; set; } = "";


        // Cờ xác định xem UI có đang xem Log trực tiếp không, hay đang xem quá khứ
        public static bool IsViewingCurrentSession { get; set; } = true;

        public static void Initialize(ListView lv, ListView clientLv)
        {
            _listView = lv;
            _lvClient = clientLv; // Lưu tham chiếu đến listView danh sách client
        }

        public static void AddLog(string message)
        {
            if (_listView == null) return;
            if (_listView.InvokeRequired)
            {
                _listView.Invoke(new Action(() => AddLog(message)));
                return;
            }

            string time = DateTime.Now.ToString("HH:mm:ss");

            // --- CHỈ HIỂN THỊ LÊN LISTVIEW NẾU ĐANG Ở CHẾ ĐỘ 'CURRENT' ---
            if (IsViewingCurrentSession)
            {
                ListViewItem item = new ListViewItem(new[] { time, message });
                item.UseItemStyleForSubItems = false;
                item.SubItems[0].ForeColor = System.Drawing.Color.ForestGreen;
                item.SubItems[1].ForeColor = System.Drawing.Color.Black;

                _listView.Items.Add(item);
                _listView.EnsureVisible(_listView.Items.Count - 1);
            }

            // Dù đang xem quá khứ hay hiện tại, Log mới vẫn LUÔN được đẩy xuống Database
            if (!string.IsNullOrEmpty(CurrentSessionID))
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    SERVER.Database.DatabaseConnect dbLog = new SERVER.Database.DatabaseConnect();
                    dbLog.SaveServerLog(CurrentSessionID, message);
                });
            }
        }

        // ĐỂ TẢI LỊCH SỬ LOG CỦA MỘT PHIÊN LÀM VIỆC CỤ THỂ VÀO LISTVIEW
        public static void LoadHistoryToView(List<string[]> logs)
        {
            if (_listView == null) return;
            if (_listView.InvokeRequired)
            {
                _listView.Invoke(new Action(() => LoadHistoryToView(logs)));
                return;
            }

            _listView.Items.Clear();
            foreach (var log in logs)
            {
                ListViewItem item = new ListViewItem(new[] { log[0], log[1] });
                item.UseItemStyleForSubItems = false;
                item.SubItems[0].ForeColor = System.Drawing.Color.MediumBlue; // Đổi màu để nhận diện log cũ
                item.SubItems[1].ForeColor = System.Drawing.Color.Black;
                _listView.Items.Add(item);
            }
            if (_listView.Items.Count > 0)
            {
                _listView.EnsureVisible(_listView.Items.Count - 1);
            }
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