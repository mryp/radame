using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;

namespace RadameBgTask
{
    /// <summary>
    /// ライブタイル作成クラス
    /// </summary>
    public sealed class LiveTileUpdateTask : XamlRenderingBackgroundTask
    {
        /// <summary>
        /// 画像の位置情報クラス
        /// </summary>
        private class ImagePosition
        {
            public float ZoomFactor
            {
                get;
                set;
            }

            public int HorizontalOffset
            {
                get;
                set;
            }

            public int VerticalOffset
            {
                get;
                set;
            }
        }

        private const string RADAR_LOCAL_FILE_NAME = "radame.png";
        private BackgroundTaskDeferral m_deferral;
        
        /// <summary>
        /// ライブタイル更新起動
        /// </summary>
        /// <param name="taskInstance"></param>
        protected async override void OnRun(IBackgroundTaskInstance taskInstance)
        {
            m_deferral = taskInstance.GetDeferral();

            //更新処理開始
            await updateTile();

            m_deferral.Complete();
        }

        /// <summary>
        /// トールそth幼児
        /// </summary>
        /// <param name="text"></param>
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

            
            string className = typeof(LiveTileUpdateTask).FullName;
            string taskName = className;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                }
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = taskName;
            builder.TaskEntryPoint = className;
            builder.SetTrigger(new TimeTrigger(time, false));
            builder.Register();

            await updateTile();
            return;
        }

        /// <summary>
        /// アプリ設定からエリアコードを取得する
        /// </summary>
        /// <returns></returns>
        private static string getSettingAreaCode()
        {
            return (string)ApplicationData.Current.RoamingSettings.Values["AreaCode"];
        }

        /// <summary>
        /// アプリ設定から中タイルに表示する画像の位置情報を取得する
        /// </summary>
        /// <returns></returns>
        private static ImagePosition getSettingMiddleTilePosition()
        {
            ImagePosition pos = new ImagePosition()
            {
                HorizontalOffset = (int)ApplicationData.Current.RoamingSettings.Values["MiddleTileHorizontalOffset"],
                VerticalOffset = (int)ApplicationData.Current.RoamingSettings.Values["MiddleTileVerticalOffset"],
                ZoomFactor = (float)ApplicationData.Current.RoamingSettings.Values["MiddleTileZoomFactor"],
            };

            return pos;
        }

        /// <summary>
        /// アプリ設定から横長タイルに表示する画像の位置情報を取得する
        /// </summary>
        /// <returns></returns>
        private static ImagePosition getSettingWideTilePosition()
        {
            ImagePosition pos = new ImagePosition()
            {
                HorizontalOffset = (int)ApplicationData.Current.RoamingSettings.Values["WideTileHorizontalOffset"],
                VerticalOffset = (int)ApplicationData.Current.RoamingSettings.Values["WideTileVerticalOffset"],
                ZoomFactor = (float)ApplicationData.Current.RoamingSettings.Values["WideTileZoomFactor"],
            };

            return pos;
        }

        /// <summary>
        /// ライブタイルの画像を生成しタイルに設定し直す
        /// </summary>
        /// <returns></returns>
        private static async Task updateTile()
        {
            //最新のレーダー画像を取得
            RadameItem item = await RadameDataTask.GetLatestItem(getSettingAreaCode());
            string nowTime = DateTime.Now.ToString("HH:mm") + "更新";
            StorageFile imageFile = await RadameDataTask.GetHttpFile(item.ImageUrl, getLocalFolder(), RADAR_LOCAL_FILE_NAME);
            if (imageFile == null)
            {
                return;
            }

            //設定された位置の画像を生成する
            Debug.WriteLine("updateTile imageFile.Path=" + imageFile.Path);
            StorageFile mediumImage = await resizeBitmap(imageFile, 150, 150
                , getSettingMiddleTilePosition());
            StorageFile wideImage = await resizeBitmap(imageFile, 310, 150
                , getSettingWideTilePosition());

            //タイルXML作成
            Debug.WriteLine("updateTile time=" + nowTime + " url=" + item.ImageUrl);
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.None,
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = new TileImageSource(mediumImage.Path),
                                Overlay = 0,
                            },
                            Children =
                            {
                                new TileText() { Text = nowTime }
                            },
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = new TileImageSource(wideImage.Path),
                                Overlay = 0,
                            },
                            Children =
                            {
                                new TileText() { Text = nowTime }
                            },
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = new TileImageSource(imageFile.Path),
                                Overlay = 0,
                            },
                            Children =
                            {
                                new TileText() { Text = nowTime }
                            },
                        }
                    }
                }
            };

            //更新設定
            XmlDocument doc = content.GetXml();
            TileNotification tileNotification = new TileNotification(doc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
        
        /// <summary>
        /// アプリケーション用フォルダ（書き込みに権限不要）を取得する
        /// </summary>
        /// <returns></returns>
        public static StorageFolder getLocalFolder()
        {
            return ApplicationData.Current.LocalFolder;
        }

        /// <summary>
        /// 指定したサイズと位置情報に画像ファイルをリサイズする
        /// </summary>
        /// <param name="file"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private static async Task<StorageFile> resizeBitmap(StorageFile file, int width, int height, ImagePosition pos)
        {
            WriteableBitmap wb;
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                wb = await BitmapFactory.New(1, 1).FromStream(stream);
            }
            WriteableBitmap resizeWb = wb.Resize((int)(wb.PixelWidth * pos.ZoomFactor), (int)(wb .PixelHeight * pos.ZoomFactor), WriteableBitmapExtensions.Interpolation.Bilinear);
            WriteableBitmap croppedWb = resizeWb.Crop(pos.HorizontalOffset, pos.VerticalOffset, width, height);
            
            //ファイルに保存
            StorageFolder folder = getLocalFolder();
            string fileName = "radame_" + width.ToString() + "x" + height.ToString() + ".png";
            StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine("resizeBitmap path=" + saveFile.Path);

            await saveToPngFile(croppedWb, saveFile);
            return saveFile;
        }

        /// <summary>
        /// PNGファイルとして保存する
        /// </summary>
        /// <param name="writeableBitmap"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        private static async Task saveToPngFile(WriteableBitmap writeableBitmap, StorageFile outputFile)
        {
            Guid encoderId = BitmapEncoder.PngEncoderId;
            Stream stream = writeableBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[(uint)stream.Length];
            await stream.ReadAsync(pixels, 0, pixels.Length);

            using (IRandomAccessStream writeStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, writeStream);
                encoder.SetPixelData(
                    BitmapPixelFormat.Rgba8,
                    BitmapAlphaMode.Ignore,
                    (uint)writeableBitmap.PixelWidth,
                    (uint)writeableBitmap.PixelHeight,
                    96,
                    96,
                    pixels);
                await encoder.FlushAsync();

                using (IOutputStream outputStream = writeStream.GetOutputStreamAt(0))
                {
                    await outputStream.FlushAsync();
                }
            }
        }
    }
}
