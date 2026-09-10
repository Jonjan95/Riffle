using System;

namespace RiffleCreek
{
    [Serializable]
    public sealed class Progression
    {
        public int Gold { get; private set; }
        public int LifetimeGold { get; private set; }
        public int PanLevel { get; private set; }
        public int RiffleLevel { get; private set; }
        public int ScoopLevel { get; private set; }
        public int CollectedPans { get; private set; }
        public const int Milestone = 40;
        public const int MaxLevel = 3;
        int unlocked, enabled;
        static readonly int[][] costs = { new[] {8,60,140}, new[] {10,50,120}, new[] {12,90,200} };
        static readonly int[] assistCosts = {80,220,190};
        public static readonly string[] UpgradeNames = {"Better pan","Better riffles","Richer scoop"};
        public static readonly string[] AssistNames = {"Auto Work","Auto Wash","Auto Collect"};
        public bool SluiceActive => LifetimeGold >= Milestone;
        public bool FullyAssisted => HasAutomation(2);
        public int Level(int upgrade) => upgrade == 0 ? PanLevel : upgrade == 1 ? RiffleLevel : upgrade == 2 ? ScoopLevel : 0;
        public int Cost(int upgrade) => upgrade >= 0 && upgrade < 3 && Level(upgrade) < MaxLevel ? costs[upgrade][Level(upgrade)] : 0;
        public bool CanBuy(int upgrade) => upgrade >= 0 && upgrade < 3 && Level(upgrade) < MaxLevel && Gold >= Cost(upgrade);
        public bool Buy(int upgrade)
        {
            if (!CanBuy(upgrade)) return false;
            Gold -= Cost(upgrade);
            if (upgrade == 0) PanLevel++; else if (upgrade == 1) RiffleLevel++; else ScoopLevel++;
            return true;
        }
        public bool Credit(int amount)
        {
            if (amount <= 0 || Gold > int.MaxValue - amount || LifetimeGold > int.MaxValue - amount) return false;
            bool before = SluiceActive;
            Gold += amount; LifetimeGold += amount;
            return !before && SluiceActive;
        }
        public void RecordCollection() { CollectedPans = Math.Min(CollectedPans + 1, 1000000); }
        public bool HasAutomation(int kind) => kind >= 0 && kind < 3 && (unlocked & (1 << kind)) != 0;
        public bool AutomationOn(int kind) => HasAutomation(kind) && (enabled & (1 << kind)) != 0;
        public int AutomationCost(int kind) => kind >= 0 && kind < 3 ? assistCosts[kind] : 0;
        public bool AutomationAvailable(int kind)
        {
            if (kind < 0 || kind > 2 || HasAutomation(kind)) return false;
            int tools = kind + 1;
            return PanLevel >= tools && RiffleLevel >= tools && ScoopLevel >= tools && (kind == 0 || HasAutomation(kind - 1));
        }
        public string AutomationRequirement(int kind) => kind == 0 ? "All tools at level 1" : kind == 1 ? "Auto Work + all tools at level 2" : "Auto Wash + all tools at level 3";
        public bool BuyAutomation(int kind)
        {
            if (!AutomationAvailable(kind) || Gold < AutomationCost(kind)) return false;
            Gold -= AutomationCost(kind); unlocked |= 1 << kind; enabled |= 1 << kind;
            return true;
        }
        public bool ToggleAutomation(int kind)
        {
            if (!HasAutomation(kind)) return false;
            enabled ^= 1 << kind;
            return true;
        }
        public int NextAutomation => !HasAutomation(0) ? 0 : !HasAutomation(1) ? 1 : !HasAutomation(2) ? 2 : -1;
        // A recommendation, not a purchase restriction: retain player choice within the current tool tier.
        public int RecommendedUpgrade
        {
            get
            {
                int tier = NextAutomation < 0 ? 3 : NextAutomation + 1, best = -1;
                for (int i = 0; i < 3; i++)
                    if (Level(i) < tier && (best < 0 || Cost(i) < Cost(best))) best = i;
                return best;
            }
        }
        public string NextGoal
        {
            get
            {
                int upgrade = RecommendedUpgrade, assist = NextAutomation;
                if (upgrade >= 0) return UpgradeNames[upgrade] + " " + (Level(upgrade) + 1) + "  /  " + Cost(upgrade) + " gold";
                return assist >= 0 ? AssistNames[assist] + "  /  " + AutomationCost(assist) + " gold" : "Alder Creek is in good hands";
            }
        }
        public float GoalProgress
        {
            get
            {
                int upgrade = RecommendedUpgrade, assist = NextAutomation;
                int price = upgrade >= 0 ? Cost(upgrade) : assist >= 0 ? AutomationCost(assist) : 1;
                return FullyAssisted ? 1 : Math.Min(1, (float)Gold / price);
            }
        }
        public ProgressSave CaptureSave() => new ProgressSave {
            Gold=Gold, LifetimeGold=LifetimeGold, Pan=PanLevel, Riffles=RiffleLevel, Scoop=ScoopLevel,
            Unlocked=unlocked, Enabled=enabled, CollectedPans=CollectedPans
        };
        public static Progression Restore(ProgressSave data)
        {
            if (data == null || !data.Valid) throw new ArgumentException("Invalid Riffle progression save.");
            return new Progression {Gold=data.Gold, LifetimeGold=data.LifetimeGold, PanLevel=data.Pan,
                RiffleLevel=data.Riffles, ScoopLevel=data.Scoop, unlocked=data.Unlocked, enabled=data.Enabled, CollectedPans=data.CollectedPans};
        }
    }
}
