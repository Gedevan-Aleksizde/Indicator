using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ThunderRoad;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Indicator
{
    internal class IndicatorManager : ThunderScript
    {
        public override void ScriptLoaded(ModManager.ModData modData)
        {
            base.ScriptLoaded(modData);
        }
        public override void ScriptEnable()
        {
            EventManager.onCreatureSpawn += this.EventManager_onCreatureSpawn;
            base.ScriptEnable();
        }
        public override void ScriptDisable()
        {
            EventManager.onCreatureSpawn -= this.EventManager_onCreatureSpawn;
            base.ScriptDisable();
        }
        public override void ScriptUnload()
        {
            base.ScriptUnload();
        }
        private void EventManager_onCreatureSpawn(Creature creature)
        {
            if (!creature.isKilled && (creature.isPlayer || OptApplyAny))
            {
                creature.gameObject.AddComponent<Indicator>().Setup(creature);
            }
        }
        [ModOptionCategory("General", 0, "ModOpts.category_general")]
        [ModOption(order = 0, name = "activation", nameLocalizationId = "ModOpts.activation", defaultValueIndex = 1)]
        [ModOptionTooltip("A", "ModOpts.activation_desc")]
        public static bool OptActivation
        {
            get => _optActivation;
            set
            {
                foreach (Creature c in Creature.all)
                {
                    Indicator indicator = c.GetComponentInChildren<Indicator>();
                    indicator?.ToggleEffect(value);
                }
                _optActivation = value;
            }
        }
        public static bool _optActivation;
        
        [ModOptionCategory("General", 1, "ModOpts.category_general")]
        [ModOption(order = 2, name = "angle", nameLocalizationId = "ModOpts.angle", interactionType = ModOption.InteractionType.Slider, defaultValueIndex = 0, valueSourceName = nameof(OptsAngle))]
        [ModOptionTooltip("A", "ModOpts.angle_desc")]
        public static int OptAngle
        {
            get;
            set;
        }
        private static int _optAngle;
        private static ModOptionInt[] OptsAngle()
        {
            ModOptionInt[] vals = new ModOptionInt[91];
            vals[0] = new ModOptionInt("Auto", "ModOpts.value_auto", 0);
            for (int i = 1; i < vals.Length; i++)
            {
                vals[i] = new ModOptionInt(String.Format("{0,-2:d}°", i), i);
            }
            return vals;
        }
        [ModOptionCategory("General", 2, "ModOpts.category_general")]
        [ModOption(order = 1, name = "apply_any", nameLocalizationId = "ModOpts.apply_any", defaultValueIndex = 0)]
        [ModOptionTooltip("A", "ModOpts.apply_any_desc")]
        public static bool OptApplyAny { get; private set; }
        [ModOptionCategory("Advanced", 1, "ModOpts.category_advanced")]
        [ModOption(order = 0, name = "length", nameLocalizationId = "ModOpts.length", defaultValueIndex = 101, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptLength
        {
            get => _optLength;
            private set
            {
                _optLength = value;
                CallChangeOffsets(length: value);
            }
        }
        private static float _optLength;

        private static ModOptionFloat[] OptValueScale()
        {
            ModOptionFloat[] values = new ModOptionFloat[101];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new ModOptionFloat((i / 100f).ToString("F2"), i / 100f);
            }
            return values;
        }
        [ModOptionCategory("Advanced", 1, "ModOpts.category_advanced")]
        [ModOption(order = 1, name = "offset_x", nameLocalizationId = "ModOpts.offset_x", defaultValueIndex = 0, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptX {
            get => _optOffset.x;
            private set
            {
                _optOffset.x = value;
                CallChangeOffsets(offset: OptOffset);
            }
        }
        [ModOptionCategory("Advanced", 1, "ModOpts.category_advanced")]
        [ModOption(order = 2, name = "offset_y", nameLocalizationId = "ModOpts.offset_y", defaultValueIndex = 0, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptY {
            get => _optOffset.y;
            private set
            {
                _optOffset.y = value;
                CallChangeOffsets(offset: OptOffset);
            }
        }
        [ModOptionCategory("Advanced", 1, "ModOpts.category_advanced")]
        [ModOption(order = 3, name = "offset_z", nameLocalizationId = "ModOpts.offset_z", defaultValueIndex = 0, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptZ
        {
            get => _optOffset.z;
            private set
            {
                _optOffset.z = value;
                CallChangeOffsets(offset: OptOffset);
            }

        }
        private static Vector3 _optOffset;
        public static Vector3 OptOffset { get => _optOffset; }
        private static void CallChangeOffsets(Vector3? offset = null, float? length = null)
        {
            foreach (Creature c in Creature.all)
            {
                Indicator indicator = c.GetComponentInChildren<Indicator>();
                indicator?.ChangeOffset(offset, length);
            }
        }
        [ModOptionCategory("Advanced", 1, "ModOpts.category_advanced")]
        [ModOption(order = 4, name = "verbose_log", nameLocalizationId = "ModOpts.verbose_log", defaultValueIndex = 0)]
        public static bool OptVerboseLog { get; private set; }
    }
}
