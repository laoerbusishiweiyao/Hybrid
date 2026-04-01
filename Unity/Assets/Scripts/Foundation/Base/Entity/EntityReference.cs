using System;

namespace Chaos
{
    public struct EntityReference<T> : IDisposable, IEquatable<EntityReference<T>> where T : Entity
    {
        private readonly long instanceId;
        private T entity;

        public void Dispose()
        {
            var value = Entity;
            value?.Dispose();
        }

        public override int GetHashCode()
        {
            return instanceId.GetHashCode();
        }

        public bool Equals(EntityReference<T> other)
        {
            return instanceId == other.instanceId;
        }

        public override bool Equals(object other)
        {
            if (other is not EntityReference<T> entityRef)
            {
                return false;
            }

            return instanceId == entityRef.instanceId;
        }

        public static bool operator ==(EntityReference<T> left, EntityReference<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityReference<T> left, EntityReference<T> right)
        {
            return !left.Equals(right);
        }

        private EntityReference(T value)
        {
            if (value == null)
            {
                instanceId = 0;
                entity = null;
                return;
            }

            if (value.InstanceId == 0)
            {
                throw new Exception("entity is disposed, instance id == 0!");
            }

            instanceId = value.InstanceId;
            entity = value;
        }

        public T Entity
        {
            get
            {
                if (entity == null)
                {
                    return null;
                }

                if (entity.InstanceId != instanceId)
                {
                    entity = null;
                }

                return entity;
            }
        }

        public static implicit operator EntityReference<T>(T value)
        {
            return new EntityReference<T>(value);
        }

        public static implicit operator T(EntityReference<T> value)
        {
            return value.Entity;
        }

        public override string ToString()
        {
            return instanceId.ToString();
        }
    }

    public struct EntityWeakReference<T> : IDisposable, IEquatable<EntityWeakReference<T>> where T : Entity
    {
        private long instanceId;
        private readonly WeakReference<T> weakReference;

        private EntityWeakReference(T value)
        {
            if (value == null)
            {
                instanceId = 0;
                weakReference = null;
                return;
            }

            if (value.InstanceId == 0)
            {
                throw new Exception("cant convert to entity ref, entity instance id == 0!");
            }

            instanceId = value.InstanceId;
            weakReference = new WeakReference<T>(value);
        }

        public void Dispose()
        {
            var value = Entity;
            value?.Dispose();
        }

        public T Entity
        {
            get
            {
                if (instanceId == 0)
                {
                    return null;
                }

                if (weakReference != null && weakReference.TryGetTarget(out var entity) && entity.InstanceId == instanceId)
                {
                    return entity;
                }

                instanceId = 0;
                return null;
            }
        }

        public static implicit operator EntityWeakReference<T>(T value)
        {
            return new EntityWeakReference<T>(value);
        }

        public static implicit operator T(EntityWeakReference<T> value)
        {
            return value.Entity;
        }

        public override int GetHashCode()
        {
            return instanceId.GetHashCode();
        }

        public bool Equals(EntityWeakReference<T> other)
        {
            return instanceId == other.instanceId;
        }

        public override bool Equals(object other)
        {
            if (other is not EntityWeakReference<T> entityRef)
            {
                return false;
            }

            return instanceId == entityRef.instanceId;
        }

        public static bool operator ==(EntityWeakReference<T> left, EntityWeakReference<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityWeakReference<T> left, EntityWeakReference<T> right)
        {
            return !left.Equals(right);
        }
    }
}