using UnityEngine;

public class DetonatorLed : MonoBehaviour
{
    [SerializeField]
    private Material _offMaterial;
    [SerializeField]
    private Material _onMaterial;
    [SerializeField]
    private DetonatorSwitch _switch;
    [SerializeField]
    private MeshRenderer _light;
    private bool _isOn;

    void Start()
    {
        _light.material = _offMaterial;
        _isOn = false;
    }

    void Update()
    {
        if (_isOn == _switch.GetState())
            return;

        _isOn = !_isOn;

        if (_isOn)
            _light.material = _onMaterial;
        else
            _light.material = _offMaterial;
    }

}
