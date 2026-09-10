using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace RiffleCreek
{
    public sealed class ProgressSave
    {
        public int Gold, LifetimeGold, Pan, Riffles, Scoop, Unlocked, Enabled, CollectedPans;
        public bool Valid => Gold>=0 && LifetimeGold>=Gold && LifetimeGold<=100000000 &&
            Pan>=0 && Pan<=3 && Riffles>=0 && Riffles<=3 && Scoop>=0 && Scoop<=3 &&
            Unlocked>=0 && Unlocked<=7 && Enabled>=0 && (Enabled & ~Unlocked)==0 &&
            (Unlocked==0 || Unlocked==1 || Unlocked==3 || Unlocked==7) &&
            (Unlocked<1 || (Pan>=1 && Riffles>=1 && Scoop>=1)) &&
            (Unlocked<3 || (Pan>=2 && Riffles>=2 && Scoop>=2)) &&
            (Unlocked<7 || (Pan==3 && Riffles==3 && Scoop==3)) &&
            CollectedPans>=0 && CollectedPans<=1000000;
    }

    public sealed class SaveStore
    {
        public readonly string Path;
        public string Message { get; private set; }
        // Unreadable saves remain on disk rather than being silently overwritten by a fresh game.
        public bool WriteBlocked { get; private set; }
        static readonly string[] keys={"Gold","LifetimeGold","Pan","Riffles","Scoop","Unlocked","Enabled","CollectedPans"};
        public SaveStore(string path) { Path=path; }
        public Progression Load()
        {
            Message=null; WriteBlocked=false;
            if (!File.Exists(Path) && !File.Exists(Path+".bak")) return new Progression();
            Progression result;
            if (TryRead(Path,out result)) return result;
            if (TryRead(Path+".bak",out result)) { Message="Recovered your previous save."; return result; }
            WriteBlocked=true; Message="Save could not be read. Original kept; reset from Pause to start again.";
            return new Progression();
        }
        static bool TryRead(string path,out Progression result)
        {
            result=null;
            try
            {
                if (!File.Exists(path)) return false;
                result=Decode(File.ReadAllText(path));
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is FormatException || ex is OverflowException)
            { return false; }
        }
        public static string Encode(Progression progress)
        {
            var d=progress.CaptureSave();
            var values=new[]{d.Gold,d.LifetimeGold,d.Pan,d.Riffles,d.Scoop,d.Unlocked,d.Enabled,d.CollectedPans};
            var text=new StringBuilder("Riffle save v1\n");
            for(int i=0;i<keys.Length;i++)text.Append(keys[i]).Append('=').Append(values[i].ToString(CultureInfo.InvariantCulture)).Append('\n');
            return text.ToString();
        }
        public static Progression Decode(string text)
        {
            string[] lines=text.Replace("\r","").TrimEnd('\n').Split('\n');
            if(lines.Length!=9 || lines[0]!="Riffle save v1")throw new FormatException("Unsupported save format.");
            int[] n=new int[8];
            for(int i=0;i<n.Length;i++)
            {
                string prefix=keys[i]+"=";
                if(!lines[i+1].StartsWith(prefix,StringComparison.Ordinal) ||
                    !int.TryParse(lines[i+1].Substring(prefix.Length),NumberStyles.None,CultureInfo.InvariantCulture,out n[i]))
                    throw new FormatException("Invalid save field.");
            }
            return Progression.Restore(new ProgressSave {Gold=n[0],LifetimeGold=n[1],Pan=n[2],Riffles=n[3],Scoop=n[4],Unlocked=n[5],Enabled=n[6],CollectedPans=n[7]});
        }
        public bool Save(Progression progress)
        {
            if(WriteBlocked)return false;
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                File.WriteAllText(Path+".tmp",Encode(progress));
                if(File.Exists(Path))File.Replace(Path+".tmp",Path,Path+".bak");
                else File.Move(Path+".tmp",Path);
                Message=null;
                return true;
            }
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
            { Message="Could not save progress. Check available disk space and folder access."; return false; }
        }
        public bool Reset()
        {
            try
            {
                foreach(var suffix in new[]{".tmp",".bak",""})if(File.Exists(Path+suffix))File.Delete(Path+suffix);
                WriteBlocked=false;Message=null;
                return true;
            }
            catch(Exception ex) when(ex is IOException || ex is UnauthorizedAccessException)
            { Message="Could not reset the save. Progress has been kept."; return false; }
        }
    }
}
