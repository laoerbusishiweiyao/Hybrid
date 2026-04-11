using System;
using Serilog;

namespace Chaos
{
    public interface IEvent
    {
        Type EventType { get; }
    }

    public abstract class EventHandler<TScene, TEventArgs> : IEvent where TScene : class, IScene where TEventArgs : struct
    {
        Type IEvent.EventType => typeof(TEventArgs);

        protected abstract ThreadTask RunAsync(TScene scene, TEventArgs eventArgs);

        public async ThreadTask Handle(TScene scene, TEventArgs eventArgs)
        {
            try
            {
                await RunAsync(scene, eventArgs);
            }
            catch (Exception exception)
            {
                Log.Error("{exception}", exception);
            }
        }
    }
}