using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BrutalCompanyMinus;
using BrutalCompanyMinus.Minus;
using EasyTextEffects.Editor.MyBoxCopy.Extensions;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BCMERDice
{
    [HarmonyPatch]
    public class Net : NetworkBehaviour
    {
        public static Net? Instance { get; private set; }
        public static GameObject? netObject { get; private set; }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(GameNetworkManager), "Start")]
        private static void InitalizeServerObject()
        {
            Instance = SetupNetworkManagerObject<Net>();
            netObject = Instance.gameObject;
            Instance.gameObject.name = "BCMERDice_NetObject";
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        private static void SpawnNetworkHandler()
        {
            UnityEngine.Object.Instantiate(netObject).GetComponent<NetworkObject>().Spawn(destroyWithScene: false);
        }



        public static T SetupNetworkManagerObject<T>() where T : NetworkBehaviour
        {
            GameObject newPrefab = new GameObject(nameof(T));
            newPrefab.hideFlags = HideFlags.HideAndDontSave;

            T instancedBehaviour = newPrefab.AddComponent<T>();

            NetworkObject networkObject = newPrefab.AddComponent<NetworkObject>();
            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + nameof(T)));
            networkObject.GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);
            networkObject.DontDestroyWithOwner = true;
            networkObject.SceneMigrationSynchronization = true;
            networkObject.DestroyWithScene = true;
            DontDestroyOnLoad(newPrefab);

            NetworkManager.Singleton.AddNetworkPrefab(newPrefab);

            return (instancedBehaviour);
        }

        [ServerRpc(RequireOwnership = false)]
        public void EventMessageServerRpc(string name, bool type)
        {
            EventMessageClientRpc(name, type);
        }

        [ClientRpc]
        public void EventMessageClientRpc(string name, bool type)
        {
            BCMERDiceBase.Instance.mls.LogInfo($"A BCMERDice HUDMessage is being called.");
            HUDManager.Instance.DisplayTip("BCMER Dice", "An event with the name " + name + " has been forced!", type);
        }
        

        [ServerRpc(RequireOwnership = false)]
        public void SetItemDisabledServerRpc(NetworkObjectReference scrapItemRef)
        {
            SetItemDisabledClientRpc(scrapItemRef);
        }
        

        [ClientRpc]
        public void SetItemDisabledClientRpc(NetworkObjectReference scrapItemRef)
        {
            BCMERDiceBase.Instance.mls.LogInfo($"Attempting to disable a dice.");
            if (scrapItemRef.TryGet(out NetworkObject networkObject))
            {
                BCMERDiceBase.Instance.mls.LogInfo($"A BCMERDice was disabled");
                // Get the GrabbableObject component from that NetworkObject
                if (networkObject.TryGetComponent(out GrabbableObject scrapItem))
                {
                    scrapItem.grabbable = false;
                    scrapItem.grabbableToEnemies = false;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeleteItemOnServerRpc(NetworkObjectReference scrapItem)
        {
            if (!IsServer) return;

            BCMERDiceBase.Instance.mls.LogInfo($"A BCMERDice dice is being requested to delete on the host.");
            StartCoroutine(DeleteAfterSomeTimeCoroutine(scrapItem, 5f));
        }
        
        public IEnumerator DeleteAfterSomeTimeCoroutine(NetworkObjectReference scrapItem, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (scrapItem.TryGet(out NetworkObject netObj))
            {
                if (netObj != null)
                {
                    netObj.Despawn();
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ForceTheEventOnServerRpc(string names)
        {
            BCMERDiceBase.Instance.mls.LogInfo($"Forcing the event {names} on the server.");
            BrutalCompanyMinus.Minus.API.ForceEvents(new string[] { names });
        }
    }

}

