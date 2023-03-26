using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopUp : MonoBehaviour
{
    [HideInInspector] public float DamagValue;
    public float TransformUpSpeed, RotateAmount, GrowAmount;
    private TextMeshPro TextMesh;


    void Start()
    {
        TextMesh = GetComponent<TextMeshPro>();
        TextMesh.text = DamagValue.ToString();
        float Scale = DamagValue / 250;
        transform.localScale = new Vector3(Scale, Scale, Scale);
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
        StartCoroutine(nameof(Fade));
    }
    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(1f);
        for (float f = 1; f > -0.05f; f-=0.05f)
        {
            Color c = TextMesh.color;
            c.a = f;
            TextMesh.color = c;
            yield return new WaitForSeconds(0.025f);
        }
        Destroy(gameObject); // Change To Pooling Later
    }
    private void Update()
    {
        transform.Translate(TransformUpSpeed * Time.deltaTime * Vector3.up);
        transform.Rotate(RotateAmount * Time.deltaTime * Vector3.up);
        transform.localScale +=(GrowAmount * Time.deltaTime * Vector3.one);
    }
}
