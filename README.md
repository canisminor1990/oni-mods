# ONI Mods / 缺氧模组

Mods for [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included). All of them live in this one repository, same idea as [aki-art/ONI-Mods](https://github.com/aki-art/ONI-Mods).

缺氧模组合集。所有模组都在同一个仓库里（`src/<ModName>/`），不是一模一组仓库。

Edit each mod’s `README.md` (Markdown). `npm run desc` generates `Description.txt` (Steam Workshop BBCode). Paste that file into the Workshop item.

只改各模组的 `README.md`，然后 `npm run desc` 生成工坊用的 `Description.txt`。

## Mods

| Folder | Title | Description | Steam |
|--------|-------|-------------|-------|
| [CustomChineseFonts](src/CustomChineseFonts) | Custom Chinese Fonts / 自定义中文字体 | Custom Simplified Chinese UI fonts. Body defaults to HarmonyOS Sans SC; titles can be switched in Settings. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794395157) |
| [DrywallTileSkins](src/DrywallTileSkins) | Drywall Tile Skins / 干板墙瓷砖皮肤 | Turns loaded tile textures into vanilla Drywall blueprint skins. Interior cell only, no tile borders. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794025758) |
| [DuplicantPortraits](src/DuplicantPortraits) | Duplicant Portraits / 复制人头像画 | Masterpiece portraits of every loaded duplicant personality on the vanilla 2x2 Blank Canvas. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794390677) |
| [ModListPreviews](src/ModListPreviews) | Mod List Previews / 模组封面预览 | Shows Steam Workshop cover thumbnails in the in-game Mods list. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794018361) |

## Layout

| Path | Role |
|------|------|
| `src/<ModName>/` | Source and `README.md` (edit this) |
| `src/<ModName>/Description.txt` | Generated Workshop BBCode (`npm run desc`) |
| `src/<ModName>/packaging/` | `mod.yaml` / `mod_info.yaml` |
| `local/<ModName>/` | Game-loaded build (`OutputPath`, gitignored) |

```text
npm run build
npm run build -- DrywallTileSkins
npm run desc
npm run steam-ids
npm run package
```

`package` = compile every mod + regenerate Workshop descriptions. Fully quit the game to pick up a new DLL or PNG; hot reload is not enough.
