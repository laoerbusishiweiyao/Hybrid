namespace Chaos;

public interface ISingletonAwake
{
    void Awake();
}

public interface ISingletonAwake<in TP>
{
    void Awake(TP p);
}

public interface ISingletonAwake<in TP1, in TP2>
{
    void Awake(TP1 p1, TP2 p2);
}

public interface ISingletonAwake<in TP1, in TP2, in TP3>
{
    void Awake(TP1 p1, TP2 p2, TP3 p3);
}

public interface ISingletonAwake<in TP1, in TP2, in TP3, in TP4>
{
    void Awake(TP1 p1, TP2 p2, TP3 p3, TP4 p4);
}