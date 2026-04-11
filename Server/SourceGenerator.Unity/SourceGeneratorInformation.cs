namespace SourceGenerator.Unity;

public static class SourceGeneratorInformation
{
    private const string Core = "Chaos.Foundation";
    private const string Loader = "Chaos.Loader";
    private const string Model = "Chaos.Model";
    private const string Hotfix = "Chaos.Hotfix";
    private const string ModelView = "Chaos.ModelView";
    private const string HotfixView = "Chaos.HotfixView";

    public static readonly string[] AllHotfix = [Hotfix, HotfixView];

    public static readonly string[] AllModel = [Model, ModelView];

    public static readonly string[] AllModelHotfix = [Model, Hotfix, ModelView, HotfixView];

    public static readonly string[] All = [Core, Loader, Model, Hotfix, ModelView, HotfixView];

    public static readonly string[] AllLogicModel = [Model];


    public const string EntityType = "Chaos.Entity";
    public const string LockStepEntityType = "Chaos.LockStepEntity";

    public const string EntitySystemAttribute = "EntitySystem";
    public const string EntitySystemAttributeMetaName = "Chaos.EntitySystemAttribute";
    
    public const string LockStepEntitySystemAttribute = "LockStepEntitySystem";
    public const string LockStepEntitySystemAttributeMetaName = "Chaos.LockStepEntitySystemAttribute";

    public const string MemoryPackableAttribute = "MemoryPack.MemoryPackableAttribute";

    public const string GetComponentInterface = "Chaos.IGetComponentLifespan";
    public const string GetComponentMethod = "GetComponentLifespan";
}