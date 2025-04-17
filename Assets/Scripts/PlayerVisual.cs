using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisual : MonoBehaviour
{
    private Material material;
    private SpriteRenderer spriteRenderer;

    [Header("Glow Settings")]
    public Color glowColor = new Color(0f, 1f, 1f, 1f); // Default cyan color
    [Range(0.1f, 5f)]
    public float glowSpeed = 2f;
    [Range(0f, 3f)]
    public float glowIntensity = 1.5f;
    [Range(0.1f, 2f)]
    public float glowRange = 1f;

    // Jump effect settings
    private float initialGlowIntensity;
    private float targetGlowIntensity;
    private float currentVelocityY;
    private Rigidbody2D rb;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            // Create a material instance from the existing material
            material = new Material(spriteRenderer.material);
            spriteRenderer.material = material;
            UpdateMaterialProperties();
        }
        initialGlowIntensity = glowIntensity;
    }

    void Update()
    {
        if (material != null)
        {
            UpdateGlowEffect();
            UpdateMaterialProperties();
        }
    }

    void UpdateGlowEffect()
    {
        if (rb != null)
        {
            // Increase glow intensity when moving up, decrease when moving down
            float velocityInfluence = Mathf.Abs(rb.velocity.y) * 0.1f;
            targetGlowIntensity = initialGlowIntensity + velocityInfluence;

            // Smoothly interpolate current glow intensity
            glowIntensity = Mathf.Lerp(glowIntensity, targetGlowIntensity, Time.deltaTime * 5f);
        }
    }

    void UpdateMaterialProperties()
    {
        material.SetColor("_GlowColor", glowColor);
        material.SetFloat("_GlowSpeed", glowSpeed);
        material.SetFloat("_GlowIntensity", glowIntensity);
        material.SetFloat("_GlowRange", glowRange);
    }
}
