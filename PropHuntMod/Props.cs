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
        boneBottom.Add(new Prop("Bonechurch_middle_bits_0002_sack", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("RestBench Control", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("bone_bush_FG_01 (4)", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("Bone_house_pieces_0001_9", new Vector3(0, 0, 0)));
        boneBottom.Add(new Prop("Bone_house_breakable_point", Vector3.zero));
        boneBottom.Add(new Prop("Bone_house_breakable_point (1)", Vector3.zero));
        boneBottom.Add(new Prop("pilgrim_corpse_0003_1_moss_02", Vector3.zero));
        boneBottom.Add(new Prop("grass_03", Vector3.zero));
        boneBottom.Add(new Prop("moss_break_table", Vector3.zero));
    }
}