// =====================================
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BrutalCompanyMinus.Minus;
using Discord;
using GameNetcodeStuff;
using HarmonyLib;
using MysteryDice.Effects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BCMERDice
{
    [HarmonyPatch]
    public class BrutalDiceItem : GrabbableObject
    {

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            PlayerControllerB player = playerHeldBy;

            // Select a random number from 1 to 6
            // You know like a regular dice
            int randomNumber = UnityEngine.Random.Range(1, 7);
            BCMERDiceBase.Instance.mls.LogInfo($"BCMERDice rolled a {randomNumber}!");

            // Select what effect to apply
            BCMERDiceBase.Instance.mls.LogInfo($"Calling random effect.");
            ChooseRandomEffect(randomNumber);

            //Drop Item after
            BCMERDiceBase.Instance.mls.LogInfo($"Called Drop Item.");
            DropItem(player);
        }

        private static void ChooseRandomEffect(int randomNumber)
        {
            // Apply the corresponding effect based on the random number
            switch (randomNumber)
            {
                case 1: // Roller names an event.
                    BrutalDiceItem.activeEventToChat = true;
                    BCMERDiceBase.Instance.mls.LogInfo($"Opened up chat listener");

                    // Grab your player id
                    BrutalDiceItem.TargetPlayerClientId = GameNetworkManager.Instance.localPlayerController.playerClientId;
                    
                    HUDManager.Instance.DisplayTip("BCMER Dice", "Type an event into chat to force it for the next day.", false);
                    break;
                case 2:
                    // Very Bad
                    List<MEvent> veryBad = EventManager.events.Where(e => e.Type == MEvent.EventType.VeryBad || e.Type == MEvent.EventType.Insane).ToList();

                    string verybads = veryBad[UnityEngine.Random.Range(0, veryBad.Count)].Name();
                    BCMERDiceBase.Instance.mls.LogInfo($"BCMERDice selected the event {verybads} to be forced!");

                    if (Net.Instance == null)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"Net instance was null, cannot force event.");
                        break;
                    }

                    try
                    {
                        Net.Instance.ForceTheEventOnServerRpc(verybads);
                        Net.Instance.EventMessageServerRpc(verybads, true);
                    }
                    catch (Exception e)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"An error occurred while trying to force the event: {e}");
                    }
                    break;
                case 3:
                    List<MEvent> bad = EventManager.events.Where(e => e.Type == MEvent.EventType.Bad).ToList();

                    string bads = bad[UnityEngine.Random.Range(0, bad.Count)].Name();
                    BCMERDiceBase.Instance.mls.LogInfo($"BCMERDice selected the event {bads} to be forced!");

                    if (Net.Instance == null)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"Net instance was null, cannot force event.");
                        break;
                    }

                    try
                    {
                        Net.Instance.ForceTheEventOnServerRpc(bads);
                        Net.Instance.EventMessageServerRpc(bads, true);
                    }
                    catch (Exception e)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"An error occurred while trying to force the event: {e}");
                    }
                    break;
                case 4:
                    // Normal
                    List<MEvent> normal = EventManager.events.Where(e => e.Type == MEvent.EventType.Neutral || e.Type == MEvent.EventType.Remove).ToList();

                    string normals = normal[UnityEngine.Random.Range(0, normal.Count)].Name();
                    BCMERDiceBase.Instance.mls.LogInfo($"BCMERDice selected the event {normals} to be forced!");

                    if (Net.Instance == null)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"Net instance was null, cannot force event.");
                        break;
                    }

                    try
                    {
                        Net.Instance.ForceTheEventOnServerRpc(normals);
                        Net.Instance.EventMessageServerRpc(normals, false);
                    }
                    catch (Exception e)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"An error occurred while trying to force the event: {e}");
                    }
                    break;
                case 5:
                    // Good
                    List<MEvent> good = EventManager.events.Where(e => e.Type == MEvent.EventType.Good).ToList();

                    string goods = good[UnityEngine.Random.Range(0, good.Count)].Name();
                    BCMERDiceBase.Instance.mls.LogInfo($"BCMERDice selected the event {goods} to be forced!");

                    if (Net.Instance == null)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"Net instance was null, cannot force event.");
                        break;
                    }

                    try
                    {
                        Net.Instance.ForceTheEventOnServerRpc(goods);
                        Net.Instance.EventMessageServerRpc(goods, false);
                    }
                    catch (Exception e)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"An error occurred while trying to force the event: {e}");
                    }
                    break;
                case 6:
                    // Vey good and Rare
                    List<MEvent> verygood = EventManager.events.Where(e => e.Type == MEvent.EventType.VeryGood || e.Type == MEvent.EventType.Rare).ToList();

                    string verygoods = verygood[UnityEngine.Random.Range(0, verygood.Count)].Name();
                    BCMERDiceBase.Instance.mls.LogInfo($"BCMERDice selected the event {verygoods} to be forced!");

                    if (Net.Instance == null)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"Net instance was null, cannot force event.");
                        break;
                    }
                    try
                    {
                        Net.Instance.ForceTheEventOnServerRpc(verygoods);
                        Net.Instance.EventMessageServerRpc(verygoods, false);
                    }
                    catch (Exception e)
                    {
                        BCMERDiceBase.Instance.mls.LogInfo($"An error occurred while trying to force the event: {e}");
                    }
                    break;
            }
        }

        public static void DropItem(PlayerControllerB player)
        {
            // Find the player responsible for the drop
            PlayerControllerB playerWhoUsed = player;
            if (player == null) return;

            // Check the item the player is holding
            GrabbableObject heldItem = playerWhoUsed.currentlyHeldObjectServer;
            if (heldItem == null) return;

            // Drop the active item in their hand
            BCMERDiceBase.Instance.mls.LogInfo($"Forcing to drop item");
            playerWhoUsed.DiscardHeldObject();

            // Turn into network object
            NetworkObject netObj = heldItem.GetComponent<NetworkObject>();

            //Finally
            BCMERDice.Net.Instance.SetItemDisabledServerRpc(netObj);

            BCMERDiceBase.Instance.mls.LogInfo($"Requesting host connection to delete item for everyone.");
            BCMERDice.Net.Instance.DeleteItemOnServerRpc(netObj);
        }

        public static bool activeEventToChat = false;

        public static ulong TargetPlayerClientId;

        [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.AddChatMessage))]
        [HarmonyPrefix]
        private static void OnChatReceivedPrefix(string chatMessage, int playerWhoSent = -1)
        {
            if (!activeEventToChat) return;

            if (playerWhoSent == -1 || StartOfRound.Instance == null) return;

            // Set the player id to the one who sent the message 
            PlayerControllerB sender = StartOfRound.Instance.allPlayerScripts[playerWhoSent];
            if (sender == null) return;

            
            if (sender.actualClientId == TargetPlayerClientId)
            {
                BCMERDiceBase.Instance.mls.LogInfo($"Chat message sent with the content: {chatMessage}. Attempting to process as an event name.");
                ProcessCapture(chatMessage);

                activeEventToChat = false;
            }
        }

        private static void ProcessCapture(string chatMessage)
        {
            if (string.IsNullOrEmpty(chatMessage)) return;

            // Remove punctuation from the message 
            string cleanMessage = new string(chatMessage.Where(c => !char.IsPunctuation(c)).ToArray());

            // Split the message into an array o
            string[] messageWords = cleanMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Grab the first event that matches any of the words in the message, ignoring case
            var matchedEvent = EventManager.events
                .FirstOrDefault(e => e != null && messageWords.Contains(e.Name(), StringComparer.OrdinalIgnoreCase));

            BCMERDiceBase.Instance.mls.LogInfo($"First event grabbed was {matchedEvent?.Name() ?? "null"}");

            if (matchedEvent != null)
            {
                BCMERDiceBase.Instance.mls.LogInfo($"Attempting event force for event {matchedEvent.Name()}");

                if (Net.Instance == null)
                {
                    BCMERDiceBase.Instance.mls.LogInfo($"Net instance was null, cannot force event.");
                    return;
                }

                try
                {
                    Net.Instance.ForceTheEventOnServerRpc(matchedEvent.Name());
                    Net.Instance.EventMessageServerRpc(matchedEvent.Name(), true);
                }
                catch (Exception e)
                {
                    BCMERDiceBase.Instance.mls.LogInfo($"An error occurred while trying to force the event: {e}");
                }
            }
        }
    }
}