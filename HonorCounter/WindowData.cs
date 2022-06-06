using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HonorCounter
{
    /// <summary>
    /// 他プロセスで実行中のウィンドウを管理するクラス
    /// </summary>
    internal class WindowData
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);


        private IntPtr _handle;
        private string _processName;
        private string _title;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="p">対象となるウィンドウプロセス</param>
        public WindowData(Process p)
        {
            _handle = p.MainWindowHandle;
            _processName = p.ProcessName;
            _title = p.MainWindowTitle;
        }

        /// <summary>
        /// 対象ウィンドウ全体のスクリーンショットを撮る
        /// </summary>
        /// <returns>取得したスクリーンショット(失敗時はnull)</returns>
        public Bitmap? Capture()
        {
            GetWindowRect(_handle, out RECT rect);

            var rectWidth = rect.right - rect.left;
            var rectHeight = rect.bottom - rect.top;

            if (rectWidth == 0 || rectHeight == 0)
            {
                return null;
            }

            var result = new Bitmap(rectWidth, rectHeight);
            using (var g = Graphics.FromImage(result))
            {
                g.CopyFromScreen(new Point(rect.left, rect.top), new Point(0, 0), result.Size);
            }
            return result;
        }

        public override string ToString()
        {
            return $"{_title} ({_processName})";
        }
    }
}
