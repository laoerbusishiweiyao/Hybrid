using System.Collections.Generic;

namespace Chaos
{
    public sealed class Mailboxes
    {
        private readonly Dictionary<long, EntityReference<Entity>> mailboxes = new();

        public void Add(Entity mailBox)
        {
            mailboxes.Add(mailBox.Parent.InstanceId, mailBox);
        }

        public void Remove(long instanceId)
        {
            mailboxes.Remove(instanceId);
        }

        public EntityReference<Entity> Get(long instanceId)
        {
            mailboxes.TryGetValue(instanceId, out var entity);
            return entity;
        }
    }
}