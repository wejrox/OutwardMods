using Partiality.Modloader;
using UnityEngine;


namespace Marathoner
{
    /// <summary>
    /// Performs the initial setup of the mod within the game scene.
    /// </summary>
    public class MarathonerGO : PartialityMod
    {
        /// <summary>Main class containing functionality.</summary>
        public static Marathoner main;

        public MarathonerGO()
        {
            this.ModID = "Marathoner";
            this.Version = "0100";
            this.author = "JimJoms";
        }

        /// <summary>
        /// Creates the connection between the mod and the game scene through the creation of a GameObject with the 
        /// mod attached.
        /// Initialises the mod.
        /// </summary>
        public override void OnEnable()
        {
            // Call base initialiser.
            base.OnEnable();

            // Ensure the mod has a reference to this object for scene access.
            Marathoner.mod = this;

            // Create an object in the scene to house the component.
            GameObject gameObject = new GameObject();

            // Instantiate the mod in the scene.
            MarathonerGO.main = gameObject.AddComponent<Marathoner>();

            // Run the mod initialiser.
            MarathonerGO.main.Initialize();
        }
    }

}
