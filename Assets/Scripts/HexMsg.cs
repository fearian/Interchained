using System.Collections.Generic;
using UnityEngine;

public static class HexMsg
{
    public static Dictionary<Hex, string> _tileMsg = new Dictionary<Hex, string>();
    private static GUIStyle msgStyle = new GUIStyle();

    public static void AddMsg(Hex hex, string msg, bool additive = false)
    {
        if (_tileMsg.ContainsKey(hex))
        {
            if (additive) _tileMsg[hex] += "/"+msg;
            else _tileMsg[hex] = msg;
        }
        else
        {
            _tileMsg.Add(hex, msg);
        }
    }

    public static void ClearMsg(Hex hex)
    {
        if (_tileMsg.ContainsKey(hex))
        {
            _tileMsg.Remove(hex);
        }
    }

    public static void ClearMsg()
    {
        _tileMsg.Clear();
    }

#if UNITY_EDITOR
    public static void DrawMsg()
    {
        msgStyle.fontSize = 18;
        msgStyle.richText = true;
        msgStyle.alignment = TextAnchor.MiddleCenter;
        msgStyle.wordWrap = true;
        msgStyle.fixedWidth = 100f;
        foreach (var msg in _tileMsg)
        {
            UnityEditor.Handles.Label(msg.Key.ToWorld() + new Vector3(0,0,0.25f), msg.Value, msgStyle);
        }
    }
#endif

}