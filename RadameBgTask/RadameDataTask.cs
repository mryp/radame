using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace RadameBgTask
{
    /// <summary>
    /// レーダー画像情報取得タスククラス
    /// </summary>
    public sealed class RadameDataTask
    {
        private const string NOW_CAST_JS_URL = "http://www.jma.go.jp/jp/radnowc/hisjs/nowcast.js";
        private const string RADAR_JS_URL = "http://www.jma.go.jp/jp/radnowc/hisjs/radar.js";

        private const string NOW_CAST_BASE_URL = "http://www.jma.go.jp/jp/radnowc/imgs/nowcast";
        private const string RADAR_BASE_URL = "http://www.jma.go.jp/jp/radnowc/imgs/radar";


        /// <summary>
        /// 予測データリストを取得して返す
        /// </summary>
        /// <returns></returns>
        public static IAsyncOperation<IReadOnlyList<RadameItem>> GetNowCastItemList(string areaCode)
        {
            return GetNowCastItemListInternal(areaCode).AsAsyncOperation();
        }

        /// <summary>
        /// 予測データリストを取得して返す
        /// </summary>
        /// <returns></returns>
        private static async Task<IReadOnlyList<RadameItem>> GetNowCastItemListInternal(string areaCode)
        {
            List<RadameItem> itemList = new List<RadameItem>();
            string json = await getHttpTextInternal(NOW_CAST_JS_URL);
            string[] lineList = json.Split('\n');
            foreach (string line in lineList)
            {
                string fileName = getFileNameFromJsonData(line);
                if (!string.IsNullOrEmpty(fileName))
                {
                    DateTime time = getDateTimeFromFileName(fileName);
                    itemList.Add(new RadameItem()
                    {
                        ImageUrl = getImageUrl(NOW_CAST_BASE_URL, areaCode, fileName),
                        ImageTime = time,
                    });
                }
            }

            itemList.Reverse(); //時系列順にするため一番古いものが最初に来るようにする
            return itemList.ToArray();
        }

        /// <summary>
        /// 最新のデータ１件だけを取得して返す
        /// </summary>
        /// <returns></returns>
        public static IAsyncOperation<RadameItem> GetLatestItem(string areaCode)
        {
            return GetLatestItemInternal(areaCode).AsAsyncOperation();
        }

        private static async Task<RadameItem> GetLatestItemInternal(string areaCode)
        {
            RadameItem item = null;
            string json = await getHttpTextInternal(RADAR_JS_URL);
            string[] lineList = json.Split('\n');
            foreach (string line in lineList)
            {
                string fileName = getFileNameFromJsonData(line);
                if (!string.IsNullOrEmpty(fileName))
                {
                    DateTime time = getDateTimeFromFileName(fileName);
                    item = new RadameItem()
                    {
                        ImageUrl = getImageUrl(RADAR_BASE_URL, areaCode, fileName),
                        ImageTime = time,
                    };
                    break;
                }
            }

            return item;
        }

        /// <summary>
        /// 画像ファイル名から日時を取得する
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static DateTime getDateTimeFromFileName(string fileName)
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

        /// <summary>
        /// 雨雲レーダーURLを取得する
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="areaCode"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string getImageUrl(string baseUrl, string areaCode, string fileName)
        {
            return String.Format("{0}/{1}/{2}", baseUrl, areaCode, fileName);
        }

        /// <summary>
        /// レーダー画像列挙データからファイル名を取得する
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string getFileNameFromJsonData(string line)
        {
            string[] splitItems = line.Split('"');
            if (splitItems.Length < 2)
            {
                return "";
            }

            return splitItems[1];
        }

        /// <summary>
        /// 指定したURLからデータをダウンロードし文字列として返す
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IAsyncOperation<string> GetHttpText(string url)
        {

            return getHttpTextInternal(url).AsAsyncOperation();
        }
        
        /// <summary>
        /// 指定したURLからデータをダウンロードし文字列として返す
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> getHttpTextInternal(string url)
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

        /// <summary>
        /// 指定したURLからデータをダウンロードしファイルに保存後、ファイル情報を返す
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static IAsyncOperation<StorageFile> GetHttpFile(string url, StorageFolder saveFolder, string fileName)
        {
            return getHttpFileInternal(url, saveFolder, fileName).AsAsyncOperation();
        }

        /// <summary>
        /// 指定したURLからデータをダウンロードしファイルに保存後、ファイル情報を返す
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<StorageFile> getHttpFileInternal(string url, StorageFolder saveFolder, string fileName)
        {
            StorageFile saveFile = null;
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    byte[] data = await httpClient.GetByteArrayAsync(new Uri(url));
                    saveFile = await saveFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(saveFile, data);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("getHttpFile e=" + e.Message);
                return null;
            }

            return saveFile;
        }
    }
}
