using System;
using UnityEngine;

/// <summary>
/// Allows insight in the field, but disallows changing the field from editor. Can be useful for displaying debug info, 
/// or simply for disallowing edits while keeping the field serialized.
/// 
/// Can be put on any field (though only useful on serialized fields - as others aren't editable through the editor anyways)
/// 
/// Example:
/// <code>
/// [Uneditable, SerializeField]
/// private float time;
/// </code>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class UneditableAttribute : PropertyAttribute{}
