using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace Analyzer.Unity;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntitySystemCodeFixProvider)), Shared]
public sealed class EntitySystemCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId.EntitySystemAnalyzer);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var classDeclaration = root?.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

        var codeAction = CodeAction.Create(
            "Generate Entity System",
            cancelToken => GenerateEntitySystemAsync(context.Document, classDeclaration, diagnostic, cancelToken),
            equivalenceKey: nameof(EntitySystemCodeFixProvider));
        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> GenerateEntitySystemAsync(Document document, ClassDeclarationSyntax classDeclaration, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var properties = diagnostic.Properties;

        if (classDeclaration == null || root == null)
        {
            return document;
        }

        var newMembers = new SyntaxList<MemberDeclarationSyntax>();
        var sequenceContent = properties[AnalyzerInformation.EntitySystemInterfaceSequence];
        if (sequenceContent == null)
        {
            return document;
        }

        var sequenceArray = sequenceContent.Split('/');
        foreach (var methodName in sequenceArray)
        {
            var methodArgs = properties[methodName];
            if (methodArgs == null)
            {
                continue;
            }

            var methodSyntax = CreateEntitySystemMethodSyntax(methodName, methodArgs);
            if (methodSyntax != null)
            {
                newMembers = newMembers.Add(methodSyntax);
            }
            else
            {
                throw new Exception("methodSyntax==null");
            }
        }

        if (newMembers.Count == 0)
        {
            throw new Exception("newMembers.Count==0");
        }

        var newClassDeclaration = classDeclaration.WithMembers(classDeclaration.Members.InsertRange(0, newMembers)).WithAdditionalAnnotations(Formatter.Annotation);
        document = document.WithSyntaxRoot(root.ReplaceNode(classDeclaration, newClassDeclaration));
        document = await CleanupDocumentAsync(document, cancellationToken);
        return document;
    }

    private static MethodDeclarationSyntax CreateEntitySystemMethodSyntax(string methodName, string methodArgs)
    {
        var methodNameArray = methodName.Split('`');
        var methodArgsArray = methodArgs.Split('/');
        var systemAttribute = methodArgsArray[1];
        var args = string.Empty;
        if (methodArgsArray.Length > 2)
        {
            for (var i = 2; i < methodArgsArray.Length; i++)
            {
                args += $", {methodArgsArray[i]} args{i}";
            }
        }

        var code = $$"""
                             [{{systemAttribute}}]
                             private static void {{methodNameArray[0]}}(this {{methodArgsArray[0]}} self{{args}})
                             {
                                 
                             }
                     """;
        return SyntaxFactory.ParseMemberDeclaration(code) as MethodDeclarationSyntax;
    }

    internal static async Task<Document> CleanupDocumentAsync(Document document, CancellationToken cancellationToken)
    {
        if (document.SupportsSyntaxTree)
        {
            document = await ImportAdder.AddImportsAsync(document, Simplifier.AddImportsAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any node with explicit formatter annotation
            document = await Formatter.FormatAsync(document, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);

            // format any elastic whitespace
            document = await Formatter.FormatAsync(document, SyntaxAnnotation.ElasticAnnotation, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        return document;
    }
}