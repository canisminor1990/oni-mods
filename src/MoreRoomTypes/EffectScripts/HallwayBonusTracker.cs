using Klei.AI;

namespace MoreRoomTypes
{
	class HallwayBonusTracker : KMonoBehaviour, ISim200ms
	{
		public void Sim200ms(float dt)
		{
			Effects effects = GetComponent<Effects>();
			if (effects == null)
				return;
			bool inHallway = RoomTypes_AllModded.IsInTheRoom(this, RoomTypeHallwayData.RoomId);
			RoomConstraintTags.ToggleEffect(effects, RoomTypeHallwayData.EffectId, inHallway);
		}
	}
}
