using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingHandler
{
    //public static PostProcessingHandler Instance { get; private set; }

    static Volume volume;

    static Vignette vignette;
    static DepthOfField depthOfField;
    static ColorAdjustments colorAdjustments;
    static LensDistortion lensDistortion;
    static ChromaticAberration chromaticAberration;
    static WhiteBalance whiteBalance;

    static Color defaultColorFilter;
    static float defaultExposure;

    static float defaultLensDistortion;
    static public float LensDistortionValue { get; set; }

    static float defaultVignette;
    static public float VignetteValue { get; set; }

    public PostProcessingHandler(Volume volume)
    {
        //Instance = this;

        //this.volume = GetComponent<Volume>();
        PostProcessingHandler.volume = volume;
        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out depthOfField);
        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out whiteBalance);

        defaultColorFilter = colorAdjustments.colorFilter.value;
        defaultExposure = colorAdjustments.postExposure.value;
        defaultVignette = VignetteValue = vignette.intensity.value;
        defaultLensDistortion = lensDistortion.intensity.value;
    }

    public static void SetActive<T>(bool value) where T : VolumeComponent
    {
        GetEffect<T>().active = value;
    }

    public static T GetEffect<T>() where T : VolumeComponent
        => volume.profile.TryGet<T>(out var effect) ? effect : null;

    public static void SetWhiteBalance(float value = 0)
    {
        whiteBalance.temperature.value = value;
    }

    public static void SetDOF(bool blur)
    {
        depthOfField.active = blur;
    }

    public static void ResetLensDistortion(float smoothTime = 0)
    {
        LensDistortionValue = defaultLensDistortion;
        SetLensDistortion(smoothTime, LensDistortionValue);
    }

    public static void SetLensDistortion(float smoothTime, float value = 0)
    {
        lensDistortion.intensity.Lerp(smoothTime, value);
    }

    public static void SetChromaticAberration(float smoothTime, float value = 0)
    {
        chromaticAberration.intensity.Lerp(smoothTime, value);
    }

    public static void ResetColorFilter(float smoothTime)
    {
        SetColorFilter(smoothTime, defaultColorFilter);
    }

    public static void SetColorFilter(float smoothTime, Color colorFilter = default)
    {
        colorAdjustments.colorFilter.Lerp(smoothTime, colorFilter == default ? defaultColorFilter : colorFilter);
    }

    public static void SetPostExposure(float smoothTime, float value = 0)
    {
        colorAdjustments.postExposure.Lerp(smoothTime, value);
    }

    public static void ResetVignette(float smoothTime = 0)
    {
        VignetteValue = defaultVignette;
        SetVignette(smoothTime);
    }

    public static void SetVignette(float smoothTime, Vector2 offset = default, float offsetTime = 3)
    {
        SetVignette(smoothTime, VignetteValue, offset, offsetTime);
    }

    public static void SetVignette(float smoothTime, float value, Vector2 offset = default, float offsetTime = 3)
    {
        offset = offset == Vector2.zero ? new Vector2(0.5f, 0.5f) : offset;

        vignette.center.Lerp(offsetTime, offset);
        vignette.intensity.Lerp(smoothTime, value);
    }

    public static void SetVignetteColor(float smoothTime, Color value)
    {
        vignette.color.Lerp(smoothTime, value);
    }
}
