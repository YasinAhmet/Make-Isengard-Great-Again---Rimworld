using KTrie;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TestMod
{
    public class CutTree : MapComponent
    {
        List<Pawn> allPawns;
        List<Plant> allTrees;

        JobDef cutPlantJobDef;
        public CutTree(Map map) : base(map)
        {
            Log.Message("Works!!!"); //Outputs "Hello World!" to the dev console.
            cutPlantJobDef = DefDatabase<JobDef>.GetNamed("CutPlant", true);
        }

        public override void MapGenerated()
        {
            base.MapGenerated();

            // This code will run when the map is fully generated and all its components
            // are initialized, so it's a good place to get a list of all pawns.
            allPawns = map.mapPawns.AllPawns.ToList();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // Your code to execute every tick goes here
            foreach (Pawn pawn in allPawns)
            {
                if (pawn.CurJob == null || pawn.CurJob.def == JobDefOf.Wait || pawn.CurJob.def == JobDefOf.GotoWander)
                {
                    WorkTrees();
                    Thing closestTree = GenClosest.ClosestThing_Global(pawn.Position, allTrees, 9999f, plant => plant is Plant);

                    if (closestTree != null)
                    {
                        Job job = new Job(JobDefOf.CutPlant, closestTree);


                        // Clear any current job the pawn is doing
                        pawn.jobs.StopAll();
                        Log.Message("Got cutting job for " + pawn.Name);
                        pawn.jobs.StartJob(job, JobCondition.None, null, false, true, null, null, false);
                    }
                }

            }
        }

        private void WorkTrees()
        {
            Map map = Find.CurrentMap;
            List<Thing> allThings = map.listerThings.AllThings;
            allTrees = new List<Plant>();
            allPawns = map.mapPawns.FreeColonists.ToList();
            allPawns = allPawns.Where(p => p != null && p.RaceProps.Humanlike).ToList();

            foreach (Thing thing in allThings)
            {
                if (thing.def.category == ThingCategory.Plant && thing.def.plant != null && thing.def.plant.IsTree)
                {

                    if (!IsPlantBeingCut(thing as Plant))
                    {
                        allTrees.Add(thing as Plant);
                    }
                }
            }
        }

        public bool IsPlantBeingCut(Plant plant)
        {
            foreach (Pawn pawn in allPawns)
            {
                if (pawn.jobs.curJob != null &&
                    pawn.jobs.curDriver.job.def == JobDefOf.CutPlant &&
                    pawn.jobs.curJob.targetA == plant)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
