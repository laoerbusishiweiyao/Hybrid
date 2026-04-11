using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer.Unity;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EntitySystemAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntitySystemAnalyzerRule.Default, EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Default);

    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerSettings.Enabled)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeIsSystemMethodValid, SymbolKind.NamedType);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        ImmutableDictionary<string, string>.Builder builder = null;
        foreach (var systemData in allSystemData)
        {
            AnalyzeSystem(context, systemData, ref builder);
        }

        ReportNeedGenerateSystem(context, namedTypeSymbol, ref builder);
    }

    private static void AnalyzeSystem(SymbolAnalysisContext context, SystemData systemData, ref ImmutableDictionary<string, string>.Builder builder)
    {
        if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        // 筛选出含有SystemOf标签的类
        if (namedTypeSymbol.FirstAttributeOrDefault(systemData.SystemOfAttribute) is not { } attributeData)
        {
            return;
        }

        // 获取所属的实体类symbol
        if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol)
        {
            return;
        }

        var ignoreAwake = false;
        if (attributeData.ConstructorArguments.Length >= 2 && attributeData.ConstructorArguments[1].Value is bool ignore)
        {
            ignoreAwake = ignore;
        }

        // 排除非Entity子类
        if (entityTypeSymbol.BaseType?.ToString() != systemData.EntityTypeName)
        {
            return;
        }

        foreach (var interfaceSymbol in entityTypeSymbol.AllInterfaces)
        {
            if (ignoreAwake && interfaceSymbol.IsInterface(AnalyzerInformation.AwakeInterface))
            {
                continue;
            }


            foreach (var systemMethodData in systemData.SystemMethods)
            {
                if (interfaceSymbol.IsInterface(systemMethodData.InterfaceName))
                {
                    if (interfaceSymbol.IsGenericType)
                    {
                        var typeArgs = ImmutableArray.Create<ITypeSymbol>(entityTypeSymbol).AddRange(interfaceSymbol.TypeArguments);
                        if (!namedTypeSymbol.HasMethodWithParameters(systemMethodData.MethodName, typeArgs.ToArray()))
                        {
                            StringBuilder str = new();
                            str.Append(entityTypeSymbol);
                            str.Append("/");
                            str.Append(systemData.SystemAttributeShowName);
                            foreach (var typeArgument in interfaceSymbol.TypeArguments)
                            {
                                str.Append("/");
                                str.Append(typeArgument);
                            }

                            AddProperty(ref builder, $"{systemMethodData.MethodName}`{interfaceSymbol.TypeArguments.Length}", str.ToString());
                        }
                    }
                    else
                    {
                        if (interfaceSymbol.IsInterface(AnalyzerInformation.GetComponentInterface))
                        {
                            if (!namedTypeSymbol.HasMethodWithParameters(systemMethodData.MethodName, entityTypeSymbol.ToString(), "System.Type"))
                            {
                                AddProperty(ref builder, systemMethodData.MethodName, $"{entityTypeSymbol}/{systemData.SystemAttributeShowName}/System.Type");
                            }
                        }
                        else if (!namedTypeSymbol.HasMethodWithParameters(systemMethodData.MethodName, entityTypeSymbol))
                        {
                            AddProperty(ref builder, systemMethodData.MethodName, $"{entityTypeSymbol}/{systemData.SystemAttributeShowName}");
                        }
                    }

                    break;
                }
            }
        }
    }

    private static void AddProperty(ref ImmutableDictionary<string, string>.Builder builder, string methodMetaName, string methodArgs)
    {
        builder ??= ImmutableDictionary.CreateBuilder<string, string>();

        if (builder.TryGetValue(AnalyzerInformation.EntitySystemInterfaceSequence, out var seqValue))
        {
            builder[AnalyzerInformation.EntitySystemInterfaceSequence] = $"{seqValue}/{methodMetaName}";
        }
        else
        {
            builder.Add(AnalyzerInformation.EntitySystemInterfaceSequence, methodMetaName);
        }

        builder.Add(methodMetaName, methodArgs);
    }

    private static void ReportNeedGenerateSystem(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, ref ImmutableDictionary<string, string>.Builder builder)
    {
        if (builder == null)
        {
            return;
        }

        foreach (var reference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(EntitySystemAnalyzerRule.Default, classDeclarationSyntax.Identifier.GetLocation(), builder.ToImmutable(), namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeIsSystemMethodValid(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            return;
        }

        foreach (var symbol in namedTypeSymbol.GetMembers())
        {
            if (symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            foreach (var systemData in allSystemData)
            {
                if (!methodSymbol.HasAttribute(systemData.SystemAttributeMetaName))
                {
                    continue;
                }

                if (methodSymbol.Parameters.Length == 0)
                {
                    continue;
                }

                var attributeData = namedTypeSymbol.FirstAttributeOrDefault(systemData.SystemOfAttribute);
                if (attributeData?.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol || entityTypeSymbol.ToString() != methodSymbol.Parameters[0].Type.ToString())
                {
                    ReportNeedSystemOfAttr(context, methodSymbol, systemData);
                }
            }
        }
    }

    private static void ReportNeedSystemOfAttr(SymbolAnalysisContext context, IMethodSymbol methodSymbol, SystemData systemData)
    {
        foreach (var reference in methodSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Default, methodDeclarationSyntax.Identifier.GetLocation(), methodSymbol.Name, systemData.SystemAttributeShowName, systemData.SystemOfAttribute);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static readonly ImmutableArray<SystemData> allSystemData = ImmutableArray.Create(
        new SystemData(AnalyzerInformation.EntitySystemOfAttribute, AnalyzerInformation.EntitySystemAttribute, AnalyzerInformation.EntityType, AnalyzerInformation.EntitySystemAttributeMetaName,
            new SystemMethodData(AnalyzerInformation.AwakeInterface, AnalyzerInformation.AwakeMethod),
            new SystemMethodData(AnalyzerInformation.LoadInterface, AnalyzerInformation.LoadMethod),
            new SystemMethodData(AnalyzerInformation.UpdateInterface, AnalyzerInformation.UpdateMethod),
            new SystemMethodData(AnalyzerInformation.LateUpdateInterface, AnalyzerInformation.LateUpdateMethod),
            new SystemMethodData(AnalyzerInformation.DestroyInterface, AnalyzerInformation.DestroyMethod),
            new SystemMethodData(AnalyzerInformation.AddComponentInterface, AnalyzerInformation.AddComponentMethod),
            new SystemMethodData(AnalyzerInformation.GetComponentInterface, AnalyzerInformation.GetComponentMethod),
            new SystemMethodData(AnalyzerInformation.SerializeInterface, AnalyzerInformation.SerializeMethod),
            new SystemMethodData(AnalyzerInformation.DeserializeInterface, AnalyzerInformation.DeserializeMethod),
            new SystemMethodData(AnalyzerInformation.LockStepRollbackInterface, AnalyzerInformation.LockStepRollbackMethod)),
        new SystemData(AnalyzerInformation.LockStepEntitySystemOfAttribute, AnalyzerInformation.LockStepEntitySystemAttribute, AnalyzerInformation.LockStepEntityType, AnalyzerInformation.LockStepEntitySystemAttributeMetaName,
            new SystemMethodData(AnalyzerInformation.AwakeInterface, AnalyzerInformation.AwakeMethod),
            new SystemMethodData(AnalyzerInformation.LoadInterface, AnalyzerInformation.LoadMethod),
            new SystemMethodData(AnalyzerInformation.LockStepUpdateInterface, AnalyzerInformation.LockStepUpdateMethod),
            new SystemMethodData(AnalyzerInformation.DestroyInterface, AnalyzerInformation.DestroyMethod),
            new SystemMethodData(AnalyzerInformation.AddComponentInterface, AnalyzerInformation.AddComponentMethod),
            new SystemMethodData(AnalyzerInformation.GetComponentInterface, AnalyzerInformation.GetComponentMethod),
            new SystemMethodData(AnalyzerInformation.SerializeInterface, AnalyzerInformation.SerializeMethod),
            new SystemMethodData(AnalyzerInformation.DeserializeInterface, AnalyzerInformation.DeserializeMethod),
            new SystemMethodData(AnalyzerInformation.LockStepRollbackInterface, AnalyzerInformation.LockStepRollbackMethod)
        )
    );

    public readonly struct SystemData(string systemOfAttribute, string systemAttributeShowName, string entityTypeName, string systemAttributeMetaName, params SystemMethodData[] systemMethods)
    {
        public readonly string EntityTypeName = entityTypeName;
        public readonly string SystemOfAttribute = systemOfAttribute;
        public readonly string SystemAttributeShowName = systemAttributeShowName;
        public readonly string SystemAttributeMetaName = systemAttributeMetaName;
        public readonly SystemMethodData[] SystemMethods = systemMethods;
    }

    public readonly struct SystemMethodData(string interfaceName, string methodName)
    {
        public readonly string InterfaceName = interfaceName;
        public readonly string MethodName = methodName;
    }
}