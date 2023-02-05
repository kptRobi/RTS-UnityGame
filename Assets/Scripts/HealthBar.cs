using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour {

    const string WORLD_CANVAS = "World Canvas";
    Slider slider;
    Unit unit;
    Transform parent;
    [SerializeField]
    Vector3 offset;
    
	void Awake () {
        slider = GetComponent<Slider>();
        parent = transform.parent;
        unit = GetComponentInParent<Unit>();

        //zmień parenta na canvas (chyba)
        var canvas = GameObject.FindGameObjectWithTag(WORLD_CANVAS);
        if (canvas) transform.SetParent(canvas.transform);

	}

    private void Update()
    {
        //śmierć
        if (!parent)
        {
            Destroy(this.gameObject);
            return;
        }

        //odczyt życia
        if(unit) slider.value = unit.HealthPercent;
        transform.position = parent.transform.position + offset;


    }
}
