# UnitySliceImporter
Import slice data from Resolume to Unity primitives
<h4>usage</h4>

- Create your input map in Resolume and save it
- Create a new Unity project
- Download the SliceImporter.unitypackage from Releases
- Double click the .unitypackage and import all
- Apply the SliceImporter.cs script on an empty GameObject
- In the Inspector, fill out the name of your Resolume setup file
- Hit play

<h4>optional</h4>

- Enable Spout in Resolume
- ???
- Profit

<h4>limitations</h4>

- PC only. If someone wants to add Syphon for Mac people, open a PR
- No polygons, no input masks, no fixture slices. If someone wants to add those, open a PR

<h4>acknowledgements</h4>
SliceImporter bundles the excellent KlakSpout by Keijiro: https://github.com/keijiro/KlakSpout
