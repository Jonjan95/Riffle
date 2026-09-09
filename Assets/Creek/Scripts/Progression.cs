using System;

namespace RiffleCreek
{
    [Serializable]
    public sealed class Progression
    {
        public int Gold {get;private set;}
        public int LifetimeGold {get;private set;}
        public int PanLevel {get;private set;}
        public int RiffleLevel {get;private set;}
        public int ScoopLevel {get;private set;}
        public const int Milestone=40;
        public const int MaxLevel=3;
        public bool SluiceActive => LifetimeGold>=Milestone;
        public int Level(int upgrade) => upgrade==0?PanLevel:upgrade==1?RiffleLevel:ScoopLevel;
        public int Cost(int upgrade) => (upgrade==0?8:upgrade==1?10:12)+Level(upgrade)*10;
        public bool CanBuy(int upgrade) => upgrade>=0 && upgrade<3 && Level(upgrade)<MaxLevel && Gold>=Cost(upgrade);
        public bool Buy(int upgrade)
        {
            if(!CanBuy(upgrade))return false;
            Gold-=Cost(upgrade);
            if(upgrade==0)PanLevel++;else if(upgrade==1)RiffleLevel++;else ScoopLevel++;
            return true;
        }
        public bool Credit(int amount)
        {
            if(amount<=0)return false;
            bool before=SluiceActive;
            Gold+=amount;LifetimeGold+=amount;
            return !before&&SluiceActive;
        }
    }
}
