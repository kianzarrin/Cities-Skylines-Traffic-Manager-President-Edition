﻿namespace TrafficManager.Custom.AI {
    using API.Traffic.Data;
    using API.Traffic.Enums;
    using ColossalFramework;
    using JetBrains.Annotations;
    using Manager.Impl;
    using PathFinding;
    using RedirectionFramework.Attributes;
    using UnityEngine;

    [TargetType(typeof(AmbulanceAI))]
    public class CustomAmbulanceAI : CarAI {
        [RedirectMethod]
        [UsedImplicitly]
        public bool CustomStartPathFind(ushort vehicleId,
                                        ref Vehicle vehicleData,
                                        Vector3 startPos,
                                        Vector3 endPos,
                                        bool startBothWays,
                                        bool endBothWays,
                                        bool undergroundTarget) {
            ExtVehicleType emergencyVehType = (vehicleData.m_flags & Vehicle.Flags.Emergency2) != 0
                                     ? ExtVehicleType.Emergency
                                     : ExtVehicleType.Service;
            ExtVehicleType vehicleType = ExtVehicleManager.Instance.OnStartPathFind(vehicleId, ref vehicleData, emergencyVehType);

            VehicleInfo info = m_info;
            bool allowUnderground = (vehicleData.m_flags & (Vehicle.Flags.Underground | Vehicle.Flags.Transition)) != 0;

            if (PathManager.FindPathPosition(
                    startPos,
                    ItemClass.Service.Road,
                    NetInfo.LaneType.Vehicle
                    | NetInfo.LaneType.TransportVehicle,
                    info.m_vehicleType,
                    allowUnderground,
                    false,
                    32f,
                    out PathUnit.Position startPosA,
                    out PathUnit.Position startPosB,
                    out float startDistSqrA,
                    out _)
                && PathManager.FindPathPosition(
                    endPos,
                    ItemClass.Service.Road,
                    NetInfo.LaneType.Vehicle
                    | NetInfo.LaneType.TransportVehicle,
                    info.m_vehicleType,
                    undergroundTarget,
                    false,
                    32f,
                    out PathUnit.Position endPosA,
                    out PathUnit.Position endPosB,
                    out float endDistSqrA,
                    out _)) {
                if (!startBothWays || startDistSqrA < 10f) {
                    startPosB = default;
                }

                if (!endBothWays || endDistSqrA < 10f) {
                    endPosB = default;
                }

                // NON-STOCK CODE START
                PathCreationArgs args;
                args.extPathType = ExtPathType.None;
                args.extVehicleType = vehicleType;
                args.vehicleId = vehicleId;
                args.spawned = (vehicleData.m_flags & Vehicle.Flags.Spawned) != 0;
                args.buildIndex = Singleton<SimulationManager>.instance.m_currentBuildIndex;
                args.startPosA = startPosA;
                args.startPosB = startPosB;
                args.endPosA = endPosA;
                args.endPosB = endPosB;
                args.vehiclePosition = default;
                args.laneTypes = NetInfo.LaneType.Vehicle
                                 | NetInfo.LaneType.TransportVehicle;
                args.vehicleTypes = info.m_vehicleType;
                args.maxLength = 20000f;
                args.isHeavyVehicle = IsHeavyVehicle();
                args.hasCombustionEngine = CombustionEngine();
                args.ignoreBlocked = IgnoreBlocked(vehicleId, ref vehicleData);
                args.ignoreFlooded = false;
                args.ignoreCosts = false;
                args.randomParking = false;
                args.stablePath = false;
                args.skipQueue = (vehicleData.m_flags & Vehicle.Flags.Spawned) != 0;

                if (CustomPathManager._instance.CustomCreatePath(
                    out uint path, ref Singleton<SimulationManager>.instance.m_randomizer, args)) {
                    // NON-STOCK CODE END
                    if (vehicleData.m_path != 0u) {
                        Singleton<PathManager>.instance.ReleasePath(vehicleData.m_path);
                    }

                    vehicleData.m_path = path;
                    vehicleData.m_flags |= Vehicle.Flags.WaitingPath;
                    return true;
                }
            } else {
                PathfindFailure(vehicleId, ref vehicleData);
            }

            return false;
        }
    }
}
