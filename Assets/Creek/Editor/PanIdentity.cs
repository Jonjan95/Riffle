using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek.Editor
{
    // Authored cross sections and color regions, baked into a saved mesh asset. No runtime bowl generation.
    public static class PanIdentity
    {
        const string Path="Assets/Creek/Generated/PanIdentity.asset";
        static Color C(string hex) {ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        static Transform Group(string name,Transform p) {var t=new GameObject(name).transform;t.SetParent(p,false);return t;}
        static Mesh Surface(string name,Vector2[] profile,Color[] colors,int segments=80,float start=0,float arc=360,bool raisedBack=false)
        {
            var mesh=Geometry.Lathe(name,profile,segments,start,arc);var v=mesh.vertices;var tint=new Color[v.Length];
            for(int s=0;s<=segments;s++)for(int r=0;r<profile.Length;r++)
            {
                int i=s*profile.Length+r;float a=(start+arc*s/segments)*Mathf.Deg2Rad;
                if(raisedBack)v[i].y+=(1+Mathf.Cos(a))*.055f*Mathf.Clamp01((profile[r].x-1.94f)/.94f);
                tint[i]=colors[Mathf.Min(r,colors.Length-1)];
            }
            mesh.vertices=v;mesh.colors=tint;mesh.RecalculateNormals();
            // Weld the shading at a full circular seam, while retaining a clean authored UV-free mesh.
            if(arc==360)
            {
                var n=mesh.normals;
                for(int i=0;i<profile.Length;i++) {int last=segments*profile.Length+i;var avg=(n[i]+n[last]).normalized;n[i]=n[last]=avg;}
                mesh.normals=n;
            }
            mesh.RecalculateBounds();return mesh;
        }
        static Mesh Flake()
        {
            float[] radius={1,.73f,.92f,1.08f,.70f,.88f,.76f,1.03f,.64f};
            var v=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
            for(int i=0;i<radius.Length;i++)
            {
                float a=i*Mathf.PI*2/radius.Length,b=(i+1)*Mathf.PI*2/radius.Length;
                int n=v.Count;v.Add(new Vector3(.12f,.48f,-.10f));v.Add(new Vector3(Mathf.Sin(b)*radius[(i+1)%radius.Length],0,Mathf.Cos(b)*radius[(i+1)%radius.Length]));v.Add(new Vector3(Mathf.Sin(a)*radius[i],0,Mathf.Cos(a)*radius[i]));
                triangles.AddRange(new[]{n,n+2,n+1});float shade=i%3==0?.84f:1;for(int k=0;k<3;k++)colors.Add(new Color(shade,shade,shade));
            }
            var m=Geometry.Make("Uneven folded gold flake",v.ToArray(),triangles.ToArray());m.colors=colors.ToArray();return m;
        }
        [MenuItem("Riffle/Author signature pan and build")]
        public static void ApplyAndBuild()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Creek/Scenes/AlderCreek.unity");var pan=UnityEngine.Object.FindAnyObjectByType<PanView>();
            var art=AssetDatabase.LoadAssetAtPath<ArtLibrary>(Path);
            foreach(var name in new[]{"Bowl geometry","Enamel and brass finishing","Front working riffles • 108 degree sector"})
            {var old=pan.transform.Find(name);if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);}
            if(art) {foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(Path))if(obj!=art)UnityEngine.Object.DestroyImmediate(obj,true);}
            else {art=ScriptableObject.CreateInstance<ArtLibrary>();AssetDatabase.CreateAsset(art,Path);}
            var shader=Shader.Find("Creek/PanSurface");if(!shader)throw new Exception("Pan surface shader missing");
            Material Mat(string name,string color,float sheen=0)
            {var m=Geometry.Mat(name,color,shader);m.SetFloat("_Sheen",sheen);m.SetFloat("_ReceiveShadows",.08f);return m;}
            var enamel=Mat("Riffle signature enamel","#FFFFFF",.045f);
            var shell=Group("Bowl geometry",pan.transform);var details=Group("Enamel and brass finishing",pan.transform);
            void SurfaceObject(string n,Transform p,Vector2[] profile,string[] colors,Material m,bool back=false,int segments=80,float start=0,float arc=360)
            {var cs=Array.ConvertAll(colors,C);Geometry.MeshObject(n,p,Surface(n,profile,cs,segments,start,arc,back),m,Vector3.zero);}
            SurfaceObject("Deep enamel outer skirt",shell,new[]{new Vector2(0,-.14f),new Vector2(1.98f,-.14f),new Vector2(2.80f,.57f),new Vector2(2.98f,.75f),new Vector2(2.98f,.83f)},new[]{"#1D4049","#1D4049","#28565F","#326873","#417D80"},enamel,true);
            SurfaceObject("Sculpted enamel bowl",shell,new[]{new Vector2(2.88f,.83f),new Vector2(2.75f,.77f),new Vector2(2.52f,.58f),new Vector2(2.20f,.33f),new Vector2(1.94f,.17f),new Vector2(1.82f,.17f),new Vector2(0,.17f)},new[]{"#7BB7AC","#66A69E","#4D9290","#367B80","#326F75","#418487","#529591"},enamel,true);
            SurfaceObject("Plain enamel edge",shell,new[]{new Vector2(2.98f,.79f),new Vector2(2.98f,.85f),new Vector2(2.92f,.855f),new Vector2(2.88f,.83f)},new[]{"#326873","#417D80","#69A69E","#7BB7AC"},enamel,true);
            pan.riffleAccent=Group("Front working riffles • 108 degree sector",pan.transform);pan.riffleAccent.localRotation=Quaternion.Euler(0,180,0);
            for(int i=0;i<4;i++)
            {
                float r=2.05f+i*.18f,y=.17f+(r-1.94f)*.68f;
                SurfaceObject("Sculpted front riffle "+i,pan.riffleAccent,new[]{new Vector2(r+.07f,y+.035f),new Vector2(r+.012f,y+.077f),new Vector2(r-.040f,y+.052f),new Vector2(r-.035f,y-.012f)},new[]{"#447F7F","#76AAA0","#2C6168","#214C55"},enamel,false,48,-48+i*2,96-i*4);
            }
            pan.goldFlake=Flake();
            pan.grainMaterials[3]=Mat("Riffle flake gold","#FFD05A",.14f);
            pan.grainMaterials[0]=Mat("Riffle loose earth","#A98763");
            pan.grainMaterials[2]=Mat("Riffle mineral concentrate","#34464B");
            var silt=Surface("Layered scoop mound",new[]{new Vector2(2.08f,-.1f),new Vector2(2.02f,.06f),new Vector2(1.5f,.50f),new Vector2(.7f,.72f),new Vector2(0,.65f)},new[]{C("#9BA18E"),C("#BFC1AC"),C("#EBDEBF"),Color.white,C("#F1E5CA")},64);
            pan.sedimentBed.GetComponent<MeshFilter>().sharedMesh=silt;pan.sedimentBed.GetComponent<Renderer>().sharedMaterial=pan.grainMaterials[0];
            var pocket=Surface("Layered charcoal pocket",new[]{new Vector2(2.08f,0),new Vector2(1.82f,.13f),new Vector2(1.1f,.50f),new Vector2(0,.58f)},new[]{C("#899CA0"),C("#B1C1BE"),C("#D5DED2"),C("#D5DED2")},64);
            pan.blackBed.GetComponent<MeshFilter>().sharedMesh=pocket;pan.blackBed.GetComponent<Renderer>().sharedMaterial=pan.grainMaterials[2];
            // Shared non-gold pebble meshes have no authored color channel: retain their established matte shader.
            pan.grainMaterials[0]=Geometry.Mat("Warm silt grains","#B7956B",Shader.Find("Creek/Matte"));
            pan.grainMaterials[2]=Geometry.Mat("Fine charcoal grains","#34464B",Shader.Find("Creek/Matte"));
            void Save(UnityEngine.Object obj) {if(obj&&!AssetDatabase.Contains(obj))AssetDatabase.AddObjectToAsset(obj,art);}
            foreach(var root in new[]{shell,details,pan.riffleAccent})foreach(var r in root.GetComponentsInChildren<Renderer>(true))r.shadowCastingMode=ShadowCastingMode.Off;
            foreach(var f in pan.GetComponentsInChildren<MeshFilter>(true))Save(f.sharedMesh);
            foreach(var r in pan.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)Save(m);
            Save(pan.goldFlake);foreach(var m in pan.grainMaterials)Save(m);
            EditorUtility.SetDirty(pan);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Verify();PolishCreek.VerifyPresentation();EditorWorkflowChecks.VerifyStartupScene();BuildCreek.BuildCurrent();
        }
        public static void Verify()
        {
            var p=UnityEngine.Object.FindAnyObjectByType<PanView>();if(!p||!p.goldFlake)throw new Exception("Signature pan is incomplete");
            var bowl=p.transform.Find("Bowl geometry/Sculpted enamel bowl").GetComponent<MeshFilter>().sharedMesh;
            if(bowl.colors.Length!=bowl.vertexCount||bowl.bounds.size.y<.65f)throw new Exception("Bowl depth or authored colors are missing");
            if(bowl.normals[bowl.normals.Length-2].y<.9f)throw new Exception("Bowl floor must face upward");
            foreach(var f in p.riffleAccent.GetComponentsInChildren<MeshFilter>())
            {bool top=false;foreach(var n in f.sharedMesh.normals)top|=n.y>.5f;if(!top)throw new Exception("Riffle crown must face upward");}
            foreach(var n in p.goldFlake.normals)if(n.y<=0)throw new Exception("Gold flake faces away from the camera");
            File.WriteAllText("Playtest/pan-identity-checks.txt","PASS: Saved signature pan and dedicated gold mesh\nPASS: Authored bowl depth and vertex colors\nPASS: Bowl floor faces upward\nPASS: Every gold flake face points upward\nPASS: Riffle crowns have upward-facing surfaces\n");
        }
    }
}
