using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class Breathing : MonoBehaviour
{
    public float min = 0.4f;
    public float max = 0.9f;
    public float timeLength = 2;

    private Image _target;
    
    private float _currentTime;

    private void Awake()
    {
        _target = GetComponent<Image>();
    }

    void Start()
    {
        _currentTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _currentTime += Time.deltaTime;
        _currentTime %= timeLength;
        var alpha = min + (max - min) * 0.5f * (Mathf.Cos((_currentTime / timeLength) * 2 * Mathf.PI) + 1);
        var newCol = _target.color;
        newCol.a = alpha;
        _target.color = newCol;
    }
}
