# ONI Mods / 缺氧模组

Mods for [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included). All of them live in this one repository, same idea as [aki-art/ONI-Mods](https://github.com/aki-art/ONI-Mods).

缺氧模组合集。所有模组都在同一个仓库里（`src/<ModName>/`），不是一模一组仓库。

Steam Workshop item descriptions are BBCode in each mod’s `Description.txt` (paste as-is). GitHub copy is Markdown in `README.md`.

创意工坊物品描述用各模组的 `Description.txt`（BBCode，直接粘贴）。GitHub 说明用 `README.md`（Markdown）。

## Mods

| Folder | Title | Description |
|--------|-------|-------------|
| [CustomChineseFonts](src/CustomChineseFonts) | Custom Chinese Fonts / 自定义中文字体 | Custom Simplified Chinese UI fonts. Body defaults to HarmonyOS Sans SC; titles can be switched in Settings. |
| [DrywallTileSkins](src/DrywallTileSkins) | Drywall Tile Skins / 干板墙瓷砖皮肤 | Turns loaded tile textures into vanilla Drywall blueprint skins. Interior cell only, no tile borders. |
| [DuplicantPortraits](src/DuplicantPortraits) | Duplicant Portraits / 复制人头像画 | Masterpiece portraits of every loaded duplicant personality on the vanilla 2x2 Blank Canvas. |
| [ModListPreviews](src/ModListPreviews) | Mod List Previews / 模组封面预览 | Shows Steam Workshop cover thumbnails in the in-game Mods list. |

## Layout

| Path | Role |
|------|------|
| `src/<ModName>/` | Source, GitHub `README.md`, Workshop `Description.txt` |
| `src/<ModName>/packaging/` | `mod.yaml` / `mod_info.yaml` |
| `local/<ModName>/` | Game-loaded build (`OutputPath`, gitignored) |

```text
dotnet build -c Release
```

Run that in the `src/<ModName>` folder. Fully quit the game to pick up a new DLL or PNG; hot reload is not enough.
