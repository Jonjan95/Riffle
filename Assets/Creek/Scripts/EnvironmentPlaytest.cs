using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace RiffleCreek
{
    // Focused visual smoke check for the authored Alder Creek environment.
    public sealed class EnvironmentPlaytest : MonoBehaviour
    {
        readonly StringBuilder report=new StringBuilder();
        CreekGame game;
        bool failed;

        void Check(bool valid,string label)
        {
            report.AppendLine((valid?"PASS: ":"FAIL: ")+label);if(!valid)failed=true;
        }

        IEnumerator Start()
        {
            game=GetComponent<CreekGame>();game.VerificationDrive=true;
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Playtest/Environment"));
            Directory.CreateDirectory(folder);
            yield return new WaitForSeconds(.8f);
            var authored=game.diorama.transform.Find("Authored creek environment");
            var motion=authored?authored.GetComponent<CreekEnvironmentMotion>():null;
            Check(authored&&authored.gameObject.activeInHierarchy,"Authored Alder Creek environment is active in the player");
            Check(authored&&authored.Find("Layered creek banks")&&authored.Find("Alder tree groups"),"Bank transitions and authored tree groups are present");
            Check(authored&&authored.Find("Camp ground connections"),"Camp grounding shapes are present");
            bool supported=true;
            foreach(var renderer in game.diorama.GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)supported&=material&&material.shader&&material.shader.isSupported;
            Check(supported,"All environment renderer shaders are supported");
            yield return Capture(Path.Combine(folder,"01-player-composition.png"));

            Quaternion before=motion&&motion.MotionProbe?motion.MotionProbe.localRotation:Quaternion.identity;
            yield return new WaitForSeconds(.8f);
            Check(motion&&motion.MotionProbe&&Quaternion.Angle(before,motion.MotionProbe.localRotation)>.01f,"Authored foliage motion updates subtly at runtime");

            var ui=GetComponent<CreekUI>();if(ui)ui.enabled=false;
            game.pan.gameObject.SetActive(false);
            game.sceneCamera.orthographicSize=6.45f;
            game.sceneCamera.transform.LookAt(new Vector3(0,.25f,1.1f));
            yield return new WaitForSeconds(.2f);
            yield return Capture(Path.Combine(folder,"02-environment-wide.png"));
            Check(game.sceneCamera.WorldToViewportPoint(new Vector3(-6.7f,.7f,3.65f)).z>0,"Camp remains in front of the environment camera");
            File.WriteAllText(Path.Combine(folder,"runtime-environment-checks.txt"),report.ToString());
            yield return new WaitForSeconds(.15f);Application.Quit(failed?2:0);
        }

        static IEnumerator Capture(string path)
        {
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(path);yield return null;
        }
    }
}
