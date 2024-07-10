using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Indicator.Extensions;

namespace Indicator
{
    public class Indicator : ThunderBehaviour
    {
        // attact this to a creature
        private Creature creature;
        private EdgeDatum edgeDataLeft;
        private EdgeDatum edgeDataRight;
        private int counter = 0;
        private static readonly ParticleSystem.MinMaxGradient gcRed = new(Color.red, Color.red);
        private static readonly ParticleSystem.MinMaxGradient gcGreen = new(Color.green, Color.green);
        public class EdgeDatum
        {
            public string transformName;
            public float VerticalAngle;
            public float HorizontalAngle;
            public float NormalAngle;
            public PhysicBody body;
            public ParticleSystem effect;
            public Damager damager;
            public EdgeDatum(string name, Item item, Damager damager, float vAnlge, float hAngle, float nAngle)
            {
                this.transformName = name;
                this.body = item.physicBody;
                this.damager = damager;
                this.VerticalAngle = vAnlge;
                this.HorizontalAngle = hAngle;
                this.NormalAngle = nAngle;
            }
        }
        public void Setup(Creature creature)
        {
            this.creature = creature;
            this.creature.handLeft.OnGrabEvent += this.GetItemDamager_OnGrabEvent;
            this.creature.handLeft.OnUnGrabEvent += this.RemoveEffect_OnUnGrabEvent;
            this.creature.handRight.OnGrabEvent += this.GetItemDamager_OnGrabEvent;
            this.creature.handRight.OnUnGrabEvent += this.RemoveEffect_OnUnGrabEvent;
            this.creature.OnDespawnEvent += this.UnEvent_OnDespawnEvent;
        }
        private void UnEvent_OnDespawnEvent(EventTime eventTime)
        {
            if(eventTime == EventTime.OnEnd)
            {
                this.creature.handLeft.OnGrabEvent -= this.GetItemDamager_OnGrabEvent;
                this.creature.handLeft.OnUnGrabEvent -= this.RemoveEffect_OnUnGrabEvent;
                this.creature.handRight.OnGrabEvent -= this.GetItemDamager_OnGrabEvent;
                this.creature.handRight.OnUnGrabEvent -= this.RemoveEffect_OnUnGrabEvent;
                this.creature.OnDespawnEvent -= this.UnEvent_OnDespawnEvent;
            }
        }
        private void GetItemDamager_OnGrabEvent(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
        {
            if (handle.item) // in case the player's creature is killed
            {
                if (eventTime == EventTime.OnEnd)
                {
                    //TODO: more efficient impl.
                    // IEnumerable<Damager> slashDamagers =  handle.item.mainCollisionHandler.damagers.Where(damager => damager is { penetrationDepth: > 0, penetrationLength: 0 });
                    IEnumerable<Damager> slashDamagers = handle.item.mainCollisionHandler.damagers
                        .Where<Damager>(d => d.penetrationDepth != 0 /*&& d.colliderGroup.colliders
                        .Where<Collider>(c => c.sharedMaterial.name == "Blade").Count() > 0*/
                    );
                    if (slashDamagers.Count() > 0)
                    {
                        Damager slashDamager = slashDamagers.OrderBy(d => d.penetrationLength == 0).First();
                        if (IndicatorManager.OptVerboseLog) Debug.Log($"slash damager detected in {handle.item.data.displayName} ({slashDamager.name}) (len={slashDamagers.Count()})");
                        this.AddTrailRenderer(handle.item, slashDamager, side);
                    }
                    else
                    {
                        if (IndicatorManager.OptVerboseLog) Debug.Log($"slash damager not detected ({handle.item.name})");
                    }
                }
            }   
        }
        private void RemoveEffect_OnUnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime)
        {
            if(side == Side.Left && this.edgeDataLeft != null)
            {
                if(this.edgeDataRight?.effect != this.edgeDataLeft.effect) Object.Destroy(this.edgeDataLeft.effect.gameObject);
                this.edgeDataLeft = null;
            }
            else if (side == Side.Right && this.edgeDataRight != null)
            {
                if(this.edgeDataLeft?.effect != this.edgeDataRight.effect) Object.Destroy(this.edgeDataRight.effect.gameObject);
                this.edgeDataRight = null;
            }
        }
        private void AddTrailRenderer(Item item, Damager primalDamager, Side side)
        {
            // TODO: attach and scale with parry point length
            // item.parryPoint はAIの行動時に参照するだけ?
            Catalog.InstantiateAsync(
                "Indicator.WeaponTrail",
                Vector3.zero,
                Quaternion.Euler(0f, 0f, 90f),
                null,
                delegate(GameObject go)
                {
                    // TODO: in the case both hands grip the same weapon
                    if (item.parryTargets.Count > 0 )
                    {
                        go.transform.SetParent(item.parryTargets[0].transform, false);
                        go.transform.localScale = Vector3.one * item.parryTargets[0].length;
                    }
                    else
                    {
                        go.transform.SetParent(item.physicBody.rigidBody.gameObject.transform, false);
                        go.transform.localScale = Vector3.one * IndicatorManager.OptLength;
                        go.transform.localPosition = IndicatorManager.OptOffset;
                    }
                    ParticleSystem.MainModule main = go.GetComponent<ParticleSystem>().main;
                    main.startColor = gcRed;
                    if (side == Side.Left)
                    {
                        this.edgeDataLeft = new("test", item, primalDamager, 0f, 0f, 0f)
                        {
                            effect = go.GetComponent<ParticleSystem>()
                        };
                    }
                    else if (side == Side.Right)
                    {
                        this.edgeDataRight = new("test", item, primalDamager, 0f, 0f, 0f)
                        {
                            effect = go.GetComponent<ParticleSystem>()
                        };
                    }
                    if (!IndicatorManager.OptActivation)
                    {
                        this.edgeDataLeft?.effect?.Stop();
                        this.edgeDataRight?.effect?.Stop();
                    }
                },
                "LoadTrail"
                );
        }
        private void ChangeColorOnLoop(EdgeDatum data)
        {
            if (IndicatorManager.OptAngle == 0 && data.damager.CheckAngles(data.body.velocity))
            {
                ParticleSystem.MainModule main = data.effect.main;
                main.startColor = gcGreen;
            }
            else if(IndicatorManager.OptAngle != 0 && data.damager.CheckAnglesFixed(data.body.velocity, IndicatorManager.OptAngle))
            {
                ParticleSystem.MainModule main = data.effect.main;
                main.startColor = gcGreen;
            }
            else
            {
                ParticleSystem.MainModule main = data.effect.main;
                main.startColor = gcRed;
            }
        }
        public void ToggleEffect(bool state)
        {
            ParticleSystem left = this.edgeDataLeft?.effect;
            ParticleSystem right = this.edgeDataRight?.effect;
            if (state)
            {
                left?.Play();
                right?.Play();
            }
            else
            {
                left?.Stop();
                right?.Stop();
            }
        }
        public void ChangeOffset(Vector3? offset = null, float? length = null)
        {
            Transform left = this.edgeDataLeft?.effect?.gameObject?.transform;
            Transform right = this.edgeDataRight?.effect?.gameObject?.transform;
            if (left != null)
            {
                if (offset != null) left.localPosition = (Vector3)offset;
                if (length != null) left.localScale = Vector3.one * (length ?? 1f);
            }
            if(right != null)
            {
                if(offset != null) right.localPosition = (Vector3)offset;
                if (length != null) right.localScale = Vector3.one * (length ?? 1f) ;
            }
        }
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;
        protected override void ManagedUpdate()
        {
            if(counter % 3 == 0)
            {
                if(this.edgeDataLeft != null)
                {
                    this.ChangeColorOnLoop(this.edgeDataLeft);
                }
                if(this.edgeDataRight != null)
                {
                    this.ChangeColorOnLoop(this.edgeDataRight);
                }
                this.counter = 0;
            }
            else
            {
                this.counter++;
            }
            base.ManagedUpdate();
        }
    }
}
