using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexibleDropdown.Generator
{
    [Generator(LanguageNames.CSharp)]
    public class FlexibleDropdownEditorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // IDropdownProvider<T>を実装したクラスを検出
            var customDropdownAttributeProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsAttributeClass(s),
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static m => m is not null);

            // コンパイル情報を取得
            var compilationAndClasses = context.CompilationProvider.Combine(customDropdownAttributeProvider.Collect());

            // エディター拡張クラスを生成
            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Right, spc));
        }

        private static bool IsAttributeClass(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 } or
                   ClassDeclarationSyntax { BaseList.Types.Count: > 0 };
        }

        private static AttributeClassInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            // クラスのシンボル情報を取得
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not { } classSymbol)
                return null;

            // PropertyAttributeを継承しているかチェック
            if (!InheritsFromPropertyAttribute(classSymbol))
                return null;

            // IDropdownProvider<T>を実装しているかチェック
            var implementedInterface = GetImplementedDropdownInterface(classSymbol);
            if (implementedInterface == null)
                return null;

            // ジェネリック型パラメータを取得
            var valueType = implementedInterface.TypeArguments.FirstOrDefault();
            if (valueType == null)
                return null;

            // DropdownStyle属性をチェック
            var dropdownStyle = GetDropdownStyle(classSymbol);

            return new AttributeClassInfo(
                classSymbol.ContainingNamespace.ToDisplayString(),
                classSymbol.Name,
                valueType.ToDisplayString(),
                dropdownStyle
            );
        }

        private static bool InheritsFromPropertyAttribute(INamedTypeSymbol classSymbol)
        {
            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "PropertyAttribute" && 
                    baseType.ContainingNamespace.ToDisplayString() == "UnityEngine")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }
            return false;
        }

        private static INamedTypeSymbol? GetImplementedDropdownInterface(INamedTypeSymbol classSymbol)
        {
            return classSymbol.AllInterfaces.FirstOrDefault(i =>
                i.Name == "IDropdownProvider" &&
                i.TypeArguments.Length == 1);
        }

        private static bool GetDropdownStyle(INamedTypeSymbol classSymbol)
        {
            // DropdownStyleAttribute を探す
            var dropdownStyleAttribute = classSymbol.GetAttributes().FirstOrDefault(attr =>
                attr.AttributeClass?.Name == "DropdownStyleAttribute");

            if (dropdownStyleAttribute == null)
                return false; // デフォルトはStandard (false)

            // DropdownStyle.Advanced の場合は true を返す
            if (dropdownStyleAttribute.ConstructorArguments.Length > 0)
            {
                var styleValue = dropdownStyleAttribute.ConstructorArguments[0].Value;
                return styleValue is int intValue && intValue == 1; // Advanced = 1
            }

            return false;
        }

        private static void Execute(ImmutableArray<AttributeClassInfo?> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
                return;

            var distinctClasses = classes.Where(c => c != null).Distinct().ToList();

            foreach (var classInfo in distinctClasses)
            {
                var source = GenerateDrawerClass(classInfo!);
                var fileName = $"{classInfo!.ClassName}Drawer.g.cs";
                context.AddSource(fileName, source);
            }
        }

        private static string GenerateDrawerClass(AttributeClassInfo classInfo)
        {
            // エディター専用の名前空間を生成
            var editorNamespace = "FlexibleDropdown.Editor.Generated";

            // スタイルに応じて基底クラスを選択
            var baseClass = classInfo.IsAdvanced 
                ? "FlexibleAdvancedDropdownDrawerBase"
                : "FlexibleDropdownDrawerBase";

            var fullAttributeClassName = string.IsNullOrEmpty(classInfo.Namespace) || classInfo.Namespace == "<global namespace>"
                ? $"global::{classInfo.ClassName}"
                : $"global::{classInfo.Namespace}.{classInfo.ClassName}";

            var source = $$"""
                // <auto-generated/>
                #nullable enable

                #if UNITY_EDITOR
                namespace {{editorNamespace}}
                {
                    [global::UnityEditor.CustomPropertyDrawer(typeof({{fullAttributeClassName}}))]
                    public sealed class {{classInfo.ClassName}}Drawer : global::FlexibleDropdown.Editor.{{baseClass}}<{{fullAttributeClassName}}, {{classInfo.ValueType}}>
                    {
                    }
                }
                #endif
                """;

            return source;
        }

        private class AttributeClassInfo
        {
            public string Namespace { get; }
            public string ClassName { get; }
            public string ValueType { get; }
            public bool IsAdvanced { get; }

            public AttributeClassInfo(string @namespace, string className, string valueType, bool isAdvanced)
            {
                Namespace = @namespace;
                ClassName = className;
                ValueType = valueType;
                IsAdvanced = isAdvanced;
            }

            public override bool Equals(object? obj)
            {
                return obj is AttributeClassInfo other &&
                       Namespace == other.Namespace &&
                       ClassName == other.ClassName &&
                       ValueType == other.ValueType &&
                       IsAdvanced == other.IsAdvanced;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + Namespace.GetHashCode();
                    hash = hash * 23 + ClassName.GetHashCode();
                    hash = hash * 23 + ValueType.GetHashCode();
                    hash = hash * 23 + IsAdvanced.GetHashCode();
                    return hash;
                }
            }
        }
    }
}