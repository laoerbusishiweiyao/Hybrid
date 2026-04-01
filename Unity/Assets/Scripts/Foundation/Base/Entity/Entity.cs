using System;
using System.Collections.Generic;
using System.Diagnostics;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using Serilog;

namespace Chaos
{
    [Flags]
    public enum EntityFlags
    {
        None = 0,

        /// <summary>
        /// 实体是否来自对象池（用于回收逻辑）
        /// </summary>
        IsFromPool = 1,

        /// <summary>
        /// 实体是否已注册到事件系统
        /// </summary>
        IsRegistered = 1 << 1,

        /// <summary>
        /// 实体本身是一个 Component（而非纯 Entity）
        /// </summary>
        IsComponent = 1 << 2,

        /// <summary>
        /// 跳过反序列化 System 的执行（性能优化）
        /// </summary>
        DisableDeserializeSystem = 1 << 3,

        /// <summary>
        /// 序列化时包含父实体数据
        /// </summary>
        SerializeWithParent = 1 << 4,
    }

    [MemoryPackable(GenerateType.NoGenerate)]
    public abstract partial class Entity : DisposableObject, IPoolable
    {
        public static T Fetch<T>() where T : Entity
        {
            return ObjectPool.Rent<T>();
        }

        public virtual long GetLongHashCode()
        {
            return GetType().TypeHandle.Value.ToInt64();
        }

        public virtual long GetComponentLongHashCode(Type type)
        {
            return type.TypeHandle.Value.ToInt64();
        }

#if UNITY_EDITOR && HIERARCHY
        [BsonIgnore]
        [UnityEngine.HideInInspector]
        [MemoryPackIgnore]
        public UnityEngine.GameObject HierarchyGameObject;
#endif

        [MemoryPackIgnore]
        [BsonIgnore]
        public int InstanceId { get; protected set; }

        [BsonIgnore]
        private EntityFlags flags = EntityFlags.None;

        [MemoryPackIgnore]
        [BsonIgnore]
        public bool IsFromPool
        {
            get => (flags & EntityFlags.IsFromPool) == EntityFlags.IsFromPool;
            set
            {
                if (value)
                {
                    flags |= EntityFlags.IsFromPool;
                }
                else
                {
                    flags &= ~EntityFlags.IsFromPool;
                }
            }
        }

        [BsonIgnore]
        protected bool IsRegister
        {
            get => (flags & EntityFlags.IsRegistered) == EntityFlags.IsRegistered;
            set
            {
                if (IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    flags |= EntityFlags.IsRegistered;
                }
                else
                {
                    flags &= ~EntityFlags.IsRegistered;
                }

                if (value)
                {
                    RegisterSystem();
                }

#if UNITY_EDITOR && HIERARCHY
                if (value)
                {
                    HierarchyGameObject = new UnityEngine.GameObject(HierarchyName);
                    HierarchyGameObject.AddComponent<ComponentView>().Component = this;
                    HierarchyGameObject.transform.SetParent(parent == null ? UnityEngine.GameObject.Find("Hierarchy/Scenes").transform : parent.HierarchyGameObject.transform);
                }
                else
                {
                    UnityEngine.Object.Destroy(HierarchyGameObject);
                }
#endif
            }
        }

        protected virtual void RegisterSystem()
        {
            scene.Fiber.EntitySystem.RegisterSystem(this);
        }

        protected virtual string HierarchyName => GetType().FullName;

        [BsonIgnore]
        protected bool IsComponent
        {
            get => (flags & EntityFlags.IsComponent) == EntityFlags.IsComponent;
            set
            {
                if (value)
                {
                    flags |= EntityFlags.IsComponent;
                }
                else
                {
                    flags &= ~EntityFlags.IsComponent;
                }
            }
        }

        [BsonIgnore]
        protected bool DisableDeserializeSystem
        {
            get => (flags & EntityFlags.DisableDeserializeSystem) == EntityFlags.DisableDeserializeSystem;
            set
            {
                if (value)
                {
                    flags |= EntityFlags.DisableDeserializeSystem;
                }
                else
                {
                    flags &= ~EntityFlags.DisableDeserializeSystem;
                }
            }
        }

        [BsonIgnore]
        public bool SerializeWithParent
        {
            get => (flags & EntityFlags.SerializeWithParent) == EntityFlags.SerializeWithParent;
            set
            {
                if (value)
                {
                    flags |= EntityFlags.SerializeWithParent;
                }
                else
                {
                    flags &= ~EntityFlags.SerializeWithParent;
                }
            }
        }

        [MemoryPackIgnore]
        [BsonIgnore]
        public bool IsDisposed => InstanceId == 0;

        private Entity parent;

        [MemoryPackIgnore]
        [BsonIgnore]
        public Entity Parent
        {
            get => parent;
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {GetType().FullName}");
                }

                // 严格限制parent必须要有iSence,也就是说parent必须在数据树上面
                if (value.Scene == null)
                {
                    throw new Exception($"cant set parent because parent iSence is null: {GetType().FullName} {value.GetType().FullName}");
                }

                if (parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (parent == value)
                    {
                        Log.Error($"重复设置了Parent: {GetType().FullName} parent: {parent.GetType().FullName}");
                        return;
                    }

                    parent.RemoveChild(Id, false);
                }

                parent = value;
                IsComponent = false;
                parent.AddToChildren(this);

                if (this is IScene sceneEntity)
                {
                    sceneEntity.Fiber = parent.scene.Fiber;
                    Scene = sceneEntity;
                }
                else
                {
                    Scene = parent.scene;
                }

#if UNITY_EDITOR && HIERARCHY
                HierarchyGameObject.GetComponent<ComponentView>().Component = this;
                HierarchyGameObject.transform.SetParent(parent == null ? UnityEngine.GameObject.Find("Hierarchy").transform : parent.HierarchyGameObject.transform);
                foreach (var child in Children.Values)
                {
                    child.HierarchyGameObject.transform.SetParent(HierarchyGameObject.transform);
                }

                foreach (var comp in Components.Values)
                {
                    comp.HierarchyGameObject.transform.SetParent(HierarchyGameObject.transform);
                }
#endif
            }
        }

        // 该方法只能在AddComponent中调用，其他人不允许调用
        [BsonIgnore]
        private Entity ComponentParent
        {
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {GetType().FullName}");
                }

                if (value == this)
                {
                    throw new Exception($"cant set parent self: {GetType().FullName}");
                }

                // 严格限制parent必须要有iSence,也就是说parent必须在数据树上面
                if (value.Scene == null)
                {
                    throw new Exception($"cant set parent because parent iSence is null: {GetType().FullName} {value.GetType().FullName}");
                }

                if (parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (parent == value)
                    {
                        Log.Error($"重复设置了Parent: {GetType().FullName} parent: {parent.GetType().FullName}");
                        return;
                    }

                    parent.RemoveComponent(GetType(), false);
                }

                parent = value;
                IsComponent = true;
                parent.AddToComponents(this);

                if (this is IScene sceneEntity)
                {
                    sceneEntity.Fiber = parent.scene.Fiber;
                    Scene = sceneEntity;
                }
                else
                {
                    Scene = parent.scene;
                }
            }
        }

        public T GetParent<T>() where T : Entity
        {
            return parent as T;
        }

        [BsonIgnoreIfDefault]
        [BsonDefaultValue(0L)]
        [BsonElement]
        [BsonId]
        public long Id { get; protected set; }

        [BsonIgnore]
        protected IScene scene;

        [MemoryPackIgnore]
        [BsonIgnore]
        public IScene Scene
        {
            get => scene;
            protected set
            {
                if (value == null)
                {
                    throw new Exception($"iScene cant set null: {GetType().FullName}");
                }

                if (scene == value)
                {
                    return;
                }

                if (scene != null)
                {
                    scene = value;
                    return;
                }

                scene = value;

                if (InstanceId == 0)
                {
                    InstanceId = scene.Fiber.NewInstanceId();
                }

                IsRegister = true;

                if (this is ISerializableEntity)
                {
                    SerializeWithParent = true;
                }

                // 反序列化出来的需要设置父子关系
                if (components != null)
                {
                    foreach (var (_, component) in components)
                    {
                        component.IsComponent = true;
                        component.parent = this;
                        component.Scene = scene;
                    }
                }

                if (children != null)
                {
                    foreach (var (_, child) in children)
                    {
                        child.IsComponent = false;
                        child.parent = this;
                        child.Scene = scene;
                    }
                }

                if (!DisableDeserializeSystem)
                {
                    EntitySystemRegistry.Default.Deserialize(this);
                }
            }
        }

        [MemoryPackInclude]
        [BsonElement]
        [BsonIgnoreIfNull]
        protected ChildCollection children;

        [MemoryPackIgnore]
        [BsonIgnore]
        public ChildCollection Children
        {
            get { return children ??= ObjectPool.Rent<ChildCollection>(); }
        }

        private void AddToChildren(Entity entity)
        {
            Children.Add(entity.Id, entity);
        }

        [MemoryPackInclude]
        [BsonElement]
        [BsonIgnoreIfNull]
        protected ComponentCollection components;

        [MemoryPackIgnore]
        [BsonIgnore]
        public ComponentCollection Components
        {
            get { return components ??= ObjectPool.Rent<ComponentCollection>(); }
        }

        public int ComponentCount => components?.Count ?? 0;

        public int ChildCount => children?.Count ?? 0;

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsRegister = false;
            InstanceId = 0;

            // 清理Children
            if (children != null)
            {
                foreach (var child in children.Values)
                {
                    child.Dispose();
                }

                children.Dispose();
                children = null;
            }

            // 清理Component
            if (components != null)
            {
                foreach (var kv in components)
                {
                    kv.Value.Dispose();
                }

                components.Dispose();
                components = null;
            }

            // 触发Destroy事件
            if (this is IDestroy)
            {
                EntitySystemRegistry.Default.Destroy(this);
            }

            scene = null;

            if (parent != null && !parent.IsDisposed)
            {
                if (IsComponent)
                {
                    parent.RemoveComponent(GetType(), false);
                }
                else
                {
                    parent.RemoveChild(Id, false);
                }
            }

            parent = null;

            base.Dispose();

            // 把status字段除了IsFromPool其它的status标记都还原
            var isFromPool = IsFromPool;
            flags = EntityFlags.None;
            IsFromPool = isFromPool;

            ObjectPool.Recycle(this);
        }

        private void AddToComponents(Entity component)
        {
            Components.Add(component.GetLongHashCode(), component);
        }

        public TChild GetChild<TChild>(long id) where TChild : Entity
        {
            CheckThread();

            if (children == null)
            {
                return null;
            }

            children.TryGetValue(id, out var child);
            return child as TChild;
        }

        public bool RemoveChild(long id, bool isDispose = true)
        {
            if (children == null)
            {
                return false;
            }

            if (!children.Remove(id, out var child))
            {
                return false;
            }

            if (children.Count == 0)
            {
                children.Dispose();
                children = null;
            }

            if (isDispose)
            {
                child.Dispose();
            }
            else
            {
                child.Reset();
            }

            return true;
        }

        public void RemoveComponent<TComponent>(bool isDispose = true) where TComponent : Entity
        {
            if (IsDisposed)
            {
                return;
            }

            if (components == null)
            {
                return;
            }

            var type = typeof(TComponent);

            if (!components.Remove(GetComponentLongHashCode(type), out var c))
            {
                return;
            }

            if (isDispose)
            {
                c.Dispose();
            }
            else
            {
                c.Reset();
            }
        }

        public void RemoveComponent(Type type, bool isDispose = true)
        {
            if (IsDisposed)
            {
                return;
            }

            if (components == null)
            {
                return;
            }

            if (!components.Remove(GetComponentLongHashCode(type), out var c))
            {
                return;
            }

            if (isDispose)
            {
                c.Dispose();
            }
            else
            {
                c.Reset();
            }
        }

        [Conditional("DEBUG")]
        private void CheckThread()
        {
            var fiber = Scene.Fiber;
            if (fiber.ThreadSynchronizationContext.ThreadId != Environment.CurrentManagedThreadId)
            {
                throw new Exception($"Fiber {fiber.Id} {fiber.Name} is not in fiber thread {fiber.ThreadSynchronizationContext.ThreadId} {Environment.CurrentManagedThreadId}");
            }
        }

        public TComponent GetComponent<TComponent>() where TComponent : Entity
        {
            CheckThread();

            if (components == null)
            {
                return null;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponentLifespanSystem)
            {
                EntitySystemRegistry.Default.GetComponent(this, typeof(TComponent));
            }

            if (!components.TryGetValue(GetComponentLongHashCode(typeof(TComponent)), out var component))
            {
                return null;
            }

            return (TComponent)component;
        }

        public Entity GetComponent(Type type)
        {
            CheckThread();

            if (components == null)
            {
                return null;
            }

            if (this is IGetComponentLifespanSystem)
            {
                EntitySystemRegistry.Default.GetComponent(this, type);
            }

            return components.GetValueOrDefault(GetComponentLongHashCode(type));
        }

        private static Entity Create(Type type, bool isFromPool)
        {
            var component = (Entity)ObjectPool.Rent(type, isFromPool);

            component.IsFromPool = isFromPool;
            component.DisableDeserializeSystem = true;
            component.Id = 0;
            return component;
        }

        public Entity AddComponent(Entity component)
        {
            CheckThread();

            var type = component.GetType();
            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;


            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component;
        }

        public Entity AddComponent(Type type, bool isFromPool = false)
        {
            CheckThread();

            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = Id;
            component.ComponentParent = this;
            EntitySystemRegistry.Default.Awake(component);

            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component;
        }

        public TComponent AddComponentWithId<TComponent>(long id, bool isFromPool = false) where TComponent : Entity, IAwake, new()
        {
            CheckThread();

            var type = typeof(TComponent);
            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemRegistry.Default.Awake(component);

            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component as TComponent;
        }

        public TComponent AddComponentWithId<TComponent, TP1>(long id, TP1 p1, bool isFromPool = false) where TComponent : Entity, IAwake<TP1>, new()
        {
            CheckThread();

            var type = typeof(TComponent);
            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemRegistry.Default.Awake(component, p1);

            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component as TComponent;
        }

        public TComponent AddComponentWithId<TComponent, TP1, TP2>(long id, TP1 p1, TP2 p2, bool isFromPool = false) where TComponent : Entity, IAwake<TP1, TP2>, new()
        {
            CheckThread();

            var type = typeof(TComponent);
            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemRegistry.Default.Awake(component, p1, p2);

            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component as TComponent;
        }

        public TComponent AddComponentWithId<TComponent, TP1, TP2, TP3>(long id, TP1 p1, TP2 p2, TP3 p3, bool isFromPool = false) where TComponent : Entity, IAwake<TP1, TP2, TP3>, new()
        {
            CheckThread();

            var type = typeof(TComponent);
            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemRegistry.Default.Awake(component, p1, p2, p3);

            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component as TComponent;
        }

        public TComponent AddComponentWithId<TComponent, TP1, TP2, TP3, TP4>(long id, TP1 p1, TP2 p2, TP3 p3, TP4 p4, bool isFromPool = false) where TComponent : Entity, IAwake<TP1, TP2, TP3, TP4>, new()
        {
            CheckThread();

            var type = typeof(TComponent);
            if (components != null && components.ContainsKey(GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemRegistry.Default.Awake(component, p1, p2, p3, p4);

            if (this is IAddComponentLifespan)
            {
                EntitySystemRegistry.Default.AddComponent(this, type);
            }

            return component as TComponent;
        }

        public TComponent AddComponent<TComponent>(bool isFromPool = false) where TComponent : Entity, IAwake, new()
        {
            return AddComponentWithId<TComponent>(Id, isFromPool);
        }

        public TComponent AddComponent<TComponent, TP1>(TP1 p1, bool isFromPool = false) where TComponent : Entity, IAwake<TP1>, new()
        {
            return AddComponentWithId<TComponent, TP1>(Id, p1, isFromPool);
        }

        public TComponent AddComponent<TComponent, TP1, TP2>(TP1 p1, TP2 p2, bool isFromPool = false) where TComponent : Entity, IAwake<TP1, TP2>, new()
        {
            return AddComponentWithId<TComponent, TP1, TP2>(Id, p1, p2, isFromPool);
        }

        public TComponent AddComponent<TComponent, TP1, TP2, TP3>(TP1 p1, TP2 p2, TP3 p3, bool isFromPool = false) where TComponent : Entity, IAwake<TP1, TP2, TP3>, new()
        {
            return AddComponentWithId<TComponent, TP1, TP2, TP3>(Id, p1, p2, p3, isFromPool);
        }

        public TComponent AddComponent<TComponent, TP1, TP2, TP3, TP4>(TP1 p1, TP2 p2, TP3 p3, TP4 p4, bool isFromPool = false) where TComponent : Entity, IAwake<TP1, TP2, TP3, TP4>, new()
        {
            return AddComponentWithId<TComponent, TP1, TP2, TP3, TP4>(Id, p1, p2, p3, p4, isFromPool);
        }

        public Entity AddChild(Entity entity)
        {
            entity.Parent = this;
            return entity;
        }

        public TChild AddChild<TChild>(bool isFromPool = false) where TChild : Entity, IAwake
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = IdGenerator.Default.GenerateId();
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component);
            return component;
        }

        public TChild AddChild<TChild, TP1>(TP1 p1, bool isFromPool = false) where TChild : Entity, IAwake<TP1>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = IdGenerator.Default.GenerateId();
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1);
            return component;
        }

        public TChild AddChild<TChild, TP1, TP2>(TP1 p1, TP2 p2, bool isFromPool = false) where TChild : Entity, IAwake<TP1, TP2>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = IdGenerator.Default.GenerateId();
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1, p2);
            return component;
        }

        public TChild AddChild<TChild, TP1, TP2, TP3>(TP1 p1, TP2 p2, TP3 p3, bool isFromPool = false) where TChild : Entity, IAwake<TP1, TP2, TP3>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = IdGenerator.Default.GenerateId();
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1, p2, p3);
            return component;
        }

        public TChild AddChild<TChild, TP1, TP2, TP3, TP4>(TP1 p1, TP2 p2, TP3 p3, TP4 p4, bool isFromPool = false) where TChild : Entity, IAwake<TP1, TP2, TP3, TP4>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = IdGenerator.Default.GenerateId();
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1, p2, p3, p4);
            return component;
        }

        public TChild AddChildWithId<TChild>(long id, bool isFromPool = false) where TChild : Entity, IAwake
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntitySystemRegistry.Default.Awake(component);
            return component;
        }

        public TChild AddChildWithId<TChild, TP1>(long id, TP1 p1, bool isFromPool = false) where TChild : Entity, IAwake<TP1>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1);
            return component;
        }

        public TChild AddChildWithId<TChild, TP1, TP2>(long id, TP1 p1, TP2 p2, bool isFromPool = false) where TChild : Entity, IAwake<TP1, TP2>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1, p2);
            return component;
        }

        public TChild AddChildWithId<TChild, TP1, TP2, TP3, TP4>(long id, TP1 p1, TP2 p2, TP3 p3, TP4 p4, bool isFromPool = false) where TChild : Entity, IAwake<TP1, TP2, TP3, TP4>
        {
            CheckThread();

            var type = typeof(TChild);
            var component = (TChild)Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;

            EntitySystemRegistry.Default.Awake(component, p1, p2, p3, p4);
            return component;
        }

        public override void BeginInit()
        {
            if (scene == null)
            {
                return;
            }

            EntitySystemRegistry.Default.Serialize(this);

            if (components is { Count: > 0 })
            {
                foreach (var (_, entity) in components)
                {
                    if (entity.SerializeWithParent)
                    {
                        entity.BeginInit();
                    }
                }
            }

            if (children is { Count: > 0 })
            {
                foreach (var (_, entity) in children)
                {
                    if (entity.SerializeWithParent)
                    {
                        entity.BeginInit();
                    }
                }
            }
        }

        private void Reset()
        {
            if (scene == null)
            {
                return;
            }

            InstanceId = 0;
            scene = null;
            DisableDeserializeSystem = false;
            IsRegister = false;

            if (components is { Count: > 0 })
            {
                foreach (var (_, entity) in components)
                {
                    entity.Reset();
                }
            }

            if (children is { Count: > 0 })
            {
                foreach (var (_, entity) in children)
                {
                    entity.Reset();
                }
            }
        }

        public void ClearUnserialized()
        {
            if (components is { Count: > 0 })
            {
                using var types = ListComponent<Type>.Create();
                foreach (var (_, entity) in components)
                {
                    if (!entity.SerializeWithParent)
                    {
                        types.Add(entity.GetType());
                        continue;
                    }

                    entity.ClearUnserialized();
                }

                foreach (var removeType in types)
                {
                    RemoveComponent(removeType);
                }
            }

            if (children is { Count: > 0 })
            {
                using var ids = ListComponent<long>.Create();

                foreach (var (id, entity) in children)
                {
                    if (!entity.SerializeWithParent)
                    {
                        ids.Add(id);
                        continue;
                    }

                    entity.ClearUnserialized();
                }

                foreach (var id in ids)
                {
                    RemoveChild(id);
                }
            }
        }
    }
}