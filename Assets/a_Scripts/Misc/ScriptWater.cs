using UnityEngine;
using System.Collections;

public class ScriptWater : MonoBehaviour
{
    public float speed = 0.3f;

    public float alpha = 0.65f;

    public float scale = 12f;

    private Color col;

    private float theTime;

    private float moveWater;

    void Update()
    {
        theTime = Time.time;

        moveWater = Mathf.PingPong(theTime * speed, 100f) * 0.15f;

        gameObject.renderer.material.mainTextureOffset = new Vector2(moveWater, moveWater);

        col = gameObject.renderer.material.color;
        col.a = alpha;
        gameObject.renderer.material.color = col;

        gameObject.renderer.material.mainTextureScale = new Vector2(scale, scale);
    }

}





