# ONI modding reference

## i18n recipe

```csharp
[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
public static class Localization_Initialize_Patch
{
	public static void Postfix() => I18n.Register();
}
```

Load order inside `Register`: `Localization.RegisterForTranslation(typeof(STRINGS))` → `LoadStringsFile(path, false)` → `Localization.OverloadStrings` → `LocString.CreateLocStringKeys(typeof(STRINGS), null)`. Optional: `Localization.GenerateStringsTemplate` into `Manager.GetDirectory()/strings_templates`.

`.po` header: `Language: zh`, `Content-Type: text/plain; charset=UTF-8`. Each entry:

```
msgctxt "DrywallTileSkins.STRINGS.DRYWALL_TILE_SKINS.FACADE_DESC"
msgid "Applies this tile's interior cell to Drywall, without tile borders."
msgstr "将此瓷砖的中间格纹理铺在干板墙上，无瓷砖边框。"
```

`msgctxt` is the full CLR path of the `LocString` field. AfterBuild must copy `translations\` into `mods\local\<Mod>\translations\`.

Locale codes to try (stop at first existing file):

1. `Localization.GetLocale()?.Code` and `GetCurrentLanguageCode()`
2. hyphen/underscore variants and language prefix (`zh-CN` → `zh`)
3. `schinese` / `tchinese` / `zh_*` → `zh`
4. If `Application.systemLanguage` is Chinese / ChineseSimplified / ChineseTraditional → `zh`
5. If a vanilla string already contains CJK (`U+4E00`–`U+9FFF`) → `zh`

Known failure: log line `codes=en` while UI shows 蓝图/选项. Dumping Sign may also print `Language code is en.`

Snapshot types: `Database.PermitResource` stores `Name` and `Description` as `string`. `BuildingFacades.Add(id, LocString Name, LocString Desc, ...)` converts immediately. Resolve `STRINGS.*` at `Add` time. Tile/building **names** from `BuildingDef.Name` / `Element.name` are already game-localized; only **mod** sentences need `.po`.

Inventory: `Strings.Add("STRINGS.UI.KLEI_INVENTORY_SCREEN.SUBCATEGORIES." + id.ToUpperInvariant(), display)`.

## Kanim clone

Capture from vanilla file (`walls_kanim` etc.): `animBytes`, `buildBytes`, `textureList[0]`. Create `KAnimFile` via `ScriptableObject.CreateInstance`, fill `mod = new KAnimFile.Mod { anim, build, textures }`, `FinalizeLoading()`, then `KAnimGroupFile.GetGroupFile().AddAnimFile`, parse build/anim with `KGlobalAnimParser`, copy `renderType` / `maxVisibleSymbols` from the template group, set `maxGroupSize = 1`, register in `Assets.Anims` / `AnimTable`.

Paint a new atlas the same UV layout as the template. Body vs cap vs `ui` symbols from `KAnim.Build.Symbol` hashes. Normalized UVs still work if atlas resolution is scaled (e.g. 256 → 512).

Vanilla `walls_kanim` on this install: 256×256, `filter=Trilinear`, `mips=9`, `wrap=Repeat`, `aniso=4`. Body opaque region ~103px; at 2× atlas ~206–209px.

## Texture filters

| Symptom | Cause | Fix |
|---------|--------|-----|
| Blocky / pixel-art | `FilterMode.Point` or nearest stamp | Bilinear magnify, never nearest for painted art |
| Jagged flicker when zooming out | No mipmaps | `new Texture2D(..., mipChain: true)` + `Apply(true, false)` + `Trilinear` |
| Moire after shrinking 512→~206 | Bilinear 4-tap minify | Box/area average over the dest pixel’s source rect |
| Menu hitch | Huge CPU atlas (2048) per skin | Stay at 512; stamp once |

`Graphics.Blit` + readable copy is the way to snapshot non-readable GPU textures.

## YAML

`mod.yaml`: `title`, `description`, `staticID`. `description` is a short in-game blurb, not the Workshop page.

GitHub: root `README.md` lists every mod; each `src/<Mod>/README.md` is Markdown. Workshop item description is `src/<Mod>/Description.txt`, written in Steam BBCode (not Markdown). English `[h1]Name[/h1]` first, `[hr][/hr]`, then Chinese `[h1]中文名 / Name[/h1]`. Both languages include `[img]https://raw.githubusercontent.com/Sgt-Imalas/Sgt_Imalas-Oni-Mods/refs/heads/master/Compat_All.png[/img]`. Tags that work: `[h1]` `[h3]` `[b]` `[i]` `[u]` `[strike]` `[url=…]` `[img]` `[list]` `[olist]` `[*]` `[code]` `[hr][/hr]` `[noparse]` `[quote]`. Do not use Markdown, `[h2]`, or forum-only tags (`[table]`, `[color]`, `[size]`, `[spoiler]`, `[previewyoutube]`).

`mod_info.yaml`:

```yaml
supportedContent: ALL
minimumSupportedBuild: 736649
APIVersion: 2
version: 1.0.0
```

`ALL` replaces old `VANILLA_ID,EXPANSION1_ID` for current builds. Missing yaml in the **loaded** folder (not only `src`): Klei skips the mod.

## Decompile without Unity

Game DLL:

```text
E:\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll
```

Curated excerpts: `ref/oni-api/` (`INDEX.md`). Search there first.

If the type is missing or the game updated:

```text
python ref/oni-api/scan_dll.py BuildingFacades
```

Then add a small `.cs` excerpt in `ref/oni-api/` and a row in `INDEX.md`. Do not dump the whole assembly. Do not write new dumps to `%TEMP%\oni-dump`.

## UI clone notes

`ModsScreen` row checkboxes are `MultiToggle`, not `KButton`. After clone, disable raycast on decorative `LocText` so clicks hit the toggle. Prefer `Object.FindObjectsByType` over obsolete `FindObjectOfType` when touching new Unity 6 APIs (game is Unity 6000.x).

## Facades

```csharp
facades.Add(id, nameLoc, descLoc, PermitRarity.Universal, ExteriorWallConfig.ID, kanimName, null, null, null, null);
resource.Init();
permits.Add(resource);
```

`Init` needs the prefab and anim already in `Assets`. Retry register from `BuildingFacades` ctor, `Db.Initialize` Postfix, `BuildingFacades.PostProcess`, and `OnAllModsLoaded`.
