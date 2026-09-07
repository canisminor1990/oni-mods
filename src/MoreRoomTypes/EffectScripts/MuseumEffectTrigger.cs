using UnityEngine;
using Klei.AI;

namespace MoreRoomTypes
{
	class MuseumEffectTrigger : KMonoBehaviour
	{
		static readonly EventSystem.IntraObjectHandler<MuseumEffectTrigger> TriggerRoomEffectsDelegate =
			new EventSystem.IntraObjectHandler<MuseumEffectTrigger>((component, data) => component.TriggerRoomEffects(data));

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			Subscribe<MuseumEffectTrigger>(-832141045, TriggerRoomEffectsDelegate);
		}

		void TriggerRoomEffects(object data)
		{
			bool isMuseum = RoomTypes_AllModded.IsInTheRoom(this, RoomTypeMuseumData.RoomId);
			bool isSpace = RoomTypes_AllModded.IsInTheRoom(this, RoomTypeMuseumSpaceData.RoomId);
			if (!isMuseum && !isSpace)
				return;

			GameObject gameObject = (GameObject)data;
			MinionModifiers modifiers = gameObject.GetComponent<MinionModifiers>();
			if (modifiers == null)
				return;

			Effect effect;
			if (isSpace)
			{
				Room room = Game.Instance.roomProber.GetRoomOfGameObject(this.gameObject);
				int uniqueArtifacts = RoomsExpanded_Patches_MuseumSpace.CountUniqueArtifacts(room);
				effect = RoomsExpanded_Patches_MuseumSpace.CalculateEffectBonus(modifiers, uniqueArtifacts);
			}
			else
				effect = RoomsExpanded_Patches_Museum.CalculateEffectBonus(modifiers);

			if (effect == null)
			{
				Debug.Log($"{Mod.Namespace}: Error - could not create effect for {(isSpace ? "Space " : "")}Museum");
				return;
			}

			Effects effects = gameObject.GetComponent<Effects>();
			if (effects != null && !effects.HasEffect(effect.Id))
				effects.Add(effect, true);
		}
	}
}
