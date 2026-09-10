// Power Control Station and Farm Station use TinkerStation (not ComplexFabricator).
// UpdateChore: operational && (ToolsRequested() || alwaysTinker) && HasMaterial.
// ToolsRequested: a tinkerable needs this tool AND world inventory of outputPrefab is 0.
// alwaysTinker is not serialized; set it each tick for Maintain / Forever.

public class TinkerStation : Workable, ISim1000ms
{
	public bool alwaysTinker;
	public Tag outputPrefab;
	public float massPerTinker;
}
