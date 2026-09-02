# Agent notes (ONI mods)

This folder is a **Klei mods workspace**, not a Unity project. Playbook: `.cursor/skills/oni-modding/SKILL.md` and `reference.md`.

GitHub: https://github.com/canisminor1990/oni-mods (one repo, all mods). Steam author vanity: `canisminor` (ONI appid `457140`).

## Layout

| Role | Path |
|------|------|
| Source | `src/<ModName>/` |
| Game-loaded build | `local/<ModName>/` (csproj `OutputPath`, gitignored) |
| Scripts / commands | `package.json`, `scripts/` |
| Game API dumps | `ref/oni-api/` (`INDEX.md`) |
| Game | `E:\Steam\steamapps\common\OxygenNotIncluded` |
| Log | `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log` |

Do not put source under Klei `mods/Dev` (the game loads that folder). Do not copy DLLs into Steam.

One git repo at **this folder only**. Nested `src/<Mod>/.git` is forbidden. `.gitignore` is an allowlist: `/*` then `!src/` `!.cursor/` `!ref/oni-api/` `!scripts/` `!package.json`. New top-level names stay ignored until listed.

## Commands

| Script | What |
|--------|------|
| `npm run build` | `dotnet build -c Release` every `src/<Mod>` → `local/<Mod>/` |
| `npm run build -- <Mod>` | one mod |
| `npm run desc` | `README.md` → `Description.txt` (BBCode) |
| `npm run readme` | reverse (only if Description was edited on purpose) |
| `npm run steam-ids` | scrape workshop page → `oniMods.steamId` + README links |
| `npm run package` | build + desc |

User **fully quits** ONI to load DLL/PNG. Hot reload is not enough.

## Docs / Workshop

- Edit `src/<Mod>/README.md` only (Markdown). Do not hand-edit `Description.txt`.
- After README changes: `npm run desc`. Paste Description.txt into the Workshop item.
- English first, `---`, then Chinese. Compat_All.png in both languages.
- Related mods go under **Recommended** / **建议订阅**: optional, the mod runs alone. Do not tick Steam “Required items” unless it really cannot load without them.
- Avoid `[MPM]` inside BBCode/Markdown links (parsed as a tag). Write `(MPM)`.

## Steam IDs (this machine)

`IPublishedFileService/QueryFiles` needs a Web API key — do not use it. `npm run steam-ids` scrapes `https://steamcommunity.com/id/canisminor/myworkshopfiles/?appid=457140` and matches `workshopTitle` / `mod.yaml` title / spaced PascalCase.

Watt Toolkit (`Server: WattToolkit`) intercepts Steam HTTPS here: Node `fetch` TLS-fails or returns **HTTP 200 empty**. Scripts must use `curl.exe -k` (not the PowerShell `curl` alias). Do not add `browsefilter=mysubmissions` — that URL comes back empty. `GetPublishedFileDetails` (POST, no key) works for a **known** file id.

## Defaults

- `UserMod2` + `netstandard2.1` + `HarmonyLib` + `APIVersion: 2`
- Chinese UI; `GetCurrentLanguageCode()` is often `en` — still load `translations/zh.po`
- No new building / custom picker if vanilla Drywall blueprints suffice

## After code changes

Grep Player.log for `[ModName]`. Missing `mod_info.yaml` in `local/<Mod>/` means Klei skipped the mod. Look up types in `ref/oni-api/` before scanning `Assembly-CSharp.dll`.
