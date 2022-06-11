using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace HonorCounter
{
    /// <summary>
    /// メイン処理を行うクラス
    /// </summary>
    class MainModel
    {
        private static string _pathVictory;
        private static string _pathLose;
        private static string _pathPick;
        private const double threshold = 0.85;

        /// <summary>
        /// 画面定期確認用のタイマー
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// 英雄ピックが始まったらtrue ⇒ trueの間は勝敗チェックをする
        /// 勝敗が決まったらfalse ⇒ 次のピックが始まるまでは勝敗チェックをしない
        /// </summary>
        private bool _pickFlag = true;

        /// <summary>
        /// 勝敗が決まったときに発生するイベント
        /// </summary>
        public event Action<bool>? ResultEvent;

        /// <summary>
        /// ウィンドウ取得失敗時のイベント
        /// </summary>
        public event Action? ErrorEvent;

        static MainModel()
        {
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;
            _pathVictory = Path.Combine(directory, "Image", "VICTORY.png");
            _pathLose = Path.Combine(directory, "Image", "LOSE.png");
            _pathPick = Path.Combine(directory, "Image", "PICK.png");
        }

        public MainModel(double interval, WindowData window)
        {
            _timer = new Timer(interval);

            _timer.Elapsed += (sender, e) =>
            {
                using (var bitmap = window.Capture())
                {
                    if (bitmap == null)
                    {
                        ErrorEvent?.Invoke();
                        return;
                    }

                    if (_pickFlag)
                    {
                        CheckResult(bitmap);
                    }
                    else
                    {
                        CheckBeginPick(bitmap);
                    }
                }
            };
        }

        /// <summary>
        /// 試合結果が出ているか確認する
        /// </summary>
        /// <param name="bitmap"></param>
        private void CheckResult(Bitmap bitmap)
        {
            using (var b = TrimCenter(bitmap))
            using (var t = b.ToMat())
            using (var target = t.CvtColor(ColorConversionCodes.BGRA2BGR))
            {
                if (ImageMatch(target, _pathVictory))
                {
                    ResultEvent?.Invoke(true);
                    _pickFlag = false;
                }
                else if (ImageMatch(target, _pathLose))
                {
                    ResultEvent?.Invoke(false);
                    _pickFlag = false;
                }
            }
        }

        /// <summary>
        /// 英雄ピックが始まっているか確認する
        /// </summary>
        /// <param name="bitmap"></param>
        private void CheckBeginPick(Bitmap bitmap)
        {
            using (var b = TrimTopLeft(bitmap))
            using (var t = b.ToMat())
            using (var target = t.CvtColor(ColorConversionCodes.BGRA2BGR))
            {
                if (ImageMatch(target, _pathPick))
                {
                    _pickFlag = true;
                }
            }
        }

        /// <summary>
        /// 画像の上1/4、左1/2を切り取る
        /// </summary>
        /// <param name="bitmap">対象の画像</param>
        /// <returns>切り抜いた画像(縦117px)</returns>
        private Bitmap TrimTopLeft(Bitmap bitmap)
        {
            var result = new Bitmap((234 * bitmap.Width) / bitmap.Height, 117);
            using (var g = Graphics.FromImage(result))
            {
                var dstRect = new RectangleF(0, 0, result.Width, result.Height);
                var srcRect = new RectangleF(0, 0, bitmap.Width / 2, bitmap.Height / 4);
                g.DrawImage(bitmap, dstRect, srcRect, GraphicsUnit.Pixel);
            }
            return result;
        }

        /// <summary>
        /// 画像の中央を切り取る
        /// </summary>
        /// <param name="bitmap">対象の画像</param>
        /// <returns>切り抜いた画像(縦468px)</returns>
        private Bitmap TrimCenter(Bitmap bitmap)
        {
            var result = new Bitmap(468, 234);
            using (var g = Graphics.FromImage(result))
            {
                var dstRect = new RectangleF(0, 0, result.Width, result.Height);
                var srcRect = new RectangleF((bitmap.Width - bitmap.Height) / 2, bitmap.Height / 4, bitmap.Height, bitmap.Height / 2);
                g.DrawImage(bitmap, dstRect, srcRect, GraphicsUnit.Pixel);
            }
            return result;
        }



        private bool ImageMatch(Mat target, string templatePath)
        {
            using (var template = new Mat(templatePath))
            using (var result = new Mat())
            {
                Cv2.MatchTemplate(target, template, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out var minval, out var maxval, out var minloc, out var maxloc);
                return maxval >= threshold;
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
