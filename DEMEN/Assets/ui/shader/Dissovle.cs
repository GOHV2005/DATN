using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dissovle : MonoBehaviour
{
    [SerializeField] public float _dissolveTime = 0.75f;

    private SpriteRenderer[] _spriteRenderers;
    private Material[] _materials;

    private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int _verticalDissolveAmount = Shader.PropertyToID("_VerticalDissovle");

    private void Start()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _materials = new Material[_spriteRenderers.Length];
        for (int i=0; i<_spriteRenderers.Length; i++)
        {
            _materials[i] =  _spriteRenderers[i].material;
        }
        StartCoroutine(Appear(true, false));
    }
    private void Update()
    {
        //if (Keyboard.current.kKey.wasPressedThisFrame)
        //{
        //    StartCoroutine(Vanish(true, false));
        //}
        //if (Keyboard.current.lKey.wasPressedThisFrame)
        //{
        //    StartCoroutine(Appear(true, false));
        //}
    }
    public IEnumerator Vanish(bool useDisssolve, bool useVertical)
    {
        float elapsedTime = 0f;
        while(elapsedTime < _dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(0, 1.1f, (elapsedTime / _dissolveTime));
            float lerpedVerticalDissolve = Mathf.Lerp(0f, 1.1f, (elapsedTime /_dissolveTime));
            for(int i = 0;i < _materials.Length; i++)
            {
                if(useDisssolve)
                _materials[i].SetFloat(_dissolveAmount,lerpedDissolve);
                if(useVertical)
                _materials[i].SetFloat(_verticalDissolveAmount, lerpedVerticalDissolve);
            }
            yield return null;
        }
    }
    public IEnumerator Appear(bool useDisssolve, bool useVertical)
    {
        float elapsedTime = 0f;
        while (elapsedTime < _dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float lerpedDissolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));
            float lerpedVerticalDissolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));
            for (int i = 0; i < _materials.Length; i++)
            {
                if (useDisssolve)
                    _materials[i].SetFloat(_dissolveAmount, lerpedDissolve);
                if (useVertical)
                    _materials[i].SetFloat(_verticalDissolveAmount, lerpedVerticalDissolve);
            }
            yield return null;
        }
    }
}
