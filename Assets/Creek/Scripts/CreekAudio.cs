using UnityEngine;

namespace RiffleCreek
{
    public sealed class CreekAudio : MonoBehaviour
    {
        AudioSource ambient, wash, notes;
        AudioClip stone, gold, ready, milestone, scoop;
        AudioClip pending;
        float pendingVolume;
        float lastStone=-10, lastMovement=-10;
        public bool Muted {get;private set;}
        public void Initialize()
        {
            ambient=Source("Quiet low water",Clip("Creek",SoftAudioSamples.Water(10,712,.08f)),true,.18f);
            wash=Source("Soft water movement",Clip("Water",SoftAudioSamples.Water(8,715,.10f)),true,0);
            notes=Source("One gentle feedback voice",null,false,.3f);
            stone=Clip("Low rounded stone",SoftAudioSamples.Clack());
            gold=Clip("Gold collected",SoftAudioSamples.Note(.8f,440,.20f,true));
            ready=Clip("Gold ready",SoftAudioSamples.Note(.55f,523.25f,.12f));
            milestone=Clip("Sluice wake",SoftAudioSamples.Note(1.3f,349.23f,.20f,true));
            scoop=Clip("Scoop water",SoftAudioSamples.Water(.65f,721,.12f));
        }
        AudioSource Source(string title,AudioClip clip,bool loop,float volume)
        {
            var go=new GameObject(title);go.transform.SetParent(transform,false);
            var s=go.AddComponent<AudioSource>();s.clip=clip;s.loop=loop;s.volume=volume;s.spatialBlend=0;s.playOnAwake=false;
            if(loop)s.Play();return s;
        }
        static AudioClip Clip(string name,float[] data)
        {
            var clip=AudioClip.Create(name,data.Length,1,SoftAudioSamples.Rate,false);clip.SetData(data,0);return clip;
        }
        public void Tick(PanIntent intent,bool loaded,float dt)
        {
            if(pending && !notes.isPlaying) {var clip=pending;pending=null;Play(clip,pendingVolume);}
            float target=loaded?intent.Wash*.26f+intent.Work*.035f:0;
            wash.volume=Mathf.MoveTowards(wash.volume,target,dt*.35f);
            // No continuous gravel scrape. Infrequent rounded clacks suggest a shifting load.
            if(loaded && intent.Work>0 && Time.unscaledTime-lastMovement>1.8f)
            {
                lastMovement=Time.unscaledTime;Stone(.30f);
            }
        }
        void Play(AudioClip clip,float volume,bool priority=false)
        {
            if(Muted)return;
            // Only one effects voice, even when several stones leave on the same frame.
            // Let the current envelope finish; cutting a waveform mid-cycle can click.
            if(notes.isPlaying) {if(priority) {pending=clip;pendingVolume=volume;}return;}
            notes.clip=clip;notes.volume=volume;notes.Play();
        }
        void Stone(float volume)
        {
            if(Time.unscaledTime-lastStone<.32f || notes.isPlaying || pending)return;
            lastStone=Time.unscaledTime;Play(stone,volume);
        }
        public void Stone() => Stone(.45f);
        public void Scoop() => Play(scoop,.3f);
        public void Ready() => Play(ready,.35f,true);
        public void Gold(bool unlock) => Play(unlock?milestone:gold,.4f,true);
        public void Upgrade() => Play(gold,.25f);
        public void ToggleMute() {Muted=!Muted;if(Muted)pending=null;AudioListener.volume=Muted?0:1;}
        void OnDestroy()
        {
            if(ambient&&ambient.clip)Destroy(ambient.clip);if(wash&&wash.clip)Destroy(wash.clip);
            foreach(var clip in new[]{stone,gold,ready,milestone,scoop})if(clip)Destroy(clip);
        }
    }
}
