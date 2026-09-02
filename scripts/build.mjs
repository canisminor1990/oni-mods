import { spawnSync } from "node:child_process";
import { existsSync, readdirSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const srcDir = join(root, "src");

function listMods() {
	return readdirSync(srcDir, { withFileTypes: true })
		.filter((entry) => entry.isDirectory())
		.map((entry) => entry.name)
		.filter((name) => existsSync(join(srcDir, name, `${name}.csproj`)));
}

const requested = process.argv.slice(2).filter((arg) => !arg.startsWith("-"));
const mods = requested.length > 0 ? requested : listMods();

if (mods.length === 0) {
	console.error("No mods to build.");
	process.exit(1);
}

for (const name of mods) {
	const project = join(srcDir, name, `${name}.csproj`);
	if (!existsSync(project)) {
		console.error(`Missing ${project}`);
		process.exit(1);
	}

	console.log(`\n== build ${name} ==`);
	const result = spawnSync("dotnet", ["build", project, "-c", "Release", "--nologo"], {
		cwd: root,
		stdio: "inherit",
		shell: false,
	});
	if (result.status !== 0) {
		process.exit(result.status ?? 1);
	}
}

console.log("\nOutput: local/<ModName>/  (fully quit ONI to load DLL/PNG)");
