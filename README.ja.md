# MirageUI

[English](README.md)

![MirageUI](docs/screenshots/hero-1280x720.png)

Dalamud プラグイン向けの ImGui UI ライブラリです。テキスト、テーマ、ウィンドウ、および2列レイアウトを構成します。現在のバージョンは 2.0.0 です。

**公開 API の名前は `Mirage` から始まります。**

```csharp
global using MirageUI;
global using static MirageUI.Ui.MirageLayout;  // Style、Cursor など
```

## 目次

- [セットアップ](#セットアップ)
- [ウィンドウ](#ウィンドウ)
- [プラグインページ](#プラグインページ)
- [言語](#言語)
- [2列レイアウト](#2列レイアウト)
- [ウィジェット](#ウィジェット)
- [テーマ](#テーマ)
- [レイアウト](#レイアウト)

## セットアップ

最初に `ConfigureTheme` で色を設定してから、`Init` を呼び出します。`Init` はフォント、画像、MirageUI の文字列、およびプラグインページを準備します。`Image` を使用する場合は `ITextureProvider` を渡してください。

```csharp
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using MirageUI;
using MirageUI.Theme;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface, ITextureProvider textureProvider, IPluginLog log)
    {
        MirageUi.ConfigureTheme(() => MirageColorSettings.CreateDefault());
        MirageUi.Init(pluginInterface, textureProvider, log);
    }

    public void Dispose()
    {
        MirageWindowCapture.Dispose();
        MirageUi.Dispose();
    }
}
```

ウィンドウでは、描画前に色を適用し、描画後に復元します。スクリーンショットのコピーは `PostDraw` で行います。

```csharp
public override void PreDraw() =>
    _colorScope = MirageTheme.PushCustom(MirageTheme.ResolveAppliedColors());

public override void PostDraw()
{
    MirageWindowCapture.Copy();
    MirageTheme.Pop(_colorScope);
}
```

## ウィンドウ

`MirageWindowHeader` はタイトルバーと左側のレールを描画します。`Draw` は `BlurStrength` でウィンドウ背景を一度ぼかします。タイトルバーとレールは `ChromeBg`、ページは `PageBg` です。同じフレームの同じウィンドウでは、重ねてぼかしません。メニュー、折りたたみ、閉じる、およびレールの展開ボタンの位置は固定されています。その他のボタンはすべて `MirageWindowFeature` として渡します。

ページ項目は、指定された順序通りの `Section` リストになります。このリストは機能ボタンとは別になっています。

```csharp
var sections = new MirageWindowHeader.Section[]
{
    new("modules", "Modules", FontAwesomeIcon.ThLarge),
};

var features = new MirageWindowFeature[]
{
    new()
    {
        Id = "plugin",
        PageId = "plugin",
        Icon = FontAwesomeIcon.Heart,
        ShowOnTitle = true,
        TitleKind = MirageWindowFeatureKind.OpenPage,
        TitleOrder = 0,
    },
};

var rail = MirageWindowHeader.RailWidth(railCollapsed, sections, features);
MirageWindowHeader.Draw(this, title, ref collapsed, leftInset: 0f, ref pageId, features);
MirageWindowHeader.DrawRail(sections, ref pageId, ref railCollapsed, showSections: !collapsed, features);
```

各機能は以下のフィールドで指定します。

| フィールド | 設定値 |
| --- | --- |
| ページ | `PageId` を設定。空の場合はページなし |
| タイトル | `ShowOnTitle`。`TitleKind` は `OpenPage` または `Shortcut` |
| タイトルフッター | `Shortcut` かつ `TitleOpensPage` の場合、設定ページを開く行を表示 |
| レール | `ShowOnRail`。`RailWhen` は `Collapsed`、`Expanded`、または `Both` |
| レールの種類 | `RailKind` |
| レールフッター | `Shortcut` かつ `RailOpensPage` の場合、設定ページを開く行を表示 |

`OpenPage` とフッターは `PageId` が設定されている場合にのみ適用されます。タイトルからページを開くとウィンドウが展開されます。レールからページを開いた場合は選択状態のみが変更されます。

順序は `TitleOrder` と `RailOrder` で指定します。数値が小さい方が先に配置されます。数値が同じ場合は渡された順序が維持されます。ボタンを表示する場合は `Icon` を渡します。ショートカットの内容は `DrawPopup` で描画し、幅は `PopupTextWidth`、各行は `MirageWindowHeader.PopupRow` で描画します。

レールのラベルは省略できます。すべてのラベルが空の場合、アイコンは横一列に並びます。ラベルが1つでも設定されている場合、ボタンは垂直に配置されます。折りたたみ中は、同じ順序でアイコンのみが上から下に表示されます。

Dalamud の IconFont は Font Awesome Free Solid のみ対応しています。ブランドアイコンは使用できません。

## プラグインページ

`Init` は、マニフェストから名前、作者、バージョン、GitHub URL、および `Data/plugin-icon.png` を読み込んで設定します。メッセージ、Discord、サポート URL は `ConfigurePluginInfo` で追加します。空の行は省略されます。

デフォルトのページでは、名前、バージョン、`by author`（作者名）、メッセージ、リンクが中央揃えで配置されます。`Data/plugin-icon.png` があるときだけ、その上にアイコンを出します。無いときは、その分の空白も出しません。`SetPluginPage` に描画コールバックを渡すと、`DrawPluginPage` がそのコールバックを使用してページ全体を描画します。`null` を渡すとデフォルトに戻ります。

画像が無いとき、左レールのマークは `TitleBarIcon` です。既定は `FontAwesomeIcon.Heart` です。別のアイコンは `ConfigurePluginInfo` で指定します。`IconCornerFraction` はアイコンの一辺に対する角の半径です。既定は `88/512` です。`0` にするとレールのシャドウとホバー下地は四角のままです。

2列レイアウトのオーバーレイは `TogglePluginPage` または `OpenPluginPage` で開きます。このオーバーレイは `SetPluginPage` を使わず、デフォルトのページを描画します。

```csharp
MirageUi.ConfigurePluginInfo(info =>
{
    info.Message = "Short note.";
    info.DiscordUrl = "https://discord.example";
});

MirageUi.SetPluginPage(() =>
{
    MirageUi.Header("Plugin");
});
```

### 表

`Table.Draw` はヘッダーと、任意の追加行を固定します。その間の行は縦にスクロールします。列幅が画面より広いときは、横スクロールバーひとつでヘッダー、行、追加行が一緒に動きます。表は入力欄と同じ角丸のパネルで、列を格子にはしません。`Stretch` の列は `Width` を最小幅にし、余った幅を受け取ります。`Align` は `Left` または `Center` で、ヘッダーとセル内のボタン文言に使います。コントロール二つ分くらいまでの狭い列では、`Center` はチェックボックスのように入力と同じ高さのコントロールもセル中央に置きます。既定は左寄せです。`CenterItem` は、入力より低いコントロールを入力と同じ縦の中心に置きます。`NextColumn` で次のセルへ進みます。行コールバックが `true` を返すと、その行で一覧を止めます。削除した行のあとを描かないときに使います。

## 言語

英語と日本語に対応しています。`SetLanguage` はコードが `ja` で始まる場合のみ日本語を選択します。それ以外のコードの場合は英語が選択されます。Dalamud の言語設定には連動しません。

`Init` は MirageUI の文字列をロードします。アプリ固有の文字列は、その後に `Register` または `RegisterEmbedded` で追加します。同じキーが存在する場合は、後から追加された値が保持されます。日本語のキーが存在しない場合は英語が返されます。英語も存在しない場合は、キー自体が返されます。

埋め込みリソースの論理名は `{prefix}.en.json` および `{prefix}.ja.json` です。

```csharp
MirageUi.I18n.SetLanguage("ja");
MirageUi.I18n.RegisterEmbedded(typeof(Plugin).Assembly, "MyPlugin.Localization");
var label = MirageUi.I18n.Get("nav.modules");
```

## 2列レイアウト

左側がサイドバー、右側がメインコンテンツになります。`MirageTwoColumnState` で表示内容を選択します。

```csharp
using MirageUI.Layout;

var state = new MirageTwoColumnState
{
    ShowSearch = true,
    ShowEntryToggle = true,
    Entries =
    [
        new MirageTwoColumnEntry { Id = "settings", Label = "Settings", Enabled = true },
    ],
    SelectedId = "settings",
    OnSelectionChanged = id => { },
    OnEnabledChanged = (id, enabled) => { },
};

MirageUi.TwoColumn.Draw(state, () =>
{
    MirageUi.Header("Details");
});
```

| プロパティ | デフォルト値 | 説明 |
| --- | --- | --- |
| `ShowSidebar` | `true` | 左列。`false` にすると右列が画面全体の幅を使用します |
| `ShowSearch` | `false` | サイドバー上部の検索バー |
| `ShowEntryToggle` | `false` | 各エントリーのチェックボックス |
| `SidebarWidth` | `304` | 左列の幅 |
| `SearchHint` | 空 | 空の場合は MirageUI の "Search" が使用されます |
| `SearchTrailingActions` | 空 | 検索欄と `×` の間に置くアイコンボタン |
| `SearchFilter` | 空 | 検索テキスト |
| `AutoSelectFirstOnSearch` | `true` | テキスト入力時に最初に一致した項目を選択。テキストをクリアしても選択は保持されます |
| `OnSearchFilterChanged` | null | 検索バーを描画するたびに、検索テキストを受け取ります |

検索行は `[検索][×]` です。`SearchTrailingActions` を設定すると `[検索][ボタン] | [×]` になります。`×` を押すとサイドバーが折りたたまれます。虫眼鏡アイコンは左側に配置されます。ID またはラベルの部分一致（大文字・小文字を区別しない）で検索されます。検索中、一致したフォルダーは展開され、エントリーの並び替えはできなくなります。

エントリーには `LabelColor` と `ContextMenuItems` を渡せます。右クリックでその行を選択し、メニューを開きます。`SidebarNodes` はフォルダーか、単独のページ項目です。`AlwaysExpanded` を指定したフォルダーは開いたままになります。`EnableEntryReorder` を有効にすると、フォルダー内、またはフラットな一覧の中でドラッグして並べ替えられます。新しい位置は `OnEntryReordered` に渡されます。

リスト部分のみがスクロールします。検索バーは固定されたままになります。

## ウィジェット

### テキスト

| メソッド | 用途 | デフォルトの色 | デフォルトのサイズ |
| --- | --- | --- | --- |
| `Header` | 大見出し | `Color.Title` | `FontSize.Large` |
| `HeaderWithBool` | 右側に ON/OFF を備えた見出し | `Color.Title` | `FontSize.Large` |
| `HeaderWithRunToggle` | 右側に Start/Stop を備えた見出し | `Color.Title` | `FontSize.Large` |
| `SubHeader` | 小見出し | `Color.Title` | `FontSize.Default` |
| `Text` | 本文 | `Color.Default` | `FontSize.Default` |

`MirageUi.Color` には `Default`、`Secondary`、`Accent`、`Title`、`Warning`、`PanelOverlay` があります。

### ボタン

`Primary` は強調された塗りを適用します。`Secondary` は控えめな表示になります。`width` を `0` にするとラベルの大きさに合わせられます。`width` を `0` より大きくすると固定幅になります。

| API | 説明 |
| --- | --- |
| `Button(label, kind, ...)` | `Primary` または `Secondary` |
| `PrimaryButton` / `SecondaryButton` | `Button` のショートカット名 |

### ドロップダウン

1行でコンテンツ領域の幅を満たします。左側のラベル幅と右側のコントロール幅はそれぞれ設定可能です。デフォルトのラベル幅は 128 で、残りの幅をコントロールが占めます。

| API | 説明 |
| --- | --- |
| `Dropdown(label, ref selected, items, ...)` | 文字列リスト |
| `MultiSelectDropdown(label, preview, items, selected, ...)` | 複数行を選んだままにします。選択中はヘッダー背景になり、ポップアップは閉じません |
| `SearchableDropdown(..., ref searchFilter, ...)` | 検索機能付き |
| `BeginDropdown` / `EndDropdown` | 項目を自前で描画 |
| `DropdownItem(label, selected)` | 1行分。選択中の行にフォーカスが当たります |
| `labelWidth` / `width` | `<= 0` に設定した側が引き伸ばされます |

ラベルが重複していると選択された行を区別できません。`label##id` のように記述して区別してください。

### 入力フィールド

レイアウトはドロップダウンと同様です。

| API | 説明 |
| --- | --- |
| `InputText` | 文字列。`hint` は空の時のテキスト |
| `InputInt` / `InputFloat` | 数値 |
| `DragInt` / `DragFloat` | ドラッグによる数値入力 |
| `SliderInt` / `SliderFloat` | スライダー |
| `InputWidthFill` | 右列の幅いっぱいに広げる |

### 画像とアイコン

| API | 説明 |
| --- | --- |
| `Image(path, width, height, ...)` | ローカルファイル。読み込まれていない場合は `false` を返します |
| `GameIcon(iconId, width, height, ...)` | ゲーム内アイコン。`framed: true` で入力欄と同じ枠に描きます |
| `ImageFrame(texture, min, max)` | その枠の中に画像を描きます。テクスチャが null のときは空の枠です |
| `TryGetGameIcon` | テクスチャのみを取得 |
| `IconButton` | アイコンボタン。デフォルトでは枠線なし |
| `IconToggleButton` | 2つの状態を持つボタン。デフォルトでは枠線あり |
| `IconChecklistButton` | チェックリストを開くボタン |
| `IconToolbar` | フィルター、並び替え、検索をまとめた1行ツールバー |
| `IconLabelToggleGrid` | アイコンとラベルのタイル表示 |
| `MultiSelectHighlightList` | 複数選択ハイライトリスト |

### その他

| API | Description |
| --- | --- |
| `PaddedSeparator` | 上下に余白のある区切り線 |
| `Checkbox` / `Radio` | チェックボックスとラジオボタン。入力欄と同じ高さと角丸 |
| `CheckboxGroup` | 下のブロックをインデントします。終了時は `using` を破棄します |
| `DisabledIf` | 子要素を無効化します。終了時は `using` を破棄します |
| `SearchFilter` | 左側に虫眼鏡アイコンが付いた入力フィールド |
| `MatchesFilter` | ID とラベルでの部分一致検索 |
| `Link` | リンクボタン |
| `ColorEdit4` | カラーピッカー |
| `ListRow` | タイトルと詳細の行 |
| `CollapsibleSubHeader` | 折りたたみ可能な小見出し |
| `Message.Draw` | 確認ダイアログ。親ウィンドウの最後に描画されます |
| `ContextMenu` | 右クリックメニュー |
| `PushFont` | `Default` または `Large` |

`Message` には最大 3 つのボタンを配置できます。各ボタンには `Primary`、`OnClick`、`CloseOnClick` が設定可能です。

## テーマ

適用する `MirageColorSettings` を返す関数を `ConfigureTheme` に渡します。`CreateDefault` は新しい白色の設定オブジェクトを作成して返します。`CreateWhite` および `CreateBlack` も利用可能です。`WindowBg` は透明度を含み、タイトルバーも同じ色を使います。`ChromeBg` はタイトルバー、サイドバー、メニューに重ねる色です。`PageBg` はページに重ねる色です。入力欄、ドロップダウン、枠付きアイコンボタンはアクセントです。ボタンとメッセージの枠もアクセントです。`Separator` はタイトルバーの下線、サイドバーとページの境、サイドバー内の区切り、ボタングループの区切り、2 カラムメニューの枠、ウィンドウメニューとショートカットの枠、メニューや見出しやショートカットの区切り線です。検索欄の枠は同じ色で、透明度は 0.28 です。`BlurStrength` の初期値は 8 です。0 にするとブラーは無効になります。

色は JSON 形式で読み書きできます。

```csharp
var json = MirageColorSettingsJson.Export(settings);
MirageColorSettingsJson.TryImport(json, out var imported);
```

`SetColor` と `GetColor` は、各 `MirageUi.Color` のテキスト色を読み書きします。

## レイアウト

```csharp
using static MirageUI.Ui.MirageLayout;

var avail = Style.ContentRegionAvail;
Cursor.Y += 8f;
```

| メンバー | 説明 |
| --- | --- |
| `Style.Scale` | Dalamud 全体のスケール |
| `Style.ContentRegionAvail` | 利用可能な残りの領域 |
| `Cursor.Position` | ウィンドウ内のカーソル位置 |
| `Cursor.ScreenPosition` | 画面上のカーソル位置 |

## ライセンス

AGPL-3.0-or-later