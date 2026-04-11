using System.Linq;

namespace Chaos
{
    [EntitySystemOf(typeof(ObjectWait))]
    public static partial class ObjectWaitSystem
    {
        [EntitySystem]
        private static void Awake(this ObjectWait self)
        {
            self.AllTask.Clear();
        }

        [EntitySystem]
        private static void Destroy(this ObjectWait self)
        {
            foreach (var callback in self.AllTask.Values.ToArray())
            {
                ((IObjectWaitCallbackDestroy)callback).SetResult();
            }
        }

        public static async ThreadTask<T> WaitAsync<T>(this ObjectWait self) where T : struct, IWaitType
        {
            ObjectWaitCallback<T> callback = new();
            var cancelSignal = await ThreadTask.GetContextAsync<CancelSignal>();
            self.AllTask.Add(typeof(T), callback);
            EntityReference<ObjectWait> reference = self;

            T result;
            try
            {
                cancelSignal?.Add(CancelAction);
                result = await callback.Task;
            }
            finally
            {
                cancelSignal?.Remove(CancelAction);
            }

            return result;

            void CancelAction()
            {
                ObjectWait objectWait = reference;
                objectWait.Notify(new T { StatusCode = WaitTypeCodes.Cancel });
            }
        }

        public static void Notify<T>(this ObjectWait self, T result) where T : struct, IWaitType
        {
            var type = typeof(T);
            if (!self.AllTask.Remove(type, out var callback))
            {
                return;
            }

            ((ObjectWaitCallback<T>)callback).SetResult(result);
        }
    }
}