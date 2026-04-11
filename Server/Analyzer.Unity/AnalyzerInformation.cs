namespace Analyzer.Unity;

public static class AnalyzerInformation
{
    public const string EntityType = "Chaos.Entity";
    public const string LockStepEntityType = "Chaos.LockStepEntity";

    public const string EntitySystemAttribute = "EntitySystem";
    public const string EntitySystemAttributeMetaName = "Chaos.EntitySystemAttribute";

    public const string EntitySystemOfAttribute = "Chaos.EntitySystemOfAttribute";
    public const string EntitySystemInterfaceSequence = "EntitySystemInterfaceSequence";

    public const string AwakeInterface = "Chaos.IAwake";
    public const string AwakeMethod = "Awake";

    public const string LoadInterface = "Chaos.ILoad";
    public const string LoadMethod = "Load";

    public const string UpdateInterface = "Chaos.IUpdate";
    public const string UpdateMethod = "Update";

    public const string LateUpdateInterface = "Chaos.ILateUpdate";
    public const string LateUpdateMethod = "LateUpdate";

    public const string DestroyInterface = "Chaos.IDestroy";
    public const string DestroyMethod = "Destroy";

    public const string AddComponentInterface = "Chaos.IAddComponentLifespan";
    public const string AddComponentMethod = "AddComponentLifespan";

    public const string GetComponentInterface = "Chaos.IGetComponentLifespan";
    public const string GetComponentMethod = "GetComponentLifespan";

    public const string SerializeInterface = "Chaos.ISerialize";
    public const string SerializeMethod = "Serialize";

    public const string DeserializeInterface = "Chaos.IDeserialize";
    public const string DeserializeMethod = "Deserialize";

    public const string LockStepEntitySystemAttribute = "LockStepEntitySystem";
    public const string LockStepEntitySystemAttributeMetaName = "Chaos.LockStepEntitySystemAttribute";
    public const string LockStepEntitySystemOfAttribute = "Chaos.LockStepEntitySystemOfAttribute";

    public const string LockStepUpdateInterface = "Chaos.ILockStepUpdate";
    public const string LockStepUpdateMethod = "LockStepUpdate";

    public const string LockStepRollbackInterface = "Chaos.ILockStepRollback";
    public const string LockStepRollbackMethod = "LockStepRollback";
}