using UnityEngine;

namespace RiffleCreek
{
    // Slow, small motion for authored foliage and bank ripples. Gameplay never reads this component.
    public sealed class CreekEnvironmentMotion : MonoBehaviour
    {
        public Transform[] treeCrowns;
        public Transform[] foregroundGrass;
        public Transform[] bankRipples;
        Quaternion[] crownRest, grassRest;
        Vector3[] rippleRest;
        public Transform MotionProbe => treeCrowns!=null&&treeCrowns.Length>0?treeCrowns[0]:null;

        void Awake()
        {
            treeCrowns=treeCrowns??System.Array.Empty<Transform>();
            foregroundGrass=foregroundGrass??System.Array.Empty<Transform>();
            bankRipples=bankRipples??System.Array.Empty<Transform>();
            crownRest=new Quaternion[treeCrowns.Length];
            grassRest=new Quaternion[foregroundGrass.Length];
            rippleRest=new Vector3[bankRipples.Length];
            for(int i=0;i<treeCrowns.Length;i++)crownRest[i]=treeCrowns[i].localRotation;
            for(int i=0;i<foregroundGrass.Length;i++)grassRest[i]=foregroundGrass[i].localRotation;
            for(int i=0;i<bankRipples.Length;i++)rippleRest[i]=bankRipples[i].localScale;
        }

        void Update()
        {
            float t=Time.time;
            for(int i=0;i<treeCrowns.Length;i++)
                treeCrowns[i].localRotation=crownRest[i]*Quaternion.Euler(0,0,Mathf.Sin(t*.48f+i*1.7f)*1.15f);
            for(int i=0;i<foregroundGrass.Length;i++)
                foregroundGrass[i].localRotation=grassRest[i]*Quaternion.Euler(Mathf.Sin(t*.85f+i)*2.2f,0,Mathf.Cos(t*.63f+i*1.4f)*2.8f);
            for(int i=0;i<bankRipples.Length;i++)
            {
                float pulse=1+Mathf.Sin(t*.55f+i*1.3f)*.07f;
                bankRipples[i].localScale=new Vector3(rippleRest[i].x*pulse,rippleRest[i].y,rippleRest[i].z);
            }
        }
    }
}
