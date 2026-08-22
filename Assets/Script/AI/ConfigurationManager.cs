using System.Collections.Generic;
using System.IO;
using UnityEngine;




public class ConfigurationManager : MonoBehaviour
{
    private static ConfigurationManager _instance;
    public static ConfigurationManager Instance
    {
        get
        {
            if (_instance == null && Application.isPlaying)
            {
                GameObject go = new GameObject("[ConfigurationManager]");
                _instance = go.AddComponent<ConfigurationManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

  #region ��������ֱ�Ӳ�ѯ������װ�ص�ʵ�壬ֻͨ��idӳ��
  #region �����ù���
    public Dictionary<int, ConstructionConfigItem> ConstructionConfigItems { get; } = new Dictionary<int, ConstructionConfigItem>();
    /* public Dictionary<int, EntityResourceConfigItem> EntityResourceConfigItems { get; } = new Dictionary<int, EntityResourceConfigItem>();
     public Dictionary<int, TechNodeConfig> TechNodeConfigItems { get; } = new Dictionary<int, TechNodeConfig>();
     public Dictionary<int, OccultNodeConfig> OccultNodeConfigItems { get; } = new Dictionary<int, OccultNodeConfig>();
     public Dictionary<int, SkillNodeConfig> SkillNodeConfigItems { get; } = new Dictionary<int, SkillNodeConfig>();
     public Dictionary<int, BuffConfig> BuffConfigItems { get; } = new Dictionary<int, BuffConfig>();
     public Dictionary<int, ReactionConfig> ReactionConfigItems { get; } = new Dictionary<int, ReactionConfig>();
     public Dictionary<int, EntityConfig> EntityConfigItems { get; } = new Dictionary<int, EntityConfig>();
     public Dictionary<int, MessageConfig> MessageConfigItems { get; } = new Dictionary<int, MessageConfig>();
     public Dictionary<int, RoleConfig> RoleConfigItems { get; } = new Dictionary<int, RoleConfig>();
     public Dictionary<int, RoleTalkConfig> RoleTalkConfigItems { get; } = new Dictionary<int, RoleTalkConfig>();
     public Dictionary<int, SoundConfigItem> SoundConfigItems { get; } = new Dictionary<int, SoundConfigItem>();
     public Dictionary<int, AITaskConfigItem> AITaskConfigItems { get; } = new Dictionary<int, AITaskConfigItem>();
     public Dictionary<int, SideTaskConfig> SideTaskConfigItems { get; } = new Dictionary<int, SideTaskConfig>();
     public Dictionary<int, RoomItemConfig> RoomItemConfigItems { get; } = new Dictionary<int, RoomItemConfig>();
     public Dictionary<int, GermConfig> GermConfigItems { get; } = new Dictionary<int, GermConfig>();
     public Dictionary<int, MapAreaConfig> MapAreaConfigItems { get; } = new Dictionary<int, MapAreaConfig>();*/
     #endregion

/*   public Dictionary<int, CookBookItem> CookBookConfigs { get; } = new Dictionary<int, CookBookItem>();
     public Dictionary<int, PlantItemConfig> PlantItemConfigs { get; } = new Dictionary<int, PlantItemConfig>();
     public Dictionary<int, SeedConfigItem> SeedConfigItems { get; } = new Dictionary<int, SeedConfigItem>();*/
     #endregion

  /*   private CookBookConfigComponent cookBookConfigComponent;
     private PlantConfig plantConfig;
     private SeedConfig seedConfig;

     private RandomStringConfig shipPrefixs;
     private RandomStringConfig shipSuffixs;

     private EnumConfig enumConfig;
     public EnumConfig EnumConfig => enumConfig;

     private void Awake()
     {
         InitializConfigs();
         InitializConfigPuls();




     }




     private void InitializConfigs()
     {
         cookBookConfigComponent = Resources.Load<CookBookConfigComponent>("Config/cookBookConfigComponent");
         plantConfig = Resources.Load<PlantConfig>("Config/plantConfig");
         seedConfig = Resources.Load<SeedConfig>("Config/SeedConfig");

         shipPrefixs = Resources.Load<RandomStringConfig>("Config/Random/FleetStrs_0");
         shipSuffixs = Resources.Load<RandomStringConfig>("Config/Random/FleetStrs_1");
         enumConfig = Resources.Load<EnumConfig>("Config/Basic/EnumConfig");

         cookBookConfigComponent.configs.ForEach(x => CookBookConfigs.Add(x.id, x));
         plantConfig.items.ForEach(x => PlantItemConfigs.Add(x.id, x));
         seedConfig.items.ForEach(x => SeedConfigItems.Add(x.id, x));
     }

     private void InitializConfigPuls()
     {
         var entityResourceConfigComponent = Resources.Load<EntityResourceConfigComponent>("Config/EntityResourceConfigComponent");
         var constructionConfigComponent = Resources.Load<ConstructionConfigComponent>("Config/ConstructionConfigComponent");
         var techTreeConfig = Resources.Load<TechTreeConfig>("Config/TechTreeConfig");
         var occultNodeConfigComponent = Resources.Load<OccultNodeConfigComponent>("Config/OccultNodeConfigComponent");
         var skillNodeConfigComponent = Resources.Load<SkillNodeConfigComponent>("Config/SkillNodeConfigComponent");
         var buffConfigComponent = Resources.Load<BuffConfigComponent>("Config/BuffConfigComponent");
         var reactionConfigComponent = Resources.Load<ReactionConfigComponent>("Config/ReactionConfigComponent");
         var entityConfigComponent = Resources.Load<EntityConfigComponent>("Config/EntityConfigComponent");
         var messageConfigComponent = Resources.Load<InformationConfig>("Config/InformationConfig");
         var roleConfigComponent = Resources.Load<RoleConfigComponent>("Config/RoleConfigComponent");
         var roleTalkConfigComponent = Resources.Load<RoleTalkConfigComponent>("Config/RoleTalkConfigComponent");
         var soundConfigComponent = Resources.Load<SoundConfig>("Config/Option/SoundConfig");
         var aITaskConfigComponent = Resources.Load<AITaskConfigComponent>("Config/AITaskConfigComponent");
         var sideTaskConfigComponent = Resources.Load<SideTaskConfigComponent>("Config/SideTaskConfigComponent");
         var roomItemConfigComponent = Resources.Load<RoomItemConfigComponent>("Config/RoomItemConfigComponent");
         var germConfigComponent = Resources.Load<GermConfigComponent>("Config/GermConfigComponent");
         var mapAreaConfigComponent = Resources.Load<MapAreaConfigComponent>("Config/Basic/MapAreaConfigComponent");

         entityResourceConfigComponent.entityResources.ForEach(x => EntityResourceConfigItems.Add(x.id, x));
         constructionConfigComponent.constructionConfigs.ForEach(x => ConstructionConfigItems.Add(x.id, x));
         techTreeConfig.configItems.ForEach(x => TechNodeConfigItems.Add(x.id, x));
         occultNodeConfigComponent.configItems.ForEach(x => OccultNodeConfigItems.Add(x.id, x));
         skillNodeConfigComponent.configItems.ForEach(x => SkillNodeConfigItems.Add(x.id, x));
         buffConfigComponent.configItems.ForEach(x => BuffConfigItems.Add(x.id, x));
         reactionConfigComponent.configItems.ForEach(x => ReactionConfigItems.Add(x.id, x));
         entityConfigComponent.configItems.ForEach(x => EntityConfigItems.Add(x.id, x));
         messageConfigComponent.configItems.ForEach(x => MessageConfigItems.Add(x.id, x));
         roleConfigComponent.configItems.ForEach(x => RoleConfigItems.Add(x.id, x));
         roleTalkConfigComponent.configItems.ForEach(x => RoleTalkConfigItems.Add(x.id, x));
         soundConfigComponent.configItems.ForEach(x => SoundConfigItems.Add(x.id, x));
         aITaskConfigComponent.configItems.ForEach(x => AITaskConfigItems.Add(x.id, x));
         sideTaskConfigComponent.configItems.ForEach(x => SideTaskConfigItems.Add(x.id, x));
         roomItemConfigComponent.configItems.ForEach(x => RoomItemConfigItems.Add(x.id, x));
         germConfigComponent.configItems.ForEach(x => GermConfigItems.Add(x.id, x));
         mapAreaConfigComponent.configItems.ForEach(x => MapAreaConfigItems.Add(x.id, x));


     }




     public void TranslateConfig()
     {
 #if !UNITY_EDITOR
         OnTranslateConfig();
 #endif
     }



     private void OnTranslateConfig()
     {
         foreach (var config in OccultNodeConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in SkillNodeConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in BuffConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in ReactionConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in ConstructionConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in EntityResourceConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in TechNodeConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in EntityConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in MessageConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in RoleConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in RoleTalkConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in AITaskConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in SideTaskConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in RoomItemConfigItems.Values)
         {
             config.OnTranslation();
         }
         foreach (var config in GermConfigItems.Values)
         {
             config.OnTranslation();
         }

         shipPrefixs.OnTranslation();
         shipSuffixs.OnTranslation();
         enumConfig.OnTranslation();
     }

     #region ���������Զ���



     private void GetConfigText()
     {
         var str = "";
         foreach (var item in GermConfigItems)
         {
             str += $"Config/Germ/{item.Key}/Title,\"{item.Value.title}\"\n" +
                 $"Config/Germ/{item.Key}/Explain,\"{item.Value.explain}\"\n";
         }
         File.WriteAllText($"{Application.persistentDataPath}/Temp/ConfigText.txt", str);
         Debug.Log("Get Config Text");
     }



     private void WriteConfigText()
     {
 #if UNITY_EDITOR
         foreach (var item in GermConfigItems)
         {




             item.Value.translationNameTitle = $"Config/Germ/{item.Key}/Title";
             item.Value.translationNameExplain = $"Config/Germ/{item.Key}/Explain";
             UnityEditor.EditorUtility.SetDirty(item.Value);
         }
         UnityEditor.AssetDatabase.SaveAssets();
         Debug.Log("Write Config Text");
 #endif
     }




     private void RneP2OtherAssets()
     {
 #if UNITY_EDITOR
         var str = "";
         foreach (var item in RoleTalkConfigItems)
         {
             for (int i = 0; i < item.Value.talks.Count; i++)
             {
                 str += $"Config/Talk/Role/{item.Key}/Talk/{i},\"{item.Value.talks[i]}\"\n";
                 item.Value.translationNameTalks.Add($"Config/Talk/Role/{item.Key}/Talk/{i}");
             }




             UnityEditor.EditorUtility.SetDirty(item.Value);
         }
         UnityEditor.AssetDatabase.SaveAssets();
         File.AppendAllText($"{Application.persistentDataPath}/Temp/TranslateUIText.txt", str + "\n");
 #endif
     }
     #endregion*/
}