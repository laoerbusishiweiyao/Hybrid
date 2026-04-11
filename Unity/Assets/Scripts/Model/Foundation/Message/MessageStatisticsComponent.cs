using System;
using System.Collections.Generic;

namespace Chaos
{
    [ComponentOf]
    public sealed class MessageStatisticsComponent : Entity, IAwake, IDestroy
    {
        /// <summary>
        /// 当前秒的消息统计（Key为消息类型，Value为数量）
        /// </summary>
        public Dictionary<Type, int> MessageStats { get; set; } = new();

        /// <summary>
        /// 当前秒内的消息总数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前统计周期的开始时间（毫秒）
        /// </summary>
        public long CurrentSecondStart { get; set; }

        /// <summary>
        /// 统计周期（毫秒）默认1秒
        /// </summary>
        public long WindowSize { get; set; } = 1000;
    }
}