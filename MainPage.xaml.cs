using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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

        /// <summary>
        /// スクロール領域のサイズ
        /// </summary>
        private Size? m_contentSize = null;

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
            
            ShowStatusBar();
        }

        /// <summary>
        /// アプリ画面アクティブ化・非アクティブ化時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            Debug.WriteLine(e.WindowActivationState);
            if (e.WindowActivationState == CoreWindowActivationState.CodeActivated)
            {
                Debug.WriteLine("Window_Activated state=" + e.WindowActivationState.ToString());
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
            Debug.WriteLine("OnNavigatedTo mode=" + e.NavigationMode);
            Window.Current.Activated += Window_Activated;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                this.Frame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
            this.DataContext = this.ViewModel;
            if (e.NavigationMode == NavigationMode.Back)
            {
                updateTask(false);
            }
        }

        /// <summary>
        /// このページから画面遷移する時のイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Debug.WriteLine("OnNavigatedFrom mode=" + e.NavigationMode);
            Window.Current.Activated -= Window_Activated;
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
        private async void updateTask(bool isTimeCheck)
        {
            if (isTimeCheck)
            {
                if (m_lastReloadTime > DateTime.Now.AddMinutes(0 - NO_REALOAD_MINUTE).Ticks)
                {
                    //更新がチェックしない範囲なので更新しない
                    Debug.WriteLine("updateTask nocheck");
                    return;
                }
            }

            Debug.WriteLine("updateTask");
            await this.ViewModel.InitClear();
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
            this.Frame.Navigate(typeof(SettingPage));
        }

        private void VersionInfoButton_Click(object sender, RoutedEventArgs e)
        {

        }
        
        /// <summary>
        /// ピボット位置を変更した時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("imagePivot_SelectionChanged");
            //updateImage(imagePivot.SelectedItem as PivotItem, m_contentSize, false);
        }

        /// <summary>
        /// スクロールバーサイズ設定時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imageScrollViewr_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("imageScrollViewr_SizeChanged");
            m_contentSize = e.NewSize;
            updateImage(imagePivot.SelectedItem as PivotItem, m_contentSize, true);
        }

        /// <summary>
        /// 画像を設定する
        /// </summary>
        /// <param name="item"></param>
        /// <param name="contentSize"></param>
        private void updateImage(PivotItem item, Size? contentSize, bool reload)
        {
            if (item == null || !contentSize.HasValue)
            {
                return;
            }

            Debug.WriteLine("updateImage item=" + item.Name);
            item.SetImage(contentSize.Value, reload);
            this.ViewModel.IsLoading = false;
        }
    }
}
