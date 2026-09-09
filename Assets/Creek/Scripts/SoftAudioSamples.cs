using System;

namespace RiffleCreek
{
    // Pure generation lets checks measure peaks, edges, and high-frequency energy before playback.
    public static class SoftAudioSamples
    {
        public const int Rate=44100;
        public static float[] Water(float seconds,int seed,float peak)
        {
            var samples=new float[(int)(seconds*Rate)];var random=new Random(seed);
            double a=1-Math.Exp(-2*Math.PI*240/Rate), b=1-Math.Exp(-2*Math.PI*180/Rate), low=0, softer=0;
            for(int i=0;i<samples.Length;i++)
            {
                low+=a*(random.NextDouble()*2-1-low);softer+=b*(low-softer);
                double t=(double)i/Rate;
                double envelope=.78+.12*Math.Sin(t*2*Math.PI*.4)+.10*Math.Sin(t*2*Math.PI*.7);
                samples[i]=(float)(softer*envelope);
            }
            float maximum=.001f;foreach(float s in samples)maximum=Math.Max(maximum,Math.Abs(s));
            for(int i=0;i<samples.Length;i++)samples[i]=samples[i]/maximum*peak*Edge(i,samples.Length,.35f);
            return samples;
        }
        public static float[] Note(float seconds,float frequency,float peak,bool chord=false)
        {
            var samples=new float[(int)(seconds*Rate)];
            for(int i=0;i<samples.Length;i++)
            {
                double t=(double)i/Rate;
                double envelope=(1-Math.Exp(-t/.035))*Math.Exp(-t*5/seconds);
                double wave=Math.Sin(t*frequency*Math.PI*2);
                if(chord)wave=.68*wave+.20*Math.Sin(t*frequency*1.25*Math.PI*2)+.12*Math.Sin(t*frequency*1.5*Math.PI*2);
                samples[i]=(float)(wave*envelope*peak)*Edge(i,samples.Length,.04f);
            }
            return samples;
        }
        public static float[] Clack()
        {
            const float seconds=.19f;var samples=new float[(int)(seconds*Rate)];
            for(int i=0;i<samples.Length;i++)
            {
                double t=(double)i/Rate;
                double envelope=(1-Math.Exp(-t/.006))*Math.Exp(-t*32);
                samples[i]=(float)((Math.Sin(t*185*Math.PI*2)*.8+Math.Sin(t*310*Math.PI*2)*.2)*envelope*.12)*Edge(i,samples.Length,.012f);
            }
            return samples;
        }
        static float Edge(int i,int count,float seconds)
        {
            float x=Math.Min(1,Math.Min(i,(count-1)-i)/(Rate*seconds));
            return x*x*(3-2*x);
        }
    }
}
