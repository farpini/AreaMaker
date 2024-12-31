using PolygonLib;
using UnityEngine;

public abstract class AreaRenderer : MonoBehaviour
{
    [SerializeField] protected MeshRenderer meshRenderer;
    [SerializeField] protected MeshFilter meshFilter;

    protected Color currentColor;
    protected static PolygonGroup currentPolygonArea;
    protected static float currentPrecision;

    private void Awake ()
    {
        currentColor = Color.white;
    }

    public virtual void SetPolygon (PolygonGroup polygonGroup, float precision)
    {
        currentPolygonArea = polygonGroup;
        currentPrecision = precision;
    }

    public virtual void SetColor (Color color)
    {
        currentColor = color;
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_MainColor", currentColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    public virtual void SetWidth (float widthValue)
    {
    }

    public virtual void SetSpeed (float speedValue)
    {
    }
}