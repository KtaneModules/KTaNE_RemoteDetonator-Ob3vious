using UnityEngine;

public class DetonatorButton : MonoBehaviour
{
    [SerializeField]
    private Transform _buttonTransform;
    [SerializeField]
    private KMAudio _audio;
    [SerializeField]
    private DetonatorSwitch _armingSwitch;
    [SerializeField]
    private RemoteDetonatorScript _detonator;

    private float _state;
    private bool _targetState;

    private static readonly float _pressTime = 0.1f;

    void Start()
    {
        KMSelectable selectable = GetComponent<KMSelectable>();

        selectable.OnInteract += () =>
        {
            _targetState = true;
            _audio.PlaySoundAtTransform("buttonpress", _buttonTransform);
            return false;
        };

        selectable.OnInteractEnded += () =>
        {
            _targetState = false;
            _audio.PlaySoundAtTransform("buttonrelease", _buttonTransform);
        };
    }

    void Update()
    {
        float prevState = _state;
        if (_targetState)
            _state = Mathf.Min(_state + Time.deltaTime / _pressTime, _armingSwitch.GetState() ? 1 : 0.25f);
        else
            _state = Mathf.Max(_state - Time.deltaTime / _pressTime, 0);

        if ((_state > 0.75f) != (prevState > 0.75f))
            _detonator.TriggerDetonate();

        _buttonTransform.localPosition = Vector3.down * (.00375f * _state);
    }
}
