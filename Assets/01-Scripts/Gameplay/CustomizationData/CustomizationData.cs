using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using CharacterCustomization;
using System;

[Serializable]
public class CustomizationData : INetworkSerializable
{
    public Dictionary<SlotType, string> equippedItemIds;
    public Dictionary<SlotType, Color32> equippedColors;
    public Dictionary<SlotType, string> equippedTextures;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        SerializeStringDict(serializer, ref equippedItemIds);
        SerializeColorDict(serializer, ref equippedColors);
        SerializeStringDict(serializer, ref equippedTextures);
    }

    private void SerializeStringDict<T>(BufferSerializer<T> serializer, ref Dictionary<SlotType, string> dict) where T : IReaderWriter
    {
        if (dict == null)
            dict = new Dictionary<SlotType, string>();

        if (serializer.IsWriter)
        {
            int count = dict.Count;
            serializer.SerializeValue(ref count);

            foreach (var kvp in dict)
            {
                int key = (int)kvp.Key;
                string value = kvp.Value ?? string.Empty;

                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref value);
            }
        }
        else
        {
            int count = 0;
            serializer.SerializeValue(ref count);

            dict = new Dictionary<SlotType, string>();

            for (int i = 0; i < count; i++)
            {
                int key = 0;
                string value = string.Empty;

                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref value);

                dict[(SlotType)key] = value;
            }
        }
    }

    private void SerializeColorDict<T>(BufferSerializer<T> serializer, ref Dictionary<SlotType, Color32> dict) where T : IReaderWriter
    {
        if (dict == null)
            dict = new Dictionary<SlotType, Color32>();

        if (serializer.IsWriter)
        {
            int count = dict.Count;
            serializer.SerializeValue(ref count);

            foreach (var kvp in dict)
            {
                int key = (int)kvp.Key;
                Color32 color = kvp.Value;

                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref color.r);
                serializer.SerializeValue(ref color.g);
                serializer.SerializeValue(ref color.b);
                serializer.SerializeValue(ref color.a);
            }
        }
        else
        {
            int count = 0;
            serializer.SerializeValue(ref count);

            dict = new Dictionary<SlotType, Color32>();

            for (int i = 0; i < count; i++)
            {
                int key = 0;
                byte r = 255, g = 255, b = 255, a = 255;

                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref r);
                serializer.SerializeValue(ref g);
                serializer.SerializeValue(ref b);
                serializer.SerializeValue(ref a);

                dict[(SlotType)key] = new Color32(r, g, b, a);
            }
        }
    }

    public void SetItem(SlotType slot, string itemId)
    {
        equippedItemIds ??= new();
        equippedItemIds[slot] = itemId;
    }

    public void SetColor(SlotType slot, Color color)
    {
        equippedColors ??= new();
        equippedColors[slot] = color;
        Debug.Log($"[CustomizationData] 🎨 SetColor → {slot}: {ColorUtility.ToHtmlStringRGBA(color)}");
    }

    public void SetTexture(SlotType slot, string texture)
    {
        equippedTextures ??= new();
        equippedTextures[slot] = texture;
    }

    public bool TryGetColor(SlotType slot, out Color color)
    {
        color = Color.white;
        if (equippedColors != null && equippedColors.TryGetValue(slot, out var storedColor))
        {
            color = storedColor;
            return true;
        }
        return false;
    }

    public bool TryGetTexture(SlotType slot, out string texture)
    {
        texture = null;
        return equippedTextures != null && equippedTextures.TryGetValue(slot, out texture);
    }

    public bool TryGetItem(SlotType slot, out string itemId)
    {
        itemId = null;
        return equippedItemIds != null && equippedItemIds.TryGetValue(slot, out itemId);
    }

    public void EnsureInitialized()
    {
        equippedItemIds ??= new();
        equippedColors ??= new();
        equippedTextures ??= new();
    }
}
