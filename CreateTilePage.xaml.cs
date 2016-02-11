using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;
using RadameBgTask;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Radame
{
    /// <summary>
    /// タイル作成ページ
    /// </summary>
    public sealed partial class CreateTilePage : Page
    {
        /// <summary>
        /// 拡大縮小ボタン押下時の処理単位
        /// </summary>
        private const float ZOOM_UPDOWN_UNIT = 0.25f;

        /// <summary>
        /// ビューモデル
        /// </summary>
        public CreateTileViewModel ViewModel
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CreateTilePage()
        {
            this.InitializeComponent();
            this.ViewModel = new CreateTileViewModel();
        }

        /// <summary>
        /// このページへ画面遷移した時のイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                this.Frame.CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested += SettingPage_BackRequested;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            string imageUrl = e.Parameter as string;
            Debug.WriteLine("imageUrl=" + imageUrl);

            this.ViewModel.Init(imageUrl);
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// このページから画面遷移する時のイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SystemNavigationManager.GetForCurrentView().BackRequested -= SettingPage_BackRequested;
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }

        /// <summary>
        /// 前の画面に戻る
        /// </summary>
        /// <returns></returns>
        private bool goBack()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// システムの戻るボタンをおした時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (goBack())
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// キーボード入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Back:
                    if (goBack())
                    {
                        args.Handled = true;
                    }
                    break;
            }
        }
        
        /// <summary>
        /// マウスボタンを押下したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Pointer pointer = e.Pointer;
            PointerPoint point = e.GetCurrentPoint(sender as ScrollViewer);
            if (point.Properties.PointerUpdateKind == PointerUpdateKind.XButton1Released)
            {
                goBack();
            }
        }
                
        /// <summary>
        /// 中タイルの画像読み込み完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void middleImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            middleSizeImageScrollViewr.ScrollToHorizontalOffset(AppSettings.Current.MiddleTileHorizontalOffset);
            middleSizeImageScrollViewr.ScrollToVerticalOffset(AppSettings.Current.MiddleTileVerticalOffset);
            middleSizeImageScrollViewr.ZoomToFactor(AppSettings.Current.MiddleTileZoomFactor);
        }

        /// <summary>
        /// 横長タイルの画像読み込み完了時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wideImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            wideSizeImageScrollViewr.ScrollToHorizontalOffset(AppSettings.Current.WideTileHorizontalOffset);
            wideSizeImageScrollViewr.ScrollToVerticalOffset(AppSettings.Current.WideTileVerticalOffset);
            wideSizeImageScrollViewr.ZoomToFactor(AppSettings.Current.WideTileZoomFactor);
        }

        /// <summary>
        /// タイル作成開始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TileCreateButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("middle scroll HorizontalOffset=" + middleSizeImageScrollViewr.HorizontalOffset.ToString()
                + " VerticalOffset=" + middleSizeImageScrollViewr.VerticalOffset.ToString()
                + " ZoomFactor=" + middleSizeImageScrollViewr.ZoomFactor.ToString());
            Debug.WriteLine("wide scroll HorizontalOffset=" + wideSizeImageScrollViewr.HorizontalOffset.ToString()
                + " VerticalOffset=" + wideSizeImageScrollViewr.VerticalOffset.ToString()
                + " ZoomFactor=" + wideSizeImageScrollViewr.ZoomFactor.ToString());

            AppSettings.Current.MiddleTileHorizontalOffset = (int)middleSizeImageScrollViewr.HorizontalOffset;
            AppSettings.Current.MiddleTileVerticalOffset = (int)middleSizeImageScrollViewr.VerticalOffset;
            AppSettings.Current.MiddleTileZoomFactor = middleSizeImageScrollViewr.ZoomFactor;

            AppSettings.Current.WideTileHorizontalOffset = (int)wideSizeImageScrollViewr.HorizontalOffset;
            AppSettings.Current.WideTileVerticalOffset = (int)wideSizeImageScrollViewr.VerticalOffset;
            AppSettings.Current.WideTileZoomFactor = wideSizeImageScrollViewr.ZoomFactor;
            
            LiveTileUpdateTask.RegistTileUpdateTask(uint.Parse(ViewModel.UpdateIntervalSelected));

            MessageDialog message = new MessageDialog("ライブタイルの更新完了", "情報");
            await message.ShowAsync();
        }

        /// <summary>
        /// 中タイル拡大ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void middleImageScaleUpButton_Click(object sender, RoutedEventArgs e)
        {
            scaleUpScrollViewer(middleSizeImageScrollViewr);
        }

        /// <summary>
        /// 中タイル縮小ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void middleImageScaleDownButton_Click(object sender, RoutedEventArgs e)
        {
            scaleDownScrollViewer(middleSizeImageScrollViewr);
        }

        /// <summary>
        /// 横長タイル拡大ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wideImageScaleUpButton_Click(object sender, RoutedEventArgs e)
        {
            scaleUpScrollViewer(wideSizeImageScrollViewr);
        }

        /// <summary>
        /// 横長タイル縮小ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wideImageScaleDownButton_Click(object sender, RoutedEventArgs e)
        {
            scaleDownScrollViewer(wideSizeImageScrollViewr);
        }

        /// <summary>
        /// スクロールビューを1段階拡大する
        /// </summary>
        /// <param name="scrollViewer"></param>
        private void scaleUpScrollViewer(ScrollViewer scrollViewer)
        {
            float factor = scrollViewer.ZoomFactor;
            if (factor >= this.ViewModel.MaxZoomFactor)
            {
                return;
            }

            scrollViewer.ZoomToFactor(factor + ZOOM_UPDOWN_UNIT);
        }

        /// <summary>
        /// スクロールビューを1段階縮小する
        /// </summary>
        /// <param name="scrollViewer"></param>
        private void scaleDownScrollViewer(ScrollViewer scrollViewer)
        {
            float factor = scrollViewer.ZoomFactor;
            if (factor <= this.ViewModel.MinZoomFactor)
            {
                return;
            }

            scrollViewer.ZoomToFactor(factor - ZOOM_UPDOWN_UNIT);
        }

    }
}
