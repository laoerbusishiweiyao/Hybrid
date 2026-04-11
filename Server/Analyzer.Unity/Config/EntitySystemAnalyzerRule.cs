using Microsoft.CodeAnalysis;

namespace Analyzer.Unity;

public static class EntitySystemAnalyzerRule
{
    private const string Title = "Entity类存在未生成的生命周期函数";

    private const string MessageFormat = "Entity类: {0} 存在未生成的生命周期函数";

    private const string Description = "Entity类存在未生成的生命周期函数.";

    public static readonly DiagnosticDescriptor Default = new(
        DiagnosticId.EntitySystemAnalyzer,
        Title,
        MessageFormat,
        DiagnosticCategories.Model,
        DiagnosticSeverity.Error,
        true,
        Description);
}