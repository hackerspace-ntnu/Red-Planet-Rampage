using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class PrefabOnlyAttribute : PropertyAttribute{ public PrefabOnlyAttribute(){} }