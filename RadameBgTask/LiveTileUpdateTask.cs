using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace RadameBgTask
{
    public sealed class LiveTileUpdateTask : IBackgroundTask
    {
        BackgroundTaskDeferral m_deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            m_deferral = taskInstance.GetDeferral();

            showToast("ライブタイル更新：" + DateTime.Now.ToString("HH:mm:ss"));

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
    }
}
