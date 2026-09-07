# ONI Mods / 缺氧模组

[Mods made by CanisMinor](https://steamcommunity.com/workshop/filedetails/?id=3794379693)

## Mods

<!-- mods-table -->
| Cover | Folder | Title | Description | Steam |
|-------|--------|-------|-------------|-------|
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/CustomChineseFonts/packaging/preview.png?raw=true" width="96" alt="Custom Chinese Fonts"> | [CustomChineseFonts](src/CustomChineseFonts) | Custom Chinese Fonts / 自定义中文字体 | Replaces the Simplified Chinese UI fonts. Body text defaults to HarmonyOS Sans SC; titles can be changed in Settings. You can also add your own TTF/OTF. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794395157) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/DrywallTileSkins/packaging/preview.png?raw=true" width="96" alt="Drywall Tile Skins"> | [DrywallTileSkins](src/DrywallTileSkins) | Drywall Tile Skins / 干板墙瓷砖皮肤 | Turns loaded tile textures into blueprint skins for vanilla Drywall. Only the interior cell is repeated, without tile borders. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794025758) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/DuplicantPortraits/packaging/preview.png?raw=true" width="96" alt="Duplicant Portraits"> | [DuplicantPortraits](src/DuplicantPortraits) | Duplicant Portraits / 复制人头像画 | Adds a masterpiece portrait of every duplicant personality to the vanilla 2x2 Blank Canvas. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794390677) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/ModListPreviews/packaging/preview.png?raw=true" width="96" alt="Mod List Previews"> | [ModListPreviews](src/ModListPreviews) | Mod List Previews / 模组封面预览 | Shows Steam Workshop cover thumbnails in the in-game Mods list. Local and Dev mods use `preview.png` / `preview.jpg` if present. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3794018361) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/MoreRoomTypes/packaging/preview.png?raw=true" width="96" alt="More Room Types"> | [MoreRoomTypes](src/MoreRoomTypes) | More Room Types / 更多房间 | Adds extra colony room types. This standalone mod is built with thanks on pether.pg's [Rooms Expanded](https://steamcommunity.com/sharedfiles/filedetails/?id=2348079481); you do not need to subscribe to the original. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3797339501) |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/OnDemandProduction/packaging/preview.png?raw=true" width="96" alt="On-Demand Production"> | [OnDemandProduction](src/OnDemandProduction) | On-Demand Production / 按需生产 | Adds a third production mode to every production building. Click the Forever button to cycle Once, Maintain, and Forever. Maintain keeps a product in stock using this world's Resources list. |  |
| <img src="https://github.com/canisminor1990/oni-mods/blob/master/src/ThermalInfoCards/packaging/preview.png?raw=true" width="96" alt="Thermal Info Cards"> | [ThermalInfoCards](src/ThermalInfoCards) | Thermal Info Cards / 热量信息卡 | Shows thermal conductivity, thermal mass, heat energy, and phase-change temperatures on hover and in the build menu. Hover cards can stack matching objects and sum thermal mass and heat energy. Peter Han's [Thermal Tooltips](https://steamcommunity.com/sharedfiles/filedetails/?id=1983504552) and Aze's [Better Info Cards](https://steamcommunity.com/sharedfiles/filedetails/?id=1960947963) conflict when both are enabled; this remake exists to combine both features so you do not need either original or PLib. | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3797325396) |
<!-- /mods-table -->

## Commands

```text
npm run build
npm run build -- DrywallTileSkins
npm run desc
npm run index
npm run workshop-desc
npm run steam-ids
npm run package
```

| Script | What |
|--------|------|
| `build` | Compile every mod into `local/<Mod>/` |
| `build -- <Mod>` | Compile one mod |
| `desc` | Generate Workshop `Description.txt` and refresh the mods table |
| `index` | Rebuild the root README mods table from `src/*/README.md` |
| `workshop-desc` | Push `Description.txt` to Steam `english` + `schinese` (Steam client must be logged in; no pack upload) |
| `steam-ids` | Fill Steam Workshop ids, then refresh the mods table |
| `package` | `build` + `desc` |

Fully quit the game after a DLL or PNG change.

## License

[MIT](LICENSE)
