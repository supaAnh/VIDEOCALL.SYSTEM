

using System.Runtime.InteropServices;

namespace CLIENT
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            string ffmpegBinaryPath = @"D:\ffmpeg\bin";

            // Cập nhật biến môi trường PATH cho ứng dụng
            var path = Environment.GetEnvironmentVariable("PATH");
            if (!path.Contains(ffmpegBinaryPath))
            {
                Environment.SetEnvironmentVariable("PATH", path + ";" + ffmpegBinaryPath);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmConnected());

        }
    }
}