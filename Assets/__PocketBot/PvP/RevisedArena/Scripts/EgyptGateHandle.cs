using UnityEngine;

public class EgyptGateHandle : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.1f;
    [SerializeField] private float waveAmplitude = 0.05f;
    [SerializeField] private float waveFrequency = 1.0f;

    private Material _waterMaterial;
    private Vector2 _baseOffset;
    private float _randomOffsetX;
    private float _randomOffsetY;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] materials = renderer.materials; // Clone toàn bộ để không ảnh hưởng global

            if (materials.Length > 1 && materials[1] != null)
            {
                // Tạo instance riêng cho material ở index 1
                materials[1] = Instantiate(materials[1]);
                renderer.materials = materials;

                _waterMaterial = materials[1];
                _randomOffsetX = Random.Range(0f, 100f);
                _randomOffsetY = Random.Range(0f, 100f);
            }
        }
    }

    private void Update()
    {
        if (_waterMaterial == null) return;

        float time = Time.time;

        float offsetX = Mathf.Sin(time * waveFrequency + _randomOffsetX) * waveAmplitude;
        float offsetY = Mathf.Cos(time * waveFrequency + _randomOffsetY) * waveAmplitude;

        _baseOffset += Vector2.one * scrollSpeed * Time.deltaTime;

        Vector2 finalOffset = _baseOffset + new Vector2(offsetX, offsetY);
        _waterMaterial.mainTextureOffset = finalOffset;
    }
}
