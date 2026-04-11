using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Unity;

[Generator(LanguageNames.CSharp)]
public sealed class EntitySerializerFormatterGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<string> allType = context.SyntaxProvider.CreateSyntaxProvider(OnPredicate, OnTransform).Where(type => type is not null)!;

        var allInformation = context.CompilationProvider.Combine(allType.Collect());

        context.RegisterSourceOutput(allInformation, OnSourceOutput);
    }

    private static void OnSourceOutput(SourceProductionContext context, (Compilation Compilation, ImmutableArray<string> Classes) information)
    {
        var types = information.Classes
            .Where(_ => SourceGeneratorBuilder.IsAssemblyNeedAnalyze(information.Compilation.AssemblyName, SourceGeneratorInformation.AllLogicModel))
            .ToList();

        if (types.Count == 0)
        {
            return;
        }

        Build(context, types);
    }

    private static void Build(SourceProductionContext context, List<string> types)
    {
        var count = types.Count;
        var typeHashCodeMapDeclaration = GenerateTypeHashCodeMapDeclaration(types);
        var serializeContent = GenerateSerializeContent(types);
        var deserializeContent = GenerateDeserializeContent(types);

        var content = $$"""
                        #nullable enable
                        #pragma warning disable CS0108 // hides inherited member
                        #pragma warning disable CS0162 // Unreachable code
                        #pragma warning disable CS0164 // This label has not been referenced
                        #pragma warning disable CS0219 // Variable assigned but never used
                        #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        #pragma warning disable CS8601 // Possible null reference assignment
                        #pragma warning disable CS8602
                        #pragma warning disable CS8604 // Possible null reference argument for parameter
                        #pragma warning disable CS8619
                        #pragma warning disable CS8620
                        #pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method
                        #pragma warning disable CS8765 // Nullability of type of parameter
                        #pragma warning disable CS9074 // The 'scoped' modifier of parameter doesn't match overridden or implemented member
                        #pragma warning disable CA1050 // Declare types in namespaces.

                        using System;
                        using MemoryPack;

                        namespace Chaos
                        {
                            [global::MemoryPack.Internal.Preserve]
                            public sealed class EntityMemoryPackFormatter : MemoryPackFormatter<global::{{SourceGeneratorInformation.EntityType}}>
                            {
                                static readonly System.Collections.Generic.Dictionary<Type, long> __typeToTag = new({{count}})
                                {
                        {{typeHashCodeMapDeclaration}}
                                };
                            
                                [global::MemoryPack.Internal.Preserve]
                                public override void Serialize<TBufferWriter>(ref MemoryPackWriter<TBufferWriter> writer, ref global::{{SourceGeneratorInformation.EntityType}}? value)
                                {

                                    if (value == null)
                                    {
                                        writer.WriteNullUnionHeader();
                                        return;
                                    }

                                    if (__typeToTag.TryGetValue(value.GetType(), out var tag))
                                    {
                                        writer.WriteValue<byte>(global::MemoryPack.MemoryPackCode.WideTag);
                                        writer.WriteValue<long>(tag);
                                        switch (tag)
                                        {
                        {{serializeContent}}               
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof(global::{{SourceGeneratorInformation.EntityType}}));
                                    }
                                }
                            
                                [global::MemoryPack.Internal.Preserve]
                                public override void Deserialize(ref MemoryPackReader reader, ref global::{{SourceGeneratorInformation.EntityType}}? value)
                                {

                                    bool isNull = reader.ReadValue<byte>() == global::MemoryPack.MemoryPackCode.NullObject;
                                    if (isNull)
                                    {
                                        value = default;
                                        return;
                                    }
                                
                                    var tag = reader.ReadValue<long>();

                                    switch (tag)
                                    {
                        {{deserializeContent}}
                                        default:
                                            //MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(global::IForExternalUnion));
                                            break;
                                    }
                                }
                            }
                            
                            public static partial class EntitySerializerRegister
                            {
                                static partial void Register()
                                {
                                    if (!global::MemoryPack.MemoryPackFormatterProvider.IsRegistered<global::{{SourceGeneratorInformation.EntityType}}>())
                                    {
                                        global::MemoryPack.MemoryPackFormatterProvider.Register(new EntityMemoryPackFormatter());
                                    }
                                }
                            }
                        }
                        """;

        context.AddSource("EntityMemoryPackFormatterGenerator.g.cs", content);
    }

    private static string GenerateTypeHashCodeMapDeclaration(IEnumerable<string> types)
    {
        var builder = new StringBuilder();
        foreach (var type in types)
        {
            builder.AppendLine($$"""        { typeof(global::{{type}}), {{type.GetLongHashCode()}} },""");
        }

        return builder.ToString();
    }

    private static string GenerateSerializeContent(IEnumerable<string> entityNames)
    {
        var builder = new StringBuilder();
        foreach (var entityName in entityNames)
        {
            builder.AppendLine($$"""                case {{entityName.GetLongHashCode()}}: writer.WritePackable(System.Runtime.CompilerServices.Unsafe.As<global::{{SourceGeneratorInformation.EntityType}}?, global::{{entityName}}>(ref value)); break;""");
        }

        return builder.ToString();
    }

    private static string GenerateDeserializeContent(IEnumerable<string> entityNames)
    {
        var builder = new StringBuilder();
        foreach (var entityName in entityNames)
        {
            builder.AppendLine($$"""
                                             case {{entityName.GetLongHashCode()}}:
                                                     if(value == null)
                                                     {
                                                         value = global::{{entityName}}.Fetch<global::{{entityName}}>();
                                                     }
                                                     if(value is global::{{entityName}})
                                                     {
                                                         reader.ReadPackable(ref System.Runtime.CompilerServices.Unsafe.As<global::{{SourceGeneratorInformation.EntityType}}?, global::{{entityName}}>(ref value));
                                                     }else{
                                                         value = (global::{{entityName}})reader.ReadPackable<global::{{entityName}}>();
                                                     }
                                                     break;
                                 """);
        }

        return builder.ToString();
    }

    private static bool OnPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return false;
        }

        if (!classDeclarationSyntax.HasAnyBaseType(SourceGeneratorInformation.EntityType, SourceGeneratorInformation.LockStepEntityType))
        {
            return false;
        }

        if (!classDeclarationSyntax.HasAttribute(SourceGeneratorInformation.MemoryPackableAttribute))
        {
            return false;
        }

        return true;
    }


    private static string OnTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return null;
        }

        if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not { } classTypeSymbol)
        {
            return null;
        }

        if (!classTypeSymbol.HasAnyBaseType(SourceGeneratorInformation.EntityType, SourceGeneratorInformation.LockStepEntityType))
        {
            return null;
        }

        if (!classTypeSymbol.HasAttribute(SourceGeneratorInformation.MemoryPackableAttribute))
        {
            return null;
        }

        return classTypeSymbol.ToString();
    }
}