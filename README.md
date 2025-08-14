# Flexible Dropdown

Unity Inspectorで柔軟なドロップダウンメニューを簡単に作成できるUnityエディター拡張です。属性にインターフェースを実装するだけで、その属性を付けたフィールドをドロップダウンメニューで選択できるようになります。

## 特徴

- 🎯 **シンプルな実装**: わずか数行のコードでカスタムドロップダウンを作成可能
- 🔧 **柔軟な設定**: 任意のデータ型に対応し、表示形式をカスタマイズ可能
- 🚀 **Source Generator**: PropertyDrawerを自動生成
- 🎨 **2つのスタイル**: 標準的なPopupFieldと検索可能なAdvancedDropdownを選択可能

## インストール

### Unity Package Manager経由

1. Unity EditorでWindow > Package Managerを開く
2. "+" ボタンをクリック > "Add package from git URL..."
3. 以下のURLを入力:
```
https://github.com/gameshalico/FlexibleDropdown.git?path=Packages/com.gameshalico.flexibledropdown
```

## 使用方法

### 基本的な使い方

1. `PropertyAttribute`を継承し、`IDropdownProvider<T>`インターフェースを実装した属性を作成:

```csharp
[AttributeUsage(AttributeTargets.Field)]
public class SampleDropdownAttribute : PropertyAttribute, IDropdownProvider<string>
{
    public IEnumerable<string> Choices => new[] { "Option1", "Option2", "Option3" };
}
```

2. フィールドに属性を設定する

```csharp
public class DemoComponent : MonoBehaviour
{
    [SerializeField]
    [SampleDropdown]
    private string _sceneName;
}
```

### 高度な設定

#### カスタム表示形式

```csharp
public class CustomDropdownAttribute : PropertyAttribute, IDropdownProvider<MyCustomType>
{
    public IEnumerable<MyCustomType> Choices => GetChoices();

    // リスト項目の表示形式をカスタマイズ
    public string FormatListItem(MyCustomType item)
    {
        return $"{item.Name} ({item.Id})";
    }

    // 選択された値の表示形式をカスタマイズ
    public string FormatSelectedValue(MyCustomType value)
    {
        return value?.Name ?? "Custom";
    }

    ...
}
```

#### プロパティフィールドの有効化

```csharp
public class PropertyFieldDropdownAttribute : PropertyAttribute, IDropdownProvider<string>
{
    public IEnumerable<string> Choices => new[] { "A", "B", "C" };
    
    // 手動入力を可能にする
    public bool IsPropertyFieldEnabled => true;
}
```

### ドロップダウンスタイル

#### Advanced Dropdown
AddComponentMenuのような検索可能なドロップダウンを使用可能

```csharp
[DropdownStyle(DropdownStyle.Advanced)]
public class SearchableDropdownAttribute : PropertyAttribute, IDropdownProvider<string>
{
    public IEnumerable<string> Choices => GetManyChoices();
}
```

## API リファレンス

### IDropdownProvider<TValue>

ドロップダウンの選択肢とカスタマイズを定義するインターフェース。

#### プロパティ

- `IEnumerable<TValue> Choices`: ドロップダウンの選択肢
- `TValue DefaultValue`: デフォルト値（省略可能、デフォルト: `default(TValue)`）
- `bool IsPropertyFieldEnabled`: 手動入力の有効化（省略可能、デフォルト: `false`）

#### メソッド

- `string FormatListItem(TValue item)`: リスト項目の表示形式
- `string FormatSelectedValue(TValue value)`: 選択値の表示形式

### DropdownStyleAttribute

ドロップダウンのスタイルを指定する属性。

#### DropdownStyle enum

- `Standard`: 標準的なPopupField
- `Advanced`: 検索可能なAdvancedDropdown

## サンプル

詳細なサンプルコードは[Assets/Scripts](Assets/Scripts)フォルダを参照してください。

## 要件

- Unity 2022.3.12f1 以降
