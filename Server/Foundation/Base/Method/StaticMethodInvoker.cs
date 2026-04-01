using System.Reflection;

namespace Chaos;

public sealed class StaticMethodInvoker : IStaticMethodInvoker
{
    private readonly MethodInfo methodInfo;

    private readonly object[] parameters;

    public StaticMethodInvoker(Assembly assembly, string typeName, string methodName)
    {
        methodInfo = assembly.GetType(typeName)?.GetMethod(methodName) ?? throw new ArgumentNullException();
        parameters = new object[methodInfo.GetParameters().Length];
    }

    public void Run()
    {
        methodInfo.Invoke(null, parameters);
    }

    public void Run(object p1)
    {
        parameters[0] = p1;
        methodInfo.Invoke(null, parameters);
    }

    public void Run(object p1, object p2)
    {
        parameters[0] = p1;
        parameters[1] = p2;
        methodInfo.Invoke(null, parameters);
    }

    public void Run(object p1, object p2, object p3)
    {
        parameters[0] = p1;
        parameters[1] = p2;
        parameters[2] = p3;
        methodInfo.Invoke(null, parameters);
    }

    public void Run(object p1, object p2, object p3, object p4)
    {
        parameters[0] = p1;
        parameters[1] = p2;
        parameters[2] = p3;
        parameters[3] = p4;
        methodInfo.Invoke(null, parameters);
    }
}