using System;

namespace Chaos
{
    public interface IAwake
    {
    }

    public interface IAwake<in TP1>
    {
    }

    public interface IAwake<in TP1, in TP2>
    {
    }

    public interface IAwake<in TP1, in TP2, in TP3>
    {
    }

    public interface IAwake<in TP1, in TP2, in TP3, in TP4>
    {
    }

    public interface IAwakeSystem : ISystemType
    {
        void Run(Entity entity);
    }

    public interface IAwakeSystem<in TP1> : ISystemType
    {
        void Run(Entity entity, TP1 p1);
    }

    public interface IAwakeSystem<in TP1, in TP2> : ISystemType
    {
        void Run(Entity entity, TP1 p1, TP2 p2);
    }

    public interface IAwakeSystem<in TP1, in TP2, in TP3> : ISystemType
    {
        void Run(Entity entity, TP1 p1, TP2 p2, TP3 p3);
    }

    public interface IAwakeSystem<in TP1, in TP2, in TP3, in TP4> : ISystemType
    {
        void Run(Entity entity, TP1 p1, TP2 p2, TP3 p3, TP4 p4);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T> : SystemObject, IAwakeSystem where T : Entity, IAwake
    {
        Type ISystemType.EntityType => typeof(T);

        Type ISystemType.SystemType => typeof(IAwakeSystem);

        void IAwakeSystem.Run(Entity entity)
        {
            Awake((T)entity);
        }

        protected abstract void Awake(T self);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T, TP1> : SystemObject, IAwakeSystem<TP1> where T : Entity, IAwake<TP1>
    {
        Type ISystemType.EntityType => typeof(T);

        Type ISystemType.SystemType => typeof(IAwakeSystem<TP1>);

        void IAwakeSystem<TP1>.Run(Entity entity, TP1 p1)
        {
            Awake((T)entity, p1);
        }

        protected abstract void Awake(T self, TP1 p1);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T, TP1, TP2> : SystemObject, IAwakeSystem<TP1, TP2> where T : Entity, IAwake<TP1, TP2>
    {
        Type ISystemType.EntityType => typeof(T);

        Type ISystemType.SystemType => typeof(IAwakeSystem<TP1, TP2>);

        void IAwakeSystem<TP1, TP2>.Run(Entity entity, TP1 p1, TP2 p2)
        {
            Awake((T)entity, p1, p2);
        }

        protected abstract void Awake(T self, TP1 p1, TP2 p2);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T, TP1, TP2, TP3> : SystemObject, IAwakeSystem<TP1, TP2, TP3> where T : Entity, IAwake<TP1, TP2, TP3>
    {
        Type ISystemType.EntityType => typeof(T);

        Type ISystemType.SystemType => typeof(IAwakeSystem<TP1, TP2, TP3>);

        void IAwakeSystem<TP1, TP2, TP3>.Run(Entity entity, TP1 p1, TP2 p2, TP3 p3)
        {
            Awake((T)entity, p1, p2, p3);
        }

        protected abstract void Awake(T self, TP1 p1, TP2 p2, TP3 p3);
    }

    [EntitySystem]
    public abstract class AwakeSystem<T, TP1, TP2, TP3, TP4> : SystemObject, IAwakeSystem<TP1, TP2, TP3, TP4> where T : Entity, IAwake<TP1, TP2, TP3, TP4>
    {
        Type ISystemType.EntityType => typeof(T);

        Type ISystemType.SystemType => typeof(IAwakeSystem<TP1, TP2, TP3, TP4>);

        void IAwakeSystem<TP1, TP2, TP3, TP4>.Run(Entity entity, TP1 p1, TP2 p2, TP3 p3, TP4 p4)
        {
            Awake((T)entity, p1, p2, p3, p4);
        }

        protected abstract void Awake(T self, TP1 p1, TP2 p2, TP3 p3, TP4 p4);
    }
}