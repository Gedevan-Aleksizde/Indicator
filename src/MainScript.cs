using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using Unity.Collections.LowLevel.Unsafe;

namespace Indicator
{
    internal class IndicatorManager: ThunderScript
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
        [ModOption(order = 0, name = "apply_any", nameLocalizationId = "ModOpts.apply_any", defaultValueIndex = 0)]
        [ModOptionTooltip("A", "ModOpts.apply_any_desc")]
        public bool OptApplyAny { get; private set; }
        [ModOption(order = 1, name = "length", nameLocalizationId = "ModOpts.length", defaultValueIndex = 101, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptLength { get; private set; }

        private static ModOptionFloat[] OptValueScale()
        {
            ModOptionFloat[] values = new ModOptionFloat[101];
            for(int i = 0; i < values.Length; i++)
            {
                values[i] = new ModOptionFloat((i / 100f).ToString("F2"), i / 100f);
            }
            return values;
        }
        [ModOption(order = 2, name = "offset_x", nameLocalizationId = "ModOpts.offset_x", defaultValueIndex = 0, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptX { get; private set; }
        [ModOption(order = 3, name = "offset_y", nameLocalizationId = "ModOpts.offset_y", defaultValueIndex = 0, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptY { get; private set; }
        [ModOption(order = 4, name = "offset_z", nameLocalizationId = "ModOpts.offset_z", defaultValueIndex = 0, valueSourceName = nameof(OptValueScale), interactionType = ModOption.InteractionType.Slider)]
        public static float OptZ { get; private set; }
        }
}
