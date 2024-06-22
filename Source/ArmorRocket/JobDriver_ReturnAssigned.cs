using ArmorRacks.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using Verse;
using ArmorRacks.Things;
using ArmorRacks.ThingComps;
using System.Net.Sockets;
using static RimWorld.PsychicRitualRoleDef;

namespace ArmorRocket
{
    public class JobDriver_ReturnAssigned : JobDriver_BaseRackJob
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            foreach (Toil toil in base.MakeNewToils())
            {
                yield return toil;
            }
            Apparel bracelet = (Apparel)TargetThingB;
            CompTargetBracelet targeter = bracelet.TryGetComp<CompTargetBracelet>();
            if (targeter != null)
            {
                ArmorRack c = (ArmorRack)targeter.armorRocket.parent;
                List<Thing> equipedThings = new List<Thing>();
                equipedThings.AddRange(bracelet.Wearer.apparel.WornApparel.ToList<Thing>());
                equipedThings.Add(bracelet.Wearer.equipment.Primary);
                foreach (Thing t in equipedThings)
                {
                    if (targeter.armorRocket.assignedThings.Contains(t))
                    {
                        if (c.Accepts(t) || c.Settings.AllowedToAccept(t))
                        {
                            Toil toil1 = new Toil();
                            toil1.finishActions = new List<Action>();
                            if (t.def.IsWeapon)
                            {
                                Thing weapon = c.GetStoredWeapon();
                                if (weapon != null)
                                {
                                    toil1.finishActions.Add(delegate
                                    {
                                        c.InnerContainer.Remove(weapon);
                                        GenDrop.TryDropSpawn(weapon, c.Position, c.Map, ThingPlaceMode.Near, out weapon);
                                    });
                                }
                                toil1.finishActions.Add(delegate
                                {
                                    bracelet.Wearer.equipment.Remove((ThingWithComps)t);
                                    c.InnerContainer.TryAdd(t);
                                });
                            }
                            else if (t.def.IsApparel)
                            {
                                if (c.GetStoredApparel() != null && c.GetStoredApparel().Count() > 0)
                                {
                                    foreach (Thing app in c.GetStoredApparel())
                                    {
                                        if (!ApparelUtility.CanWearTogether(app.def, t.def, c.PawnKindDef.RaceProps.body))
                                        {
                                            Verse.Log.Warning("Hello: ");
                                            toil1.finishActions.Add(delegate
                                            {
                                                c.InnerContainer.Remove(app);
                                                GenDrop.TryDropSpawn(app, c.Position, c.Map, ThingPlaceMode.Near, out Thing apper);
                                            });
                                        }
                                    }
                                }
                                toil1.finishActions.Add(delegate
                                {
                                    bracelet.Wearer.apparel.Remove((Apparel)t);
                                    c.InnerContainer.TryAdd(t);
                                });
                            }
                            yield return toil1;
                        }
                        targeter.armorRocket.assignedThings.Remove(t);
                    }
                }
            }
    }
        public override int WaitTicks//stolen directly from https://github.com/khamenman/armor-racks/blob/master/Source/ArmorRacks/Jobs/JobDriver_WearRackBase.cs
        {
            get
            {
                var armorRack = (ArmorRack)TargetThingA;
                var pawnTotalEquipDelay = 0f;
                var rackTotalEquipDelay = 0f;
                var rackApparel = armorRack.GetStoredApparel();
                var pawnApparel = pawn.apparel.WornApparel;

                foreach (var apparel in rackApparel)
                {
                    var equipDelay = apparel.GetStatValue(StatDefOf.EquipDelay);
                    rackTotalEquipDelay += equipDelay;
                }
                foreach (var apparel in pawnApparel)
                {
                    var equipDelay = apparel.GetStatValue(StatDefOf.EquipDelay);
                    pawnTotalEquipDelay += equipDelay;
                }
                var totalEquipDelay = Math.Max(rackTotalEquipDelay, pawnTotalEquipDelay);

                var armorRackProps = armorRack.GetComp<ArmorRackComp>().Props;
                var powerComp = armorRack.GetComp<CompPowerTrader>();
                var powerOn = powerComp != null && powerComp.PowerOn;
                float equipDelayFactor = powerOn ? armorRackProps.equipDelayFactorPowered : armorRackProps.equipDelayFactor;
                var waitTicks = totalEquipDelay * equipDelayFactor * 60f;
                return (int)waitTicks;
            }
        }
    }
}
