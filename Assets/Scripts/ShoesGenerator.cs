using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ShoesGenerator : MonoBehaviour
{
    public bool fromPrefab;
    public GameObject prefab;
    public GameObject sceneObject;

    private void Start()
    {

    }

    private void Update()
    {

    }

    public void OnSelected(Hand hand)
    {
        StartCoroutine(DoGenerator());
    }

    private IEnumerator DoGenerator()
    {
        var shoes = fromPrefab ? Instantiate<GameObject>(prefab) : sceneObject;
        shoes.transform.position = transform.position;
        shoes.transform.rotation = Quaternion.Euler(-22.61f, 0f, 0f);

        Rigidbody rigidbody = shoes.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.isKinematic = true;
        }

        var initialScale = Vector3.one * 0.01f;
        var targetScale = Vector3.one * 0.2f;

        var startTime = Time.time;
        var overTime = 0.5f;
        var endTime = startTime + overTime;

        while (Time.time < endTime)
        {
            shoes.transform.localScale = Vector3.Slerp(initialScale, targetScale, (Time.time - startTime) / overTime);
            yield return null;
        }

        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
        }
    }
}
