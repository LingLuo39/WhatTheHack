﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Needs
{
    class Need_Maintenance : Need
    {
        private float lastLevel = 0;

        public Need_Maintenance(Pawn pawn) : base(pawn)
        {
        }

        public override void SetInitialLevel()
        {
            base.CurLevelPercentage = 1.0f;
            lastLevel = 1.0f;
        }

        private float FallPerTick()
        {
            if (pawn.health != null && pawn.health.hediffSet.HasNaturallyHealingInjury()) //damaged pawns detoriate quicker
            {
                return MaxLevel / (GenDate.TicksPerDay * 4f);
            }
            return MaxLevel / (GenDate.TicksPerDay * 6f);
        }

        public override void NeedInterval()
        {
            if (!base.IsFrozen)
            {
                this.lastLevel = CurLevel;
                this.CurLevel -= FallPerTick() * 150f;
            }
            SetHediffs();
        }
        public void SetHediffs()
        {
            TrySetHediff(MaintenanceCategory.LowMaintenance, WTH_DefOf.WTH_LowMaintenance);
            TrySetHediff(MaintenanceCategory.VeryLowMaintenance, WTH_DefOf.WTH_VeryLowMaintenance);
        }
        private void TrySetHediff(MaintenanceCategory cat, HediffDef hediffDef)
        {
            if (CurCategory != cat && pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef, false) is Hediff hediff)
            {
                pawn.health.RemoveHediff(hediff);
            }
            if (CurCategory == cat && !pawn.health.hediffSet.HasHediff(hediffDef))
            {
                Hediff addedHeddif = HediffMaker.MakeHediff(hediffDef, pawn);
                pawn.health.AddHediff(addedHeddif);
            }

        }

        public float PercentageThreshVeryLowMaintenance
        {
            get
            {
                return 0.1f;//TODO

            }
        }

        public float PercentageThreshLowMaintenance
        {
            get
            {
                return 0.2f;//TODO
            }
        }

        public MaintenanceCategory CurCategory
        {
            get
            {
                if (base.CurLevelPercentage < this.PercentageThreshVeryLowMaintenance)
                {
                    return MaintenanceCategory.VeryLowMaintenance;
                }
                if (base.CurLevelPercentage < this.PercentageThreshLowMaintenance)
                {
                    return MaintenanceCategory.LowMaintenance;
                }
                return MaintenanceCategory.EnoughMaintenance;
            }
        }

        public override int GUIChangeArrow
        {
            get
            {
                if (CurLevel > lastLevel)
                {
                    return 1;
                }
                else if (CurLevel < lastLevel)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
        public override float MaxLevel
        {
            get
            {
                return pawn.BodySize * 100;//TODO
            }
        }
        public float MaintenanceWanted
        {
            get
            {
                return this.MaxLevel - this.CurLevel;
            }
        }


        public override string GetTipString()
        {
            return string.Concat(new string[]
            {
                base.LabelCap,
                ": ",
                base.CurLevelPercentage.ToStringPercent(),
                " (",
                this.CurLevel.ToString("0.##"),
                " / ",
                this.MaxLevel.ToString("0.##"),
                ")\n",
                this.def.description
            });
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
        {
            if (this.threshPercents == null)
            {
                this.threshPercents = new List<float>();
            }
            this.threshPercents.Clear();
            this.threshPercents.Add(this.PercentageThreshLowMaintenance);
            this.threshPercents.Add(this.PercentageThreshVeryLowMaintenance);
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref lastLevel, "lastLevel");
        }

    }
}