using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Radame
{
    /// <summary>
    /// 設定画面用ビューモデル
    /// </summary>
    public class SettingPageModelView : INotifyPropertyChanged
    {
        /// <summary>
        /// 地方一覧を表示するための固定リスト
        /// </summary>
        private static readonly List<AreaCodeItem> DEF_AREA_CODE_LIST = new List<AreaCodeItem>()
        {
            new AreaCodeItem() { AreaCode="000", Name="全国" },
            new AreaCodeItem() { AreaCode="201", Name="北海道地方(北西部)" },
            new AreaCodeItem() { AreaCode="202", Name="北海道地方(東部)" },
            new AreaCodeItem() { AreaCode="203", Name="北海道地方(南西部)" },
            new AreaCodeItem() { AreaCode="204", Name="東北地方(北部)" },
            new AreaCodeItem() { AreaCode="205", Name="東北地方(南部)" },
            new AreaCodeItem() { AreaCode="206", Name="関東地方" },
            new AreaCodeItem() { AreaCode="207", Name="甲信地方" },
            new AreaCodeItem() { AreaCode="208", Name="北陸地方(東部)" },
            new AreaCodeItem() { AreaCode="209", Name="北陸地方(西部)" },
            new AreaCodeItem() { AreaCode="210", Name="東海地方" },
            new AreaCodeItem() { AreaCode="211", Name="近畿地方" },
            new AreaCodeItem() { AreaCode="212", Name="中国地方" },
            new AreaCodeItem() { AreaCode="213", Name="四国地方" },
            new AreaCodeItem() { AreaCode="214", Name="九州地方(北部)" },
            new AreaCodeItem() { AreaCode="215", Name="九州地方(南部)" },
            new AreaCodeItem() { AreaCode="216", Name="奄美地方" },
            new AreaCodeItem() { AreaCode="217", Name="沖縄本島地方" },
            new AreaCodeItem() { AreaCode="218", Name="大東島地方" },
            new AreaCodeItem() { AreaCode="219", Name="宮古・八重山地方" },
        };

        private string m_areaCodeSelectedCode = "";
        private ObservableCollection<AreaCodeItem> m_areaCodeList = new ObservableCollection<AreaCodeItem>();
        private string m_versionInfo = "";
        
        /// <summary>
        /// 現在選択しているエリアコード
        /// </summary>
        public string AreaCodeSelectedCode
        {
            get
            {
                return m_areaCodeSelectedCode;
            }
            set
            {
                if (value != m_areaCodeSelectedCode)
                {
                    AppSettings.Current.AreaCode = value;
                    m_areaCodeSelectedCode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// エリアコード一覧
        /// </summary>
        public ObservableCollection<AreaCodeItem> AreaCodeList
        {
            get
            {
                return m_areaCodeList;
            }
            set
            {
                if (value != m_areaCodeList)
                {
                    m_areaCodeList = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// バージョン情報
        /// </summary>
        public string VersionInfo
        {
            get
            {
                return m_versionInfo;
            }
            set
            {
                if (value != m_versionInfo)
                {
                    m_versionInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            this.AreaCodeList.Clear();
            foreach (AreaCodeItem item in DEF_AREA_CODE_LIST)
            {
                this.AreaCodeList.Add(item);
            }
            this.AreaCodeSelectedCode = AppSettings.Current.AreaCode;

            PackageVersion versionInfo = Package.Current.Id.Version;
            this.VersionInfo = String.Format("{0} version {1}.{2}.{3}.{4}"
                , Package.Current.DisplayName
                , versionInfo.Major, versionInfo.Minor, versionInfo.Build, versionInfo.Revision);
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
