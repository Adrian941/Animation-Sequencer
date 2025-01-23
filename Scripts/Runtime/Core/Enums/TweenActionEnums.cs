namespace BrunoMikoski.AnimationSequencer
{
    public enum AnimationDirection
    {
        To,
        From
    }

    public enum DataInputType
    {
        Vector,
        Object
    }

    public enum DataInputTypeWithAnchor
    {
        Vector,
        Object,
        Anchor
    }

    public enum AnchorPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}