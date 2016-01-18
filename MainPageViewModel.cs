using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Radame
{
    /// <summary>
    /// メイン画面のビューモデル
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {


        /// <summary>
        /// フォルダ・ファイルリスト
        /// </summary>
        private ObservableCollection<PivotItem> m_itemList = new ObservableCollection<PivotItem>();

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

        public void Init()
        {
            this.ItemList.Clear();
            this.ItemList.Add(new PivotItem() { Name = "01/18 22:20", ImageUrl = "http://www.jma.go.jp/jp/radnowc/imgs/radar/211/201601182220-00.png" });
            this.ItemList.Add(new PivotItem() { Name = "01/18 22:25の予想", ImageUrl = "http://www.jma.go.jp/jp/radnowc/imgs/nowcast/211/201601182215-02.png" });
            this.ItemList.Add(new PivotItem() { Name = "01/18 22:30の予想", ImageUrl = "http://www.jma.go.jp/jp/radnowc/imgs/nowcast/211/201601182215-03.png" });
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
