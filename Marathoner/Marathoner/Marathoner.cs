using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Marathoner
{
    public class Marathoner : MonoBehaviour
    {
        // Directory in which to find the configuration options for this mod.
        private const string configDir = "Mods/MarathonerConfig.json";

        // Unity GameObject containing this class as a component.
        public static MarathonerGO mod;

        // Parsed contents of the configuration file.
        ModOptions modOptions;

        private float defaultMoveSpeed;
        
        // The player-specific RewiredInputs instances are in a private variable, and must be acquired by reflection.
        // Since it will be accessed on every Update(), it is exposed and cached here for convenience & performance.
        public static Dictionary<int, RewiredInputs> m_playerInputManager = (Dictionary<int, RewiredInputs>)typeof(ControlsInput).GetField("m_playerInputManager", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null); // null for static field

        // Point in time that this class was initialised.
        FieldInfo m_lastUpdateTime = typeof(CharacterStats).GetField("m_lastUpdateTime", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Current movement speed.
        FieldInfo m_movementSpeed = typeof(CharacterStats).GetField("m_movementSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Character reference.
        FieldInfo m_character = typeof(CharacterStats).GetField("m_character", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// Use Monobehaviour's `Initialize` function.
        /// </summary>
        public void Initialize()
        {
            modOptions = this.LoadSettings();

            this.Patch();
        }

        /// <summary>
        /// Decide whether or not the regeneration should occur based on whether or not the game is paused.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">Always a PlayerCharacterStats instance.</param>
        /// <returns></returns>
        private float UpdateDeltaTime<T>(T instance)
        {
            // If the game is paused, return 0, otherwise return the difference between now and the last update time.
            return ((float)m_lastUpdateTime.GetValue(instance) == -999f) ? 0f : (Time.time - (float)m_lastUpdateTime.GetValue(instance));
        }

        /// <summary>
        /// Hook onto the PlayerCharacterStats instance and add a hook to run the patch when stats are being updated. 
        /// </summary>
        public void Patch()
        {
            On.PlayerCharacterStats.OnStart += new On.PlayerCharacterStats.hook_OnStart(this.GetDetails);
            On.PlayerCharacterStats.OnUpdateStats += new On.PlayerCharacterStats.hook_OnUpdateStats(this.ApplyMod);
        }

        private void GetDetails(On.PlayerCharacterStats.orig_OnStart original, PlayerCharacterStats instance)
        {
            this.defaultMoveSpeed = instance.MovementSpeed;
        }

        private void ApplyMod(On.PlayerCharacterStats.orig_OnUpdateStats original, PlayerCharacterStats instance)
        {
            // Update the instance before applying changes so that all the stats are normalised again.
            original.Invoke(instance);

            // Get the required player details.
            Character character = (Character)m_character.GetValue(instance);
            int playerId = character.OwnerPlayerSys.PlayerID;

            // If sprinting, modify the movement speed.
            if (m_playerInputManager[playerId].GetButton("Sprint"))
            {
                m_movementSpeed.SetValue(instance, new Stat(defaultMoveSpeed * modOptions.SprintSpeedMultiplier));
                // UNTESTED.
                //character.Speed = moveSpeed.CurrentValue * modOptions.SprintSpeedMultiplier;
            }
            else
            {
                m_movementSpeed.SetValue(instance, new Stat(defaultMoveSpeed));
            }
        }

        /// <summary>
        /// Loads the ModOptions from the configuration file located within the mods folder.
        /// </summary>
        /// <returns>Populated GameData.</returns>
        private ModOptions LoadSettings()
        {
            try
            {
                // Read the configuration file (path is relative to exe dir).
                using (StreamReader streamReader = new StreamReader(configDir))
                {
                    try
                    {
                        ModOptions gameData = JsonUtility.FromJson<ModOptions>(streamReader.ReadToEnd());
                        return gameData;
                    }
                    catch (ArgumentNullException ex)
                    {
                        Debug.Log("Argument null exception");
                    }
                    catch (FormatException ex)
                    {
                        Debug.Log("Format Exception");
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Debug.Log("File Not Found Exception");
            }
            catch (IOException ex)
            {
                Debug.Log("General IO Exception");
            }
            catch (Exception ex)
            {
                Debug.Log("Meditation exception: " + ex.Message);
            }

            // If it's made it this far something is wrong, return null.
            return null;
        }
    }
}
