using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeScript : MonoBehaviour
{
    [SerializeField] public int axeDamage = 5;

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 4f))
            {
                if (hit.collider.GetComponent<TreeHealth>())
                {
                    hit.collider.GetComponent<TreeHealth>().takeDamage(axeDamage, transform.root.gameObject);
                }
            }
        }
    }
}
