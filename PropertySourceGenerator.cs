using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator;

[Generator]
public class PropertySourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) =>
        context.RegisterForSyntaxNotifications(() => new FieldSyntaxReceiver());

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not FieldSyntaxReceiver syntaxReciever) return;
        var notifySymbol = context.Compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged");
        foreach (var containingClassGroup in syntaxReciever.IdentifiedFields.GroupBy(x => x.ContainingType))
        {
            var containingClass = containingClassGroup.Key;
            var namespaced = containingClass.ContainingNamespace;
            var hasNotifyImplementation = notifySymbol != null && containingClass.Interfaces.Contains(notifySymbol);
            var source = GenerateClass(context, containingClass, namespaced, containingClassGroup.ToList(),
                !hasNotifyImplementation);
            context.AddSource($"{containingClass.Name}_AutoNotify.generated", SourceText.From(source, Encoding.UTF8));
        }
    }

    private string GenerateClass(GeneratorExecutionContext context, INamedTypeSymbol @class,
        INamespaceSymbol @namespace, List<IFieldSymbol> fields, bool implementNotifyPropertyChange)
    {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using System;");
        if (implementNotifyPropertyChange)
        {
            var notifyPropertyChangedSymbol =
                context.Compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged");
            var callerMemberSymbol =
                context.Compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.CallerMemberNameAttribute");
            classBuilder.AppendLine($"using {notifyPropertyChangedSymbol?.ContainingNamespace};");
            classBuilder.AppendLine($"using {callerMemberSymbol?.ContainingNamespace};");
            classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"public partial class {@class.Name}:{notifyPropertyChangedSymbol?.Name}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine(GenerateNotifyPropertyChangeImplementation());
        }
        else
        {
            classBuilder.AppendLine($"namespace {@namespace.ToDisplayString()}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"public partial class {@class.Name}");
            classBuilder.AppendLine("{");
        }

        foreach (var field in fields)
        {
            var fieldName = field.Name;
            var fieldType = field.Type.Name;
            classBuilder.AppendLine($"public {fieldType} {NormalizePropertyName(fieldName)}");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"get=> {fieldName};");
            classBuilder.AppendLine("set");
            classBuilder.AppendLine("{");
            classBuilder.AppendLine($"if({fieldName} == value) return;");
            classBuilder.AppendLine($"{fieldName} = value;");
            classBuilder.AppendLine("OnPropertyChanged();");
            classBuilder.AppendLine("}");
            classBuilder.AppendLine("}");
        }

        classBuilder.AppendLine("}");
        classBuilder.AppendLine("}");
        return classBuilder.ToString();
    }

    private string NormalizePropertyName(string fieldName)
    {
        return Regex.Replace(fieldName, "_[a-z]", delegate(Match m) { return m.ToString().TrimStart('_').ToUpper(); });
    }

    private string GenerateNotifyPropertyChangeImplementation()
    {
        return """
               public event PropertyChangedEventHandler? PropertyChanged;
               
                   private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
               {
                   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
               }
                   
               """;
    }


    private class FieldSyntaxReceiver : ISyntaxContextReceiver //where TAttribute:Attribute
    {
        public List<IFieldSymbol> IdentifiedFields { get; } = [];

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not FieldDeclarationSyntax fieldDeclaration ||
                !fieldDeclaration.AttributeLists.Any()) return;
            var variableDeclaration = fieldDeclaration.Declaration.Variables;
            foreach (var field in variableDeclaration.Select(variable =>
                         context.SemanticModel.GetDeclaredSymbol(variable)))
                if (field is IFieldSymbol fieldInfo && fieldInfo.GetAttributes().Any(x =>
                        x.AttributeClass?.Name == "ObservablePropertyAttribute"))
                    IdentifiedFields.Add(fieldInfo);
        }
    }
}