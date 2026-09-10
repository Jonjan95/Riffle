using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace RiffleCreek
{
    // Opt-in visual smoke test with the same isolated save mechanism as other verification modes.
    public sealed class TentPlaytest : MonoBehaviour
    {
        IEnumerator Start()
        {
            var game=GetComponent<CreekGame>();var report=new StringBuilder();bool failed=false;
            void Check(bool valid,string label){report.AppendLine((valid?"PASS: ":"FAIL: ")+label);failed|=!valid;}
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Playtest/Tent"));
            Directory.CreateDirectory(folder);
            yield return new WaitForSeconds(.6f);
            var tent=game.diorama.transform.Find("Blender prospecting tent");
            Check(tent&&tent.gameObject.activeInHierarchy,"Imported Blender tent is active");
            bool valid=tent!=null;
            if(tent)foreach(var r in tent.GetComponentsInChildren<MeshRenderer>())
                foreach(var m in r.sharedMaterials)valid&=m&&m.shader&&m.shader.isSupported;
            Check(valid,"Tent materials render with supported shaders");
            Check(!game.diorama.transform.Find("Static camp and riverbank/Ochre A-frame tent").gameObject.activeSelf,
                "Legacy tent source is hidden after removal from batches");
            for(int i=0;i<45;i++){game.TickActions(new PanIntent{Work=1},1f/60);yield return null;}
            for(int i=0;i<45;i++){game.TickActions(new PanIntent{Wash=1},1f/60);yield return null;}
            Check(game.Simulation.Loaded&&game.Intent.Wash==1,"Playable Work and Wash still respond");
            game.TickActions(default,.01f);
            yield return Capture(Path.Combine(folder,"01-tent-in-game.png"));
            GetComponent<CreekUI>().enabled=false;
            // Inspection-only camera, never saved into the scene or used in ordinary play.
            game.sceneCamera.transform.position=new Vector3(-4.3f,3.6f,-1.3f);
            game.sceneCamera.transform.LookAt(tent.position+Vector3.up*.7f);
            game.sceneCamera.orthographicSize=2.25f;
            yield return null;
            yield return Capture(Path.Combine(folder,"02-tent-detail.png"));
            File.WriteAllText(Path.Combine(folder,"runtime-tent-checks.txt"),report.ToString());
            Application.Quit(failed?2:0);
        }
        static IEnumerator Capture(string file)
        {
            yield return new WaitForEndOfFrame();
            var texture=ScreenCapture.CaptureScreenshotAsTexture();
            if(!texture)throw new System.Exception("Tent capture failed");
            File.WriteAllBytes(file,texture.EncodeToPNG());Destroy(texture);
            yield return null;
        }
    }
}
