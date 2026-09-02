# Agent notes (ONI mods)

This folder is a **Klei mods workspace**, not a Unity project. Full playbook: `.cursor/skills/oni-modding/SKILL.md` and `reference.md`.

## Layout

| Role | Path |
|------|------|
| Source | `src/<ModName>/` |
| Game-loaded build | `local/<ModName>/` (csproj `OutputPath`) |
| Game API dumps | `ref/oni-api/` (see `INDEX.md`) |
| Game | `E:\Steam\steamapps\common\OxygenNotIncluded` |
| Log | `%USERPROFILE%\AppData\LocalLow\Klei\Oxygen Not Included\Player.log` |

Do not put source under Klei `mods/Dev` (the game loads that folder). Do not copy DLLs into Steam.

One git repo at this folder only. Nested `src/<Mod>/.git` is forbidden. `.gitignore` is an allowlist (`/*` then `!src/` `!.cursor/` `!ref/oni-api/`).

## Defaults

- `UserMod2` + `netstandard2.1` + `HarmonyLib` + `APIVersion: 2`
- `dotnet build -c Release` in the `src` project; user **fully quits** ONI to load DLL/PNG
- Chinese UI; `GetCurrentLanguageCode()` is often `en` — still load `translations/zh.po`
- No new building / custom picker if vanilla Drywall blueprints suffice
- One GitHub repo for all mods (`src/<ModName>/`), not one repo per mod
- GitHub `README.md` is Markdown; Workshop `Description.txt` is BBCode (English first, then Chinese, Compat_All.png)

## After code changes

Grep Player.log for `[ModName]`. Missing `mod_info.yaml` in `local/<Mod>/` means Klei skipped the mod.

Look up game types in `ref/oni-api/` before scanning `Assembly-CSharp.dll`. Add a new excerpt there when you decompile something useful.
