using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    #region --Fields / Properties--
    
    [SerializeField, Tooltip("Optional coefficient for the selected GraphType.")]
    private float _coefficient;

    [SerializeField, Tooltip("Optional Y-axis intercept for the selected GraphType.")]
    private float _yIntercept;
    
    [SerializeField, Tooltip("Desired GraphType to be drawn.")]
    private GraphType _currentGraphType;

    [SerializeField, Tooltip("How fast the graph should be drawn.")]
    private float _graphSpeed = 1;

    [SerializeField, Tooltip("Game object to be used for the motion trail left by the graphing node as it moves across the screen.")]
    private GameObject _motionTrailPrefab;

    [SerializeField, Tooltip("Desired color of the motion trail prefab.")]
    private Color _motionTrailColor;
    
    /// <summary>
    /// Cached Transform component.
    /// </summary>
    private Transform _transform;

    /// <summary>
    /// Current position of the graphing node (this game object).
    /// </summary>
    private Vector3 _movePosition;

    /// <summary>
    /// Tracks if the graph is performing a reset.
    /// </summary>
    private bool _isResetting;

    /// <summary>
    /// Input value for the selected GraphType.
    /// </summary>
    private float _input;

    /// <summary>
    /// How long to wait between ResetGraph calls.
    /// </summary>
    private float _resetDelayTime = 1f;
    
    /// <summary>
    /// Tracks the time elapsed for the _resetDelayTime.
    /// </summary>
    private float _resetDelayTimer;

    /// <summary>
    /// How long to wait before calling ResetGraph.
    /// </summary>
    private float _resetTime = 5f;
    
    /// <summary>
    /// Tracks the time elapsed for the _resetTime.
    /// </summary>
    private float _resetTimer;

    /// <summary>
    /// Records the starting position of the graphing node so it can be reset properly.
    /// </summary>
    private Vector3 _startingPosition;

    /// <summary>
    /// Used to display the order in which graphing nodes are spawned.
    /// </summary>
    private int _nodeIndex;
    
    /// <summary>
    /// Contains all the graphing nodes that are spawned so they can be reset properly.
    /// </summary>
    private List<GameObject> _spawnedNodes = new List<GameObject>();

    #endregion
    
    #region --Unity Specific Methods---

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        InvokeRepeating($"SpawnNode", 0f, .5f);
    }

    private void Update()
    {
        CheckGraph();
        UpdateInput();
        DrawGraph();
    }
    
    #endregion
    
    #region --Custom Methods--
    
    /// <summary>
    /// Initializes variables and caches components.
    /// </summary>
    private void Init()
    {
        _transform = transform;
        _startingPosition = _transform.position;
    }

    /// <summary>
    /// Used to spawn a node.
    /// </summary>
    private void SpawnNode()
    {
        if (_isResetting) return;
        
        GameObject _node = Instantiate(_motionTrailPrefab, new Vector3(_movePosition.y, _transform.position.y), Quaternion.identity);
        _node.transform.SetParent(_transform.parent, true);
        _node.name = _currentGraphType + " " + _nodeIndex;
        
        UpdateMaterialPropertyBlock(_node);
        _spawnedNodes.Add(_node);
        _nodeIndex++;
    }

    /// <summary>
    /// Used to change the color of the motion trail game objects as they are spawned.
    /// </summary>
    private void UpdateMaterialPropertyBlock(GameObject _node)
    {
        Renderer _renderer = _node.GetComponent<Renderer>();
        MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_Color", _motionTrailColor);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    /// <summary>
    /// Checks the current graph behavior for any changes.
    /// </summary>
    private void CheckGraph()
    {
        if (!_isResetting)
        {
            _resetTimer += Time.deltaTime;
            if (_resetTimer > _resetTime)
            {
                ResetGraph();
                _resetTimer = 0;
            }
        }
        else
        {
            _resetDelayTimer += Time.deltaTime;
            if (_resetDelayTimer > _resetDelayTime)
            {
                ResetGraph();
                _resetDelayTimer = 0;
                _isResetting = false;
            }
        }
    }

    /// <summary>
    /// Resets graph to default.
    /// </summary>
    private void ResetGraph()
    {
        _isResetting = true;
        for (int i = 0; i < _spawnedNodes.Count; i++)
        {
            Destroy(_spawnedNodes[i]);
        }
        _spawnedNodes.Clear();
        _transform.position = _startingPosition;
        _input = 0;
        _nodeIndex = 0;
        _movePosition = UpdateGraphFunction();
    }

    /// <summary>
    /// Updates the mathematical function to be used for the selected GraphType.
    /// </summary>
    private Vector3 UpdateGraphFunction()
    {
        GraphFunction _function = GraphFunctionLibrary.GetGraphFunction(_currentGraphType);
        return _function(_input, _coefficient, _yIntercept);
    }

    /// <summary>
    /// Increments the input value passed into the selected GraphType.
    /// </summary>
    private void UpdateInput()
    {
        if (_isResetting) return;

        _input = _movePosition.x + _graphSpeed * Time.deltaTime;
        _movePosition = new Vector3(_movePosition.y, _startingPosition.y, 0);
        _transform.position = _movePosition;
    }
    
    /// <summary>
    /// Updates the line renderer component to draw the selected GraphType.
    /// </summary>
    private void DrawGraph()
    {
        if (_isResetting) return;

        _movePosition = UpdateGraphFunction();
    }
    
    #endregion
    
}
