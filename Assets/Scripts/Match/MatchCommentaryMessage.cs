using System;

[Serializable]
public sealed class MatchCommentaryMessage
{
    public int rollValue;
    public string text;
    public string[] parameters;
    public string character;
    public string side;
    public string resourcePath;
}
