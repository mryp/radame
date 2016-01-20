using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        private const int AREA_CODE = 211;

        /// <summary>
        /// フォルダ・ファイルリスト
        /// </summary>
        private ObservableCollection<PivotItem> m_itemList = new ObservableCollection<PivotItem>();

        /// <summary>
        /// コマンドバーに表示するタイトル
        /// </summary>
        private string m_title = "";

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

        public async void Init()
        {
            this.ItemList.Clear();
            this.Title = "Radame - " + DateTime.Now.ToString("MM/dd HH:mm") + "取得";

            PivotItem latestItem = await getLatestItem();
            if (latestItem != null)
            {
                this.ItemList.Add(latestItem);
            }

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
        }

        private async Task<PivotItem> getLatestItem()
        {
            //最新のデータ１件だけを取得して返す
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
                        ImageUrl = getImageUrl(RADAR_BASE_URL, AREA_CODE, fileName),
                        Time = time,
                    };
                    break;
                }
            }

            return item;
        }

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
                        ImageUrl = getImageUrl(NOW_CAST_BASE_URL, AREA_CODE, fileName),
                        Time = time,
                    });
                }
            }

            itemList.Reverse(); //時系列順にするため一番古いものが最初に来るようにする
            return itemList.ToArray();
        }

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

        private string getImageUrl(string baseUrl, int areaCode, string fileName)
        {
            return String.Format("{0}/{1}/{2}", baseUrl, areaCode, fileName);
        }

        private string getFileNameFromJsonData(string line)
        {
            string[] splitItems = line.Split('"');
            if (splitItems.Length < 2)
            {
                return "";
            }

            return splitItems[1];
        }

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
