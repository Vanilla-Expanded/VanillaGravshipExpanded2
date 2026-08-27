using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
	[DefOf]
	public static class InternalDefOf
	{
		static InternalDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
		}

		[MayRequireBiotech]
		public static GeneDef PerfectImmunity;

        public static HediffDef Burn;
        public static HediffDef VGE_ExowormInfestation;
        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static HediffDef VFEI2_NullipedeSpawn;

        public static ThingDef VGE_GiantWormspitter;
        public static ThingDef VGE_ExoHive_Building;
        public static ThingDef VGE_EggSac;
        public static ThingDef VGE_ExowormCocoon;
        public static ThingDef VGE_SpaceInfestationSpawner;
        public static ThingDef VGE_Projectile_InfestedChunkMedium;
        public static ThingDef VGE_Projectile_InfestedChunkLarge;
		public static ThingDef VGE_Filth_Astrofuel;
		public static ThingDef VGE_InfestedVent;
		[MayRequire("OskarPotocki.VFE.Insectoid2")]
		public static ThingDef VFEI2_PherocoreExo;
        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static ThingDef VFEI2_VGE_ArtificialExoHive;
        [MayRequire("OskarPotocki.VFE.Insectoid2")]
        public static ThingDef VFEI2_VGE_Subcreeper;

        public static PawnKindDef VGE_ExowormKnot;
        public static PawnKindDef VGE_Exopede;
        public static PawnKindDef VGE_Exoleech;
        public static PawnKindDef VGE_Exoworm;
        public static SoundDef VEG_InsectoidTurretTargetAcquired;

		public static TerrainDef VGE_GravshipSubarmor;
		public static TerrainDef VGE_EnemySubstructure;
		public static TerrainDef VGE_EnemySubarmor;
		public static TerrainDef VGE_Subcreep;

        public static DesignationCategoryDef VGE_Designer;
		[DefAlias("VGE_EmptySpace")]
		public static WorldObjectDef VGE_EmptySpaceObj;
		public static MapGeneratorDef VGE_EmptySpace;
		public static SoundDef OrbitalTargeter_Fire;
		public static StatDef VGE_GravshipTargeting;
		public static StatDef VGE_GravshipVisibilityFactor;
		public static StatDef VGE_GravshipLaunchVisibilityOffset;
		public static ThingDef VGE_GravshipArmor;
		public static SoundDef Hive_Spawn;
		public static ThingDef VGE_MechanoidGravTether;
		public static ThingDef VGE_EnemyGravjumperEngine;
		public static ThingDef VGE_EnemyGravEngine;
		public static ThingDef VGE_EnemyGravhulkEngine;
		public static ThingDef VGE_EnemyGravlockTether;
		public static ThingDef VGE_Warcomputer;
		public static ThingDef VGE_GaussGun;
		public static ThingDef VGE_GaussHowitzer;
		public static ThingDef VGE_JavelinPod;
		public static ThingDef VGE_JavelinLauncher;
		public static ThingDef VGE_AnticraftCaster;
		public static ThingDef VGE_EnemyPilotCockpit;
		public static ThingDef VGE_AnticraftEmitter;
		public static ThingDef VGE_AncientGravmine;
		public static ThingDef VGE_WarpodLeaving;
		public static ThingDef VGE_WarpodIncoming;
		public static WorldObjectDef VGE_TravellingWarpod;
		public static ThingDef VGE_GravshipBlackBox;
		public static ThingDef VGE_EnemySignalJammer, VGE_MechanoidSignalJammer, VGE_AncientSignalJammer, SignalJammer;
		public static ThingDef VGE_EnemyAnticraftBeamStrike;
		public static JobDef VGE_OperateEnemyTerminal;
		public static ThingDef VGE_Mote_AnticraftBeam;
		public static JobDef VGE_FormKnot;
		public static JobDef VGE_EscapePod_Enter;
		public static JobDef VGE_EscapePod_InsertPawn;
		public static JobDef VGE_EscapePod_InsertPawnDrafted;
		public static JobDef VGE_CallSalvagerStation;
		public static JobDef VGE_SpacePrisonEscape_UseEscapePod;
		public static DutyDef VGE_SpacePrisonerEscape;
		public static FleckDef BlastEMP;
		public static ThingDef OrbitalTargeterBombardment;
		public static ThingDef VGE_Compressed_Vacstone;
		public static ThingDef ChunkVacstone;
		public static OrbitalDebrisDef VGE_GravshipDebris;
		public static SitePartDef VGE_GravshipGraveyard;
		public static SitePartDef VGE_InfestedInstallation;
		public static SitePartDef VGE_SalvagerStronghold;
		public static HistoryAutoRecorderDef VGE_GravshipVisibilityRecorder;
		public static ThingDef VGE_SalvagerDropship;
		public static ThingDef VGE_SalvagerDropshipBombardment;
		public static TransportShipDef VGE_Ship_SalvagerDropship;
		public static ThingDef VGE_Turret_DefenderTurret;
		public static GravshipThreatDef VGE_SalvagerStation;
		public static RulePackDef VGE_NamerPirateOrbitalStation;
		public static ThingDef VGE_GravjumperEngine;
		public static ThingDef VGE_GravhulkEngine;
		public static PawnsArrivalModeDef VGE_SalvagerDropshipRaid;
		public static ThingDef Proj_Rocket;
		public static ThingDef VGE_Apparel_DisposableOxygenPack;
		public static ThingDef VGE_Apparel_Astrorig;
		public static ThingDef VGE_OxygenCanister;
		public static TerrainAffordanceDef VGE_SubstructureAndOrbitalPlatform;
		public static ThingDef VGE_LandingStructure_EnemyGravjumper;
		public static ThingDef VGE_LandingStructure_EnemyGravjumperCaptured;
		public static ThingDef VGE_LandingStructure_StructureSet;
		public static GravshipThreatDef VGE_EnemyGravjumper;
		public static RulePackDef VGE_NamerEnemyGravship;
		public static VEF.Storyteller.StructureSetDef VGE_EnemyGravjumperSet;
		public static VEF.Storyteller.StructureSetDef VGE_EnemyGravjumperSet_Complete;
		public static RaidStrategyDef Siege;
		public static RaidStrategyDef ImmediateAttackSappers;
		public static RaidStrategyDef ImmediateAttackSmart;
		public static VEF.Storyteller.StructureSetDef VGE_EnemyGravshipSet;
		public static ThingDef VGE_LandingStructure_EnemyGravship;
        public static ThingDef VGE_Projectile_GravJunk;
        public static ThingDef VGE_GravJunk;
        public static DamageDef BombSuper;
        public static HediffDef Shredded;
        public static HediffDef Crack;
        public static ThingDef VGE_LandingStructure_EnemyGravshipRaid;
        public static ThingDef AncientMiningCharge;
        public static ThingDef AncientDrillPlatform;
    }
}
