// Compost is ElementConverter + Operational SM (not ComplexFabricator).
// Output: SimHashes.Dirt. Flip chore then composting while operational.

public class CompostConfig : IBuildingConfig
{
	public const string ID = "Compost";
	public static readonly Tag COMPOST_TAG = GameTags.Compostable;
	private const SimHashes OUTPUT_ELEMENT = SimHashes.Dirt;
}

public class Compost : StateMachineComponent<Compost.StatesInstance>
{
	[MyCmpGet]
	private Operational operational;
}
