using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : Unit, ISelectable {
    public void setSelected(bool selected)
    {
        healthBar.gameObject.SetActive(selected);
    }

    void Command(Vector3 destination)
    {
        nav.SetDestination(destination);
        task = Task.move;
        target = null;
    }
    void Command(Soldier SoldierToFollow)
    {
        target = SoldierToFollow.transform;
        task = Task.follow;
    }
    void Command(Dragon dragonToKill)
    {
        //todo
    }
}
