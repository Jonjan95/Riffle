using UnityEngine;

namespace RiffleCreek
{
    // Device-independent held actions. A future upgrade can supply these same two values.
    // Collection remains an explicit, guarded CreekGame.Collect() call.
    public struct PanIntent
    {
        public float Work, Wash;
        public bool Working => Work>0 || Wash>0;
    }

    public sealed class PanInput
    {
        Vector3 anchor;
        Transform currentPan;
        bool leftGrab,rightGrab,previousLeft,previousRight;

        public PanIntent Read(Camera camera,Transform pan,bool pointerBlocked,bool controlsBlocked)
        {
            return Sample(camera,pan,Input.mousePosition,Input.GetMouseButton(0),Input.GetMouseButton(1),Input.GetKey(KeyCode.Space),Application.isFocused,pointerBlocked,controlsBlocked);
        }

        public PanIntent Sample(Camera camera,Transform pan,Vector3 screenPosition,bool left,bool right,bool space,bool focused,bool pointerBlocked,bool controlsBlocked=false)
        {
            if(currentPan!=pan) {currentPan=pan;anchor=pan.position;}
            var ray=camera.ScreenPointToRay(screenPosition);
            var plane=new Plane(Vector3.up,anchor+Vector3.up*.35f);
            bool inside=false;
            if(plane.Raycast(ray,out float distance))
            {
                Vector3 local=ray.GetPoint(distance)-anchor;
                inside=new Vector2(local.x,local.z).sqrMagnitude<3.05f*3.05f;
            }
            return Map(left,right,space,focused,inside,pointerBlocked,controlsBlocked);
        }

        // Position is used only to grab the pan. There is no motion, speed, angle, or gesture scoring.
        public PanIntent Map(bool left,bool right,bool space,bool focused,bool inside,bool pointerBlocked,bool controlsBlocked=false)
        {
            if(!focused || controlsBlocked)
            {
                leftGrab=rightGrab=false;previousLeft=left;previousRight=right;return default;
            }
            if(!left)leftGrab=false;
            if(!right)rightGrab=false;
            if(left&&!previousLeft&&inside&&!pointerBlocked)leftGrab=true;
            if(right&&!previousRight&&inside&&!pointerBlocked)rightGrab=true;
            previousLeft=left;previousRight=right;
            // Once grabbed, holding continues even if the cursor drifts. Space works anywhere in the focused game.
            return new PanIntent {Work=leftGrab?1:0,Wash=rightGrab||space?1:0};
        }
    }
}
