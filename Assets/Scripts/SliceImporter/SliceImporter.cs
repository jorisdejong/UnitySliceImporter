
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;
using System.Xml;

public class SliceImporter : MonoBehaviour
{
    public string OutputSetupName = "UnityTest";
    
    private RenderTexture rt;
    private Klak.Spout.SpoutReceiver receiver;

    private GameObject slices;
    private bool isConnected = false;
    void Start()
    {
        //create spout component
        rt = new RenderTexture(1920, 1080, 8);
        rt.name = "Spout Input";
        receiver = gameObject.AddComponent<Klak.Spout.SpoutReceiver>();
        receiver.sourceName = "Arena - Composition";

        ////get the ass file
        string assFile = "Resolume Arena/Presets/Advanced Output/" + OutputSetupName + ".xml";
        string assFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), assFile);
        if (File.Exists(assFilePath))
        {
            slices = new GameObject("Slices");

            //parse the xml
            string xmlString = File.ReadAllText(assFilePath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            //get the comp size
            XmlNode compTextureNode = xmlDoc.SelectSingleNode("//ScreenSetup/CurrentCompositionTextureSize");
            Vector2 resolution = new Vector2(float.Parse(compTextureNode.Attributes.GetNamedItem("width").Value), float.Parse(compTextureNode.Attributes.GetNamedItem("height").Value));
            resolution /= 100.0f; //1 unit = 1 pixel

            //set the camera
            Camera.main.transform.position = new Vector3(0, 0, -resolution.y);

            //loop through every slice entry
            XmlNodeList sliceNodes = xmlDoc.SelectNodes("//ScreenSetup/screens/Screen/layers/Slice");
            foreach (XmlNode sliceNode in sliceNodes)
            {
                string sliceName = "Slice";

                //find its name
                foreach (XmlNode paramNode in sliceNode.SelectNodes("Params/Param"))
                {
                    if (paramNode.Attributes.GetNamedItem("name").Value == "Name")
                    {
                        sliceName = paramNode.Attributes.GetNamedItem("value").Value;
                    }
                }

                //create a mesh
                Mesh mesh = new Mesh();
                GameObject go = new GameObject(sliceName, typeof(MeshFilter), typeof(MeshRenderer));

                //parse the inputrect
                List<Vector3> vertices = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();
                XmlNode inputRectNode = sliceNode["InputRect"];

                foreach (XmlNode child in inputRectNode.ChildNodes)
                {
                    //get x and y into unityspace
                    float x = float.Parse(child.Attributes.GetNamedItem("x").Value) / 100.0f;
                    float y = resolution.y - float.Parse(child.Attributes.GetNamedItem("y").Value) / 100.0f;
                    vertices.Add(new Vector3(x - resolution.x * 0.5f, y - resolution.y * 0.5f, 0.0f));
                    uvs.Add(new Vector2(x / resolution.x, y / resolution.y));
                }
                //place in center and rotate upright;
                Vector3 center = (vertices[2] - vertices[0]) / 2 + vertices[0];
                float orientation = float.Parse(inputRectNode.Attributes.GetNamedItem("orientation").Value);
                for (int i = 0; i < vertices.Count; i++)
                {
                    vertices[i] = vertices[i] - center;

                    if (orientation != 0.0f)
                    {
                        Quaternion rotation = Quaternion.Euler(0, 0, orientation * (180.0f / 3.141592654f));
                        vertices[i] = rotation * vertices[i];
                    }
                }

                mesh.vertices = vertices.ToArray();
                mesh.uv = uvs.ToArray();
                //inputrects are always two triangles in the same order
                mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
                Vector3 normal = new Vector3(0, 0, -1);
                mesh.normals = new Vector3[4] { normal, normal, normal, normal };

                //assign mesh and material
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

                //reposition using Unity's transform
                //Unity's rotation is CCW for positive, so it needs to be flipped
                go.transform.position = center;
                Quaternion reverseRotation = Quaternion.Euler(0, 0, -orientation * (180.0f / 3.141592654f));
                go.transform.rotation = reverseRotation;

                //add to the slices object
                go.transform.SetParent(slices.transform);
            }
        }
    }

    void Update()
    {
        //check for spout input
        bool connected = receiver.receivedTexture != null;

        //if the connection status has changed, update the texture
        if (connected != isConnected)
        {
            receiver.targetTexture = connected ? rt : null;
            foreach (Transform slice in slices.transform)
                slice.GetComponent<MeshRenderer>().material.mainTexture = connected ? rt : null;
            isConnected = connected;
        }

        //if the size has changed, update the receiving texture size
        if (isConnected)
        {
            if (receiver.receivedTexture.width != rt.width || receiver.targetTexture.height != rt.height)
            {
                rt.width = receiver.receivedTexture.width;
                rt.height = receiver.receivedTexture.height;
            }
        }
    }
}
