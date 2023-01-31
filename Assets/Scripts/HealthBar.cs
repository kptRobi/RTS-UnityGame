using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour {

    const string HP_CANVAS = "HP Canvas";
    Slider slider;
    Unit unit;
    Transform parent;
    [SerializeField]
    Vector3 offset;
    Transform cameraTransform;
    
	void Awake () {
        slider = GetComponent<Slider>();
        parent = transform.parent;
        unit = GetComponentInParent<Unit>();

        //zmień parenta na canvas (chyba)
        var canvas = GameObject.FindGameObjectWithTag(HP_CANVAS);
        if (canvas) transform.SetParent(canvas.transform);

        //patrz sliderem w kamere
        cameraTransform = Camera.main.transform;
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

        //pozycja nad jednostką
        transform.position = parent.transform.position + offset;
        transform.LookAt(cameraTransform);
        var rotation = transform.localEulerAngles;
        rotation.y = 180;
        transform.localEulerAngles = rotation;
    }
}
