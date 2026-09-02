---
name: oni-modding
description: Develop Oxygen Not Included (缺氧) C# Harmony mods in this workspace. Use when writing, debugging, or packaging ONI mods; when the user mentions 缺氧, ONI, Harmony, UserMod2, kanim, LocString, facades, DrywallTileSkins, ModListPreviews, or mods/src; when Player.log, mod.yaml, or Assembly-CSharp is involved.
---

# ONI modding (agent)

Work as the Cursor agent in this mods workspace. Always-on constraints: repo `AGENTS.md` and `.cursor/rules/`. This skill is the longer playbook.

Do not tell the user to install Unity, Visual Studio templates, ILSpy GUI, or ILMerge.

## This machine

| What | Path |
|------|------|
| Game | `E:\Steam\steamapps\common\OxygenNotIncluded` |
| Managed DLLs | `E:\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed` |
| Workspace / mods root | `C:\Users\Administrator\Documents\Klei\OxygenNotIncluded\mods` |
| Source | `mods\src\<ModName>\` |
| Loaded build | `mods\local\<ModName>\` (`OutputPath` in csproj) |
| Player.log | `C:\Users\Administrator\AppData\LocalLow\Klei\Oxygen Not Included\Player.log` |
| Config | `mods\config\` |
| Game API dumps | `ref/oni-api/` (`INDEX.md`) |

Player: Chinese UI, 4K (`3840×2054`). Game language **code is often `en`** even when UI is Chinese (`System Language=ChineseSimplified`). All mods live in this one GitHub repo (`src/<ModName>/`). Only the workspace root has `.git`. GitHub copy is Markdown (`README.md`); Workshop paste is BBCode (`Description.txt`, English then Chinese, Compat_All.png in both languages).

## Ignore outdated guides

[O-n-y template](https://github.com/O-n-y/OxygenNotIncludedModTemplate) is useful for Harmony *ideas* only. Do **not** copy:

- .NET Framework 4.0 Class Library
- `using Harmony;` / Harmony 1 `HarmonyPatch` on old API
- static `Loader.OnLoad()` without `UserMod2`
- ILMerge shared-lib pipeline
- `lastWorkingBuild: 449549` era `mod_info.yaml`
- OOL `TranslationBase` / `LanguageSelection` (this workspace uses `LocString` + `translations\zh.po`)

Current stack (match existing `src` mods):

- SDK csproj, `netstandard2.1`, `HarmonyLib`, game `0Harmony.dll`
- `KMod.UserMod2`, `APIVersion: 2`, `supportedContent: ALL`
- `packaging\mod.yaml` + `packaging\mod_info.yaml` copied AfterBuild
- Game refs `Private=false`

## New or existing mod

1. Clone layout from `src\CustomChineseFonts` or `src\DrywallTileSkins` (smallest vs feature-rich).
2. `staticID` in `mod.yaml` like `DarrenLee.<PascalName>`.
3. `mod_info.yaml`: `supportedContent: ALL`, `APIVersion: 2`, `minimumSupportedBuild: 736649` (or current game build from Player.log `U59-…`).
4. Entry:

```csharp
public class Mod : UserMod2
{
	public static Harmony HarmonyInstance;
	public static string ContentPath;

	public override void OnLoad(Harmony harmony)
	{
		HarmonyInstance = harmony;
		base.OnLoad(harmony);
		ContentPath = path;
		Debug.Log("[ModName] loaded, content=" + ContentPath);
	}
}
```

5. `OutputPath` → `$(MSBuildProjectDirectory)\..\..\local\<ModName>\`
6. AfterBuild copy yaml, `translations\`, and any `packaging\` assets.
7. Build: `dotnet build -c Release` in the `src` project folder.
8. User must **fully quit the game** to pick up DLL/PNG. Hot reload is not enough.
9. GitHub `README.md` (Markdown) plus Workshop `Description.txt` (BBCode, English then Chinese, Compat_All.png). Add a row to the root `README.md` table. See below.

Add Unity module refs only when the compiler asks (`ImageConversionModule`, `UI`, `TextMeshPro`, `JSONSerializeModule`, …).

MSB3277 on `System.IO.Compression` / `System.Net.Http` vs Assembly-CSharp is expected. Ignore unless the build fails.

## Harmony

- Namespace `HarmonyLib`. Prefix/Postfix/Transpiler as usual; `__instance`, `__result`, `__state`.
- Prefer `[HarmonyPatch(typeof(T), nameof(T.M))]`. If the type is not in the compiler-facing DLL surface, `AccessTools.TypeByName` + patch inside `Db.Initialize` Prefix.
- `OnLoad` Harmony instance is already created by `UserMod2`; extra `new Harmony(...)` is usually wrong.
- Cross-mod (True Tiles, etc.): hook `OnAllModsLoaded` and/or their registration method; they may populate assets **after** `Db.Initialize`.
- Capture vanilla kanim in `KAnimFile.FinalizeLoading` Prefix (name check). `Assets.GetAnim` can still be null that early.

Startup (typical): first `Localization.Initialize` (may be **before** DLL load) → DLL `OnLoad` → second `Localization.Initialize` (I18n hook) → `Db.Initialize` / `LoadGeneratedBuildings` → `OnAllModsLoaded`. Register strings on the **second** Initialize. Register facades when `Db.Get().Permits` exists; retry from several postfixes if the first run has no patterns yet.

## I18n (this player)

Vanilla UI can be Chinese while `Localization.GetCurrentLanguageCode()` is `"en"`. Loading only `en.po` leaves mod text in English. Always treat Chinese OS / CJK vanilla strings as “load `zh.po`”.

Required:

- `STRINGS` nested class with `LocString` fields (English msgid).
- `translations\zh.po` with `msgctxt "Namespace.STRINGS...."` matching `RegisterForTranslation`.
- Hook `Localization.Initialize` Postfix: `RegisterForTranslation` → load `.po` → `OverloadStrings` → `CreateLocStringKeys`.
- Locale list: `GetLocale().Code`, `GetCurrentLanguageCode()`, aliases (`schinese` → `zh`), **plus** `Application.systemLanguage` in `{Chinese, ChineseSimplified, ChineseTraditional}`, **plus** CJK in a vanilla `Strings.TryGet` if needed.
- Log folder, codes, and whether a `.po` loaded. `codes=en` + Chinese UI = bug.

`PermitResource.Name` / `Description` and `BuildingFacades.Add(..., LocString, LocString)` **snapshot strings**. Load `.po` before `Collect` / `facades.Add`. Prefer resolving `STRINGS.*` at register time, not a stored English `string` from collect time.

Do not invent a custom picker if vanilla blueprints/inventory exist. `Strings.Add` for inventory subcategory keys if you inject `InventoryOrganization` ids.

Details: [reference.md](reference.md).

## Workshop Description.txt (BBCode) vs GitHub README.md

This workspace is a **single repository** of every mod, like [aki-art/ONI-Mods](https://github.com/aki-art/ONI-Mods). Keep new mods under `src/<ModName>/`. Do not create a separate GitHub repo per mod.

| File | Audience | Syntax |
|------|----------|--------|
| root `README.md` | GitHub index | Markdown table of mods |
| `src/<Mod>/README.md` | GitHub | Markdown; same facts as the Workshop page |
| `src/<Mod>/Description.txt` | Steam Workshop paste | BBCode |

Each `Description.txt` is pasted into the Steam Workshop item description. Steam renders BBCode, not Markdown. Match `src/ModListPreviews/Description.txt`.

Layout:

```
[h1]English Name[/h1]

Plain English pitch.

[img]https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/refs/heads/master/Compat_All.png[/img]

[h3]Features[/h3]
[list]
[*]…
[/list]

[hr][/hr]

[h1]中文名 / English Name[/h1]

[i]中文一句话介绍。[/i]

这是一个 Oxygen Not Included …模组。

[img]https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/refs/heads/master/Compat_All.png[/img]

[h3]功能[/h3]
[list]
[*]…
[/list]
```

English first, then `[hr][/hr]`, then Chinese. Both languages must include the Compat_All `[img]`. Use `[h1]` / `[h3]` only (no `[h2]`). Chinese pitch is `[i]…[/i]`; English pitch is plain text. `[b]`, `[i]` (also for paths), `[list]` / `[olist]` with `[*]`, `[code]`, `[url=https://…]label[/url]`. Numbered how-tos are `[olist]`. JSON or literal square brackets go in `[code]` or `[noparse]`.

`Description.txt` must **not** use Markdown (`#` headings, `**bold**`, `- ` bullets, `` `code` ``, `[text](url)`). Those show as raw characters on the Workshop page. Skip `[table]`, `[color]`, `[size]`, `[spoiler]`, `[previewyoutube]`. `README.md` uses Markdown for the same facts, plus `![ALL DLC](Compat_All url)` and a link to `Description.txt`.

Details: [reference.md](reference.md).

## Textures, kanim, 4K

Vanilla wallpaper/kanim textures typically: **Trilinear, mipmaps, aniso**. Runtime `new Texture2D(w,h,RGBA32,false)` + `Apply(false)` has **no mips** → jagged shimmer when the camera zooms out. Point / nearest upsample looks like pixel art (this player rejects that). Bilinear minify without mips also crawls.

- Generate mipmaps (`mipChain: true`, `Apply(true, false)`), `FilterMode.Trilinear`.
- Downsample with **area/box** (or GPU blit from a mipped source), not 4-tap bilinear.
- Do not stamp 2048 atlases on CPU per skin (load hitch). 512 atlas (2× vanilla 256) is the current compromise; build file UVs stay 0–1.
- Copy wrap/aniso from the vanilla template texture when cloning kanims.
- 9-slice tile interiors: crop center cell (~1/6 inset). Hide drywall caps via `AnimTileable` symbol visibility, not a new building.
- Per-skin kanim group: `maxGroupSize = 1` so batching does not share one atlas.

Details: [reference.md](reference.md).

## UI

- Clone vanilla widgets (`ModsScreen` `EnabledToggle` **MultiToggle**), not `KButton` + `"☑"` text.
- `LocText` on hints steals clicks unless `raycastTarget = false`.
- Settings that change registered content usually need a full restart; say so in the UI string.

## Debug

1. `dotnet build -c Release`.
2. Fully quit game, relaunch, enable mod under **local**.
3. Grep Player.log for `[ModName]`. Missing `mod_info.yaml` in the loaded folder → “will not be loaded”.
4. Decompile when guessing APIs: read `ref/oni-api/` first. If missing, `python ref/oni-api/scan_dll.py TypeName`, then paste a focused excerpt into that folder and update `INDEX.md`. Do not ask the user to click Unity.

Constraints this player has used: no new building when hanging on vanilla is enough; no custom blueprint UI when `BuildingFacades` works; Chinese strings in `zh.po`; one GitHub repo (root `.git` only, allowlist `.gitignore`); `README.md` Markdown + `Description.txt` Workshop BBCode (en then zh, Compat_All.png).
