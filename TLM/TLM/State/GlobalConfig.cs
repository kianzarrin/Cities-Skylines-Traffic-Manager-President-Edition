﻿#define RUSHHOUR

using ColossalFramework;
using CSUtil.Commons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TrafficManager.Manager;
using TrafficManager.Traffic;
using TrafficManager.TrafficLight;

namespace TrafficManager.State {
	[XmlRootAttribute("GlobalConfig", Namespace = "http://www.viathinksoft.de/tmpe", IsNullable = false)]
	public class GlobalConfig {
		public const string FILENAME = "TMPE_GlobalConfig.xml";
		public const string BACKUP_FILENAME = FILENAME + ".bak";
		private static int LATEST_VERSION = 11;
#if DEBUG
		private static uint lastModificationCheckFrame = 0;
#endif

		public static int? RushHourParkingSearchRadius { get; private set; } = null;

#if RUSHHOUR
		private static DateTime? rushHourConfigModifiedTime = null;
		private const string RUSHHOUR_CONFIG_FILENAME = "RushHourOptions.xml";
#endif

		public static GlobalConfig Instance { get; private set; } = null;

		static GlobalConfig() {
			Reload();
		}

		internal static void OnLevelUnloading() {
#if RUSHHOUR
			rushHourConfigModifiedTime = null;
			RushHourParkingSearchRadius = null;
#endif
		}

		//public static GlobalConfig Instance() {
		//#if DEBUG
		//			uint curDebugFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex >> 10;
		//			if (lastModificationCheckFrame == 0) {
		//				lastModificationCheckFrame = curDebugFrame;
		//			} else if (lastModificationCheckFrame < curDebugFrame) {
		//				lastModificationCheckFrame = curDebugFrame;
		//				ReloadIfNewer();
		//			}
		//#endif
		//return Instance;
		//}

		private static DateTime ModifiedTime = DateTime.MinValue;

		/// <summary>
		/// Configuration version
		/// </summary>
		public int Version = LATEST_VERSION;

		public bool[] DebugSwitches = {
			false, // 0: path-find debug log
			false, // 1: path-find costs debug log
			false, // 2: parking ai debug log (basic)
			false, // 3: do not actually repair stuck vehicles/cims, just report
			false, // 4: parking ai debug log (extended)
			false, // 5: geometry debug log
			false, // 6: debug parking AI distance issue
			false, // 7: debug TTL
			false, // 8: debug routing
			false, // 9: debug vehicle to segment end linking
			false, // 10: prevent routing recalculation on global configuration reload
			false, // 11: disable custom routing
			false, // 12: pedestrian path-find debug log
			false, // 13: priority rules debug
			false, // 14: disable GUI overlay of citizens having a valid path
			false, // 15: disable checking of other vehicles for trams
			false // 16: debug TramBaseAI.SimulationStep (2)
		};

#if DEBUG
		public int PathFindDebugNodeId = 0;
		public int PathFindDebugStartSegmentId = 0;
		public int PathFindDebugEndSegmentId = 0;
		public int PathFindDebugVehicleId = 0;
		public ExtVehicleType PathFindDebugExtVehicleType = ExtVehicleType.None;
		public ushort TTLDebugNodeId = 0;
#endif

		/// <summary>
		/// Language to use (if null then the game's language is being used)
		/// </summary>
		public string LanguageCode = null;

		/// <summary>
		/// TTL wait/flow calculation mode
		/// </summary>
		public FlowWaitCalcMode TTLFlowWaitCalcMode = FlowWaitCalcMode.Mean;

		/// <summary>
		/// Default TTL flow-to-wait ratio
		/// </summary>
		public float DefaultTTLFlowToWaitRatio = 0.8f;

		/// <summary>
		/// TTL smoothing factor for flowing/waiting vehicles
		/// </summary>
		public float TTLSmoothingFactor = 0.1f;

		/// <summary>
		/// base lane changing cost factor on highways
		/// </summary>
		public float HighwayLaneChangingBaseCost = 1.1f;

		/// <summary>
		/// base lane changing cost factor on city streets
		/// </summary>
		public float CityRoadLaneChangingBaseCost = 1.1f;

		/// <summary>
		/// congestion lane changing base cost
		/// </summary>
		public float CongestionLaneChangingCostFactor = 1f;

		/// <summary>
		/// heavy vehicle lane changing cost factor
		/// </summary>
		public float HeavyVehicleLaneChangingCostFactor = 1.5f;

		/// <summary>
		/// > 1 lane changing cost factor
		/// </summary>
		public float MoreThanOneLaneChangingCostFactor = 2f;

		/// <summary>
		/// Lane changing base costs in front of highway junctions
		/// </summary>
		public float HighwayInterchangeLaneChangingBaseCost = 1.25f;

		/// <summary>
		/// Used for discretizing path-find usage values
		/// </summary>
		public float LaneUsageDiscretization = 25f;

		/// <summary>
		/// Used for discretizing lane traffic measurements
		/// </summary>
		public float LaneDensityDiscretization = 25f;

		/// <summary>
		/// Relative factor for lane speed cost calculation
		/// </summary>
		public float UsageCostFactor = 1f;

		/// <summary>
		/// Relative factor for lane traffic cost calculation
		/// </summary>
		public float TrafficCostFactor = 2f;

		/// <summary>
		/// lane changing cost reduction modulo. vehicles hitting zero want to change lanes
		/// </summary>
		public uint RandomizedLaneChangingModulo = 5;

		/// <summary>
		/// artifical lane distance for u-turns
		/// </summary>
		public int UturnLaneDistance = 2;

		/// <summary>
		/// artifical lane distance for vehicles that change to lanes which have an incompatible lane arrow configuration
		/// </summary>
		public byte IncompatibleLaneDistance = 1;

		/// <summary>
		/// lane density random interval
		/// </summary>
		public float LaneDensityRandInterval = 50f;

		/// <summary>
		/// lane usage random interval
		/// </summary>
		public float LaneUsageRandInterval = 25f;

		/// <summary>
		/// lane usage random interval
		/// </summary>
		public uint MinHighwayInterchangeSegments = 3;

		/// <summary>
		/// lane usage random interval
		/// </summary>
		public uint MaxHighwayInterchangeSegments = 15;

		/// <summary>
		/// Threshold for reducing traffic buffer
		/// </summary>
		public uint MaxTrafficBuffer = 500;

		/// <summary>
		/// Threshold for reducing path-find traffic buffer
		/// </summary>
		public uint MaxPathFindTrafficBuffer = 5000;

		/// <summary>
		/// Threshold for restart segment direction congestion measurements
		/// </summary>
		public byte MaxNumCongestionMeasurements = 100;

		/// <summary>
		/// penalty for busses not driving on bus lanes
		/// </summary>
		public float PublicTransportLanePenalty = 10f;

		/// <summary>
		/// reward for public transport staying on transport lane
		/// </summary>
		public float PublicTransportLaneReward = 0.1f;

		/// <summary>
		/// maximum penalty for heavy vehicles driving on an inner lane (in %)
		/// </summary>
		public float HeavyVehicleMaxInnerLanePenalty = 1f;

		/// <summary>
		/// Path cost multiplier for vehicle restrictions
		/// </summary>
		public float VehicleRestrictionsPenalty = 100f;

		/// <summary>
		/// Maximum walking distance
		/// </summary>
		public float MaxWalkingDistance = 1000f;

		/// <summary>
		/// Target position randomization to allow opposite road-side parking
		/// </summary>
		public uint ParkingSpacePositionRand = 32;

		/// <summary>
		/// parking space search in vicinity is randomized. Cims do not always select the nearest parking space possible.
		/// A value of 1u always selects the nearest parking space.
		/// A value of 2u selects the nearest parking space with 50% chance, the next one with 25%, then 12.5% and so on.
		/// A value of 4u selects the nearest parking space with 75% chance, the next one with 18.75%, then 4.6875% and so on.
		/// A value of N selects the nearest parking space with (N-1)/N chance, the next one with (1-(N-1)/N)*(N-1)/N, then (1-(N-1)/N)^2*(N-1)/N and so on.
		/// </summary>
		public uint VicinityParkingSpaceSelectionRand = 4u;

		/// <summary>
		/// maximum number of parking attempts for passenger cars
		/// </summary>
		public int MaxParkingAttempts = 10;

		/// <summary>
		/// maximum required squared distance between citizen instance and parked vehicle before the parked car is turned into a vehicle
		/// </summary>
		public float MaxParkedCarInstanceSwitchSqrDistance = 6f;

		/// <summary>
		/// maximum distance between building and pedestrian lane
		/// </summary>
		public float MaxBuildingToPedestrianLaneDistance = 96f;

		/// <summary>
		/// Maximum allowed distance between home and parked car when travelling home without forced to use the car
		/// </summary>
		public float MaxParkedCarDistanceToHome = 256f;

		/// <summary>
		/// minimum required distance between target building and parked car for using a car
		/// </summary>
		public float MaxParkedCarDistanceToBuilding = 512f;

		/// <summary>
		/// maximum incoming vehicle square distance to junction for priority signs
		/// </summary>
		public float MaxPriorityCheckSqrDist = 225f;

		/// <summary>
		/// maximum junction approach time for priority signs
		/// </summary>
		public float MaxPriorityApproachTime = 15f;

		/// <summary>
		/// maximum waiting time at priority signs
		/// </summary>
		public uint MaxPriorityWaitTime = 100;

		/// <summary>
		/// Frame resolution for priority update timestamps
		/// </summary>
		public int VehicleStateUpdateShift = 6;

		/// <summary>
		/// average squared speed (in %) threshold for a segment to be flagged as congested
		/// </summary>
		public uint CongestionSqrSpeedThreshold = 60;

		/// <summary>
		/// public transport demand increment on path-find failure
		/// </summary>
		public uint PublicTransportDemandIncrement = 10u;

		/// <summary>
		/// public transport demand increment if waiting time was exceeded
		/// </summary>
		public uint PublicTransportDemandWaitingIncrement = 3u;

		/// <summary>
		/// public transport demand decrement on simulation step
		/// </summary>
		public uint PublicTransportDemandDecrement = 1u;

		/// <summary>
		/// public transport demand decrement on path-find success
		/// </summary>
		public uint PublicTransportDemandUsageDecrement = 7u;

		/// <summary>
		/// parking space demand decrement on simulation step
		/// </summary>
		public uint ParkingSpaceDemandDecrement = 1u;

		/// <summary>
		/// minimum parking space demand delta when a passenger car could be spawned
		/// </summary>
		public int MinSpawnedCarParkingSpaceDemandDelta = -5;

		/// <summary>
		/// maximum parking space demand delta when a passenger car could be spawned
		/// </summary>
		public int MaxSpawnedCarParkingSpaceDemandDelta = 3;

		/// <summary>
		/// minimum parking space demand delta when a parking spot could be found
		/// </summary>
		public int MinFoundParkPosParkingSpaceDemandDelta = -5;

		/// <summary>
		/// maximum parking space demand delta when a parking spot could be found
		/// </summary>
		public int MaxFoundParkPosParkingSpaceDemandDelta = 3;

		/// <summary>
		/// parking space demand increment when no parking spot could be found while trying to park
		/// </summary>
		public uint FailedParkingSpaceDemandIncrement = 5u;

		/// <summary>
		/// parking space demand increment when no parking spot could be found while trying to spawn a parked vehicle
		/// </summary>
		public uint FailedSpawnParkingSpaceDemandIncrement = 10u;

		/// <summary>
		/// Main menu button position
		/// </summary>
		public int MainMenuButtonX = 464;
		public int MainMenuButtonY = 10;
		public bool MainMenuButtonPosLocked = false;

		/// <summary>
		/// Main menu position
		/// </summary>
		public int MainMenuX = 85;
		public int MainMenuY = 60;
		public bool MainMenuPosLocked = false;

		internal static void WriteConfig() {
			ModifiedTime = WriteConfig(Instance);
		}

		private static GlobalConfig WriteDefaultConfig(GlobalConfig oldConfig, out DateTime modifiedTime) {
			Log._Debug($"Writing default config...");
			GlobalConfig conf = new GlobalConfig();
			if (oldConfig != null) {
				conf.MainMenuButtonX = oldConfig.MainMenuButtonX;
				conf.MainMenuButtonY = oldConfig.MainMenuButtonY;

				conf.MainMenuX = oldConfig.MainMenuX;
				conf.MainMenuY = oldConfig.MainMenuY;

				conf.MainMenuButtonPosLocked = oldConfig.MainMenuButtonPosLocked;
				conf.MainMenuPosLocked = oldConfig.MainMenuPosLocked;
			}
			modifiedTime = WriteConfig(conf);
			return conf;
		}

		private static DateTime WriteConfig(GlobalConfig config, string filename=FILENAME) {
			try {
				Log.Info($"Writing global config to file '{filename}'...");
				XmlSerializer serializer = new XmlSerializer(typeof(GlobalConfig));
				using (TextWriter writer = new StreamWriter(filename)) {
					serializer.Serialize(writer, config);
				}
			} catch (Exception e) {
				Log.Error($"Could not write global config: {e.ToString()}");
			}

			try {
				return File.GetLastWriteTime(FILENAME);
			} catch (Exception e) {
				Log.Warning($"Could not determine modification date of global config: {e.ToString()}");
				return DateTime.Now;
			}
		}

		public static GlobalConfig Load(out DateTime modifiedTime) {
			try {
				modifiedTime = File.GetLastWriteTime(FILENAME);

				Log.Info($"Loading global config from file '{FILENAME}'...");
				using (FileStream fs = new FileStream(FILENAME, FileMode.Open)) {
					XmlSerializer serializer = new XmlSerializer(typeof(GlobalConfig));
					Log.Info($"Global config loaded.");
					GlobalConfig conf = (GlobalConfig)serializer.Deserialize(fs);
					if (LoadingExtension.IsGameLoaded && !conf.DebugSwitches[10]) {
						Constants.ManagerFactory.RoutingManager.RequestFullRecalculation(true);
					}
					return conf;
				}
			} catch (Exception e) {
				Log.Warning($"Could not load global config: {e} Generating default config.");
				return WriteDefaultConfig(null, out modifiedTime);
			}
		}

		public static void Reload(bool checkVersion=true) {
			DateTime modifiedTime;
			GlobalConfig conf = Load(out modifiedTime);
			if (checkVersion && conf.Version != -1 && conf.Version < LATEST_VERSION) {
				// backup old config and reset
				string filename = BACKUP_FILENAME;
				try {
					int backupIndex = 0;
					while (File.Exists(filename)) {
						filename = BACKUP_FILENAME + "." + backupIndex;
						++backupIndex;
					}
					WriteConfig(conf, filename);
				} catch (Exception e) {
					Log.Warning($"Error occurred while saving backup config to '{filename}': {e.ToString()}");
				}
				Reset(conf);
			} else {
				Instance = conf;
				ModifiedTime = WriteConfig(Instance);
			}
		}

		public static void Reset(GlobalConfig oldConfig) {
			Log.Info($"Resetting global config.");
			DateTime modifiedTime;
			Instance = WriteDefaultConfig(oldConfig, out modifiedTime);
			ModifiedTime = modifiedTime;
		}

		private static void ReloadIfNewer() {
			try {
				DateTime modifiedTime = File.GetLastWriteTime(FILENAME);
				if (modifiedTime > ModifiedTime) {
					Log.Info($"Detected modification of global config.");
					Reload(false);
				}
			} catch (Exception) {
				Log.Warning("Could not determine modification date of global config.");
			}
		}

#if RUSHHOUR
		private static void ReloadRushHourConfigIfNewer() { // TODO refactor
			try {
				DateTime newModifiedTime = File.GetLastWriteTime(RUSHHOUR_CONFIG_FILENAME);
				if (rushHourConfigModifiedTime != null) {
					if (newModifiedTime <= rushHourConfigModifiedTime)
						return;
				}

				rushHourConfigModifiedTime = newModifiedTime;

				XmlDocument doc = new XmlDocument();
				doc.Load(RUSHHOUR_CONFIG_FILENAME);
				XmlNode root = doc.DocumentElement;

				XmlNode betterParkingNode = root.SelectSingleNode("OptionPanel/data/BetterParking");
				XmlNode parkingSpaceRadiusNode = root.SelectSingleNode("OptionPanel/data/ParkingSearchRadius");

				if ("True".Equals(betterParkingNode.InnerText)) {
					RushHourParkingSearchRadius = int.Parse(parkingSpaceRadiusNode.InnerText);
				}

				Log._Debug($"RushHour config has changed. Setting searchRadius={RushHourParkingSearchRadius}");
			} catch (Exception ex) {
				Log.Error("GlobalConfig.ReloadRushHourConfigIfNewer: " + ex.ToString());
			}
		}
#endif

		public void SimulationStep() {
#if RUSHHOUR
			if (LoadingExtension.IsRushHourLoaded) {
				ReloadRushHourConfigIfNewer();
			}
#endif
		}
	}
}
