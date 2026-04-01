namespace Chaos;

public interface IStaticMethodInvoker
{
    void Run();
    void Run(object p1);
    void Run(object p1, object p2);
    void Run(object p1, object p2, object p3);
    void Run(object p1, object p2, object p3, object p4);
}