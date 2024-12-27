using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPallete
{
    // Implementation based of https://iquilezles.org/articles/palettes/
    // Health is expected to be normalized.
    public static Color Health(float health)
    {
        health = Mathf.SmoothStep(0.25f, 0.68f, health);
        Vector3 contrast = new Vector3(0.6f, 0.6f, 0f);
        Vector3 brightness = new Vector3(0.6f, 0.3f, 0.5f);
        Vector3 oscillations = new Vector3(0.9f, 0.9f, 0f);
        Vector3 phase = new Vector3(0.69f, 0.35f, 0.6f);
        Vector3 octave = Mathf.PI * 2f * (oscillations * health + phase);
        Vector3 rgb = new Vector3(Mathf.Cos(octave.x), Mathf.Cos(octave.y), Mathf.Cos(octave.z));
        Vector3 palette = contrast + new Vector3(
            brightness.x * rgb.x,
            brightness.y * rgb.y,
            brightness.z * rgb.z);
        return new Color(palette.x, palette.y, palette.z);
    }
}
