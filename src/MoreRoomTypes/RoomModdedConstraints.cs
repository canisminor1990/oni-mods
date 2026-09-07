using System.Collections.Generic;
using Database;

namespace MoreRoomTypes
{
	class RoomModdedConstraints
	{
		static readonly int requiredMasterpieces = 6;
		static readonly int requiredArtifacts = 6;

		public static RoomConstraints.Constraint GRAVESTONE = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.GravestoneTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.GRAVE.NAME,
			description: STRINGS.ROOMS.CRITERIA.GRAVE.DESCRIPTION);

		public static RoomConstraints.Constraint INDUSTRIAL = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraints.ConstraintTags.IndustrialMachinery),
			null,
			name: STRINGS.ROOMS.CRITERIA.INDUSTRIAL.NAME,
			description: STRINGS.ROOMS.CRITERIA.INDUSTRIAL.DESCRIPTION);

		public static RoomConstraints.Constraint RUNNING_WHEEL = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.RunningWheelGeneratorTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.MANUALGENERATOR.NAME,
			description: STRINGS.ROOMS.CRITERIA.MANUALGENERATOR.DESCRIPTION,
			stomp_in_conflict: new List<RoomConstraints.Constraint> { RoomConstraints.REC_BUILDING });

		public static RoomConstraints.Constraint WATER_COOLER = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.WaterCoolerTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.WATERCOOLER.NAME,
			description: STRINGS.ROOMS.CRITERIA.WATERCOOLER.DESCRIPTION);

		public static RoomConstraints.Constraint PEDESTAL = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.ItemPedestalTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.PEDESTAL.NAME,
			description: STRINGS.ROOMS.CRITERIA.PEDESTAL.DESCRIPTION);

		public static RoomConstraints.Constraint MASTERPIECES = new RoomConstraints.Constraint(
			null,
			room =>
			{
				string great = Db.Get().ArtableStatuses.LookingGreat.Id;
				ArtableStages stages = Db.GetArtableStages();
				int count = 0;
				if (room != null)
				{
					foreach (KPrefabID building in room.buildings)
					{
						if (building == null)
							continue;
						Artable art = building.GetComponent<Artable>();
						if (art == null)
							continue;
						ArtableStage artableStage = stages.TryGet(art.CurrentStage);
						if (artableStage != null && artableStage.statusItem.Id == great)
							count++;
					}
				}
				return count >= requiredMasterpieces;
			},
			name: string.Format(STRINGS.ROOMS.CRITERIA.MASTERPIECES.NAME, requiredMasterpieces),
			description: string.Format(STRINGS.ROOMS.CRITERIA.MASTERPIECES.DESCRIPTION, requiredMasterpieces));

		public static RoomConstraints.Constraint ARTIFACTS = new RoomConstraints.Constraint(
			null,
			room => RoomsExpanded_Patches_MuseumSpace.CountUniqueArtifacts(room) >= requiredArtifacts,
			name: string.Format(STRINGS.ROOMS.CRITERIA.ARTIIFACTS.NAME, requiredArtifacts),
			description: string.Format(STRINGS.ROOMS.CRITERIA.ARTIIFACTS.DESCRIPTION, requiredArtifacts));

		public static RoomConstraints.Constraint FLUSH_TOILET_EXACTLY_ONE = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraints.ConstraintTags.FlushToiletType),
			room => RoomConstraintTags.CountBuildings(room, RoomConstraints.ConstraintTags.FlushToiletType) == 1,
			name: STRINGS.ROOMS.CRITERIA.FLUSHTOILET_ONE.NAME,
			description: STRINGS.ROOMS.CRITERIA.FLUSHTOILET_ONE.DESCRIPTION);

		public static RoomConstraints.Constraint SINK_EXACTLY_ONE = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.SinkBuildingTag),
			room => RoomConstraintTags.CountBuildings(room, RoomConstraintTags.SinkBuildingTag) == 1,
			name: STRINGS.ROOMS.CRITERIA.SINK_ONE.NAME,
			description: STRINGS.ROOMS.CRITERIA.SINK_ONE.DESCRIPTION);

		public static RoomConstraints.Constraint SHOWER_EXACTLY_ONE = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.ShowerBuildingTag),
			room => RoomConstraintTags.CountBuildings(room, RoomConstraintTags.ShowerBuildingTag) == 1,
			name: STRINGS.ROOMS.CRITERIA.SHOWER_ONE.NAME,
			description: STRINGS.ROOMS.CRITERIA.SHOWER_ONE.DESCRIPTION);

		public static RoomConstraints.Constraint STORAGE_BUILDINGS = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.StorageBuildingTag),
			room => RoomConstraintTags.CountBuildings(room, RoomConstraintTags.StorageBuildingTag) >= 4,
			name: STRINGS.ROOMS.CRITERIA.STORAGE_BUILDINGS.NAME,
			description: STRINGS.ROOMS.CRITERIA.STORAGE_BUILDINGS.DESCRIPTION);

		public static RoomConstraints.Constraint BATTERIES = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.BatteryBuildingTag),
			room => RoomConstraintTags.CountBuildings(room, RoomConstraintTags.BatteryBuildingTag) >= 8,
			name: STRINGS.ROOMS.CRITERIA.BATTERIES.NAME,
			description: STRINGS.ROOMS.CRITERIA.BATTERIES.DESCRIPTION);

		public static RoomConstraints.Constraint NUCLEAR_REACTOR = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.NuclearReactorTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.NUCLEAR_REACTOR.NAME,
			description: STRINGS.ROOMS.CRITERIA.NUCLEAR_REACTOR.DESCRIPTION);

		public static RoomConstraints.Constraint STEAM_TURBINE = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.SteamTurbineTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.STEAM_TURBINE.NAME,
			description: STRINGS.ROOMS.CRITERIA.STEAM_TURBINE.DESCRIPTION);

		public static RoomConstraints.Constraint OXYGEN_PRODUCER = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.OxygenProducerTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.OXYGEN_PRODUCER.NAME,
			description: STRINGS.ROOMS.CRITERIA.OXYGEN_PRODUCER.DESCRIPTION);

		public static RoomConstraints.Constraint GAS_PUMP = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.GasPumpBuildingTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.GAS_PUMP.NAME,
			description: STRINGS.ROOMS.CRITERIA.GAS_PUMP.DESCRIPTION);

		public static RoomConstraints.Constraint COMPOST = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.CompostBuildingTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.COMPOST.NAME,
			description: STRINGS.ROOMS.CRITERIA.COMPOST.DESCRIPTION);

		public static RoomConstraints.Constraint WASTE_PROCESSOR = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.WasteProcessorTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.WASTE_PROCESSOR.NAME,
			description: STRINGS.ROOMS.CRITERIA.WASTE_PROCESSOR.DESCRIPTION);

		public static RoomConstraints.Constraint WATER_TREATMENT = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.WaterTreatmentTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.WATER_TREATMENT.NAME,
			description: STRINGS.ROOMS.CRITERIA.WATER_TREATMENT.DESCRIPTION);

		public static RoomConstraints.Constraint LADDER = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.LadderBuildingTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.LADDER.NAME,
			description: STRINGS.ROOMS.CRITERIA.LADDER.DESCRIPTION);

		public static RoomConstraints.Constraint FIRE_POLE = new RoomConstraints.Constraint(
			bc => bc.HasTag(RoomConstraintTags.FirePoleBuildingTag),
			null,
			name: STRINGS.ROOMS.CRITERIA.FIRE_POLE.NAME,
			description: STRINGS.ROOMS.CRITERIA.FIRE_POLE.DESCRIPTION);
	}
}
