using UnityEngine;

public class DetonatorSwitch : MonoBehaviour
{
    [SerializeField]
    private Transform _switchTransform;
    [SerializeField]
    private Transform _capTransform;
    [SerializeField]
    private Transform _nutTransform;
    [SerializeField]
    private KMAudio _audio;
    private float _state;
    private float _capState;
    private bool _targetCapState;
    private bool _targetState;

    private bool _isHighlighted = false;

    public static DetonatorSwitch LastFlip = null;

    private static readonly float _flipTime = 0.1f;

    void Start()
    {
        KMSelectable selectable = GetComponent<KMSelectable>();

        selectable.OnHighlight += () =>
        {
            _targetCapState = true;
            _isHighlighted = true;
        };

        selectable.OnHighlightEnded += () =>
        {
            _targetCapState = _targetState;
            _isHighlighted = false;
        };

        selectable.OnInteract += () =>
        {
            _targetState = !_targetState;
            LastFlip = this;
            return false;
        };

        _nutTransform.localEulerAngles = new Vector3(-90, 0, UnityEngine.Random.Range(0f, 60f));
    }

    void Update()
    {
        if (_capTransform != null && LastFlip != this && !_isHighlighted)
        {
            _targetState = false;
            _targetCapState = false;
        }

        float prevState = _state;
        if (_targetState)
            _state = Mathf.Min(_state + Time.deltaTime / _flipTime, 1);
        else
            _state = Mathf.Max(_state - Time.deltaTime / _flipTime, 0);

        float prevCapState = _state;
        if (_targetCapState)
            _capState = Mathf.Min(_capState + Time.deltaTime / _flipTime, 1);
        else
            _capState = Mathf.Max(_capState - Time.deltaTime / _flipTime, 0);

        if ((_state > 0.5f) != (prevState > 0.5f))
            _audio.PlaySoundAtTransform("switch" + (_state > 0.5f ? "on" : "off"), _switchTransform);

        if (_capTransform != null && _capState <= 0f && prevCapState > 0f)
            _audio.PlaySoundAtTransform("plastic", _switchTransform);

        _switchTransform.localEulerAngles = Vector3.right * (-120 + 60 * _state);

        if (_capTransform != null)
            _capTransform.localEulerAngles = Vector3.right * (-90 + 90 * _capState);
    }

    public void SetInitialState(bool state)
    {
        _state = state ? 1 : 0;
        _targetState = state;
        _capState = _state;
        _targetCapState = state;
    }

    public bool GetState()
    {
        return _state > 0.5f;
    }
}
