unity-procedural-tree
=====================

Procedural tree builder for Unity.

![Demo](https://raw.githubusercontent.com/mattatz/unity-procedural-tree/master/Captures/Demo.gif)

## Usage

TreeData class has properties of ProceduralTree.

![TreeData](https://raw.githubusercontent.com/mattatz/unity-procedural-tree/master/Captures/TreeData.png)

Setup a TreeData instance and pass it to ProceduralTree.Build function.

```cs
// Setup TreeData for properties of ProceduralTree
TreeData data = new TreeData();
// data.randomSeed = 100;
// data.branchesMin = 1; data.branchesMax = 3;

Mesh mesh = ProceduralTree.Build(
    data,
    6, // generations of a tree
    1.5f, // base height of a tree
    0.15f // base radius of a tree
);
```

