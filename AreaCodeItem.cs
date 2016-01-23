using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radame
{
    /// <summary>
    /// 地方選択用アイテム
    /// </summary>
    public class AreaCodeItem
    {
        /// <summary>
        /// エリアコード
        /// </summary>
        public string AreaCode
        {
            get;
            set;
        }

        /// <summary>
        /// 表示名
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 文字列
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
