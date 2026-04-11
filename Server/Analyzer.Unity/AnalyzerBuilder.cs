using Microsoft.CodeAnalysis;

namespace Analyzer.Unity;

public static class AnalyzerBuilder
{
    public static AttributeData FirstAttributeOrDefault(this INamedTypeSymbol self, string attributeName)
    {
        return self.GetAttributes().FirstOrDefault(attributeData => attributeData.AttributeClass?.ToString() == attributeName);
    }

    public static bool IsInterface(this INamedTypeSymbol self, string interfaceName)
    {
        return $"{self.NameSpaceOrDefault()}.{self.Name}" == interfaceName;
    }

    public static string NameSpaceOrDefault(this INamedTypeSymbol self)
    {
        var namespaceSymbol = self.ContainingNamespace;
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

        return string.IsNullOrEmpty(namespaceName) ? null : namespaceName;
    }

    public static bool HasMethodWithParameters(this INamedTypeSymbol self, string methodName, params ITypeSymbol[] typeSymbols)
    {
        foreach (var member in self.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.Name != methodName)
            {
                continue;
            }

            if (typeSymbols.Length != methodSymbol.Parameters.Length)
            {
                continue;
            }

            if (typeSymbols.Length == 0)
            {
                return true;
            }

            if (!typeSymbols.Where((typeSymbol, i) => typeSymbol.ToString() != methodSymbol.Parameters[i].Type.ToString()).Any())
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasMethodWithParameters(this INamedTypeSymbol self, string methodName, params string[] typeSymbols)
    {
        foreach (var member in self.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.Name != methodName)
            {
                continue;
            }

            if (typeSymbols.Length != methodSymbol.Parameters.Length)
            {
                continue;
            }

            if (typeSymbols.Length == 0)
            {
                return true;
            }

            if (!typeSymbols.Where((t, i) => t != methodSymbol.Parameters[i].Type.ToString()).Any())
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAttribute(this IMethodSymbol self, string attributeName)
    {
        return self.GetAttributes().Any(attributeData => attributeData?.AttributeClass?.ToString() == attributeName);
    }
}