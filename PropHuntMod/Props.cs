using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public struct Prop
{
    public string name;
    public Vector3 positionOffset;

    public Prop(string name, Vector3 positionOffset)
    {
        this.name = name;
        this.positionOffset = positionOffset;
    }
}

internal class Props
{
    public List<Prop> boneBottom = new List<Prop>();

    public void Initialize()
    {
        boneBottom.Add(new Prop("moss_clump_set", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("tent_pillows", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("bone_bush_01", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("Bone_moss_house_moss_wall_02", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("grass_02", Vector3.zero));
    }
}