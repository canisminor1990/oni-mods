using UnityEngine;

namespace MoreRoomTypes
{
	class EquipmentEfficiencyTracker : KMonoBehaviour, ISim1000ms
	{
		public const float Bonus = 0.1f;

		float originalPumpRate = -1f;

		public static void Attach(GameObject go)
		{
			if (go != null)
				go.AddOrGet<EquipmentEfficiencyTracker>();
		}

		public static bool ShouldBoost(Component component)
		{
			if (component == null)
				return false;

			bool oxygen = RoomConstraintTags.HasBuildingTag(component, RoomConstraintTags.OxygenProducerTag)
				|| RoomConstraintTags.HasBuildingTag(component, RoomConstraintTags.GasPumpBuildingTag);
			bool waste = RoomConstraintTags.HasBuildingTag(component, RoomConstraintTags.CompostBuildingTag)
				|| RoomConstraintTags.HasBuildingTag(component, RoomConstraintTags.WasteProcessorTag);
			bool water = RoomConstraintTags.HasBuildingTag(component, RoomConstraintTags.WaterTreatmentTag);
			if (!oxygen && !waste && !water)
				return false;

			if (Game.Instance == null || Game.Instance.roomProber == null)
				return false;

			CavityInfo info = Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(component.gameObject));
			if (info == null || info.room == null || info.room.roomType == null)
				return false;

			string roomId = info.room.roomType.Id;
			if (oxygen && roomId == RoomTypeOxygenRoomData.RoomId)
				return true;
			if (waste && roomId == RoomTypeWasteRoomData.RoomId)
				return true;
			if (water && roomId == RoomTypeWaterRoomData.RoomId)
				return true;
			return false;
		}

		public void Sim1000ms(float dt)
		{
			bool boost = ShouldBoost(this);
			float speed = boost ? 1f + Bonus : 1f;

			ElementConverter converter = GetComponent<ElementConverter>();
			if (converter != null)
				converter.SetWorkSpeedMultiplier(speed);

			UpdatePumpRate(boost);
		}

		void UpdatePumpRate(bool boost)
		{
			if (!RoomConstraintTags.HasBuildingTag(this, RoomConstraintTags.GasPumpBuildingTag))
				return;

			ElementConsumer consumer = GetComponent<ElementConsumer>();
			if (consumer == null)
				return;

			if (originalPumpRate < 0f)
				originalPumpRate = consumer.consumptionRate;

			float target = boost ? originalPumpRate * (1f + Bonus) : originalPumpRate;
			if (!Mathf.Approximately(consumer.consumptionRate, target))
			{
				consumer.consumptionRate = target;
				consumer.RefreshConsumptionRate();
			}
		}
	}
}
