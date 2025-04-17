using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WallShader : MonoBehaviour
{
    [Header("Işık Efekti Ayarları")]
    public float waveMaxDistance = 10f;        // Işığın maksimum yayılma mesafesi
    public float waveWidth = 3f;               // Işık dalgalarının genişliği
    public Color lightColor = Color.yellow;    // Işık rengi
    public float lightIntensity = 1.2f;        // Işık yoğunluğu
    public float fadeSpeed = 2.0f;             // Işık dalgalanma hızı
    public float waveDuration = 2f;            // Efektin süresi

    // İç değişkenler
    private Material wallMaterial;
    private bool waveActive = false;
    private float waveCurrentDistance = 0f;
    private Vector3 waveOrigin;
    private float waveTimer = 0f;

    // Shader property IDs (performans için önbelleğe alınmış)
    private int waveOriginID;
    private int waveDistanceID;
    private int waveWidthID;
    private int lightColorID;
    private int lightIntensityID;
    private int fadeSpeedID;

    // Debug
    [Header("Debug")]
    public bool debugMode = true;

    private void Awake()
    {
        // Duvarın materyalini al
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            Debug.LogError("SpriteRenderer bulunamadı! " + gameObject.name + " nesnesinde SpriteRenderer bileşeni olduğundan emin olun.");
            return;
        }

        // Yeni bir materyal oluştur (instance)
        wallMaterial = new Material(Shader.Find("Custom/WallLightEffect"));

        if (wallMaterial == null)
        {
            Debug.LogError("Custom/WallLightEffect shader bulunamadı! Shader dosyasının doğru yerde olduğundan emin olun.");
            return;
        }

        // Ana materyal özelliklerini kopyala
        if (renderer.material != null)
        {
            wallMaterial.mainTexture = renderer.material.mainTexture;
            wallMaterial.color = renderer.material.color;
        }

        // Materyali renderer'a ata
        renderer.material = wallMaterial;

        Debug.Log("WallShader başlatıldı: " + gameObject.name);

        // Shader property ID'lerini önbelleğe al
        waveOriginID = Shader.PropertyToID("_WaveOrigin");
        waveDistanceID = Shader.PropertyToID("_WaveDistance");
        waveWidthID = Shader.PropertyToID("_WaveWidth");
        lightColorID = Shader.PropertyToID("_LightColor");
        lightIntensityID = Shader.PropertyToID("_LightIntensity");
        fadeSpeedID = Shader.PropertyToID("_FadeSpeed");

        // Shader parametrelerini ayarla
        ResetWaveEffect();

        // İlk değerleri ata
        wallMaterial.SetFloat(waveWidthID, waveWidth);
        wallMaterial.SetColor(lightColorID, lightColor);
        wallMaterial.SetFloat(lightIntensityID, lightIntensity);
        wallMaterial.SetFloat(fadeSpeedID, fadeSpeed);
    }

    private void Update()
    {
        if (waveActive)
        {
            // Zamanlayıcıyı güncelle
            waveTimer += Time.deltaTime;

            // Dalgayı genişlet
            waveCurrentDistance = Mathf.Lerp(0, waveMaxDistance, waveTimer / waveDuration);

            // Shader parametrelerini güncelle
            wallMaterial.SetFloat(waveDistanceID, waveCurrentDistance);

            if (debugMode && Time.frameCount % 30 == 0)
            {
                Debug.Log("Dalga ilerliyor: " + waveCurrentDistance + " / " + waveMaxDistance + " (Süre: " + waveTimer + " / " + waveDuration + ")");
            }

            // Dalga efekti tamamlandı mı?
            if (waveTimer >= waveDuration)
            {
                waveActive = false;
                ResetWaveEffect();

                if (debugMode)
                {
                    Debug.Log("Dalga efekti tamamlandı");
                }
            }
        }
    }

    // Dalga efektini sıfırla
    private void ResetWaveEffect()
    {
        waveCurrentDistance = 0f;
        waveTimer = 0f;

        // Shader parametrelerini sıfırla
        if (wallMaterial != null)
        {
            wallMaterial.SetVector(waveOriginID, Vector4.zero);
            wallMaterial.SetFloat(waveDistanceID, 0f);
            wallMaterial.SetFloat(waveWidthID, waveWidth);
            wallMaterial.SetColor(lightColorID, lightColor);
            wallMaterial.SetFloat(lightIntensityID, lightIntensity);
            wallMaterial.SetFloat(fadeSpeedID, fadeSpeed);
        }
    }

    // Çarpışmayı algıla ve dalga efektini başlat
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (debugMode)
            {
                Debug.Log("Oyuncu ile çarpışma algılandı: " + gameObject.name);
            }

            // Çarpışma noktasını al
            ContactPoint2D contact = collision.GetContact(0);
            StartWaveEffect(contact.point);
        }
    }

    // Dalga efektini başlat
    public void StartWaveEffect(Vector2 origin)
    {
        if (wallMaterial == null)
        {
            Debug.LogError("WallMaterial null! Efekt başlatılamıyor.");
            return;
        }

        // Çarpışma noktasını dünya koordinatlarından model koordinatlarına dönüştür
        waveOrigin = transform.InverseTransformPoint(origin);

        // Shader'a çarpışma noktasını ayarla
        wallMaterial.SetVector(waveOriginID, new Vector4(origin.x, origin.y, 0, 0));

        if (debugMode)
        {
            Debug.Log("Dalga efekti başlatıldı: " + origin + " noktasından");
        }

        // Dalga efektini başlat
        waveActive = true;
        waveTimer = 0f;
        waveCurrentDistance = 0f;
    }
}
