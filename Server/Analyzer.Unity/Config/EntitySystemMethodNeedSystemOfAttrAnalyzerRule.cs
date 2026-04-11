using Microsoft.CodeAnalysis;

namespace Analyzer.Unity;

public static class EntitySystemMethodNeedSystemOfAttrAnalyzerRule
{
    private const string Title = "EntitySystem标签只能添加在含有EntitySystemOf标签的静态类中";

    private const string MessageFormat = "方法:{0}的{1}标签只能添加在含有{2}标签的静态类中";

    private const string Description = "EntitySystem标签只能添加在含有EntitySystemOf标签的静态类中.";

    public static readonly DiagnosticDescriptor Default = new(
        DiagnosticId.EntitySystemMethodNeedSystemOfAttrAnalyzer,
        Title,
        MessageFormat,
        DiagnosticCategories.Model,
        DiagnosticSeverity.Error,
        true,
        Description);
}