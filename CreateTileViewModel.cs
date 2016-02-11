using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace Radame
{
    public class CreateTileViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 地方一覧を表示するための固定リスト
        /// </summary>
        private static readonly List<UpdateIntervalItem> DEF_INTERVAL_TIME_LIST = new List<UpdateIntervalItem>()
        {
            new UpdateIntervalItem() { IntervalTime="15", Name="15分" },
            new UpdateIntervalItem() { IntervalTime="30", Name="30分" },
            new UpdateIntervalItem() { IntervalTime="60", Name="1時間" },
            new UpdateIntervalItem() { IntervalTime="120", Name="2時間" },
            new UpdateIntervalItem() { IntervalTime="180", Name="3時間" },
            new UpdateIntervalItem() { IntervalTime="240", Name="4時間" },
        };

        private const float DEF_MAX_ZOOM_FACTOR = 8.0f;
        private const float DEF_MIN_ZOOM_FACTOR = 0.25f;
        private const int DEF_INTERVAL_TIME = 15;

        private string m_updateIntervalSelected;
        private ObservableCollection<UpdateIntervalItem> m_updateIntervalTimeList = new ObservableCollection<UpdateIntervalItem>();

        private PivotItem m_middleTileItem;
        private PivotItem m_wideTileItem;
        private float m_minZoomFactor;
        private float m_maxZoomFactor;

        public PivotItem MiddleTileItem
        {
            get
            {
                return m_middleTileItem;
            }
            set
            {
                if (value != m_middleTileItem)
                {
                    m_middleTileItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public PivotItem WideTileItem
        {
            get
            {
                return m_wideTileItem;
            }
            set
            {
                if (value != m_wideTileItem)
                {
                    m_wideTileItem = value;
                    OnPropertyChanged();
                }
            }
        }

        public float MinZoomFactor
        {
            get
            {
                return m_minZoomFactor;
            }
            set
            {
                if (value != m_minZoomFactor)
                {
                    m_minZoomFactor = value;
                    OnPropertyChanged();
                }
            }
        }

        public float MaxZoomFactor
        {
            get
            {
                return m_maxZoomFactor;
            }
            set
            {
                if (value != m_maxZoomFactor)
                {
                    m_maxZoomFactor = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string UpdateIntervalSelected
        {
            get
            {
                return m_updateIntervalSelected;
            }
            set
            {
                if (value != m_updateIntervalSelected)
                {
                    AppSettings.Current.TileUpdateTime = int.Parse(value);
                    m_updateIntervalSelected = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<UpdateIntervalItem> UpdateIntervalList
        {
            get
            {
                return m_updateIntervalTimeList;
            }
            set
            {
                if (value != m_updateIntervalTimeList)
                {
                    m_updateIntervalTimeList = value;
                    OnPropertyChanged();
                }
            }
        }


        public CreateTileViewModel()
        {
        }

        public void Init(string imageUrl)
        {
            this.MaxZoomFactor = DEF_MAX_ZOOM_FACTOR;
            this.MinZoomFactor = DEF_MIN_ZOOM_FACTOR;

            this.MiddleTileItem = new PivotItem()
            {
                ImageUrl = imageUrl,
            };
            this.MiddleTileItem.SetImage(new Size(150, 150), false);

            this.WideTileItem = new PivotItem()
            {
                ImageUrl = imageUrl,
            };
            this.WideTileItem.SetImage(new Size(310, 150), false);
            
            this.UpdateIntervalList.Clear();
            foreach (UpdateIntervalItem item in DEF_INTERVAL_TIME_LIST)
            {
                this.UpdateIntervalList.Add(item);
            }
            this.UpdateIntervalSelected = AppSettings.Current.TileUpdateTime.ToString();
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
