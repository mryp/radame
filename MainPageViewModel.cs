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
        private const string NOW_CAST_JS_URL = "http://www.jma.go.jp/jp/radnowc/hisjs/nowcast.js";
        private const string RADAR_JS_URL = "http://www.jma.go.jp/jp/radnowc/hisjs/radar.js";

        private const string NOW_CAST_BASE_URL = "http://www.jma.go.jp/jp/radnowc/imgs/nowcast";
        private const string RADAR_BASE_URL = "http://www.jma.go.jp/jp/radnowc/imgs/radar";

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
            PivotItem item = null;
            string json = await getHttpText(RADAR_JS_URL);
            string[] lineList = json.Split('\n');
            foreach (string line in lineList)
            {
                string fileName = getFileNameFromJsonData(line);
                if (!string.IsNullOrEmpty(fileName))
                {
                    DateTime time = getDateTimeFromFileName(fileName);
                    item = new PivotItem()
                    {
                        Name = getPivotHeaderText(time),
                        ImageUrl = getImageUrl(RADAR_BASE_URL, getAreaCode(), fileName),
                        Time = time,
                    };
                    break;
                }
            }

            return item;
        }

        /// <summary>
        /// 予測データリストを取得して返す
        /// </summary>
        /// <returns></returns>
        private async Task<PivotItem[]> GetNowCastItemList()
        {
            List<PivotItem> itemList = new List<PivotItem>();
            string json = await getHttpText(NOW_CAST_JS_URL);
            string[] lineList = json.Split('\n');
            foreach (string line in lineList)
            {
                string fileName = getFileNameFromJsonData(line);
                if (!string.IsNullOrEmpty(fileName))
                {
                    DateTime time = getDateTimeFromFileName(fileName);
                    itemList.Add(new PivotItem()
                    {
                        Name = getPivotHeaderText(time) + "(予想)",
                        ImageUrl = getImageUrl(NOW_CAST_BASE_URL, getAreaCode(), fileName),
                        Time = time,
                    });
                }
            }

            itemList.Reverse(); //時系列順にするため一番古いものが最初に来るようにする
            return itemList.ToArray();
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
        /// 画像ファイル名から日時を取得する
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private DateTime getDateTimeFromFileName(string fileName)
        {
            //201601192310-00.png
            string[] dateItemList = Path.GetFileNameWithoutExtension(fileName).Split('-');
            if (dateItemList.Length < 2)
            {
                return DateTime.MinValue;
            }

            DateTime output;
            if (!DateTime.TryParseExact(dateItemList[0], "yyyyMMddHHmm"
                , System.Globalization.DateTimeFormatInfo.InvariantInfo
                , System.Globalization.DateTimeStyles.None
                , out output))
            {
                return DateTime.MinValue;
            }
            
            int addTime;
            if (!int.TryParse(dateItemList[1], out addTime))
            {
                return DateTime.MinValue;
            }
            
            return output.AddMinutes(addTime * 5);
        }

        private string getAreaCode()
        {
            return AppSettings.Current.AreaCode;
        }
        
        /// <summary>
        /// 画像URLを取得する
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="areaCode"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string getImageUrl(string baseUrl, string areaCode, string fileName)
        {
            return String.Format("{0}/{1}/{2}", baseUrl, areaCode, fileName);
        }

        /// <summary>
        /// JSONの行文字列からファイル名を取得する
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string getFileNameFromJsonData(string line)
        {
            string[] splitItems = line.Split('"');
            if (splitItems.Length < 2)
            {
                return "";
            }

            return splitItems[1];
        }

        /// <summary>
        /// HTTP通信で文字列をダウンロードする
        /// 失敗時は空文字を返す
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> getHttpText(string url)
        {
            string text = "";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage message = await httpClient.GetAsync(new Uri(url));
                    text = await message.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("getHttpText e=" + e.Message);
                text = "";
            }

            return text;
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
