using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Radame
{
    public class PivotItem : INotifyPropertyChanged
    {
        private const string NOT_FOUND_IMAGE = "ms-appx:///Assets/no_data.png";
        private const int NOT_FOUND_WIDTH = 300;
        private const int NOT_FOUND_HEIGHT = 300;

        private string m_name;
        private string m_imageUrl;
        private DateTime m_time;
        private BitmapImage m_imageData;
        private double m_height;
        private double m_width;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PivotItem()
        {
            m_name = "";
            m_imageUrl = "";
            m_imageData = null;
            m_time = DateTime.MinValue;
        }

        /// <summary>
        /// タブ名
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (value != m_name)
                {
                    m_name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 画像URL
        /// </summary>
        public string ImageUrl
        {
            get
            {
                return m_imageUrl;
            }
            set
            {
                if (value != m_imageUrl)
                {
                    m_imageUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 画像データ時刻
        /// </summary>
        public DateTime Time
        {
            get
            {
                return m_time;
            }
            set
            {
                if (value != m_time)
                {
                    m_time = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 画像データ（SetImageを呼び出すまではnull）
        /// </summary>
        public BitmapImage ImageData
        {
            get
            {
                return m_imageData;
            }
            set
            {
                if (value != m_imageData)
                {
                    m_imageData = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 画像表示高さ
        /// </summary>
        public double Height
        {
            get
            {
                return m_height;
            }
            set
            {
                if (value != m_height)
                {
                    m_height = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 画像表示幅
        /// </summary>
        public double Width
        {
            get
            {
                return m_width;
            }
            set
            {
                if (value != m_width)
                {
                    m_width = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 画像を読み込みサイズをセットする
        /// </summary>
        /// <param name="contentSize"></param>
        /// <param name="reload"></param>
        public void SetImage(Size contentSize, bool reload)
        {
            if (reload == false && m_imageData != null)
            {
                return; //すでに格納済みのため何もしない
            }
            
            try
            {
                BitmapImage image = new BitmapImage(new Uri(m_imageUrl));
                image.ImageOpened += (sender, e) =>
                {
                    //画像読み込み完了時に画像サイズを画面ぴったりにする
                    double scale = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                    double width = image.PixelWidth / scale;
                    double height = image.PixelHeight / scale;
                    if (contentSize.Height > contentSize.Width)
                    {
                        double ws = contentSize.Width / (image.PixelWidth / scale);
                        width = contentSize.Width;
                        height = (image.PixelHeight / scale) * ws;
                    }
                    else
                    {
                        double hs = contentSize.Height / (image.PixelHeight / scale);
                        width = (image.PixelWidth / scale) * hs;
                        height = contentSize.Height;
                    }

                    this.Width = width;
                    this.Height = height;
                };
                image.ImageFailed += (sender, e) =>
                {
                    Debug.WriteLine("画像読み込み失敗 e=" + e.ErrorMessage);
                    setErrorImage();
                };
                this.ImageData = image;
            }
            catch (Exception e)
            {
                Debug.WriteLine("画像読み込み失敗 e=" + e.Message);
                setErrorImage();
            }
        }

        /// <summary>
        /// 画像設定失敗時のエラー画像を設定する
        /// </summary>
        private void setErrorImage()
        {
            BitmapImage image = new BitmapImage(new Uri(NOT_FOUND_IMAGE));
            this.ImageData = image;
            this.Width = NOT_FOUND_WIDTH;
            this.Height = NOT_FOUND_HEIGHT;
        }

        private void Image_ImageOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
