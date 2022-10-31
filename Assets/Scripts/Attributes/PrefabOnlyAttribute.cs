using System;
using UnityEngine;

/// <summary>
/// Disallows references to in-scene objects.
/// Can be put on any GameObject field or component fields 
/// (i.e. fields of classes inheriting from the base class Component, 
/// meaning: all built-in components, as well as custom MonoBehaviours)
/// 
/// Example:
/// <code>
/// [PrefabOnly, SerializeField]
/// private GameObject somePrefab;
/// </code>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class PrefabOnlyAttribute : PropertyAttribute{}
