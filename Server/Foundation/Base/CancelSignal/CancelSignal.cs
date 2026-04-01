namespace Chaos;

public class CancelSignal
{
    private HashSetComponent<Action> actions = HashSetComponent<Action>.Create();

    public object Context;

    public void Add(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        actions.Add(callback);
    }

    public void Remove(Action callback)
    {
        actions?.Remove(callback);
    }

    public bool IsDisposed => actions == null;

    public void Cancel()
    {
        if (actions == null)
        {
            return;
        }

        Invoke();
    }

    private void Invoke()
    {
        using var runActions = actions;
        actions = null;
        try
        {
            foreach (var action in runActions)
            {
                action.Invoke();
            }
        }
        catch (Exception exception)
        {
            ThreadTask.ExceptionHandler.Invoke(exception);
        }
    }
}