using Riftaria.Common.Drops;
using Riftaria.Common.Items;
using RiftTools;

string lootPath = FindFile("Common", "Drops", "loot-pools.js");
string affixPath = FindFile("Common", "Items", "affixes.js");
LootFile loot = LootTable.Parse(File.ReadAllText(lootPath));
AffixFile affixes = AffixTable.Parse(File.ReadAllText(affixPath));
Console.WriteLine(lootPath);
Console.WriteLine(affixPath);
Console.WriteLine();

DropReport report = DropChecks.Run(affixes, loot);
foreach (string line in report.Lines) {
	Console.WriteLine(line);
}

LayoutReport layouts = LayoutChecks.Run();
foreach (string line in layouts.Lines) {
	Console.WriteLine(line);
}

if (!report.Ok || !layouts.Ok) {
	foreach (string failure in report.Failures.Concat(layouts.Failures)) {
		Console.WriteLine("FAIL  " + failure);
	}

	return 1;
}

Console.WriteLine("ok");
return 0;

static string FindFile(params string[] parts)
{
	var dir = new DirectoryInfo(AppContext.BaseDirectory);
	while (dir != null) {
		string candidate = Path.Combine(new[] { dir.FullName, "Riftaria" }.Concat(parts).ToArray());
		if (File.Exists(candidate)) {
			return candidate;
		}

		dir = dir.Parent;
	}

	throw new FileNotFoundException(parts[^1]);
}
