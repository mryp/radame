using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace Radame
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// データ更新の最小時間（分）
        /// </summary>
        private const int NO_REALOAD_MINUTE = 5;

        /// <summary>
        /// 拡大倍率
        /// </summary>
        private const double SCALE_VALUE = 2.0;

        /// <summary>
        /// 最終更新時間
        /// </summary>
        private static long m_lastReloadTime = 0;

        private Size m_windowSize = new Size();

        /// <summary>
        /// モデルビュー
        /// </summary>
        public MainPageViewModel ViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.ViewModel = new MainPageViewModel();

            //イベント設定
            Application.Current.Resuming += App_Resuming;
            Window.Current.Activated += Window_Activated;
            ShowStatusBar();
        }

        /// <summary>
        /// アプリ再開起動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_Resuming(object sender, object e)
        {
        }

        /// <summary>
        /// アプリ画面アクティブ化・非アクティブ化時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            Debug.WriteLine(e.WindowActivationState);
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.CodeActivated)
            {
                updateTask(true);
            }
        }

        /// <summary>
        /// このページへ画面遷移した時のイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// このページから画面遷移する時のイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("Page_SizeChanged size=" + e.NewSize.ToString());
            m_windowSize = e.NewSize;
        }

        /// <summary>
        /// ステータスバーの表示初期化
        /// </summary>
        private async void ShowStatusBar()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                //モバイル時のみステータスバーの色を背景色に合わせる
                var statusbar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                await statusbar.ShowAsync();
                statusbar.BackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6);
                statusbar.BackgroundOpacity = 1;
                statusbar.ForegroundColor = Windows.UI.Colors.Black;
            }
        }

        /// <summary>
        /// データを更新する
        /// </summary>
        /// <param name="isTimeCheck"></param>
        private void updateTask(bool isTimeCheck)
        {
            if (isTimeCheck)
            {
                if (m_lastReloadTime > DateTime.Now.AddMinutes(0 - NO_REALOAD_MINUTE).Ticks)
                {
                    //更新がチェックしない範囲なので更新しない
                    return;
                }
            }

            m_lastReloadTime = DateTime.Now.Ticks;
            this.ViewModel.Init();
        }

        /// <summary>
        /// 更新処理する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            updateTask(false);
        }

        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VersionInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Image image = sender as Image;
            ScrollViewer scrollViewr = image.Parent as ScrollViewer;
            if (image == null || scrollViewr == null)
            {
                return;
            }
            Point touchPos = e.GetPosition(image);

            Debug.WriteLine("Image_DoubleTapped touchPos=" + touchPos.ToString());
            if (image.ActualWidth > m_windowSize.Width || image.ActualHeight > m_windowSize.Height)
            {
                Size? orgSize = image.Tag as Size?;
                if (orgSize != null)
                {
                    scrollViewr.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    image.Height = orgSize.Value.Height;
                    image.Width = orgSize.Value.Width;
                }
            }
            else
            {
                image.Tag = new Size(image.ActualWidth, image.ActualHeight);    //もとに戻すため現在のサイズを覚える
                image.Height = image.ActualHeight * SCALE_VALUE;
                image.Width = image.ActualWidth * SCALE_VALUE;

                /*
                scrollViewr.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                scrollViewr.ScrollToHorizontalOffset(touchPos.X);
                scrollViewr.ScrollToVerticalOffset(touchPos.Y);
                */
            }
        }
    }
}
