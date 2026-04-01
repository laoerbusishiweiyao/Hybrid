using Unity.Mathematics;

namespace Chaos
{
    public readonly struct UnitChangePositionEventArgs
    {
        public readonly EntityReference<Unit> Unit;
        public readonly float3 OldPosition;

        public UnitChangePositionEventArgs(Unit unit, float3 oldPosition)
        {
            Unit = unit;
            OldPosition = oldPosition;
        }
    }

    public readonly struct UnitChangeRotationEventArgs
    {
        public readonly EntityReference<Unit> Unit;

        public UnitChangeRotationEventArgs(Unit unit)
        {
            Unit = unit;
        }
    }
}