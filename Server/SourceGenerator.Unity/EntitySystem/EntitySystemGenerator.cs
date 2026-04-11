using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Unity;

[Generator(LanguageNames.CSharp)]
public sealed class EntitySystemGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var allDeclaration = context.SyntaxProvider.CreateSyntaxProvider(OnPredicate, OnTransform).Where(pair => pair != default);

        var allInformation = context.CompilationProvider.Combine(allDeclaration.Collect());

        context.RegisterSourceOutput(allInformation, OnSourceOutput);
    }

    private static void OnSourceOutput(SourceProductionContext context, (Compilation Compilation, ImmutableArray<(ClassDeclarationSyntax ClassSyntax, MethodDeclarationSyntax MethodSyntax)> Declaration) information)
    {
        var (compilation, declaration) = information;

        var methods = declaration.GroupBy(pair => pair.ClassSyntax)
            .ToImmutableDictionary(group => group.Key, group => group.Select(pair => pair.MethodSyntax).ToImmutableHashSet());

        Build(context, compilation, methods);
    }

    private static void Build(SourceProductionContext context, Compilation compilation, ImmutableDictionary<ClassDeclarationSyntax, ImmutableHashSet<MethodDeclarationSyntax>> methods)
    {
        Dictionary<string, StringBuilder> builders = new();

        foreach (var pair in methods)
        {
            var classSyntax = pair.Key;
            var methodSyntaxes = pair.Value;

            GenerateCSharpFilesForClass(classSyntax, methodSyntaxes, compilation, builders);
        }

        foreach (var data in builders)
        {
            var key = data.Key;
            var value = data.Value;
            var keyArr = key.Split('|');
            var namespaceName = keyArr[0];
            var className = keyArr[1];
            var allFileName = $"{namespaceName}.{className}.EntitySystems.g.cs";
            var allCode = $$"""
                            namespace {{namespaceName}}
                            {
                                public static partial class {{className}}
                                {
                            {{value}}
                                }
                            }
                            """;
            context.AddSource(allFileName, allCode);
        }
    }

    private static void GenerateCSharpFilesForClass(ClassDeclarationSyntax classDeclarationSyntax, ImmutableHashSet<MethodDeclarationSyntax> methodDeclarationSyntaxes, Compilation compilation, Dictionary<string, StringBuilder> builders)
    {
        var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not { } classSymbol)
        {
            return;
        }

        var namespaceSymbol = classSymbol.ContainingNamespace;
        var namespaceName = namespaceSymbol?.Name;
        while (namespaceSymbol?.ContainingNamespace != null)
        {
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
            if (string.IsNullOrEmpty(namespaceSymbol.Name))
            {
                break;
            }

            namespaceName = $"{namespaceSymbol.Name}.{namespaceName}";
        }

        var className = classSymbol.Name;

        if (namespaceName == null)
        {
            throw new Exception($"{className} namespace is null");
        }

        GenerateSystemCodeByTemplate(namespaceName, className, classDeclarationSyntax, methodDeclarationSyntaxes, compilation, semanticModel, builders);
    }

    private static void GenerateSystemCodeByTemplate(string namespaceName, string className, ClassDeclarationSyntax classSyntax, ImmutableHashSet<MethodDeclarationSyntax> methodSyntaxes, Compilation compilation, SemanticModel semanticModel, Dictionary<string, StringBuilder> builders)
    {
        var key = $"{namespaceName}|{className}";
        if (!builders.ContainsKey(key))
        {
            builders.Add(key, new StringBuilder());
        }

        var allMethodCodeBuilder = builders[key];

        foreach (var methodSyntax in methodSyntaxes)
        {
            if (semanticModel.GetDeclaredSymbol(methodSyntax) is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSyntax.ParameterList.Parameters.FirstOrDefault() is not { } parameters)
            {
                continue;
            }

            var methodName = methodSyntax.Identifier.Text;
            var componentName = parameters.Type?.ToString();

            var argsTypesList = new List<string>();
            var argsTypeVarsList = new List<string>();
            var argsVarsList = new List<string>();
            var argsTypesWithout0List = new List<string>();
            var argsTypeVarsWithout0List = new List<string>();
            var argsVarsWithout0List = new List<string>();
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var type = methodSymbol.Parameters[i].Type.ToDisplayString();
                type = type.Trim();
                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }

                var name = methodSymbol.Parameters[i].Name;

                argsTypesList.Add(type);
                argsVarsList.Add(name);
                var typeName = $"{type} {name}";
                argsTypeVarsList.Add(typeName);

                if (i == 0)
                {
                    continue;
                }

                argsTypesWithout0List.Add(type);
                argsTypeVarsWithout0List.Add(typeName);
                argsVarsWithout0List.Add(name);
            }

            foreach (var attributeList in methodSyntax.AttributeLists)
            {
                if (attributeList.Attributes.FirstOrDefault() is not { } attribute)
                {
                    continue;
                }

                var attributeType = attribute.Name.ToString();
                var attributeString = $"[{attribute.ToString()}]";

                if (!EntitySystemBuilder.Contains(attributeType))
                {
                    continue;
                }

                var code = EntitySystemBuilder.Find(attributeType);
                var argsVars = string.Join(", ", argsVarsList);
                var argsTypes = string.Join(", ", argsTypesList);
                var argsTypesVars = string.Join(", ", argsTypeVarsList);
                var argsTypesUnderLine = string.Join("_", argsTypesList).Replace(", ", "_").Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("[]", "Array").Replace("(", "_").Replace(")", "_");
                var argsTypesWithout0 = string.Join(", ", argsTypesWithout0List);
                var argsVarsWithout0 = string.Join(", ", argsVarsWithout0List);
                var argsTypesVarsWithout0 = string.Join(", ", argsTypeVarsWithout0List);

                SpecialProcessForArgs();

                if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Void)
                {
                    code = code.Replace("$returnType$", "void");
                    code = code.Replace("$return$", "");
                }
                else
                {
                    code = code.Replace("$returnType$", methodSymbol.ReturnType.ToDisplayString());
                    code = code.Replace("$return$", "return ");
                }

                code = code.Replace("$attribute$", attributeString);
                code = code.Replace("$attributeType$", attributeType);
                code = code.Replace("$methodName$", methodName);
                code = code.Replace("$className$", className);
                code = code.Replace("$entityType$", componentName);
                code = code.Replace("$argsTypes$", argsTypes);
                code = code.Replace("$argsTypesUnderLine$", argsTypesUnderLine);
                code = code.Replace("$argsTypesVars$", argsTypesVars);
                code = code.Replace("$argsVars$", argsVars);
                code = code.Replace("$argsTypesWithout0$", argsTypesWithout0);
                code = code.Replace("$argsVarsWithout0$", argsVarsWithout0);
                code = code.Replace("$argsTypesVarsWithout0$", argsTypesVarsWithout0);

                for (var i = 0; i < argsTypesList.Count; ++i)
                {
                    code = code.Replace($"$argsTypes{i}$", argsTypesList[i]);
                    code = code.Replace($"$argsVars{i}$", argsVarsList[i]);
                }

                allMethodCodeBuilder.Append(code);
                allMethodCodeBuilder.AppendLine();
                continue;

                void SpecialProcessForArgs()
                {
                    if (attributeType is SourceGeneratorInformation.EntitySystemAttribute or SourceGeneratorInformation.LockStepEntitySystemAttribute && methodName == SourceGeneratorInformation.GetComponentMethod)
                    {
                        argsTypes = argsTypes.Split(',')[0];
                    }
                }
            }
        }
    }

    private static bool OnPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is not MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return false;
        }

        return methodDeclarationSyntax.AttributeLists.Count != 0;
    }

    private static (ClassDeclarationSyntax ClassSyntax, MethodDeclarationSyntax MethodSyntax) OnTransform(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        if (context.Node is not MethodDeclarationSyntax methodSyntax || methodSyntax.ParentClassDeclaration() is not { } classSyntax)
        {
            return default;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodSyntax) is not { } methodSymbol)
        {
            return default;
        }

        if (!classSyntax.IsPartial() || methodSymbol.ContainingType is not { IsStatic: true })
        {
            return default;
        }

        if (!methodSyntax.AttributeLists.SelectMany(syntax => syntax.Attributes).Any(syntax => EntitySystemBuilder.Contains(syntax.Name.ToString())))
        {
            return default;
        }

        return (classSyntax, methodSyntax);
    }
}