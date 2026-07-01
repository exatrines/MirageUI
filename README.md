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
| `MirageTheme` | テーマ（配色）管理 |
| `MirageColorSettings` | テーマ配色設定 |
| `MirageColorSettingsJson` | 配色 JSON 入出力 |
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
| `SubHeader` | 小見出し | `Color.Title` | `FontSize.Default` | `true` |
| `Text` | 本文 | `Color.Default` | `FontSize.Default` | `false` |

```csharp
MirageUi.Header("タイトル");
MirageUi.HeaderWithBool("機能名", true, trueText: "ON", falseText: "OFF");
MirageUi.SubHeader("設定");
MirageUi.Text("説明文", color: MirageUi.Color.Secondary);
```

`MirageUi.Color`: `Default`, `Secondary`, `Accent`, `Title`, `Warning`, `PanelOverlay`

### Image

```csharp
MirageUi.Image(path, width, height, isCircle: true);
```

ローカルファイルパスの画像を表示する。テクスチャ未ロード時は `false` を返す。

### その他

| API | 説明 |
| --- | --- |
| `PaddedSeparator()` | 上下に余白を持つ区切り線 |
| `SearchFilter(id, ref filter, hint, maxLength)` | 全幅検索入力欄 |
| `MatchesFilter(key, label, filter)` | エントリの絞り込み判定 |
| `OverlayFill(screenPos, size, rounding, flags)` | 背景塗りつぶし |
| `PushFont(FontSize)` | `Default` / `Large` フォントの切り替え |

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
