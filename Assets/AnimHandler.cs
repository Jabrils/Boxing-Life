using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimHandler : MonoBehaviour
{
    public int player;
    public CTRL c;

    public void hit()
    {
        c.Hit(0, player);
    }

    public void Upper()
    {
        c.Hit(1, player);
    }

    public void DodgeRStart()
    {
        c.SetDodge(player, true, 'R');
    }

    public void DodgeREnd()
    {
        c.SetDodge(player, false, 'R');
    }

    public void DodgeLStart()
    {
        c.SetDodge(player, true, 'L');
    }

    public void DodgeLEnd()
    {
        c.SetDodge(player, false, 'L');
    }


    public void blockStart()
    {
        c.SetBlock(player, true);
    }

    public void blockEnd()
    {
        c.SetBlock(player, false);
    }

    public void ImOut()
    {
        c.SomeoneKO(player);
    }
}
