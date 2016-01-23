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

namespace Radame
{
    /// <summary>
    /// 設定画面ページ
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPageModelView ViewModel
        {
            get;
            set;
        }

        public SettingPage()
        {
            this.InitializeComponent();
            this.ViewModel = new SettingPageModelView();
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

            this.ViewModel.Init();
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

        private void initAreaComboBoxSelection()
        {
        }
    }
}
