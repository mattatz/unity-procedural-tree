unity-generative-tree
=====================

Generative tree for unity3d.

![sample tree](https://raw.githubusercontent.com/mattatz/unity-generative-tree/master/Captures/sample-tree.png)

It is able to animate to grow easily.

![sample animation tree](https://raw.githubusercontent.com/mattatz/unity-generative-tree/master/Captures/sample-animation-tree.gif)

## Preset 

unity-generative-tree prepares tree preset.

![tree preset](https://raw.githubusercontent.com/mattatz/unity-generative-tree/master/Captures/tree-preset.png)

## Example

```cs
Branch branch = Branch.LoadPreset(Vector3.up, preset);
branch.Build();
GetComponent<MeshFilter>().sharedMesh = branch.mesh;
```

## Demo

[Unity Web Player](https://mattatz.github.io/unity/generative-tree)

