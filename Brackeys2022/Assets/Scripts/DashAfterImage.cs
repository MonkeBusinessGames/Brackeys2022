using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class DashAfterImage : MonoBehaviour
{
    [Tooltip("Number of sprites per distance unit.")]
    [SerializeField] private float rate = 2f;
    [SerializeField] private float lifeTime = 0.2f;

    private SpriteRenderer baseRenderer;
    private bool isActive = false;
    private float interval;
    private Vector3 previousPos;

    private void Start()
    {
        
        baseRenderer = GetComponent<SpriteRenderer>();
        interval = 1f / rate;
    }

    private void Update()
    {
        if (isActive && Vector3.Distance(previousPos, transform.position) > interval)
        {
            SpawnTrailPart();
            previousPos = transform.position;
        }
    }

    /// <summary>
    /// Call this function to start/stop the trail.
    /// </summary>
    public void ActivateAfterImages(bool shouldActivate)
    {
        isActive = shouldActivate;
        if (isActive)
            previousPos = transform.position;
    }

    private void SpawnTrailPart()
    {
        GameObject trailPart = new GameObject(gameObject.name + " trail part");

        // Sprite renderer
        SpriteRenderer trailPartRenderer = trailPart.AddComponent<SpriteRenderer>();
        CopySpriteRenderer(trailPartRenderer, baseRenderer);

        // Transform
        trailPart.transform.position = transform.position;
        trailPart.transform.rotation = transform.rotation;
        trailPart.transform.localScale = transform.lossyScale;

        // Sprite rotation
        //trailPart.AddComponent<CameraSpriteRotater>();

        // Fade & Destroy
        StartCoroutine(FadeTrailPart(trailPartRenderer));
    }

    private IEnumerator FadeTrailPart(SpriteRenderer trailPartRenderer)
    {
        float fadeSpeed = 1 / lifeTime;

        while (trailPartRenderer.color.a > 0)
        {
            Color color = trailPartRenderer.color;
            color.a -= fadeSpeed * Time.deltaTime;
            trailPartRenderer.color = color;

            yield return new WaitForEndOfFrame();
        }

        Destroy(trailPartRenderer.gameObject);
    }

    private static void CopySpriteRenderer(SpriteRenderer copy, SpriteRenderer original)
    {
        // Can modify to only copy what you need!
        copy.sprite = original.sprite;
        copy.flipX = original.flipX;
        copy.flipY = original.flipY;
        copy.sortingLayerID = original.sortingLayerID;
        copy.sortingLayerName = original.sortingLayerName;
        copy.sortingOrder = original.sortingOrder;
    }
}