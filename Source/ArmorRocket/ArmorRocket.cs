using ArmorRacks.Drawers;
using ArmorRacks.ThingComps;
using ArmorRacks.Utils;
using ArmorRacks.Things;
using ArmorRocket;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using System.Runtime;
using Verse.AI;
using ArmorRacks.DefOfs;
using ArmorRocket.Defs;
using System.Net.Sockets;

namespace ArmorRocket
{
    /*************************
     
    This is just a plain copy because when I tried to subclass both ArmorRackInnerContainer and ArmorRack I would get the
    error undefined reference to IAssignableBuilding... This could not be found in any part of the assembly of RimWorld.
    Because of this the source of this is pretty much just ripped from https://github.com/khamenman/armor-racks/blob/master/Source/ArmorRacks/Things/ArmorRack.cs
    but with minor adjustments that I needed for my code. All Thanks goes to khamenman
     
     ************************/

    public class ArmorRocketContentsDrawer
    {
        public readonly ArmorRocketThing ArmorRocket;
        public List<ApparelGraphicRecord> ApparelGraphics;
        public bool IsApparelResolved = false;

        public ArmorRocketContentsDrawer(ArmorRocketThing armorRocket)
        {
            ArmorRocket = armorRocket;
            ApparelGraphics = new List<ApparelGraphicRecord>();
        }

        public virtual void DrawAt(Vector3 drawLoc)
        {
            if (!IsApparelResolved)
            {
                ResolveApparelGraphics();
            }
            DrawApparel(drawLoc);
            DrawWeapon(drawLoc);
        }

        public void DrawApparel(Vector3 drawLoc)
        {
            float angle = 0.0f;
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector3_1 = drawLoc;
            Vector3 vector3_2 = drawLoc;
            Mesh mesh = MeshPool.GetMeshSetForWidth(MeshPool.HumanlikeBodyWidth).MeshAt(ArmorRocket.Rotation);
            if (ArmorRocket.Rotation != Rot4.North)
            {
                vector3_2.y += 7f / 256f;
                vector3_1.y += 3f / 128f;
            }
            else
            {
                vector3_2.y += 3f / 128f;
                vector3_1.y += 7f / 256f;
            }
            Vector3 vector3_3 = quaternion * BaseHeadOffsetAt(ArmorRocket.Rotation);
            Vector3 loc = drawLoc;
            loc.y += 1f / 32f;

            for (int index = 0; index < ApparelGraphics.Count; ++index)
            {
                if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell &&
                    ApparelGraphics[index].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead &&
                    ApparelGraphics[index].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Belt
                    )
                {
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRocket.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, mat, false);
                    loc.y += 1f / 32f;
                }
            }

            Vector3 loc1 = loc + vector3_3;
            loc1.y += 1f / 32f;

            for (int index = 0; index < ApparelGraphics.Count; ++index)
            {
                if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                {
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRocket.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, loc1, quaternion, mat, false);
                }
                else if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
                {
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRocket.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, mat, false);
                }
                else if (ApparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt)
                {
                    Vector3 beltloc = loc;
                    beltloc.y = ArmorRocket.Rotation == Rot4.South ? drawLoc.y : loc.y + 1f / 32f;
                    Vector2 vector2_1 = ApparelGraphics[index].sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(ArmorRocket.Rotation, ArmorRocket.BodyTypeDef);
                    Vector2 vector2_2 = ApparelGraphics[index].sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(ArmorRocket.Rotation, ArmorRocket.BodyTypeDef);
                    Matrix4x4 matrix = Matrix4x4.Translate(beltloc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector2_1.x, 0.0f, vector2_1.y)) * Matrix4x4.Scale(new Vector3(vector2_2.x, 1f, vector2_2.y));
                    Material mat = ApparelGraphics[index].graphic.MatAt(ArmorRocket.Rotation);
                    GenDraw.DrawMeshNowOrLater(mesh, matrix, mat, false);
                    loc.y += 2f / 32f;
                }
            }
        }

        public void DrawWeapon(Vector3 drawLoc)
        {
            Thing storedWeapon = ArmorRocket.GetStoredWeapon();
            if (storedWeapon == null)
            {
                return;
            }

            Vector3 weaponDrawLoc = drawLoc;
            Mesh weaponMesh;
            float angle = -90f;
            if (ArmorRocket.Rotation == Rot4.South)
            {
                weaponDrawLoc += new Vector3(0.0f, 2.0f, -0.1f);
                weaponDrawLoc.y += 1f;
                weaponMesh = MeshPool.plane10;
                angle = -25f;
            }
            else if (ArmorRocket.Rotation == Rot4.North)
            {
                weaponDrawLoc += new Vector3(0.0f, 0.0f, -0.11f);
                ref Vector3 local = ref weaponDrawLoc;
                weaponMesh = MeshPool.plane10;
            }
            else if (ArmorRocket.Rotation == Rot4.East)
            {
                weaponDrawLoc += new Vector3(0.2f, 2.0f, -0.12f);
                weaponDrawLoc.y += 5f / 128f;
                weaponMesh = MeshPool.plane10;
                angle = 25f;
            }
            else
            {
                weaponDrawLoc += new Vector3(-0.2f, 2.0f, -0.12f);
                weaponDrawLoc.y += 5f / 128f;
                weaponMesh = MeshPool.plane10Flip;
                angle = -25f;
            }

            Graphic_StackCount graphic = storedWeapon.Graphic as Graphic_StackCount;
            Material material = graphic == null
                ? storedWeapon.Graphic.MatSingle
                : graphic.SubGraphicForStackCount(1, storedWeapon.def).MatSingle;
            Graphics.DrawMesh(weaponMesh, weaponDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), material, 0);
        }

        public void ResolveApparelGraphics()
        {
            ApparelGraphics.Clear();
            var apparelList = ArmorRocket.GetStoredApparel().ToList();
            apparelList.Sort(((a, b) => a.def.apparel.LastLayer.drawOrder.CompareTo(b.def.apparel.LastLayer.drawOrder)));
            foreach (Apparel apparel in apparelList)
            {
                ApparelGraphicRecord rec;
                if (ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, ArmorRocket.BodyTypeDef, out rec))
                {
                    ApparelGraphics.Add(rec);
                }
            }
            IsApparelResolved = true;
        }


        public Vector3 BaseHeadOffsetAt(Rot4 rotation)
        {
            Vector2 headOffset = ArmorRocket.BodyTypeDef.headOffset;
            switch (rotation.AsInt)
            {
                case 0:
                    return new Vector3(0.0f, 0.0f, headOffset.y);
                case 1:
                    return new Vector3(headOffset.x, 0.0f, headOffset.y);
                case 2:
                    return new Vector3(0.0f, 0.0f, headOffset.y);
                case 3:
                    return new Vector3(-headOffset.x, 0.0f, headOffset.y);
                default:
                    return Vector3.zero;
            }
        }
    }
    public class ArmorRocketInnerContainer : ThingOwner<Thing>
    {
        public ArmorRocketInnerContainer()
        {
        }

        public ArmorRocketInnerContainer(IThingHolder owner)
            : base(owner)
        {
        }

        public ArmorRocketInnerContainer(IThingHolder owner, bool oneStackOnly, LookMode contentsLookMode = LookMode.Deep)
            : base(owner, oneStackOnly, contentsLookMode)
        {
        }

        public override bool TryAdd(Thing item, bool canMergeWithExistingStacks = true)
        {
            var result = base.TryAdd(item, canMergeWithExistingStacks);
            var rocket = owner as ArmorRocketThing;
            rocket.ContentsChanged(item);
            rocket.addToAssigned(item);
            return result;
        }
        public bool fakeRemove(Thing item, bool remove = false)
        {
            if(remove == false) this.Remove(item);
            var rocket = owner as ArmorRocketThing;
            if (remove)
            {
                rocket.removeFromAssigned(item);
            }
            rocket.ContentsChanged(item);
            return true;
        }

        public override bool Remove(Thing item)
        {
            var result = base.Remove(item);
            return result;
        }
    }
    public class ArmorRocketThing : Building, IHaulDestination, IThingHolder
    {
        public List<Apparel> assignedArmor;//this is to pull ids for haul jobs, this doesnt drop untill the item is destroyed/getstainted/slotreplaced
        public Thing assignedWeapon = null;//^
        public Apparel targetBracelet;
        public Pawn target;
        private BodyTypeDef _BodyTypeDef;
        private PawnKindDef _PawnKindDef;
        public ArmorRocketInnerContainer InnerContainer;
        public ArmorRocketContentsDrawer ContentsDrawer;
        public StorageSettings Settings;
        public bool StorageTabVisible => false;

        public ArmorRocketThing() : base() 
        {
            InnerContainer = new ArmorRocketInnerContainer(this, false);
            ContentsDrawer = new ArmorRocketContentsDrawer(this);//noooooooooo, because of this I need to redo the animator also... the only reason Im using this mod lol, whatever just going to copy it too, refer to original mod and whatever
            assignedArmor = new List<Apparel>();
        }
        public override void PostMake()
        {
            base.PostMake();
            Settings = new StorageSettings(this);
            ThingFilter iF = new ThingFilter();
            FieldInfo iFCat = typeof(ThingFilter).GetField("categories", BindingFlags.NonPublic | BindingFlags.Instance);
            List<string> iFCategories = new List<string>();
            iFCategories.Add("Root");
            iFCat.SetValue(iF, iFCategories);
            FieldInfo IFAll = typeof(ThingFilter).GetField("allowedDefs", BindingFlags.NonPublic | BindingFlags.Instance);
            HashSet<ThingDef> iFAllowed = (HashSet<ThingDef>)IFAll.GetValue(iF);

            //meh someone else fix this lol
            DefDatabase<ThingDef>.AllDefs.ToList().ForEach(t =>
            {
                if (t.IsApparel || t.IsWeapon)
                {
                    iFAllowed.Add(t);
                }
            });
            Settings.filter = iF;



        }
        public bool canStoreApparel(Apparel apparel)
        {
            if (ArmorRackJobUtil.RaceCanWear(apparel.def, PawnKindDef.race) == false)
                return false;//will allow this just force the thing to drop on placement
            return true;
        }
        public bool canStoreWeapon(Thing weapon)
        {
            if (ArmorRackJobUtil.RaceCanEquip(weapon.def, PawnKindDef.race) == false)
                return false;//will allow this just force the thing to drop on placement
            return true;
        }
        public BodyTypeDef BodyTypeDef
        {
            get
            {
                if (_BodyTypeDef != null)
                {
                    return _BodyTypeDef;
                }
                return BodyTypeDefOf.Male;
            }
            set
            {
                _BodyTypeDef = value;
                ContentsDrawer.ResolveApparelGraphics();
            }
        }

        public PawnKindDef PawnKindDef
        {
            get
            {
                if (_PawnKindDef != null)
                {
                    return _PawnKindDef;
                }
                return PawnKindDef.Named("Colonist");
            }
            set
            {
                //add logic to see if armor needs to be removed or not, biocoded etc.
                _PawnKindDef = value;
                var defaultBodyType = ArmorRackJobUtil.GetRaceBodyTypes(value.race).ToList().First();
                BodyTypeDef = defaultBodyType;
            }
        }

        public StorageSettings GetStoreSettings()
        {
            return Settings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }


        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return InnerContainer;
        }
        public Thing GetStoredWeapon()
        {
            var innerList = InnerContainer.InnerListForReading;
            foreach (Thing storedThing in innerList)
            {
                if (storedThing.def.IsWeapon)
                {
                    return storedThing;
                }
            }
            return null;
        }
        public List<Apparel> GetStoredApparel()
        {
            var innerList = InnerContainer.InnerListForReading;
            var result = new List<Apparel>();
            foreach (Thing storedThing in innerList)
            {
                if (storedThing.def.IsApparel)
                {
                    result.Add((Apparel)storedThing);
                }
            }
            return result;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref InnerContainer, "ArmorRocketInnerContainer", this);
            Scribe_Deep.Look(ref Settings, "ArmorRocketSettings", this);
            Scribe_Defs.Look(ref _BodyTypeDef, "_BodyTypeDef");
            Scribe_Defs.Look(ref _PawnKindDef, "_PawnKindDef");
            Scribe_References.Look(ref targetBracelet, "connectedBracelet");
            Scribe_References.Look(ref target, "connectedPawn");
            //Scribe_Values.Look(ref displayTarget, "displayString");might add later?
        }

        public void DropContents()
        {
            IntVec3 dropPos = new IntVec3(DrawPos);
            InnerContainer.TryDropAll(dropPos, Map, ThingPlaceMode.Near);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            DropContents();
            base.Destroy(mode);
        }

        public virtual void ContentsChanged(Thing thing)
        {
            ContentsDrawer.IsApparelResolved = false;
        }

        //public override string GetInspectString()
        //{
        //    StringBuilder stringBuilder = new StringBuilder();
        //    var baseString = base.GetInspectString();
        //    if (baseString.Length > 0)
        //    {
        //        stringBuilder.AppendLine(baseString);
        //    }
        //    if (Faction == Faction.OfPlayer)
        //    {
        //        var c = GetComp<CompAssignableToPawn_ArmorRacks>();
        //        Pawn owner = c.AssignedPawns.Any() ? c.AssignedPawns.First() : null;
        //        var owner_string = owner != null ? owner.Label : "Nobody".Translate().ToString();
        //        stringBuilder.AppendLine("Owner".Translate() + ": " + owner_string);
        //    }
        //    return stringBuilder.ToString().TrimEndNewlines();
        //}

        public void Notify_SettingsChanged()
        {
            ContentsDrawer.IsApparelResolved = false;
        }


        /********************************************
         
                      My Added Content

         ******************************************/

        public bool Accepts(Thing t)
        {
            if (t == assignedWeapon || assignedArmor.Contains(t))
            {
                return true;
            }
            return false;
        }
        public bool fakeAccepts(Thing t)
        {
            if (t != null)
            {
                if (t.def.IsWeapon)
                {
                    return canStoreWeapon(t);
                }
                if (t.def.IsApparel)
                {
                    return canStoreApparel((Apparel)t);
                }
            }
            return false;
        }
        public void linkTargetBracelet(Thing targetBracelet)
        {
            this.targetBracelet = (Apparel)targetBracelet;
            target = this.targetBracelet.Wearer;
            this.PawnKindDef = target.kindDef;
            this._BodyTypeDef = target.story.bodyType;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {

            if(ForbidUtility.IsForbidden(this, selPawn))
                yield break;

            if (!selPawn.CanReach(this, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                FloatMenuOption failer = new FloatMenuOption("NoPath", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return failer;
                yield break;
            }


            bool canEquipWeapon = false;
            List<Apparel> replaceArmor = new List<Apparel>();
            string basicString = null;
            Thing pawnWeapon = selPawn.equipment.Primary;
            if (!selPawn.WorkTagIsDisabled(WorkTags.Violent) && GetStoredWeapon() != null)
            {
                canEquipWeapon = true;
            }
            selPawn.apparel.WornApparel.ForEach(t =>
            {
                assignedArmor.ForEach(a =>
                {
                    if (!ApparelUtility.CanWearTogether(t.def, a.def, (_PawnKindDef == null ? BodyDefOf.Human : _PawnKindDef.RaceProps.body)))
                    {
                        replaceArmor.AddDistinct(a);
                    }
                });
            });
            if (canEquipWeapon)
            {
                basicString = "Equip " + GetStoredWeapon().LabelCap;
                if (pawnWeapon != null)
                {
                    basicString += " Store " + pawnWeapon.LabelCap;
                }
                if (assignedArmor.Count > 0)
                {
                    basicString += " Equip ";
                    int i = 0;
                    assignedArmor.ForEach(t =>
                    {
                        //if (t.def != ) {
                        if (i != 0) basicString += " and ";
                        i++;
                        basicString += t.LabelCap;
                        //}
                    });

                    if (replaceArmor.Count > 0)
                    {
                        basicString += " Store ";

                        replaceArmor.ForEach(a =>
                        {
                            if (basicString.Last() != ' ') basicString += " and ";
                            basicString += a.LabelCap;
                        });
                    }
                }


            }
            else if(assignedArmor.Count > 0)
            {
                basicString = "Equip ";

                assignedArmor.ForEach(t =>
                {
                    //if (t.def != ) {
                    if (basicString != "Equip ") basicString += " and ";
                    basicString += t.LabelCap;
                    //}
                });

                if (replaceArmor.Count > 0)
                {
                    basicString += " Store ";

                    replaceArmor.ForEach(a =>
                    {
                        if (basicString.Last() != ' ') basicString += " and ";
                        basicString += a.LabelCap;
                    });
                }

            }
            else if(selPawn.apparel.WornApparelCount > 0)
            {
                basicString = "Store Worn Apparel" + (pawnWeapon != null ? " and Equiped Weapon" : "");

            }
            else if(pawnWeapon != null)
            {
                basicString = "Store Equiped Weapon";
            }

            if(basicString != null)
            {
                FloatMenuOption basic = new FloatMenuOption(basicString, delegate
                {

                    this.SetForbidden(value: false, warnOnFail: false);
                    Job job = JobMaker.MakeJob((JobDef)ArmorRocketJobDefOf.ArmorRocket_StoreSwapArmor, this);
                    job.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    //add store armor job

                });
                yield return basic;
            }

            if (!selPawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
            {
                if (canEquipWeapon)
                {
                    FloatMenuOption extractWeapon = new FloatMenuOption("Extract " + assignedWeapon.LabelCap + " and Haul", delegate
                    {

                        // add extract and haul job

                    });
                    yield return extractWeapon;
                }
                foreach (Apparel a in assignedArmor)
                {
                    FloatMenuOption extractArmor = new FloatMenuOption("Extract " + a.LabelCap + " and Haul", delegate
                    {

                        // add extract and haul job

                    });
                    yield return extractArmor;
                };
            }

            if (selPawn.apparel.WornApparel.Find(a =>
            {
                return a.HasComp<CompTargetBracelet>();
            }) != null)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Link Bracelet", delegate
                {
                    this.SetForbidden(value: false, warnOnFail: false);
                    Job job23 = JobMaker.MakeJob((JobDef)ArmorRocketJobDefOf.ArmorRocket_LinkBracelet, this);
                    job23.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job23, Verse.AI.JobTag.Misc);

                }, MenuOptionPriority.High), selPawn, this);
            }
            //add logic to do dynamic float menu option based on what the selected pawn is wearing, linkbracelet and worn armor against stored

        }
        public bool removeFromAssigned(Thing remove)
        {
            if (remove == null)
            {
                return false;
            }
            if (remove.def.IsWeapon)
            {
                if(assignedWeapon == remove)
                {
                    assignedWeapon = null;
                    return true;
                }
            }
            else
            {
                Apparel p = assignedArmor.Find(t => { return t == remove; });
                if (p != null)
                {
                    assignedArmor.Remove(p);
                    return true;
                }
            }
            return false;
        }
        public bool addToAssigned(Thing add)
        {
            if (add == null)
            {
                return false;
            }
            if (add.def.IsWeapon)
            {
                if(assignedWeapon == null)
                {
                    Verse.Log.Warning("Weapon: " + add.ToString());
                    assignedWeapon = add;
                    return true;
                }
            }
            else
            {
                Apparel p = assignedArmor.Find(t => { return t == add; });
                if (p == null)
                {
                    Verse.Log.Warning("Apparel: " + add.ToString());
                    assignedArmor.Add((Apparel)add);
                    return true;
                }
            }
            return false;
        }
        public void launchArmor(Verse.Pawn pawn)
        {
            ThingDef b = DefDatabase<ThingDef>.AllDefsListForReading.Find(c => c.defName == "ArmorRocketDef");//do a reasonable launch procedure

            ArmorProjectile launchThis = (ArmorProjectile)GenSpawn.Spawn(b, this.Position, this.Map);

            LocalTargetInfo d = new LocalTargetInfo(target);
            launchThis.Launch(this, this.DrawPos, this.target.Position, d, new ProjectileHitFlags());
            Verse.Log.Warning("Armor \"Lauched\".");
            Verse.Log.Warning(this.ToString());
        }

    }
    public class CompArmorRocket : ThingComp
    {
        public CompProperties_ArmorRocket Props => (CompProperties_ArmorRocket)this.props;
        public override void PostDraw()
        {
            ArmorRocketThing rocket = this.parent as ArmorRocketThing;
            rocket.ContentsDrawer.DrawAt(rocket.DrawPos);
        }
    }
    public class CompProperties_ArmorRocket : CompProperties
    {
        public CompProperties_ArmorRocket()
        {
            compClass = typeof(CompArmorRocket);
        }
        public CompProperties_ArmorRocket(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
    /******************REDACTED USE FOR REFERENCE TO CALL LAUNCH
    //public class CompArmorRocket : ThingComp
    //{
    //    public Apparel targetBracelet;
    //    Pawn target;
    //    String displayTarget;

    //    public void linkTargetBracelet(Thing targetBracelet)
    //    {
    //        this.targetBracelet = (Apparel)targetBracelet;
    //        target = this.targetBracelet.Wearer;
    //    }
    //    public void notifyTargetChanged()
    //    {

    //    }
    //    public void launchArmor(Verse.Pawn pawn)
    //    {
    //        ThingDef b = DefDatabase<ThingDef>.AllDefsListForReading.Find(c => c.defName == "ArmorRocketDef");

    //        ArmorProjectile launchThis = (ArmorProjectile)GenSpawn.Spawn(b, this.parent.Position, this.parent.Map);
            
    //        LocalTargetInfo d = new LocalTargetInfo(target);
    //        launchThis.Launch(this.parent, parent.DrawPos, this.target.Position, d, new ProjectileHitFlags());
    //        Verse.Log.Warning("Armor \"Lauched\".");
    //        Verse.Log.Warning(this.parent.ToString());
    //    }
    //    public override void PostExposeData()
    //    {
    //        base.PostExposeData();
    //        Scribe_References.Look(ref targetBracelet, "connectedBracelet");
    //        Scribe_References.Look(ref target, "connectedPawn");
    //        Scribe_Values.Look(ref displayTarget, "displayString");
    //    }
    //}
    //public class CompProperties_ArmorRocket : CompProperties
    //{
    //    public CompProperties_ArmorRocket() 
    //    {
    //        compClass = typeof(CompArmorRocket); 
    //    }
    //}
    REDACTED USE FOR REFERENCE TO CALL LAUNCH************************/
