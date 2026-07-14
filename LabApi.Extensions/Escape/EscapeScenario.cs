using LabApi.Features.Wrappers;
using System;
using UnityEngine;

namespace LabApi.Extensions.Escape
{
    /// <summary>
    /// Configuration template representing player escape conditions, teleportations, and visual progression scenarios.
    /// </summary>
    public class EscapeScenario
    {
        #region Core Identification

        /// <summary>
        /// Gets or sets the name or display label of this scenario.
        /// </summary>
        public string Name { get; set; } = "Default Evacuation";

        #endregion

        #region Spatial Boundaries

        /// <summary>
        /// Gets or sets the world position representing the center of the escape area.
        /// </summary>
        public Vector3 EscapeZone { get; set; } = Vector3.zero;

        /// <summary>
        /// Gets or sets the escape radius in meters.
        /// </summary>
        public float EscapeRadius { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the teleport target position after escaping.
        /// </summary>
        public Vector3 TeleportPosition { get; set; } = Vector3.zero;

        #endregion

        #region UI/UX Notifications

        /// <summary>
        /// Gets or sets the message broadcast to all players when the scenario starts.
        /// </summary>
        public string InitialHint { get; set; }

        /// <summary>
        /// Gets or sets the duration of the initial broadcast in seconds.
        /// </summary>
        public float InitialHintDuration { get; set; } = 6f;

        /// <summary>
        /// Gets or sets the message shown to the escaping player upon successful escape.
        /// </summary>
        public string SuccessHint { get; set; }

        #endregion

        #region Timeline Delays

        /// <summary>
        /// Gets or sets the delay in seconds before the escape process starts.
        /// </summary>
        public float InitialDelay { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the delay in seconds before checking for escaping players.
        /// </summary>
        public float ProcessingDelay { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the delay in seconds before changing the player's role to Spectator.
        /// </summary>
        public float FinalDelay { get; set; } = 0.5f;

        #endregion

        #region Sensory Effects

        /// <summary>
        /// Gets or sets the screen flash duration in seconds.
        /// </summary>
        public float FlashDuration { get; set; } = 1.75f;

        /// <summary>
        /// Gets or sets the blindness duration in seconds.
        /// </summary>
        public float BlindDuration { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the blur duration in seconds.
        /// </summary>
        public float BlurDuration { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the deafen duration in seconds.
        /// </summary>
        public float DeafenDuration { get; set; } = 3.75f;

        #endregion

        #region Lifecycle Events

        /// <summary>
        /// Occurs when the escape scenario starts.
        /// </summary>
        public Action OnSequenceStarted { get; set; }

        /// <summary>
        /// Occurs when the standby delay expires.
        /// </summary>
        public Action OnSequenceProcessing { get; set; }

        /// <summary>
        /// Occurs when a player successfully passes the escape criteria.
        /// </summary>
        public Action<Player> OnPlayerValidated { get; set; }

        /// <summary>
        /// Occurs when the escape scenario is completed.
        /// </summary>
        public Action OnSequenceFinalized { get; set; }

        #endregion
    }
}