# Game API excerpts

Curated decompile snippets from this install’s `Assembly-CSharp.dll`. **Look here first** before scanning the DLL or `%TEMP%\oni-dump`.

DLL: `E:\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed\Assembly-CSharp.dll`

When a type is missing or stale (game update), add/replace a file in this folder and update the table. Do not dump the whole assembly.

```text
python ref/oni-api/scan_dll.py BuildingFacades
```

## Index

| File | Types / notes |
|------|----------------|
| `kanim.cs` | `KAnimFile`, `KAnimFile.Mod` |
| `painting_art_b.txt` | 2x2 canvas layers: hollow `frame` over `canvas` |
| `kanimdata.cs` | `KAnimFileData`, `KAnim.Build.Symbol` |
| `kanimsymbol.cs` | `KAnim.Build.Symbol`, `SymbolFrameInstance` |
| `parser.cs` | `KAnimGroupFile`, `KGlobalAnimParser` |
| `batch.cs` | `KAnimBatchManager` |
| `atlas.cs` | `AnimCommandFile`, `TextureAtlas` |
| `buildingfacades.cs` | `Database.BuildingFacades` |
| `buildingfacaderesource.cs` | `Database.BuildingFacadeResource` |
| `facade.cs` | `BuildingFacadeResource`, `AnimTileable` |
| `permitresource.cs` | `Database.PermitResource` |
| `permitresources.cs` | `Database.PermitResources` |
| `permit.cs` | `PermitRarity`, `FacadeSelectionPanel` |
| `permitpres.cs` | `PermitPresentationInfo`, `Def` |
| `inventory.cs` | `InventoryOrganization` |
| `db.cs` | `Db` |
| `resourceset.cs` / `resourcesetgeneric.cs` | `ResourceSet` |
| `blocktilerenderer.cs` | `Rendering.BlockTileRenderer` |
| `more.cs` | `BuildingFacade`, `BuildingDef` (large), extra `BuildingFacades` / `KAnimGroupFile` |
| `steamugc.cs` | `SteamUGCService` (`AddClient` notifies the new client with all ids as added) |
| `kmodsteam.cs` | `KMod.Steam.UpdateMods` → `Subscribe` / `Sanitize` / `Report` |
| `kmodmanager.cs` | `KMod.Manager.Sanitize` unsubscribes `!is_subscribed` |
| `kmodmod.cs` | `KMod.Mod.is_subscribed` is not persisted |
| `modsscreen.cs` | `ModsScreen.OnActivate` calls `Sanitize` then `BuildDisplay` |

`MISSING …` headers mean that dump pass did not find the type; search another file or rescan the DLL.
