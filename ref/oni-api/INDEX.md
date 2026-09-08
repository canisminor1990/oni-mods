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
| `roomtype.cs` | `RoomType` (`effect`, `effects`, `GetRoomEffectsString`) |
| `roomdetails.cs` | `RoomDetails.EFFECT` / `EFFECTS` overlay hover |
| `roomtypes.cs` / `Database.RoomTypes.decompiled.cs` | vanilla room registration |
| `codex_rooms.cs` | `CodexEntryGenerator.GenerateRoomsEntries` + room details (effects from `GetRoomEffectsString` only) |
| `ComplexFabricator.decompiled.cs` | recipe queue, `QUEUE_INFINITE`, `SetRecipeQueueCount` |
| `ComplexFabricatorSideScreen.decompiled.cs` | fabricator recipe list / queue labels |
| `SelectedRecipeQueueScreen.decompiled.cs` | +1 / count / Forever controls |
| `WorldInventory.decompiled.cs` | `GetAmount` / `GetTotalAmount` (resource overlay totals) |
| `ComplexRecipe.decompiled.cs` | ingredients / results |
| `AllResourcesScreen.decompiled.cs` | mass vs kcal vs quantity formatting |
| `KNumberInputField.decompiled.cs` / `KInputField.decompiled.cs` | queue count field |
| `loading.cs` | startup path: `Global.Awake`, `KMod.Content`, `Assets.OnPrefabInit` |
| `LaunchInitializer.decompiled.cs` | first scene; instantiates `SpawnPrefabs` (Global / Assets) |
| `LoadingOverlay.decompiled.cs` | save-load modal (`UI.FRONTEND.LOADING`), not the Klei splash |
| `SceneInitializer.decompiled.cs` / `SceneInitializerLoader.decompiled.cs` | frontend prefab instantiate |

`MISSING …` headers mean that dump pass did not find the type; search another file or rescan the DLL.
