using RadameBgTask;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace Radame
{
    /// <summary>
    /// メイン画面のビューモデル
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 読み込み後の反映待ち時間
        /// </summary>
        private const int DEF_INIT_WAIT_TIME = 1000;

        /// <summary>
        /// フォルダ・ファイルリスト
        /// </summary>
        private ObservableCollection<PivotItem> m_itemList = new ObservableCollection<PivotItem>();

        /// <summary>
        /// コマンドバーに表示するタイトル
        /// </summary>
        private string m_title = "";

        /// <summary>
        /// データ読込中かどうか
        /// </summary>
        private bool m_isLoading = true;

        /// <summary>
        /// フォルダ・ファイルリスト
        /// </summary>
        public ObservableCollection<PivotItem> ItemList
        {
            get
            {
                return m_itemList;
            }
            set
            {
                if (value != m_itemList)
                {
                    m_itemList = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title
        {
            get
            {
                return m_title;
            }
            set
            {
                if (value != m_title)
                {
                    m_title = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// データ読込中かどうか
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return m_isLoading;
            }
            set
            {
                if (value != m_isLoading)
                {
                    m_isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// データをすべてクリアーする
        /// </summary>
        /// <returns></returns>
        public async Task<int> InitClear()
        {
            return await InitClear(DEF_INIT_WAIT_TIME);
        }

        /// <summary>
        /// データをすべてクリアーする
        /// </summary>
        /// <param name="waitTime">処理待ち時間</param>
        /// <returns></returns>
        public async Task<int> InitClear(int waitTime)
        {
            this.ItemList.Clear();
            this.IsLoading = true;
            this.Title = createTitleName("");
            if (waitTime > 0)
            {
                //ピボット切り替え直後にクリアーすると画像が表示されないことがあるため少し待つ
                await Task.Delay(waitTime);
            }

            return waitTime;
        }

        /// <summary>
        /// データ取得
        /// </summary>
        public async void Init()
        {
            if (m_itemList.Count > 0)
            {
                await this.InitClear();
            }

            PivotItem latestItem = await getLatestItem();
            if (latestItem == null)
            {
                initError();
                return;
            }
            this.ItemList.Add(latestItem);

            PivotItem[] nowcastList = await GetNowCastItemList();
            if (nowcastList != null && nowcastList.Length > 0)
            {
                foreach (PivotItem item in nowcastList)
                {
                    if (item.Time > latestItem.Time)
                    {
                        this.ItemList.Add(item);
                    }
                }
            }
            
            this.Title = createTitleName(DateTime.Now.ToString("MM/dd HH:mm") + "取得");
        }

        /// <summary>
        /// 取得失敗表示
        /// </summary>
        private void initError()
        {
            this.Title = createTitleName("データ取得エラー");
            this.IsLoading = false;
        }

        /// <summary>
        /// タイトルバーに表示する文字列を生成する
        /// </summary>
        /// <param name="addText">アプリ名の後ろにつける文字列（不要なときは空文字を指定）</param>
        /// <returns></returns>
        private string createTitleName(string addText)
        {
            string title = Windows.ApplicationModel.Package.Current.DisplayName;
            if (!string.IsNullOrEmpty(addText))
            {
                title += " - " + addText;
            }

            return title;
        }

        /// <summary>
        /// 最新のデータ１件だけを取得して返す
        /// </summary>
        /// <returns></returns>
        private async Task<PivotItem> getLatestItem()
        {
            RadameItem item = await RadameDataTask.GetLatestItem(getAreaCode());
            return new PivotItem()
            {
                Name = getPivotHeaderText(item.ImageTime.DateTime),
                ImageUrl = item.ImageUrl,
                Time = item.ImageTime.DateTime,
            };
        }

        private async Task<PivotItem[]> GetNowCastItemList()
        {
            List<PivotItem> pivotItemList = new List<PivotItem>();
            IReadOnlyList<RadameItem> radameItemList = await RadameDataTask.GetNowCastItemList(getAreaCode());
            foreach (RadameItem radameItem in radameItemList)
            {
                pivotItemList.Add(new PivotItem()
                {
                    Name = getPivotHeaderText(radameItem.ImageTime.DateTime) + "(予想)",
                    ImageUrl = radameItem.ImageUrl,
                    Time = radameItem.ImageTime.DateTime,
                });
            }

            return pivotItemList.ToArray();
        }
        
        /// <summary>
        /// 日付からピボットのタイトルヘッダー文字列を生成して返す
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string getPivotHeaderText(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return "??/?? ??:??";
            }
            if (DateTime.Now.Date == date.Date)
            {
                return date.ToString("HH:mm");
            }
            else
            {
                return date.ToString("MM/dd HH:mm");
            }
        }

        /// <summary>
        /// 現在のエリアコードを取得する
        /// </summary>
        /// <returns></returns>
        private string getAreaCode()
        {
            return AppSettings.Current.AreaCode;
        }
        #region INotifyPropertyChanged member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
