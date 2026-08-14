# MirageUI

Dalamud プラグイン向けの ImGui UI キット。テキスト・テーマ・2 カラムレイアウトを統一した API で提供する。

**公開 API はすべて `Mirage*` プレフィックス付きです。**

```csharp
global using MirageUI;
global using static MirageUI.Ui.MirageLayout;  // Style, Cursor 等
```

## 目次

- [初期化](#初期化)
- [公開 API 一覧](#公開-api-一覧)
- [MirageUi](#mirageui)
- [MirageWindowDefaults](#miragewindowdefaults)
- [MirageUi.TwoColumn](#mirageuitwocolumn)
- [MirageTheme](#miragetheme)
- [MirageLayout](#miragelayout)

## 初期化

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

    public void Dispose() => MirageUi.Dispose();
}
```

- `ConfigureTheme` … 適用する配色を指定するデリゲート
- `Init` … フォント・画像レジストリの初期化。`Image` を使う場合は `ITextureProvider`、`IPluginLog` を渡す
- `Dispose` … リソース解放

ウィンドウでテーマを適用する例:

```csharp
public override void PreDraw()
{
    MirageTheme.EnsureDefaultsCaptured();
    _colorScope = MirageTheme.PushCustom(MirageTheme.ResolveAppliedColors());
}

public override void PostDraw() => MirageTheme.Pop(_colorScope);
```

## 公開 API 一覧

| 型 | 説明 |
| --- | --- |
| `MirageUi` | UI 描画ヘルパー |
| `MirageWindowDefaults` | 2 カラムウィンドウの既定サイズ・リサイズ可否 |
| `MirageTheme` | テーマ（配色）管理 |
| `MirageColorSettings` | テーマ配色設定 |
| `MirageColorSettingsJson` | 配色 JSON 入出力 |
| `MirageMessageState` | Message ダイアログ状態 |
| `MirageMessageButton` | Message ダイアログボタン |
| `MirageTwoColumnState` | 2 カラムレイアウトの状態 |
| `MirageTwoColumnEntry` | 左カラムのリスト項目 |
| `MirageTwoColumnSidebarHeader` | 左カラムのプラグイン情報 |
| `MirageTwoColumnSearchPosition` | 検索バーの位置（`Top` / `Bottom`） |
| `MirageLayout` | ImGui レイアウト補助（`Style`, `Cursor` 等） |

## MirageUi

### Text

| メソッド | 用途 | 既定 `color` | 既定 `fontSize` | 既定 `underline` |
| --- | --- | --- | --- | --- |
| `Header` | 大見出し | `Color.Title` | `FontSize.Large` | `true` |
| `HeaderWithBool` | 右端 ON/OFF 付き見出し | `Color.Title` | `FontSize.Large` | `true` |
| `HeaderWithRunToggle` | 右端 Start/Stop トグル付き見出し | `Color.Title` | `FontSize.Large` | `true` |
| `SubHeader` | 小見出し | `Color.Title` | `FontSize.Default` | `true` |
| `Text` | 本文 | `Color.Default` | `FontSize.Default` | `false` |

```csharp
MirageUi.Header("タイトル");
MirageUi.HeaderWithBool("機能名", true, trueText: "ON", falseText: "OFF");
if (MirageUi.HeaderWithRunToggle("Auto Tribe", isRunning))
{
    // Start or Stop clicked
}
MirageUi.SubHeader("設定");
MirageUi.Text("説明文", color: MirageUi.Color.Secondary);
```

`MirageUi.Color`: `Default`, `Secondary`, `Accent`, `Title`, `Warning`, `PanelOverlay`

### Image

```csharp
MirageUi.Image(path, width, height, isCircle: true);
```

ローカルファイルパスの画像を表示する。テクスチャ未ロード時は `false` を返す。

### Message

中央モーダル。`bool` / `MirageMessageState.Visible` で表示切替。ウィンドウ全体に半透明グレーをかぶせ、その上にメッセージを表示する。

```csharp
var message = new MirageMessageState();

// 表示
message.Show(
    "Confirm",
    "Run Auto Tribe now?",
    FontAwesomeIcon.QuestionCircle,
    new MirageMessageButton { Label = "Cancel" },
    new MirageMessageButton { Label = "OK", Primary = true, OnClick = Start });

// ホストウィンドウ Draw の末尾
MirageUi.Message.Draw(message);

// または
MirageUi.Message.Draw(ref visible, "Title", "Body", FontAwesomeIcon.InfoCircle,
    new MirageMessageButton { Label = "OK", Primary = true, OnClick = () => { } });
```

| API | 説明 |
| --- | --- |
| `Message.Draw(state)` | State の Visible で表示 |
| `Message.Draw(ref visible, title, message, icon, buttons)` | bool で表示 |
| `MirageMessageButton` | ラベル / OnClick / Primary / CloseOnClick（最大 3） |

### Button

Accent-styled action button. `Primary` is filled stronger; `Secondary` is lighter.

```csharp
if (MirageUi.PrimaryButton("Change Gearset", enabled: selected >= 0))
{
    // clicked
}

if (MirageUi.SecondaryButton("Cancel"))
{
}

if (MirageUi.Button("Save", MirageUi.ButtonKind.Primary, width: 120f))
{
}
```

| API | Description |
| --- | --- |
| `Button(label, kind, ...)` | Primary / Secondary |
| `PrimaryButton` / `SecondaryButton` | Shortcuts |
| `width` | `0` = auto size; `> 0` = fixed width |

### Dropdown


アクセントカラーのプルダウン。1行をコンテンツ幅いっぱいにし、左ラベル（左寄せ）と右コントロール（固定時は右寄せ）の幅をそれぞれ指定できる。両方固定なら余りは中央、片方だけ固定ならもう片方が伸びる。既定はラベル 128・コントロールは残り幅。

```csharp
var selected = "Paladin";
if (MirageUi.Dropdown("Battle", ref selected, gearsetNames, placeholder: "(not set)"))
{
    // 選択変更
}

// カスタム項目
if (MirageUi.BeginDropdown("Job", preview, id: "job", hasValue: true))
{
    if (MirageUi.DropdownItem("Paladin", selected == "Paladin"))
        selected = "Paladin";
    MirageUi.EndDropdown();
}
```

| API | 説明 |
| --- | --- |
| `Dropdown(label, ref selected, items, ...)` | 文字列リストのプルダウン |
| `SearchableDropdown(..., ref searchFilter, ...)` | 検索付きプルダウン |
| `BeginDropdown` / `EndDropdown` | カスタム項目用 |
| `DropdownItem(label, selected)` | 項目（選択中はフォーカス） |
| `FieldLabelColumnWidth` | 左ラベル列の既定固定幅（128）。`labelWidth: 0` でラベル側を伸縮 |
| `labelWidth` / `width` | ラベル／コントロールの固定幅。`<= 0` はその側を伸縮 |

### Input


アクセントカラーのテキスト／数値入力。レイアウトは Dropdown と同じ（行幅 100%、`labelWidth` / `width`）。

```csharp
var name = "Untitled";
if (MirageUi.InputText("Name", ref name, 128))
{
    // changed
}

var level = 0;
MirageUi.InputInt("Job level (0=Any)", ref level);

var time = 0f;
MirageUi.InputFloat(string.Empty, ref time, format: "%.1f", id: "time", width: MirageUi.InputWidthFill);
```

| API | 説明 |
| --- | --- |
| `InputText` | 文字列入力（`hint` でプレースホルダ可） |
| `InputInt` | 整数入力 |
| `InputFloat` | 小数入力 |
| `InputWidthFill` | 右列いっぱい（またはラベルなし時の全幅） |

### その他

| API | 説明 |
| --- | --- |
| `PaddedSeparator()` | 上下に余白を持つ区切り線 |
| `SearchFilter(id, ref filter, hint, maxLength)` | 全幅検索入力欄 |
| `MatchesFilter(key, label, filter)` | エントリの絞り込み判定 |
| `OverlayFill(screenPos, size, rounding, flags)` | 背景塗りつぶし |
| `PushFont(FontSize)` | `Default` / `Large` フォントの切り替え |

## MirageWindowDefaults

2 カラムレイアウト用 Dalamud `Window` の既定サイズとリサイズ可否。

| メンバー | 既定値 | 説明 |
| --- | --- | --- |
| `DefaultSize` | `(900, 630)` | 初回表示時のウィンドウサイズ |
| `MaximumSize` | `(4096, 2160)` | リサイズ時の上限（`Resizable = true` のとき） |
| `Resizable` | `false` | `false` で `ImGuiWindowFlags.NoResize` を付与 |

```csharp
using Dalamud.Interface.Windowing;

public sealed class MyWindow : Window
{
    public MyWindow() : base("My Plugin###MyPlugin_Main")
    {
        MirageWindowDefaults.ApplyTo(this);
    }
}
```

## MirageUi.TwoColumn

左カラム（サイドバー）と右カラム（メインコンテンツ）の 2 カラムレイアウト。

**レイアウト種別ごとに別実装はしない。** `MirageTwoColumnState` のパラメータで表示要素を切り替える。

### 基本

```csharp
using MirageUI.Layout;

var state = new MirageTwoColumnState
{
    ShowSidebar = true,
    ShowSidebarHeader = true,
    ShowSearch = true,
    SearchPosition = MirageTwoColumnSearchPosition.Top,
    ShowEntryToggle = false,
    SidebarHeader = new MirageTwoColumnSidebarHeader
    {
        ImagePath = "path/to/icon.png",
        ImageWidth = 48f,
        ImageHeight = 48f,
        ImageIsCircle = true,
        Title = "My Plugin",
        Subtitle = "v1.0.0",
    },
    Entries =
    [
        new MirageTwoColumnEntry { Id = "settings", Label = "Settings", Enabled = true },
    ],
    SelectedId = "settings",
    OnSelectionChanged = id => { /* 選択変更 */ },
    OnEnabledChanged = (id, enabled) => { /* トグル変更 */ },
};

MirageUi.TwoColumn.Draw(state, () =>
{
    MirageUi.Header("詳細");
});
```

### 表示パラメータ

| プロパティ | 型 | 既定値 | 説明 |
| --- | --- | --- | --- |
| `ShowSidebar` | `bool` | `true` | 左カラム全体の表示/非表示 |
| `ShowSidebarHeader` | `bool` | `true` | プラグイン情報（アイコン・タイトル・サブタイトル） |
| `ShowSearch` | `bool` | `false` | 検索バー |
| `SearchPosition` | `MirageTwoColumnSearchPosition` | `Top` | 検索バーの位置 |
| `ShowEntryToggle` | `bool` | `false` | 各項目のチェックボックス |

`ShowSidebar = false` のときは右カラムのみ全幅表示する。

### 左カラムの構成

```text
[左カラム（固定領域）]
  ├ プラグイン情報 + PaddedSeparator   … ShowSidebarHeader
  ├ 検索バー（上部）                     … ShowSearch + Top
  ├ リスト（スクロール可）
  └ 検索バー（下部）                     … ShowSearch + Bottom

[右カラム]
  └ drawMainContent で渡した内容
```

スクロールできるのはリストのみ。プラグイン情報・検索バーは固定。

### 構成例

| 構成 | 設定 |
| --- | --- |
| リストのみ | `ShowSearch=false`, `ShowEntryToggle=false` |
| 検索 + リスト | `ShowSearch=true`, `SearchPosition=Top` |
| 検索 + トグル + リスト | `ShowSearch=true`, `SearchPosition=Bottom`, `ShowEntryToggle=true` |
| メインのみ | `ShowSidebar=false` |

### その他のプロパティ

| プロパティ | 説明 |
| --- | --- |
| `SidebarWidth` | 左カラム幅（デフォルト `304`） |
| `SidebarPadding` / `MainPadding` | 左カラム・右カラムの余白 |
| `SearchHint` / `SearchMaxLength` | 検索欄のヒント・最大文字数 |
| `ItemSpacing` | リスト行間の余白 |
| `Entries` | 表示項目リスト |
| `SelectedId` | 選択中の項目 ID |
| `SearchFilter` | 検索テキスト |
| `ScrollSelectedIntoView` | `true` で選択項目をスクロール表示内に寄せる |
| `ShowDebugBorders` | 子ウィンドウのデバッグ用枠線 |

## MirageTheme

```csharp
// デフォルトプリセットに戻す
MirageTheme.ResetToDefault(settings);

// Dalamud 標準配色に戻す
MirageTheme.ResetToDalamudDefaults(settings);

// JSON 入出力
var json = MirageColorSettingsJson.Export(settings);
MirageColorSettingsJson.TryImport(json, out var imported);
```

`MirageColorSettings` では ImGui の背景色・ヘッダー色・フォントサイズ等を持つ。`SetColor` / `GetColor` で `MirageUi.Color` 毎の文字色も編集できる。

## MirageLayout

ImGui のカーソル・スタイルへのショートカット。

```csharp
using static MirageUI.Ui.MirageLayout;

var avail = Style.ContentRegionAvail;
Cursor.Y += 8f;
```

| メンバー | 説明 |
| --- | --- |
| `Style.Scale` | Dalamud のグローバルスケール |
| `Style.ContentRegionAvail` | 利用可能領域サイズ |
| `Cursor.Position` | ウィンドウ内カーソル位置 |
| `Cursor.ScreenPosition` | スクリーン座標 |

## ライセンス

AGPL-3.0-or-later
