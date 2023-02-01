

# Attributes

You may be familiar with the use of [SerializeField] for exposing variables in the unity editor inspector as opposed to making fields public. This is only one of many useful attributes you can attach to a field ("Range" may be another one of interest?)

Unity allows us as developers to greatly extend the editor with custom editor scripts (CustomEditor and PropertyDrawer), With that, they also allow us to define custom attributes, and add specifications for how fields marked with said attributes are drawn in the editor.

Currently, we have the following attributes which modifies how the fields marked with them are drawn:

----

## PrefabOnly

Can be put on any `GameObject` field or component fields (i.e. fields of classes inheriting from the base class `Component` - i.e. all built-in components, as well as custom `MonoBehaviour`)

The attribute disallows references to in-scene gameobjects/components

Example:

```cs
[PrefabOnly, SerializeField]
private GameObject somePrefab;

// Note: usage of this is scary, and should only be used in a "read-only" context or in 
//		editor-related contexts. Changing it would change the prefab's Rigidbody.
[PrefabOnly]
public Rigidbody someRigidbodyOfAPrefab; 
```

## Uneditable

Can be put on any field (though only useful on serialized fields - as others aren't editable through the editor anyways)

The attribute allows insight in the field, but disallows changing the field from editor. Can be useful for displaying debug info, or simply for disallowing edits while keeping the field serialized.

Example:

```cs
[Uneditable, SerializeField]
private GameObject someObjectReferenceThatCannotBeChangedInEditor;

[Uneditable]
public float someFloatSetInternally; 
```

## LabelAs

Can be put on any field (though only useful on serialized fields - as others aren't labelled in the editor anyways)

The attribute allows "rebranding" a variable as it is shown in the editor.

Example:

```cs
[LabelAs("Some Name That Makes Sense In Editor Context"), SerializeField]
private string someVariableNameThatMakesSenseInScriptingContext;
```

## MinMax
Can be put on any int/float field, as well as IntRange and FloatRange (though only useful on serialized fields - as others aren't labelled in the editor anyways)

The attribute constraints a value to be in between Min and Max in the unity editor. (like unity's builtin Range attribute, but covering more target types!)

Example

```cs
[SerializeField, MinMax(0, 5)]
private int someValueThatShouldBeBetweenZeroAndFive;

[MinMax(5, 10)]
public IntRange someRangeDefinedBetweenFiveAndTen;
```