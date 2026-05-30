using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// 飞船装配界面核心控制器（适配左右一体化布局）
/// 功能：玩家拥有飞船卡片滚动、飞船展示、挂载点装配配件、属性显示
/// </summary>
public class SpaceshipAssemblyUI : MonoBehaviour
{
    #region UI引用（按功能分区）
    [Header("=== 左侧卡片滚动区 ===")]
    public Transform cardContainer;          // ScrollRect的Content（卡片容器）
    public GameObject shipCardPrefab;        // 飞船卡片预制体

    [Header("=== 右侧展示区 ===")]
    public Transform shipDisplayRoot;        // 飞船3D模型展示根节点
    public Camera assemblyCamera;            // 装配界面专用相机
    public Image shipPreviewImage;           // 飞船预览图Image
    public TMP_Text shipNameDisplayText;     // 展示区飞船名称文本

    [Header("=== 飞船属性面板 ===")]
    public TMP_Text totalMountPointsText;    // 总挂载点数文本
    public TMP_Text weaponPointsText;        // 武器挂载点数文本
    public TMP_Text enginePointsText;        // 引擎挂载点数文本
    public TMP_Text shieldPointsText;        // 护盾挂载点数文本
    public TMP_Text shipHPText;              // 飞船血量文本
    public TMP_Text shipSpeedText;           // 飞船速度文本
    public TMP_Text shipArmorText;           // 飞船装甲文本

    [Header("=== 游戏内对象引用 ===")]
    public GameObject gameShip;              // 游戏场景中真正的飞船对象（非预览用）

    [Header("=== 配件装配弹窗 ===")]
    public GameObject partSelectPopup;       // 配件选择弹窗（默认隐藏）
    public TMP_Text popupTitleText;          // 弹窗标题文本
    public ScrollRect partListScrollRect;    // 配件列表的ScrollRect组件（用于重置位置）
    public Transform partListContainer;      // 配件列表容器（ScrollRect的Content）
    public GameObject partItemPrefab;        // 配件项预制体（挂载SpaceshipPartItemUI）
    public Button closePopupButton;          // 关闭弹窗按钮
    #endregion

    [Header("=== 界面显隐控制 ===")]
    public GameObject uiRootPanel;           // 装配界面的根物体（Canvas下的Panel）
    public KeyCode toggleKey = KeyCode.E;    // 控制显隐的按键（默认E键）

    // ========== 新增：6个挂载点预制体配置（核心） ==========
    [Header("=== 挂载点预制体（固定炮/炮台+大中小） ===")]
    // 固定炮预制体（大/中/小）
    public GameObject fixedCannon_Large;
    public GameObject fixedCannon_Medium;
    public GameObject fixedCannon_Small;
    // 炮台预制体（大/中/小）
    public GameObject turret_Large;
    public GameObject turret_Medium;
    public GameObject turret_Small;
    #region 数据引用
    [Header("=== 数据配置 ===")]
    public PlayerSpaceshipData playerShipData;    // 玩家飞船数据（拥有的飞船）
    public List<SpaceshipPartData> allParts;      // 所有可装配的配件数据
    #endregion

    [Header("=== 全局配置 ===")]
    public TMP_FontAsset defaultFont;        // 默认字体（需支持中文）

    #region 状态变量
    public static bool IsUIOpen { get; private set; } // 全局静态变量，供游戏逻辑查询界面是否开启

    private GameObject currentShipInstance;       // 当前显示的飞船3D实例
    private List<GameObject> currentMountPoints;  // 当前显示的挂载点标记列表
    private SpaceshipData currentShipData;        // 当前选中的飞船数据
    private MountPointData selectedMountPoint;    // 当前选中的挂载点（用于装配配件）
    private Dictionary<string, GameObject> mountedParts; // 已装配的配件（挂载点名→配件实例）
    private ScrollRect shipScrollRect;            // 左侧卡片滚动组件（优化滚动体验）
    #endregion

    private void Start()
    {
     
        // 初始化滚动组件（优化体验）
        InitScrollSetting();
        // 初始化状态变量
        currentMountPoints = new List<GameObject>();
        mountedParts = new Dictionary<string, GameObject>();

        // 初始化配件弹窗
        InitPartPopup();
        // 初始隐藏飞船预览图
        shipPreviewImage.enabled = false;

        // 加载玩家数据并初始化拥有的飞船卡片
        playerShipData.LoadData();
        Debug.Log("创建");
        InitializeOwnedShipCards();
    }

    private void Update()
    {
        // 持续同步 UI 状态，防止外部修改 Active 状态导致 IsUIOpen 不一致
        if (uiRootPanel != null)
        {
            IsUIOpen = uiRootPanel.activeSelf;
        }

        // 监听按键按下（例如 E 键）
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleAssemblyUI();
        }
    }

    /// <summary>
    /// 切换装配界面的可见性
    /// </summary>
    public void ToggleAssemblyUI()
    {
        if (uiRootPanel != null)
        {
            // 防御性检查：如果 uiRootPanel 恰好就是脚本所在的物体，关了就打不开了
            if (uiRootPanel == gameObject)
            {
                Debug.LogError("[SpaceshipAssemblyUI] uiRootPanel 不能是脚本所在的物体本身，否则关了之后脚本就停止运行，无法再次打开了！请将脚本挂在一个永远开启的 Manager 物体上，或者将 uiRootPanel 设置为脚本物体的子物体。");
                return;
            }

            bool isActive = !uiRootPanel.activeSelf;
            uiRootPanel.SetActive(isActive);
            IsUIOpen = isActive; // 更新静态变量状态

            // 如果隐藏界面，同时关闭配件选择弹窗
            if (!isActive)
            {
                ClosePartPopup();
            }
            
            Debug.Log($"[SpaceshipAssemblyUI] 界面显示状态切换为: {isActive}");
        }
        else
        {
            Debug.LogWarning("[SpaceshipAssemblyUI] uiRootPanel 未赋值，无法切换显示！");
        }
    }

    #region 初始化相关
    /// <summary>
    /// 初始化滚动组件设置（适配一体化布局，防止滚动穿透）
    /// </summary>
    private void InitScrollSetting()
    {
        shipScrollRect = cardContainer.GetComponentInParent<ScrollRect>();
        if (shipScrollRect != null)
        {
            shipScrollRect.inertia = true;                  // 开启滚动惯性
            shipScrollRect.decelerationRate = 0.135f;       // 惯性减速速率（越小小越丝滑）
            shipScrollRect.movementType = ScrollRect.MovementType.Clamped; // 滚动到边缘停止
        }
    }

    /// <summary>
    /// 初始化配件弹窗
    /// </summary>
    private void InitPartPopup()
    {
        partSelectPopup.SetActive(false);
        if (closePopupButton != null)
        {
            closePopupButton.onClick.AddListener(ClosePartPopup);
        }
    }

    /// <summary>
    /// 初始化「玩家拥有的飞船卡片」（核心：只显示拥有的飞船）
    /// </summary>
    private void InitializeOwnedShipCards()
    {

        // 1. 清空现有卡片
        ClearAllChild(cardContainer);

        // 2. 获取玩家拥有的飞船列表
        List<SpaceshipData> ownedShips = playerShipData.GetOwnedShips();

        // 3. 无飞船时显示提示
        if (ownedShips.Count == 0)
        {
            CreateEmptyTip(cardContainer, "暂无拥有的飞船，去商店解锁吧！");
            return;
        }

        // 4. 生成拥有的飞船卡片
        foreach (var shipData in ownedShips)
        {
            CreateShipCard(shipData);
        }

   
    }

    /// <summary>
    /// 创建单个飞船卡片
    /// </summary>
    private void CreateShipCard(SpaceshipData shipData)
    {
        if (shipCardPrefab == null)
        {
            Debug.LogError("飞船卡片预制体未赋值！");
            return;
        }

        // 实例化卡片
        GameObject cardObj = Instantiate(shipCardPrefab, cardContainer);
        cardObj.name = $"ShipCard_{shipData.shipName}";

        // 设置卡片内容
        Image previewImage = cardObj.transform.Find("PreviewImage")?.GetComponent<Image>();
        TMP_Text nameText = cardObj.transform.Find("ShipNameText")?.GetComponent<TMP_Text>();

        if (previewImage != null) previewImage.sprite = shipData.previewImage;
        if (nameText != null) nameText.text = shipData.shipName;

        // 添加卡片点击事件
        Button cardButton = cardObj.GetComponent<Button>();
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(() => OnShipCardClicked(shipData));
        }
    }
    #endregion

    #region 飞船卡片交互
    /// <summary>
    /// 点击飞船卡片后的核心逻辑
    /// </summary>
    private void OnShipCardClicked(SpaceshipData shipData)
    {
        if (shipData == null) return;

        // 保存当前选中的飞船数据
        currentShipData = shipData;

        // 1. 显示飞船预览图和名称
        ShowShipPreview();

        // 2. 加载飞船模型并生成挂载点标记
        LoadShipModelAndMountPoints();

        // 3. 填充飞船属性面板
 //       FillShipAttributes();

        // 4. 清空已装配的配件（切换飞船时重置）
        ClearMountedParts();
    }

    /// <summary>
    /// 显示飞船预览图和名称
    /// </summary>
    private void ShowShipPreview()
    {
        shipPreviewImage.enabled = true;
        shipPreviewImage.sprite = currentShipData.previewImage;
        shipNameDisplayText.text = currentShipData.shipName;
    }

    /// <summary>
    /// 加载飞船模型并生成挂载点标记
    /// </summary>
    private void LoadShipModelAndMountPoints()
    {
        // 销毁原有飞船模型
        if (currentShipInstance != null)
        {
            Destroy(currentShipInstance);
        }

        // 销毁原有挂载点标记
        ClearMountPoints();

        // 实例化新飞船模型
        if (currentShipData != null && currentShipData.shipPrefab != null)
        {
            currentShipInstance = Instantiate(currentShipData.shipPrefab, shipDisplayRoot);
            currentShipInstance.name = $"Display_{currentShipData.shipName}";
            // 确保位置和旋转正确
            currentShipInstance.transform.localPosition = Vector3.zero;
            currentShipInstance.transform.localRotation = Quaternion.identity;
        }

        // 生成挂载点标记
        GenerateMountPointMarkers();
    }

    /// <summary>
    /// 填充飞船属性面板
    /// </summary>
 /*   private void FillShipAttributes()
    {
        // 统计各类挂载点数量
        int weaponCount = 0;
        int engineCount = 0;
        int shieldCount = 0;

        Ship shipDate = currentShipData.shipPrefab.GetComponent<Ship>();

        foreach (var point in shipDate .mountPoints)
        {
            switch (point.pointType)
            {
                case MountPointType.Weapon: weaponCount++; break;
                case MountPointType.Engine: engineCount++; break;
                case MountPointType.Shield: shieldCount++; break;
            }
        }

        // 填充文本（空值保护）
        totalMountPointsText?.SetText($"总挂载点：{shipDate .mountPoints.Length}");
        weaponPointsText?.SetText($"武器挂载点：{weaponCount}");
        enginePointsText?.SetText($"引擎挂载点：{engineCount}");
        shieldPointsText?.SetText($"护盾挂载点：{shieldCount}");

        // 填充基础属性
        shipHPText?.SetText($"最大血量：{currentShipData.maxHP}");
        shipSpeedText?.SetText($"最大速度：{currentShipData.speed}");
        shipArmorText?.SetText($"装甲值：{currentShipData.armor}");
    }*/
    #endregion

    #region 挂载点相关
    /// <summary>
    /// 生成挂载点标记（带点击事件）
    /// </summary>
    private void GenerateMountPointMarkers()
    {
        Ship shipDate = currentShipData.shipPrefab.GetComponent<Ship>();
        if (currentMountPoints != null && currentMountPoints.Count > 0)
        {
            foreach (var marker in currentMountPoints) Destroy(marker);
            currentMountPoints.Clear();
        }
        if (currentShipData == null || currentShipData == null) return;

        // 遍历所有挂载点
        foreach (var mountPoint in shipDate.mountPoints)
        {
            // 空值保护：挂载点名称为空则跳过
            if (string.IsNullOrEmpty(mountPoint.pointName))
            {
                Debug.LogWarning("挂载点名称为空，跳过生成！");
                continue;
            }

            // 核心：找到飞船实例中对应的子物体（保留子物体挂载逻辑）
/*            Transform mountPointParentTF = currentShipInstance.transform.Find(mountPoint.pointName);
            if (mountPointParentTF == null)
            {
                Debug.LogWarning($"飞船实例中未找到子物体：{mountPoint.pointName}，请检查预制体子物体名称！");
                continue;
            }*/

            // ========== 关键：根据「种类+尺寸」匹配对应的预制体 ==========
            GameObject targetPrefab = GetMountPointPrefab(mountPoint.pointType, mountPoint.maxSupportSize);
            // 空值保护：预制体未配置时，降级到固定炮-中尺寸（避免崩溃）
            if (targetPrefab == null)
            {
                Debug.LogWarning($"[{mountPoint.pointType}-{mountPoint.maxSupportSize}] 预制体未配置，使用默认预制体！");
                targetPrefab = fixedCannon_Medium;
            }

            // ========== 实例化预制体（挂载到飞船子物体下） ==========
            GameObject marker = Instantiate(targetPrefab, shipPreviewImage.transform );  // 挂在预览图下面
            // 命名规范：种类_名称_尺寸，方便调试
            marker.name = $"MountPoint_{mountPoint.pointType}_{mountPoint.pointName}_{mountPoint.maxSupportSize}";

            // ========== 设置位置（你的Vector2转3D逻辑，保留） ==========
     //      Vector3 targetLocalPos = Convert2DTo3DPos(mountPoint.mountPointLocalPos2D);
             RectTransform rt = marker.GetComponent<RectTransform>();

     // 核心：归一化坐标 → UI 位置
             rt.anchorMin = mountPoint.previewAnchorPos;
             rt.anchorMax = mountPoint.previewAnchorPos;
             rt.anchoredPosition = Vector2.zero;
             rt.localScale = Vector3.one;
   //         marker.transform.localEulerAngles = Vector3.zero; // 旋转归0，可按需调整

            // ========== 绑定点击事件（保留原有逻辑） ==========
            AddMountPointClickEvent(marker, mountPoint);
            currentMountPoints.Add(marker);
        }
    }

    /// <summary>
    /// 设置挂载点标记颜色
    /// </summary>
 /*   private void SetMountPointColor(GameObject marker, MountPointType type)
    {
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer == null) return;

        switch (type)
        {
            case MountPointType.Weapon:
                markerRenderer.material.color = weaponPointColor;
                break;
            case MountPointType.Engine:
                markerRenderer.material.color = enginePointColor;
                break;
            case MountPointType.Shield:
                markerRenderer.material.color = shieldPointColor;
                break;
        }
    }*/

    /// <summary>
    /// 给挂载点添加点击事件（打开配件选择弹窗）
    /// </summary>
    private void AddMountPointClickEvent(GameObject marker, MountPointData mountPoint)
    {
        // 1. 确保有 Image 组件且 raycastTarget 为 true (UI 点击的基础)
        Image img = marker.GetComponent<Image>();
        if (img == null)
        {
            // 如果根节点没有Image，尝试找子物体
            img = marker.GetComponentInChildren<Image>();
            
            // 如果都没有，给根节点加一个透明Image
            if (img == null)
            {
                img = marker.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0); // 透明
            }
        }
        img.raycastTarget = true;



        // 3. 使用标准 Button 组件
        Button btn = marker.GetComponent<Button>();
        if (btn == null)
        {
            btn = marker.AddComponent<Button>();
            // 设置 Transition 为 None 防止没有 Target Graphic 报错或视觉异常
            btn.transition = Selectable.Transition.None; 
        }

        // 4. 强制绑定事件 (使用 lambda 捕获 mountPoint)
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => 
        {
            Debug.Log($"[SpaceshipAssemblyUI] 按钮点击触发: {mountPoint.pointName}");
            OnMountPointClicked(mountPoint);
        });
        
        // 5. 确保 CanvasGroup 不会阻挡 (如果有)
        CanvasGroup cg = marker.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        Debug.Log($"[SpaceshipAssemblyUI] 挂载点 {marker.name} 点击事件绑定完成");
    }

    /// <summary>
    /// 点击挂载点后的处理（打开配件弹窗）
    /// </summary>
    private void OnMountPointClicked(MountPointData mountPoint)
    {
        selectedMountPoint = mountPoint;
    

        // 配置弹窗标题
 //       popupTitleText.text = $"装配 - {mountPoint.pointName} ({mountPoint.pointType})";

        // 筛选匹配的配件
        List<SpaceshipPartData> matchedParts = FilterMatchedParts(mountPoint);
        
        // 生成配件列表
        GeneratePartList(matchedParts);
        Debug.Log("显示");
        // 显示弹窗
        partSelectPopup.SetActive(true);

    }

    /// <summary>
    /// 清空挂载点标记
    /// </summary>
    private void ClearMountPoints()
    {
        foreach (var marker in currentMountPoints)
        {
            Destroy(marker);
        }
        currentMountPoints.Clear();
    }
    #endregion

    #region 配件装配相关
    /// <summary>
    /// 筛选当前挂载点可装配的配件（按类型+尺寸匹配）
    /// </summary>
    private List<SpaceshipPartData> FilterMatchedParts(MountPointData mountPoint)
    {
        List<SpaceshipPartData> matched = new List<SpaceshipPartData>();

        if (allParts == null || allParts.Count == 0)
        {
            Debug.LogError("【配置错误】SpaceshipAssemblyUI 的 All Parts 列表为空！请在 Inspector 中赋值。");
            return matched;
        }

        Debug.Log($"正在为挂载点 [{mountPoint.pointName}] 筛选配件... " +
                  $"支持类型: {string.Join(",", mountPoint.compatibleParts)}, 最大尺寸: {mountPoint.maxSupportSize}");

        foreach (var part in allParts)
        {
            if (part == null) continue;

            // 条件1：配件类型在挂载点兼容列表中
            bool typeMatch = mountPoint.compatibleParts.Contains(part.partType);
            // 条件2：配件尺寸 ≤ 挂载点支持的最大尺寸
            bool sizeMatch = (int)part.partSize <= (int)mountPoint.maxSupportSize;

            if (typeMatch && sizeMatch)
            {
                matched.Add(part);
            }
            else
            {
                // 可选：调试为何不匹配（如果需要详细调试可取消注释）
                // Debug.Log($"配件 [{part.partName}] 不匹配 - 类型匹配: {typeMatch} ({part.partType}), 尺寸匹配: {sizeMatch} ({part.partSize})");
            }
        }

        Debug.Log($"筛选结果：找到 {matched.Count} 个匹配的配件");
        return matched;
    }

    /// <summary>
    /// 生成配件选择列表
    /// </summary>
    private void GeneratePartList(List<SpaceshipPartData> parts)
    {
        // 1. 清空原有列表项
        ClearAllChild(partListContainer);

        // 2. 重置滚动位置到顶部
        if (partListScrollRect != null)
        {
            partListScrollRect.verticalNormalizedPosition = 1f;
        }

        // 3. 无匹配配件时显示提示
        if (parts.Count == 0)
        {
            CreateEmptyTip(partListContainer, "暂无匹配的配件");
            return;
        }

        // 4. 生成每个配件项
        foreach (var part in parts)
        {
            GameObject partItem = Instantiate(partItemPrefab, partListContainer);
            partItem.name = $"PartItem_{part.partName}";

            // 尝试获取 SpaceshipPartItemUI 脚本进行设置
            SpaceshipPartItemUI itemUI = partItem.GetComponent<SpaceshipPartItemUI>();
            if (itemUI != null)
            {
                // 使用新的脚本化设置
                itemUI.Setup(part, AssemblePartToMountPoint);
            }
            else
            {
                // 兼容旧的 Find 方式（如果预制体没挂脚本）
                Image partIcon = partItem.transform.Find("PartIcon")?.GetComponent<Image>();
                TMP_Text partNameText = partItem.transform.Find("PartName")?.GetComponent<TMP_Text>();

                if (partIcon != null) partIcon.sprite = part.partIcon;
                if (partNameText != null) partNameText.text = part.partName;

                Button partButton = partItem.GetComponent<Button>();
                if (partButton != null)
                {
                    partButton.onClick.AddListener(() => AssemblePartToMountPoint(part));
                }
            }
        }
    }

    /// <summary>
    /// 将配件装配到选中的挂载点
    /// </summary>
    private void AssemblePartToMountPoint(SpaceshipPartData part)
    {
        if (selectedMountPoint == null || part == null) return;

        // 1. 处理游戏中的飞船（Game Ship）
        if (gameShip != null)
        {
            ApplyPartToObject(gameShip, part, "Game");
        }

        // 2. 处理 UI 预览中的飞船实例（Preview Ship）
        if (currentShipInstance != null)
        {
            ApplyPartToObject(currentShipInstance, part, "Preview");
        }
        
        // 3. 更新 UI 预览图上的图标显示
        UpdateUIMarkerIcon(selectedMountPoint, part);

        // 4. 关闭弹窗并保存
        ClosePartPopup();
        if (currentShipData != null)
        {
            ShipConfigManager.SaveConfig(currentShipData.shipID, selectedMountPoint.pointName, part.partName);
        }

        Debug.Log($"成功装配 {part.partName} 到 {selectedMountPoint.pointName}");
    }

    /// <summary>
    /// 更新 UI 预览图上挂载点标记的图标（在挂载点上叠加显示配件图标）
    /// </summary>
    private void UpdateUIMarkerIcon(MountPointData mountPoint, SpaceshipPartData part)
    {
        if (currentMountPoints == null) return;

        foreach (var marker in currentMountPoints)
        {
            // 通过名称匹配对应的挂载点 UI 物体
            if (marker.name.Contains(mountPoint.pointName))
            {
                // 1. 查找是否已经存在叠加的图标物体
                Transform partIconTF = marker.transform.Find("EquippedPartIcon");
                GameObject partIconObj;
                
                if (partIconTF == null)
                {
                    // 2. 如果不存在，创建一个新的子物体
                    partIconObj = new GameObject("EquippedPartIcon");
                    partIconObj.transform.SetParent(marker.transform);
                    
                    // 设置 UI 位置和缩放
                    RectTransform rt = partIconObj.AddComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;
                    // 默认填满父物体（挂载点标记）
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }
                else
                {
                    partIconObj = partIconTF.gameObject;
                }

                // 3. 设置图标内容
                Image img = partIconObj.GetComponent<Image>();
                if (img == null)
                {
                    img = partIconObj.AddComponent<Image>();
                }

                img.sprite = part.partIcon;
                img.color = Color.white;
                img.preserveAspect = true;
                img.raycastTarget = false; // 叠加的图标不响应点击，防止干扰挂载点按钮

                Debug.Log($"[SpaceshipAssemblyUI] 已在挂载点 {mountPoint.pointName} 上叠加配件图标: {part.partName}");
                break;
            }
        }
    }

    /// <summary>
    /// 核心逻辑：将配件实例化并挂载到目标物体的对应挂载点下
    /// </summary>
    private void ApplyPartToObject(GameObject rootObject, SpaceshipPartData part, string tag)
    {
        // 查找目标挂载点子物体
        Transform mountPointTransform = rootObject.transform.Find(selectedMountPoint.pointName);
        if (mountPointTransform == null)
        {
            mountPointTransform = FindDeepChild(rootObject.transform, selectedMountPoint.pointName);
        }

        if (mountPointTransform != null)
        {
            // 清理该挂载点下已有的旧配件
            ClearAllChild(mountPointTransform);

            // 实例化新配件
            GameObject newPartInstance = Instantiate(part.partPrefab, mountPointTransform);
            newPartInstance.name = $"Mounted_{part.partName}_{tag}";
            
            // 重置位置、旋转、缩放
            newPartInstance.transform.localPosition = Vector3.zero;
            newPartInstance.transform.localRotation = Quaternion.identity;
            newPartInstance.transform.localScale = Vector3.one;

            // 如果是 UI 预览（可能需要调整 Layer 或 渲染顺序）
            if (tag == "Preview")
            {
                // 这里可以根据需要调整
            }
        }
        else
        {
            Debug.LogWarning($"[{tag}] 未在 {rootObject.name} 下找到挂载点：{selectedMountPoint.pointName}");
        }
    }

    /// <summary>
    /// 深度查找子物体
    /// </summary>
    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    /// <summary>
    /// 关闭配件选择弹窗
    /// </summary>
    private void ClosePartPopup()
    {
        partSelectPopup.SetActive(false);
        selectedMountPoint = null; // 清空选中的挂载点
    }

    /// <summary>
    /// 清空已装配的配件（切换飞船时调用）
    /// </summary>
    private void ClearMountedParts()
    {
        foreach (var part in mountedParts.Values)
        {
            Destroy(part);
        }
        mountedParts.Clear();
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 清空指定父物体下的所有子物体
    /// </summary>
    private void ClearAllChild(Transform parent)
    {
        if (parent == null) return;

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }
    private GameObject GetMountPointPrefab(PartType category, PartSize size)
    {
        switch (category)
        {
            // 固定炮：匹配对应尺寸的预制体
            case PartType.FixedCannon:
                return size switch
                {
                    PartSize.Large => fixedCannon_Large,
                    PartSize.Small => fixedCannon_Small,
                    _ => fixedCannon_Medium // 默认中尺寸
                };
            // 炮台：匹配对应尺寸的预制体
            case PartType.Turret:
                return size switch
                {
                    PartSize.Large => turret_Large,
                    PartSize.Small => turret_Small,
                    _ => turret_Medium // 默认中尺寸
                };
            // 未知种类：返回固定炮-中尺寸
            default:
                return fixedCannon_Medium;
        }
    }
    /// <summary>
    /// 创建空状态提示文本
    /// </summary>
    private void CreateEmptyTip(Transform parent, string tipText)
    {
        if (parent == null) return;

        GameObject emptyTip = new GameObject("EmptyTip");
        emptyTip.transform.SetParent(parent);
        emptyTip.transform.localScale = Vector3.one;

        // TMP_Text 是抽象类，必须添加 TextMeshProUGUI
        TMP_Text text = emptyTip.AddComponent<TextMeshProUGUI>();
        text.text = tipText;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 14;
        text.color = Color.gray;
        
        // 如果配置了字体，则应用
        if (defaultFont != null)
        {
            text.font = defaultFont;
        }

        // 适配布局
        RectTransform rt = emptyTip.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// 手动刷新飞船卡片列表（玩家获得新飞船后调用）
    /// </summary>
    public void RefreshOwnedShipCards()
    {
        playerShipData.LoadData();
        InitializeOwnedShipCards();
    }
    #endregion
}


