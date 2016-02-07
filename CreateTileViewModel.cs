using System;
using System.Collections.Generic;
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
        private PivotItem m_middleTileItem;
        private PivotItem m_wideTileItem;

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
        

        public CreateTileViewModel()
        {
        }

        public async void Init(string imageUrl)
        {
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
