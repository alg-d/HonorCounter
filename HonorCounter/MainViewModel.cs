using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;

namespace HonorCounter
{
    internal class MainViewModel : BindableBase
    {
        private MainModel? _model;

        /// <summary>
        /// 停止中はtrue
        /// </summary>
        public bool IsStopping
        {
            get { return _isStopping; }
            set { SetProperty(ref _isStopping, value); }
        }
        private bool _isStopping = true;

        /// <summary>
        /// 勝敗カウンター用の配列
        /// </summary>
        public int[] Values { get; } = new [] { 0, 0, 0, 0 };

        /// <summary>
        /// 表示用フォーマット
        /// </summary>
        public string Format
        {
            get { return _format; }
            set 
            {
                SetProperty(ref _format, value);
                RaisePropertyChanged(nameof(DisplayText));
                Save();
            }
        }
        private string _format = null!;

        /// <summary>
        /// 実際に表示される文字列
        /// </summary>
        public string DisplayText
        {
            get
            {
                try
                {
                    return string.Format(Format, Values[0], Values[1], Values[2], Values[3]);
                }
                catch (FormatException)
                {
                    return "フォーマットエラー";
                }
            }
        }

        /// <summary>
        /// 現在実行中のウィンドウ一覧
        /// </summary>
        public ObservableCollection<WindowData> WindowList { get; } = new ObservableCollection<WindowData>();

        /// <summary>
        /// 選択中のウィンドウ(未選択ならnull)
        /// </summary>
        public WindowData? SelectedWindow
        {
            get { return _selectedWindow; }
            set
            {
                if (SetProperty(ref _selectedWindow, value))
                {
                    StartCommand.RaiseCanExecuteChanged();
                    CheckCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private WindowData? _selectedWindow;


        /// <summary>
        /// 表示に使用するフォント名
        /// </summary>
        public string FontFamilyText
        {
            get { return _fontFamilyText; }
            set 
            { 
                if (SetProperty(ref _fontFamilyText, value))
                {
                    FontFamily = new FontFamily(_fontFamilyText);
                    Save();
                }
            }
        }
        private string _fontFamilyText = null!;

        /// <summary>
        /// 表示に使用する文字色のコード( #rrggbb 形式)
        /// </summary>
        public string ForegroundText
        {
            get { return _foregroundText; }
            set 
            {
                if (SetProperty(ref _foregroundText, value)
                    && TryGetBrush(_foregroundText, out var brush))
                {
                    Foreground = brush;
                    Save();
                }
            }
        }
        private string _foregroundText = null!;

        /// <summary>
        /// 表示に使用する背景色のコード( #rrggbb 形式)
        /// </summary>
        public string BackgroundText
        {
            get { return _backgroundText; }
            set 
            { 
                if (SetProperty(ref _backgroundText, value)
                    && TryGetBrush(_backgroundText, out var brush))
                {
                    Background = brush;
                    Save();
                }
            }
        }
        private string _backgroundText = null!;



        /// <summary>
        /// 表示に使用するフォント
        /// </summary>
        public FontFamily FontFamily
        {
            get { return _fontFamily; }
            set { SetProperty(ref _fontFamily, value); }
        }
        private FontFamily _fontFamily = null!;

        /// <summary>
        /// 表示に使用するフォントサイズ
        /// </summary>
        public double FontSize
        {
            get { return _fontSize; }
            set 
            { 
                SetProperty(ref _fontSize, value);
                Save();
            }
        }
        private double _fontSize;

        /// <summary>
        /// 表示に使用する文字色
        /// </summary>
        public Brush Foreground
        {
            get { return _foreground; }
            set { SetProperty(ref _foreground, value); }
        }
        private Brush _foreground = null!;

        /// <summary>
        /// 表示に使用する背景色
        /// </summary>
        public Brush Background
        {
            get { return _background; }
            set { SetProperty(ref _background, value); }
        }
        private Brush _background = null!;


        public MainViewModel()
        {
            Format = ConfigurationManager.AppSettings["Format"] ?? "{0}勝{1}敗";
            FontFamilyText = ConfigurationManager.AppSettings["FontFamily"] ?? "メイリオ";
            ForegroundText = ConfigurationManager.AppSettings["Foreground"] ?? "#FFFFFF";
            BackgroundText = ConfigurationManager.AppSettings["Background"] ?? "#000000";

            if (double.TryParse(ConfigurationManager.AppSettings["FontSize"], out var d))
            {
                FontSize = d;
            }
            else
            {
                FontSize = 48.0;
            }

            GetWindowList();

            var target = ConfigurationManager.AppSettings["TargetName"];
            if (target != null)
            {
                SelectedWindow = WindowList.FirstOrDefault(w => w.ToString() == target);
            }
        }

        /// <summary>
        /// ウィンドウ一覧を取得する
        /// </summary>
        private void GetWindowList()
        {
            WindowList.Clear();

            foreach (var p in Process.GetProcesses())
            {
                if (p.MainWindowHandle != IntPtr.Zero)
                {
                    WindowList.Add(new WindowData(p));
                }
            }
        }

        /// <summary>
        /// #rrggbb 形式をBrushに変換する
        /// </summary>
        /// <param name="rgb">#rrggbb 形式</param>
        /// <param name="brush">変換後のBrush</param>
        /// <returns>変換できたらtrue</returns>
        private bool TryGetBrush(string rgb, out Brush brush)
        {
            if (rgb.Length != 7 || !rgb.StartsWith("#"))
            {
                brush = new SolidColorBrush(Colors.White);
                return false;
            }

            if (TryParse(rgb.Substring(1, 2), out var r)
                && TryParse(rgb.Substring(3, 2), out var g)
                && TryParse(rgb.Substring(5, 2), out var b))
            {
                brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                return true;
            }

            brush = new SolidColorBrush(Colors.White);
            return false;

            static bool TryParse(string? s, out byte result)
            {
                return byte.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out result);
            }
        }

        /// <summary>
        /// app.configに設定した内容を保存する。
        /// </summary>
        private void Save()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Format"].Value = Format;
            config.AppSettings.Settings["FontFamily"].Value = FontFamilyText;
            config.AppSettings.Settings["Foreground"].Value = ForegroundText;
            config.AppSettings.Settings["Background"].Value = BackgroundText;
            config.AppSettings.Settings["FontSize"].Value = FontSize.ToString();
            config.AppSettings.Settings["TargetName"].Value = SelectedWindow?.ToString() ?? "";
            config.Save();
        }

        private DelegateCommand? _startCommand;
        public DelegateCommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                {
                    _startCommand = new DelegateCommand(() =>
                    {
                        if (SelectedWindow != null)
                        {
                            Save();
                            IsStopping = false;
                            _model = new MainModel(1000, SelectedWindow);
                            StartCommand.RaiseCanExecuteChanged();

                            _model.ResultEvent += (result) =>
                            {
                                if (result)
                                {
                                    Values[0]++;
                                    Values[2]++;
                                    Values[3]++;
                                }
                                else
                                {
                                    Values[1]++;
                                    Values[2]++;
                                    Values[3] = 0;
                                }
                                RaisePropertyChanged(nameof(DisplayText));
                            };
                            _model.Start();

                            StopCommand.RaiseCanExecuteChanged();
                            ReloadCommand.RaiseCanExecuteChanged();
                        }
                    },
                    () => SelectedWindow != null && _model == null);
                }
                return _startCommand;
            }
        }

        private DelegateCommand? _stopCommand;
        public DelegateCommand StopCommand
        {
            get
            {
                if (_stopCommand == null)
                {
                    _stopCommand = new DelegateCommand(() =>
                    {
                        _model?.Stop();
                        _model = null;
                        IsStopping = true;
                        StartCommand.RaiseCanExecuteChanged();
                        StopCommand.RaiseCanExecuteChanged();
                        ReloadCommand.RaiseCanExecuteChanged();
                    },
                    () => _model != null);
                }
                return _stopCommand;
            }
        }

        private DelegateCommand? _reloadCommand;
        public DelegateCommand ReloadCommand
        {
            get
            {
                if (_reloadCommand == null)
                {
                    _reloadCommand = new DelegateCommand(() =>
                    {
                        GetWindowList();
                    },
                    () => _model == null);
                }
                return _reloadCommand;
            }
        }

        private DelegateCommand? _checkCommand;
        public DelegateCommand CheckCommand
        {
            get
            {
                if (_checkCommand == null)
                {
                    _checkCommand = new DelegateCommand(() =>
                    {
                        if (SelectedWindow != null)
                        {
                            var bitmap = SelectedWindow.Capture();
                            if (bitmap != null)
                            {
                                var w = new CheckWindow(bitmap);
                                w.ShowDialog();
                            }
                        }
                    },
                    () => SelectedWindow != null);
                }
                return _checkCommand;
            }
        }

        /// <summary>
        /// フォント名・フォントサイズを設定するダイアログを表示
        /// </summary>
        public DelegateCommand SelectFontCommand
        {
            get
            {
                if (_selectFontCommand == null)
                {
                    _selectFontCommand = new DelegateCommand(() =>
                    {
                        var dialog = new System.Windows.Forms.FontDialog();

                        dialog.Font = new System.Drawing.Font(FontFamilyText, (float)FontSize);

                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            FontFamilyText = dialog.Font.FontFamily.Name;
                            FontSize = dialog.Font.SizeInPoints;
                        }
                    });
                }
                return _selectFontCommand;
            }
        }
        private DelegateCommand? _selectFontCommand;

        /// <summary>
        /// 文字色を設定するダイアログを表示
        /// </summary>
        public DelegateCommand SelectForegroundCommand
        {
            get
            {
                if (_selectForegroundCommand == null)
                {
                    _selectForegroundCommand = new DelegateCommand(() =>
                    {
                        var dialog = new System.Windows.Forms.ColorDialog();

                        dialog.Color = System.Drawing.ColorTranslator.FromHtml(ForegroundText);

                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            ForegroundText = SharpRgb(dialog.Color);
                        }
                    });
                }
                return _selectForegroundCommand;
            }
        }
        private DelegateCommand? _selectForegroundCommand;

        /// <summary>
        /// 背景色を設定するダイアログを表示
        /// </summary>
        public DelegateCommand SelectBackgroundCommand
        {
            get
            {
                if (_selectBackgroundCommand == null)
                {
                    _selectBackgroundCommand = new DelegateCommand(() =>
                    {
                        var dialog = new System.Windows.Forms.ColorDialog();

                        dialog.Color = System.Drawing.ColorTranslator.FromHtml(BackgroundText);

                        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            BackgroundText = SharpRgb(dialog.Color);
                        }
                    });
                }
                return _selectBackgroundCommand;
            }
        }
        private DelegateCommand? _selectBackgroundCommand;

        /// <summary>
        /// Colorを #rrggbb 形式に変換する
        /// </summary>
        private string SharpRgb(System.Drawing.Color color)
        {
            return "#" + color.R.ToString("X2")
                       + color.G.ToString("X2")
                       + color.B.ToString("X2");
        }


        /// <summary>
        /// カウンターを手動で変更する。
        /// </summary>
        public DelegateCommand<string> ValueCommand
        {
            get
            {
                if (_valueCommand == null)
                {
                    _valueCommand = new DelegateCommand<string>(x =>
                    {
                        if (!int.TryParse(x, out var i))
                        {
                            return;
                        }

                        if (i > 0)
                        {
                            Values[i - 1]++;
                        }
                        else if (i < 0)
                        {
                            Values[-i - 1]--;
                        }
                        RaisePropertyChanged(nameof(DisplayText));
                    });
                }
                return _valueCommand;
            }
        }
        private DelegateCommand<string>? _valueCommand;

        /// <summary>
        /// カウンターをリセットする。
        /// </summary>
        public DelegateCommand ResetCommand
        {
            get
            {
                if (_resetCommand == null)
                {
                    _resetCommand = new DelegateCommand(() =>
                    {
                        Values[0] = 0;
                        Values[1] = 0;
                        Values[2] = 0;
                        Values[3] = 0;
                        RaisePropertyChanged(nameof(DisplayText));
                    });
                }
                return _resetCommand;
            }
        }
        private DelegateCommand? _resetCommand;

        /// <summary>
        /// 表示中の文字をクリップボードにコピーする。
        /// </summary>
        public DelegateCommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                {
                    _copyCommand = new DelegateCommand(() =>
                    {
                        Clipboard.SetData(DataFormats.Text, this.DisplayText);
                    });
                }
                return _copyCommand;
            }
        }
        private DelegateCommand? _copyCommand;
    }
}
