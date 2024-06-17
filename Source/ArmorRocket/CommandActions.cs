using RimWorld;
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
        public SelectArmor_Command(Thing t)
        {
            icon = ContentFinder<Texture2D>.Get("UI/Cursors/CursorCustom");//maybe make custom text
            defaultLabel = "Assign Armor";
            defaultDesc = "Assign an Apparel Piece to this ArmorStand";
            activateSound = SoundDefOf.Click;
            rocket = t as ArmorRocketThing;
            action = delegate
            {
                //insert
                //need to add something called a designator class 
            };
        }
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            Verse.Log.Warning("Assign Armor Succesful?");
        }
    }
}
