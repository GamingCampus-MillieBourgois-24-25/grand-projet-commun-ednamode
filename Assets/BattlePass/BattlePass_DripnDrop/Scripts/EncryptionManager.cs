using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace EasyBattlePass
{
    public static class EncryptionManager
    {
        private const string EncryptionKey = "0867432156783483"; // Change to a unique 16-character value.
        private static readonly byte[] Salt = { 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9a };

        // Wrapper functions to save and load different types of data.
        public static void Save<T>(string key, T data) => SaveData(key, data);
        public static T Load<T>(string key) => LoadData<T>(key);
        public static void SaveInt(string key, int value) => SaveData(key, value);
        public static int LoadInt(string key, int defaultValue = 0) => LoadData(key, defaultValue);

        private static void SaveData<T>(string key, T data)
        {
            try
            {
                byte[] rawData = SerializeData(data);
                byte[] encryptedData = Encrypt(rawData, EncryptionKey);
                string encryptedString = Convert.ToBase64String(encryptedData);

                PlayerPrefs.SetString(key, encryptedString);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving encrypted data: {ex.Message}");
            }
        }

        private static T LoadData<T>(string key, T defaultValue = default)
        {
            try
            {
                string encryptedString = PlayerPrefs.GetString(key, null);
                if (string.IsNullOrEmpty(encryptedString))
                {
                    Debug.LogWarning($"No encrypted data found for key {key}");
                    return defaultValue;
                }

                byte[] encryptedData = Convert.FromBase64String(encryptedString);
                byte[] rawData = Decrypt(encryptedData, EncryptionKey);

                return DeserializeData<T>(rawData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading encrypted data: {ex.Message}");
                return defaultValue;
            }
        }

        private static byte[] SerializeData<T>(T data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
                return stream.ToArray();
            }
        }

        private static T DeserializeData<T>(byte[] rawData)
        {
            using (MemoryStream stream = new MemoryStream(rawData))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }

        private static byte[] Encrypt(byte[] data, string key)
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, Salt);
            using (Aes aes = Aes.Create())
            {
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static byte[] Decrypt(byte[] data, string key)
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(key, Salt);
            using (Aes aes = Aes.Create())
            {
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}


