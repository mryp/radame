using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace RadameBgTask
{
    public sealed class LiveTileUpdateTask : IBackgroundTask
    {
        private const string RADAR_JS_URL = "http://www.jma.go.jp/jp/radnowc/hisjs/radar.js";
        private const string RADAR_BASE_URL = "http://www.jma.go.jp/jp/radnowc/imgs/radar";

        private BackgroundTaskDeferral m_deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            m_deferral = taskInstance.GetDeferral();
            
            //更新処理開始
            await updateTile();

            m_deferral.Complete();
        }

        private void showToast(string text)
        {

            ToastContent content = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    TitleText = new ToastText()
                    {
                        Text = "LiveTileUpdateTask",
                    },
                    BodyTextLine1 = new ToastText()
                    {
                        Text = text,
                    }
                },
            };

            XmlDocument doc = content.GetXml();
            ToastNotification toastNotification = new ToastNotification(doc);
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        /// <summary>
        /// ライブタイル更新用バックグランドタスクを登録する
        /// </summary>
        /// <param name="time">更新間隔（分）15分以上の値を指定すること</param>
        /// <returns></returns>
        public static async void RegistTileUpdateTask(uint time)
        {
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status != BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity
            && status != BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                return;
            }

            string taskName = "LiveTileUpdateTask";
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                }
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = taskName;
            builder.TaskEntryPoint = "RadameBgTask.LiveTileUpdateTask";
            builder.SetTrigger(new TimeTrigger(time, false));
            builder.Register();

            await updateTile();
            return;
        }

        private static async Task updateTile()
        {
            string url = await getLatestImageUrl();
            string nowTime = DateTime.Now.ToString("HH:mm") + "更新";

            Debug.WriteLine("updateTime time=" + nowTime + " url=" + url);

            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        DisplayName = nowTime,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = new TileImageSource(url),
                                Overlay = 0,
                            },
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        DisplayName = nowTime,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = new TileImageSource(url),
                                Overlay = 0,
                            },
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        DisplayName = nowTime,
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = new TileImageSource(url),
                                Overlay = 0,
                            },
                        }
                    }
                }
            };

            XmlDocument doc = content.GetXml();
            TileNotification tileNotification = new TileNotification(doc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        private static async Task<string> getLatestImageUrl()
        {
            string areaCode = (string)ApplicationData.Current.RoamingSettings.Values["AreaCode"];
            string resultUrl = "";
            string json = await getHttpText(RADAR_JS_URL);
            string[] lineList = json.Split('\n');
            foreach (string line in lineList)
            {
                string fileName = getFileNameFromJsonData(line);
                if (!string.IsNullOrEmpty(fileName))
                {
                    resultUrl = getImageUrl(RADAR_BASE_URL, areaCode, fileName);
                    break;
                }
            }

            return resultUrl;
        }

        private static string getImageUrl(string baseUrl, string areaCode, string fileName)
        {
            return String.Format("{0}/{1}/{2}", baseUrl, areaCode, fileName);
        }

        private static async Task<string> getHttpText(string url)
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

        private static string getFileNameFromJsonData(string line)
        {
            string[] splitItems = line.Split('"');
            if (splitItems.Length < 2)
            {
                return "";
            }

            return splitItems[1];
        }
    }
}
