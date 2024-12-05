using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Misc Class", menuName = "Item/Misc")]
public class MiscClass : ItemClass
{
    //data for misc
    public override void Use(PlayerScript caller)
    {
        //base.Use(caller);
    }
    public override MiscClass GetMisc() { return this; }
}
