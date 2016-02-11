using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radame
{
    /// <summary>
    /// タイル更新時間
    /// </summary>
    public class UpdateIntervalItem
    {
        /// <summary>
        /// 更新時間（分）
        /// </summary>
        public string IntervalTime
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
