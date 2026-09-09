using System.IO;
using UnityEngine;

namespace RiffleCreek
{
    // Opt-in local diagnostics for testing real OS input; excluded from the normal player flow.
    public sealed class InputProbe : MonoBehaviour
    {
        CreekGame game;
        int frames, focused, leftHeld, rightHeld, working;
        float maxWork, maxWash;
        void Start() {game=GetComponent<CreekGame>();}
        void LateUpdate()
        {
            frames++;if(Application.isFocused)focused++;if(Input.GetMouseButton(0))leftHeld++;if(Input.GetMouseButton(1))rightHeld++;
            if(game.Intent.Working)working++;
            maxWork=Mathf.Max(maxWork,game.Intent.Work);maxWash=Mathf.Max(maxWash,game.Intent.Wash);
        }
        void OnApplicationQuit()
        {
            string dir=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Playtest"));Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir,"input-checks.txt"),"Frames: "+frames+"\nFocused frames: "+focused+"\nLeft held frames: "+leftHeld+"\nRight held frames: "+rightHeld+"\nWorking frames: "+working+"\nMax work: "+maxWork+"\nMax wash: "+maxWash+"\nLooseness: "+game.Simulation.Looseness+"\nStratification: "+game.Simulation.Stratification+"\n");
        }
    }
}
