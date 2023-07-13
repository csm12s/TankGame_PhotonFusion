using Fusion;
using UnityEngine;

// newInput1
public enum InputButtons
{
    Fire1,
    Fire2,
    
    Ready,

    Item1,
    Item2,
    Item3,
    Item4,
    Item5,
}

public struct NetworkInputData : INetworkInput
{
    #region Simple input
    public NetworkButtons buttons;
    public Vector2 movementInput;
    #endregion

    #region advance input
    public const uint BUTTON_FIRE_PRIMARY = 1 << 11;
    public const uint BUTTON_FIRE_SECONDARY = 1 << 12;

    public Vector2 aimDirection;
    public uint IntButtons;

    public bool IsDown(uint button)
    {
        return (IntButtons & button) == button;
    }
    #endregion
}
