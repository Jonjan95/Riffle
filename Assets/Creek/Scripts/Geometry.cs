using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RiffleCreek
{
    public static class Geometry
    {
        public static Material Mat(string name, string hex, Shader shader)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return new Material(shader) { name = name, color = c };
        }

        public static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material mat, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        public static Transform Shape(string name, Transform parent, PrimitiveType type, Vector3 p, Vector3 scale, Material mat, Vector3 rotation = default)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = p;
            go.transform.localScale = scale;
            go.transform.localEulerAngles = rotation;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go.transform;
        }

        public static Transform Beam(string name, Transform parent, Vector3 a, Vector3 b, float width, Material mat)
        {
            var t = Shape(name, parent, PrimitiveType.Cylinder, (a+b)*.5f, new Vector3(width, Vector3.Distance(a,b)*.5f, width), mat);
            t.up = parent.TransformDirection((b-a).normalized);
            return t;
        }

        public static Mesh Lathe(string name, Vector2[] profile, int segments = 72, float start = 0, float arc = 360)
        {
            var v = new Vector3[(segments+1)*profile.Length];
            var tris = new List<int>();
            for (int s=0;s<=segments;s++)
            {
                float a=(start+arc*s/segments)*Mathf.Deg2Rad;
                for (int r=0;r<profile.Length;r++)
                {
                    v[s*profile.Length+r]=new Vector3(Mathf.Sin(a)*profile[r].x, profile[r].y, Mathf.Cos(a)*profile[r].x);
                    if(s==segments || r==profile.Length-1) continue;
                    int i=s*profile.Length+r, j=i+profile.Length;
                    tris.Add(i);tris.Add(j);tris.Add(i+1);
                    tris.Add(i+1);tris.Add(j);tris.Add(j+1);
                }
            }
            return Make(name,v,tris.ToArray());
        }

        public static Mesh Make(string name, Vector3[] verts, int[] triangles)
        {
            var m=new Mesh {name=name};
            m.vertices=verts; m.triangles=triangles; m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        public static Mesh Pebble()
        {
            // Flat facets make a tiny particle legible without texture noise.
            var v = new List<Vector3>(); var tr=new List<int>();
            Vector3 top=new Vector3(0,.65f,0), bottom=new Vector3(0,-.35f,0);
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7, b=(i+1)*Mathf.PI*2/7;
                Vector3 x=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a)), y=new Vector3(Mathf.Sin(b),0,Mathf.Cos(b));
                int n=v.Count; v.Add(top);v.Add(x);v.Add(y);v.Add(bottom);v.Add(y);v.Add(x);
                for(int k=0;k<6;k++)tr.Add(n+k);
            }
            return Make("Seven-sided river pebble",v.ToArray(),tr.ToArray());
        }

        public static void Combine(Transform root)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>();
            var filters=root.GetComponentsInChildren<MeshFilter>();
            foreach(var f in filters)
            {
                var r=f.GetComponent<MeshRenderer>();
                if(r==null || !r.enabled)continue;
                if(!groups.TryGetValue(r.sharedMaterial,out var list))groups[r.sharedMaterial]=list=new List<CombineInstance>();
                list.Add(new CombineInstance {mesh=f.sharedMesh, transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});
                r.enabled=false;
            }
            foreach(var g in groups)
            {
                var mesh=new Mesh {name="Batched "+g.Key.name, indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(g.Value.ToArray());
                MeshObject(mesh.name,root,mesh,g.Key,Vector3.zero);
            }
        }
    }
}
