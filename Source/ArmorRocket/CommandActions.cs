using ArmorRocket.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using ArmorRacks.Jobs;
using Verse.AI;

namespace ArmorRocket
{
    public class Launch_Command : Command
    {
        CompArmorRocket rocket;
        public Launch_Command(Thing t)
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");//maybe make custom text
            defaultLabel = "Launch Armor";
            defaultDesc = "Launch Armor";
            activateSound = SoundDefOf.Click;
            rocket = t.TryGetComp<CompArmorRocket>();
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            rocket.launchArmor();
            //need to call the launch command here
        }
    }
    public class LaunchPawn_Command : Command //this will be assigned to pawns with the link bracelet on
    {
        CompTargetBracelet bracelet;
        public LaunchPawn_Command(CompTargetBracelet bracelet)
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");//maybe make custom text
            defaultLabel = "Launch Armor";
            defaultDesc = "Armor Launches and Homes in On Pawn to Attach";
            activateSound = SoundDefOf.Click;
            this.bracelet = bracelet;
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            bracelet.notifyArmorRocket();
        }
    }
    public class ReturnArmor_Command : Command
    {
        CompTargetBracelet bracelet;
        public ReturnArmor_Command(CompTargetBracelet bracelet)
        {
            //icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");//maybe make custom text
            defaultLabel = "Return Armor";
            defaultDesc = "Return Launched Armor to Stand";
            activateSound = SoundDefOf.Click;
            this.bracelet = bracelet;
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            Apparel brac = (Apparel)bracelet.parent;
            Job inputAssigned = JobMaker.MakeJob((JobDef)ArmorRocket.Defs.ArmorRocketJobDefOf.ArmorRocket_ReturnAssigned, bracelet.armorRocket.parent, brac);
            brac.Wearer.jobs.TryTakeOrderedJob(inputAssigned);
        }
    }
    public class SelectArmor_Command : Command_Action
    {
        ArmorRocketThing rocket;
        Designator_AssignArmor assigner;
        public SelectArmor_Command(Thing t)
        {
            icon = ContentFinder<Texture2D>.Get("UI/Cursors/CursorCustom");//maybe make custom text
            defaultLabel = "Assign Armor";
            defaultDesc = "Assign an Apparel Piece to this ArmorStand";
            activateSound = SoundDefOf.Click;
            rocket = t as ArmorRocketThing;
            assigner = new Designator_AssignArmor(rocket);
            action = delegate
            {
                //insert
                //need to add something called a designator class 
                Find.DesignatorManager.Select(assigner);
            };
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);


        }
    }
    public class Designator_AssignArmor : Designator
    {
        public override int DraggableDimensions => 0;
        private ArmorRocketThing rocket;
        private Vector2 PlaceMouseAttachmentDrawOffset = new Vector2(25f, 45f);
        public Designator_AssignArmor(ArmorRocketThing t)
        {
            defaultLabel = "Assign Armor";//give better descriptions and textures later
            defaultDesc = "DesignatorClaimDesc";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/ForbidOff");
            useMouseIcon = true;
            hotKey = KeyBindingDefOf.Misc4;
            showReverseDesignatorDisabledReason = true;
            rocket = t;
        }
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            Verse.Log.Warning("Hello");
            if (!c.InBounds(base.Map) || c.Fogged(base.Map))
            {
                return false;
            }
            Thing assinger = getThing(c);
            if (assinger == null) 
                return "Must Designate a Single Weapon or Apparel Piece";
            if (!CanDesignateThing(assinger).Accepted)
            {
                return "Must Designate a Weapon or Apparel Piece";
            }
            return true;
        }
        public override void DesignateSingleCell(IntVec3 c)
        {
            List<Thing> cellThings = c.GetThingList(base.Map);
            Thing assinger = cellThings.Find(t =>
            {
                return t.def.IsApparel || t.def.IsWeapon;
            });
            DesignateThing(assinger);
            return;
        }
        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            // i dont know I dont htink a check is needed?
            return true;
        }
        public override void DesignateThing(Thing t)
        {
            t.SetForbidden(value: false, true);
            rocket.InnerContainer.assignAddRemove(t);
        }
        public override void DrawMouseAttachments()
        {
            base.DrawMouseAttachments();
            Vector2 vector = Event.current.mousePosition;
            Rect rect = new Rect(vector.x + PlaceMouseAttachmentDrawOffset.x, vector.y + PlaceMouseAttachmentDrawOffset.y, 300f, 50f);

            Thing target = getThing(UI.MouseCell());
            if(target != null)
            {
                Thing overwrite = rocket.InnerContainer.willOverlap(target);

                string text = "Assign " + target.LabelNoParenthesisCap;

                if (overwrite != null)
                {
                    text += "\nRemove " + overwrite.LabelNoParenthesisCap;
                }
                Text.Font = GameFont.Small;
                GUIContent gUI = new GUIContent(text);
                Widgets.Label(rect, gUI);
            }
        }
        private Thing getThing(IntVec3 c)
        {
            List<Thing> cellThings = null;
            if (c.InBounds(base.Map))
            {
                cellThings = c.GetThingList(base.Map);
            }

            Thing assinger = null;
            if (cellThings != null)
            {
                
                int i = 0;

                foreach (Thing thing in cellThings)
                {
                    if (thing.def.IsApparel || thing.def.IsWeapon)
                    {
                        ++i;
                        assinger = thing;
                    }
                }
                if(i > 1)
                {
                    return null;
                }
            }
            return assinger;
        }

    }
}
