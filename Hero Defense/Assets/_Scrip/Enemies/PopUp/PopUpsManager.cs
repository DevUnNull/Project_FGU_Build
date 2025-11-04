using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpsManager : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] GameObject PopUp_Prefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -_camera.transform.position.z;

            Vector3 worldPos = _camera.ScreenToWorldPoint(mousePos);
            GameObject popUpObject = Instantiate(PopUp_Prefab, worldPos, new Quaternion());
            popUpObject.GetComponent<PopUp>().text_Value = "succset";
        }
    }
}
