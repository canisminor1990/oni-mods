# ONI Mods / 缺氧模组

[Mods made by CanisMinor](https://steamcommunity.com/workshop/filedetails/?id=3794379693)

## Mods

<!-- mods-table -->
| Cover | Folder | Title | Description | Steam |
|-------|--------|-------|-------------|-------|
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/CustomChineseFonts/packaging/preview.png?raw=true" width="96" alt="Custom Chinese Fonts"> | [CustomChineseFonts](src/CustomChineseFonts) | Custom Chinese Fonts / 自定义中文字体 | Custom Simplified Chinese UI fonts for Oxygen Not Included. Body text defaults to HarmonyOS Sans SC; titles can be switched in Settings, and you can drop in your own fonts. This mod uses HarmonyOS Sans Fonts. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794395157) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/DrywallTileSkins/packaging/preview.png?raw=true" width="96" alt="Drywall Tile Skins"> | [DrywallTileSkins](src/DrywallTileSkins) | Drywall Tile Skins / 干板墙瓷砖皮肤 | Turns loaded tile textures into blueprint skins for vanilla Drywall. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794025758) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/DuplicantPortraits/packaging/preview.png?raw=true" width="96" alt="Duplicant Portraits"> | [DuplicantPortraits](src/DuplicantPortraits) | Duplicant Portraits / 复制人头像画 | Adds a masterpiece painting of every loaded duplicant personality to the vanilla 2x2 Blank Canvas. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794390677) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/ModListPreviews/packaging/preview.png?raw=true" width="96" alt="Mod List Previews"> | [ModListPreviews](src/ModListPreviews) | Mod List Previews / 模组封面预览 | Shows Steam Workshop cover thumbnails in the in-game Mods list. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794018361) |
<!-- /mods-table -->

## Commands

```text
npm run build
npm run build -- DrywallTileSkins
npm run desc
npm run index
npm run steam-ids
npm run package
```

| Script | What |
|--------|------|
| `build` | Compile every mod into `local/<Mod>/` |
| `build -- <Mod>` | Compile one mod |
| `desc` | Generate Workshop `Description.txt` and refresh the mods table |
| `index` | Rebuild the root README mods table from `src/*/README.md` |
| `steam-ids` | Fill Steam Workshop ids, then refresh the mods table |
| `package` | `build` + `desc` |

Fully quit the game after a DLL or PNG change.

## License

[MIT](LICENSE)
