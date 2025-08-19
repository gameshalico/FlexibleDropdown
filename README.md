# Flexible Dropdown

Unityでフィールドのドロップダウン選択を簡単に実装できるエディター拡張ライブラリです。
`PropertyAttribute`に`IDropdownProvider<TValue>`インターフェースを実装するだけで、その`PropertyAttribute`を付けたフィールドをドロップダウンメニューで選択できるようになります。
シーン名、Id等の選択はもちろん、元々の`PropertyField`を併せて表示することもできるので、プリセット機能としても使えます。

## 特徴

- **Source Generator**: PropertyDrawerを自動生成するので、インターフェースの実装だけで完結
- **柔軟な設定**: 任意のシリアライズ可能なデータ型に対応し、表示名変更や階層分けも可能
- **2つのスタイル**: 標準的な`PopupField`と検索可能な`AdvancedDropdown`を選択可能
- **IMGUI/UIToolkit両対応**: IMGUI表示しかできない状況にも対応

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
    private string _sampleField;
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
