using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace Chaos
{
    public static class WebInputAdapter
    {
        /// <summary>
        /// 注入移动端 TouchData
        /// <seealso cref="TouchControl"/>
        /// </summary>
        /// <param name="touchData"></param>
        public static void Process(WebTouchData touchData)
        {
            InputSystem.QueueStateEvent(Touchscreen.current, new TouchState
            {
                touchId = touchData.Id,
                phase = touchData.Phase,
                position = new Vector2(touchData.X, touchData.Y),
            });
        }
    }
}