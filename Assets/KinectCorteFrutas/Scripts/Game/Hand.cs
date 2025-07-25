using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{

    public Transform mHandMesh;
    public Sprite Hands;

    private void Update()
    {
        mHandMesh.position = Vector3.Lerp(mHandMesh.position, transform.position, Time.deltaTime * 15.0f);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Fruit"))
            return;
        Fruit fruit = collision.gameObject.GetComponent<Fruit>();
        StartCoroutine(fruit.Pop());
    }


}
