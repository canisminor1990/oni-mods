using System.Text.Json;
using Steamworks;
using Steamworks.Ugc;

const int descriptionMax = 8000;

var root = Environment.GetEnvironmentVariable("ONI_MODS_ROOT");
var names = new List<string>();
for (var i = 0; i < args.Length; i++)
{
	if (args[i] == "--root" && i + 1 < args.Length)
	{
		root = args[++i];
		continue;
	}
	if (!args[i].StartsWith("-", StringComparison.Ordinal))
		names.Add(args[i]);
}

root = Path.GetFullPath(root ?? Directory.GetCurrentDirectory());
var pkgPath = Path.Combine(root, "package.json");
if (!File.Exists(pkgPath))
{
	Console.Error.WriteLine("Missing package.json. Pass --root <mods workspace>.");
	return 1;
}

var pkg = JsonDocument.Parse(File.ReadAllText(pkgPath));
var appId = pkg.RootElement.GetProperty("steam").GetProperty("appId").GetUInt32();
var oniMods = pkg.RootElement.GetProperty("oniMods");
var srcDir = Path.Combine(root, "src");
if (names.Count == 0)
{
	foreach (var folder in Directory.GetDirectories(srcDir))
	{
		var name = Path.GetFileName(folder);
		if (oniMods.TryGetProperty(name, out var meta) && meta.TryGetProperty("steamId", out _))
			names.Add(name);
	}
	names.Sort(StringComparer.Ordinal);
}

var jobs = new List<Job>();
foreach (var name in names)
{
	if (!oniMods.TryGetProperty(name, out var meta) || !meta.TryGetProperty("steamId", out var idEl))
	{
		Console.Error.WriteLine($"skip  {name}: no oniMods.steamId");
		continue;
	}
	if (!ulong.TryParse(idEl.GetString(), out var fileId) || fileId == 0)
	{
		Console.Error.WriteLine($"skip  {name}: bad steamId");
		continue;
	}

	var descPath = Path.Combine(srcDir, name, "Description.txt");
	if (!File.Exists(descPath))
	{
		Console.Error.WriteLine($"skip  {name}: missing Description.txt (run npm run desc)");
		continue;
	}

	var parts = SplitLocalized(File.ReadAllText(descPath));
	if (parts.English.Length == 0 || parts.Chinese.Length == 0)
	{
		Console.Error.WriteLine($"skip  {name}: Description.txt needs english + [hr] + chinese");
		continue;
	}
	if (parts.English.Length > descriptionMax || parts.Chinese.Length > descriptionMax)
	{
		Console.Error.WriteLine($"skip  {name}: a language is over {descriptionMax} characters");
		continue;
	}

	jobs.Add(new Job(name, fileId, parts.English, parts.Chinese));
}

if (jobs.Count == 0)
{
	Console.Error.WriteLine("No workshop descriptions to update.");
	return 1;
}

try
{
	SteamClient.Init(appId);
}
catch (Exception ex)
{
	Console.Error.WriteLine("SteamAPI init failed. Open Steam, log in as the workshop author, and quit Oxygen Not Included.");
	Console.Error.WriteLine(ex.Message);
	return 1;
}

try
{
	if (!SteamClient.IsValid)
	{
		Console.Error.WriteLine("Steam is not running or this account cannot use app " + appId + ".");
		return 1;
	}

	Console.WriteLine($"Steam {SteamClient.SteamId}  app {appId}");
	var failed = 0;
	foreach (var job in jobs)
	{
		if (!await PushLanguage(job, "english", job.English))
			failed += 1;
		if (!await PushLanguage(job, "schinese", job.Chinese))
			failed += 1;
	}
	return failed == 0 ? 0 : 1;
}
finally
{
	SteamClient.Shutdown();
}

static async Task<bool> PushLanguage(Job job, string language, string description)
{
	Console.Write($"  {job.Name} {language} … ");
	var result = await new Editor(job.FileId)
		.InLanguage(language)
		.WithDescription(description)
		.WithChangeLog($"Update {language} description")
		.SubmitAsync();

	if (result.Success)
	{
		Console.WriteLine("ok");
		return true;
	}

	Console.WriteLine(result.Result);
	return false;
}

static Localized SplitLocalized(string bbcode)
{
	var text = bbcode.Replace("\r\n", "\n").Trim();
	var split = text.Split(new[] { "[hr][/hr]" }, 2, StringSplitOptions.None);
	if (split.Length < 2)
		return new Localized("", "");
	return new Localized(split[0].Trim(), split[1].Trim());
}

internal readonly record struct Job(string Name, ulong FileId, string English, string Chinese);
internal readonly record struct Localized(string English, string Chinese);
