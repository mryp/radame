using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadameBgTask
{
    public sealed class RadameItem
    {
        /// <summary>
        /// 画像URL
        /// </summary>
        public string ImageUrl
        {
            get;
            set;
        }

        /// <summary>
        /// 画像の生成時刻
        /// </summary>
        public DateTimeOffset ImageTime
        {
            get;
            set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RadameItem()
        {
            this.ImageUrl = "";
            this.ImageTime = DateTimeOffset.MinValue;
        }
    }
}
