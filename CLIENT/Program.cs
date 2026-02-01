

namespace CLIENT
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            // Khởi chạy frmConnected từ namespace CLIENT.View
            try
            {
                Application.Run(new frmConnected());
            }
            catch (Exception ex)
            {
                // Hiển thị hộp thoại báo lỗi thay vì để nó đóng im lặng
                System.Windows.Forms.MessageBox.Show("Lỗi thực thi: " + ex.Message);
            }
        }
    }
}