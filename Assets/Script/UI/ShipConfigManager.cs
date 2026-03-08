using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 管理飞船配件装配数据的持久化（保存/读取）
/// </summary>
public static class ShipConfigManager
{
    [System.Serializable]
    public class MountedPartInfo
    {
        public string mountPointName;
        public string partName;
    }

    [System.Serializable]
    public class ShipConfiguration
    {
        public string shipID;
        public List<MountedPartInfo> mountedParts = new List<MountedPartInfo>();
    }

    [System.Serializable]
    private class ConfigWrapper
    {
        public List<ShipConfiguration> configs;
    }

    private const string PREF_KEY = "ShipConfigurations";
    private static List<ShipConfiguration> cachedConfigs;

    /// <summary>
    /// 保存配件装配信息
    /// </summary>
    public static void SaveConfig(string shipID, string mountPointName, string partName)
    {
        LoadAllConfigs();

        var config = cachedConfigs.Find(c => c.shipID == shipID);
        if (config == null)
        {
            config = new ShipConfiguration { shipID = shipID };
            cachedConfigs.Add(config);
        }

        // 移除该挂载点已有的配件记录
        config.mountedParts.RemoveAll(p => p.mountPointName == mountPointName);

        // 添加新配件记录
        config.mountedParts.Add(new MountedPartInfo { mountPointName = mountPointName, partName = partName });

        SaveAllConfigs();
    }

    /// <summary>
    /// 获取指定飞船指定挂载点的配件名称
    /// </summary>
    public static string GetMountedPartName(string shipID, string mountPointName)
    {
        LoadAllConfigs();
        var config = cachedConfigs.Find(c => c.shipID == shipID);
        if (config != null)
        {
            var part = config.mountedParts.Find(p => p.mountPointName == mountPointName);
            if (part != null) return part.partName;
        }
        return null;
    }

    /// <summary>
    /// 获取指定飞船的所有装配信息
    /// </summary>
    public static List<MountedPartInfo> GetShipConfig(string shipID)
    {
        LoadAllConfigs();
        var config = cachedConfigs.Find(c => c.shipID == shipID);
        return config != null ? config.mountedParts : new List<MountedPartInfo>();
    }

    private static void LoadAllConfigs()
    {
        if (cachedConfigs != null) return;

        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            string json = PlayerPrefs.GetString(PREF_KEY);
            var wrapper = JsonUtility.FromJson<ConfigWrapper>(json);
            cachedConfigs = wrapper != null ? wrapper.configs : new List<ShipConfiguration>();
        }
        else
        {
            cachedConfigs = new List<ShipConfiguration>();
        }
    }

    private static void SaveAllConfigs()
    {
        string json = JsonUtility.ToJson(new ConfigWrapper { configs = cachedConfigs });
        PlayerPrefs.SetString(PREF_KEY, json);
        PlayerPrefs.Save();
    }
}
