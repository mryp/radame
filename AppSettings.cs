using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radame
{
    /// <summary>
    /// アプリケーション設定データ管理
    /// </summary>
    public class AppSettings : AppSettingsBase
    {
        private static readonly AppSettings _current = new AppSettings();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppSettings()
        {
        }

        /// <summary>
        /// 共通で使用するカレントオブジェクト
        /// </summary>
        public static AppSettings Current
        {
            get { return _current; }
        }

        /// <summary>
        /// 画像のエリアコード（3桁数値文字列）
        /// </summary>
        public string AreaCode
        {
            get { return GetValue<string>("000", ContainerType.Roaming); }
            set
            {
                SetValue(value, ContainerType.Roaming);
                OnPropertyChanged();
            }
        }

    }
}
