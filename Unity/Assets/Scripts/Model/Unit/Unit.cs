using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using Unity.Mathematics;

namespace Chaos
{
    [ChildOf(typeof(UnitComponent))]
    [DebuggerDisplay("ViewName,nq")]
    public sealed partial class Unit : Entity, IAwake<int>
    {
        public int ConfigId { get; set; }

        // public UnitType UnitType { get; set; }

        [BsonElement]
        private float3 position;

        [BsonIgnore]
        public float3 Position
        {
            get => position;
            set
            {
                var oldPosition = position;
                position = value;
                EventSystem.Default.Publish(this.Scene(), new UnitChangePositionEventArgs(this, oldPosition));
            }
        }

        [BsonIgnore]
        public float3 Forward
        {
            get => math.mul(Rotation, math.forward());
            set => Rotation = quaternion.LookRotation(value, math.up());
        }

        [BsonElement]
        private quaternion rotation;

        [BsonIgnore]
        public quaternion Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                EventSystem.Default.Publish(this.Scene(), new UnitChangeRotationEventArgs(this));
            }
        }

        protected override string HierarchyName => $"{GetType().FullName} ({Id})";
    }
}