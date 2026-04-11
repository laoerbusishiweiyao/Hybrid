using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceGenerator.Unity;

public static class SourceGeneratorBuilder
{
    public static bool IsAssemblyNeedAnalyze(string assemblyName, params string[] analyzeAssemblyNames)
    {
        return assemblyName != null && analyzeAssemblyNames.Any(analyzeAssemblyName => assemblyName == analyzeAssemblyName);
    }

    public static bool HasAttribute(this INamedTypeSymbol self, string attributeName)
    {
        return self.GetAttributes().Any(data => data.AttributeClass?.ToString() == attributeName);
    }

    public static bool HasAnyBaseType(this INamedTypeSymbol self, params string[] baseTypeNames)
    {
        return baseTypeNames.Any(baseTypeName => self.BaseType?.ToString() == baseTypeName);
    }

    public static bool HasAttribute(this ClassDeclarationSyntax self, string attributeName)
    {
        return self.AttributeLists.SelectMany(syntax => syntax.Attributes).Any(data => data.Name.ToString() == attributeName);
    }

    public static bool HasAnyBaseType(this ClassDeclarationSyntax self, params string[] baseTypeNames)
    {
        return self.BaseList?.Types.Any(type => baseTypeNames.Contains(type.ToString())) == true;
    }

    public static ClassDeclarationSyntax ParentClassDeclaration(this SyntaxNode syntaxNode)
    {
        var parentNode = syntaxNode.Parent;
        while (parentNode != null)
        {
            if (parentNode is ClassDeclarationSyntax classDeclarationSyntax)
            {
                return classDeclarationSyntax;
            }

            parentNode = parentNode.Parent;
        }

        return null;
    }


    public static bool IsPartial(this ClassDeclarationSyntax classDeclaration) => classDeclaration.Modifiers.Any(syntaxToken => syntaxToken.IsKind(SyntaxKind.PartialKeyword));

    public static long GetLongHashCode(this string self)
    {
        const uint seed = 1313; // 31 131 1313 13131 131313 etc..

        ulong hash = 0;
        foreach (var character in self)
        {
            var high = (byte)(character >> 8);
            var low = (byte)(character & byte.MaxValue);
            hash = hash * seed + high;
            hash = hash * seed + low;
        }

        return (long)hash;
    }
}