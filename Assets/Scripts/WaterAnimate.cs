using UnityEngine;

public class WaterAnimate : MonoBehaviour {
    [SerializeField] private float xSpeed = 0.02f;
    [SerializeField] private float ySpeed = 0.01f;
    [SerializeField] private float normalTiling = 1f;

    private Material material;
    private Vector2 offset;

    void Awake() {
        material = GetComponent<Renderer>().sharedMaterial;
    }

    void Update() {
        if (material == null) return;

        offset.x += xSpeed * Time.deltaTime;
        offset.y += ySpeed * Time.deltaTime;

        material.mainTextureOffset = offset;
    }
}