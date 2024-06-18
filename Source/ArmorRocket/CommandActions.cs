﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ArmorRocket
{
    public class Launch_Command : Command
    {
        ArmorRocketThing rocket;
        public Launch_Command(Thing t)
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");//maybe make custom text
            defaultLabel = "Launch Armor";
            defaultDesc = "Launch Armor";
            activateSound = SoundDefOf.Click;
            rocket = t as ArmorRocketThing;
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            Verse.Log.Warning("Launch Succesful?");
            //need to call the launch command here
        }
    }
    public class LaunchPawn_Command : Command //this will be assigned to pawns with the link bracelet on
    {
        Pawn pawn;
        public LaunchPawn_Command()
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");//maybe make custom text
            defaultLabel = "Launch Armor";
            defaultDesc = "Fire the Armor Stand at this Pawn";
            activateSound = SoundDefOf.Click;
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            Verse.Log.Warning("Launch Succesful?");
            //need to call the launch command here
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
            if (!c.InBounds(base.Map) || c.Fogged(base.Map))
            {
                return false;
            }
            List<Thing> cellThings = c.GetThingList(base.Map);
            Thing assinger = null;
            int i = 0;
            foreach(Thing thing in cellThings)
            {
                if (thing.def.IsApparel || thing.def.IsWeapon)
                {
                    ++i;
                    assinger = thing;
                }
            }
            if (i != 1) 
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
            rocket.InnerContainer.fakeAddRemove(t);
            Verse.Log.Warning("Thing added and blocking thing removed");
        }

    }
}
